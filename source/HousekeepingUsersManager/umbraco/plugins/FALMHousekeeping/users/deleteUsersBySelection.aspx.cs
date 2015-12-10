using System;
using System.Data.SqlClient;
using System.Web.UI.WebControls;
using umbraco.BusinessLogic;
using umbraco.cms.businesslogic;
using umbraco.cms.businesslogic.language;
using umbraco.DataLayer;

namespace FALMHousekeepingUsersManager
{
	public partial class deleteUsersBySelectAction : System.Web.UI.Page
	{
		/// <summary>
		/// Initialize an SQL Helper Interface
		/// </summary>
		protected static ISqlHelper SqlHelper
		{
			get { return umbraco.BusinessLogic.Application.SqlHelper; }
		}

		/// <summary>
		/// Get Current Logged User
		/// </summary>
		protected User userCurrent = umbraco.BusinessLogic.User.GetCurrent();

		/// <summary>
		/// Backoffice User Language Id
		/// </summary>
		protected int iUserLanguageId = 0;

		/// <summary>
		/// iErr
		/// </summary>
		protected int iErr;

		/// <summary>
		/// Page_Load
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		protected void Page_Load(object sender,EventArgs e)
		{
			// Retrieve User Language
			GetUserLanguage();

			// Page Title
			PanelCleanupUser.Text = getDictionaryItem("users_PageTitle");

			// Page SubTitle
			if (getDictionaryItem("users_PageSubTitle") != string.Empty)
			{
				lblSubTitle.Text = "<p>" + getDictionaryItem("users_PageSubTitle") + "</p>";
			}

			// Button text
			btnDeleteUsers.Text = getDictionaryItem("users_Button_Delete");
		}

		/// <summary>
		/// Check if there is almost one user
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		protected void SqlDSUsers_Selected(object sender,SqlDataSourceStatusEventArgs e)
		{
			int iRows = e.AffectedRows;

			if (iRows <= 0)
			{
				//ltrlUsers.Text = "There are no users to delete.";
				ltrlUsers.Text = "<p>" + getDictionaryItem("users_NoUsersToDelete") + "</p>";
				btnDeleteUsers.Visible = false;
			}
		}

		/// <summary>
		/// Delete users action
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		protected void btnDeleteUsers_Click(object sender,EventArgs e)
		{
			string gvIDs = "";
			bool chkBox = false;
			//'Navigate through each row in the GridView for checkbox items
			foreach (GridViewRow gv in gvUsers.Rows)
			{
				CheckBox deleteChkBxItem = (CheckBox)gv.FindControl("chkbUser");
				if (deleteChkBxItem.Checked)
				{
					chkBox = true;
					// Concatenate GridView items with comma for SQL Delete
					gvIDs += ((Label)gv.FindControl("lblUserId")).Text.ToString() + ",";
				}
			}

			if (chkBox)
			{
				// All documents related to selected user(s) will change to the administrator
				string sqlDelChangeUmbracoNodeUser = "UPDATE umbracoNode SET nodeUser = 0 WHERE nodeUser IN (" + gvIDs.Substring(0,gvIDs.LastIndexOf(",")) + ")";
				sqlExecuteCleanup(sqlDelChangeUmbracoNodeUser);

				string sqlDelChangeCmsDocumentUser = "UPDATE cmsDocument SET documentUser = 0 WHERE documentUser IN (" + gvIDs.Substring(0,gvIDs.LastIndexOf(",")) + ")";
				sqlExecuteCleanup(sqlDelChangeCmsDocumentUser);

				// Clear all selected user(s) informations from the log (sessions and workflow's tasks)
				string sqlDelLogUmbracoUserLogins = "DELETE FROM umbracoUserLogins WHERE [User] IN (" + gvIDs.Substring(0,gvIDs.LastIndexOf(",")) + ")";
				sqlExecuteCleanup(sqlDelLogUmbracoUserLogins);

				string sqlDelLogCmsTask = "DELETE FROM cmsTask WHERE UserId IN (" + gvIDs.Substring(0,gvIDs.LastIndexOf(",")) + ") OR parentUserID IN (" + gvIDs.Substring(0,gvIDs.LastIndexOf(",")) + ")";
				sqlExecuteCleanup(sqlDelLogCmsTask);

				string sqlDelLogUmbracoLog = "DELETE FROM umbracoLog WHERE userId IN (" + gvIDs.Substring(0,gvIDs.LastIndexOf(",")) + ")";
				sqlExecuteCleanup(sqlDelLogUmbracoLog);

				// Delete all selected user(s) references
				string sqlDelUserUmbracoUser2app = "DELETE FROM umbracoUser2app WHERE [user] IN (" + gvIDs.Substring(0,gvIDs.LastIndexOf(",")) + ")";
				sqlExecuteCleanup(sqlDelUserUmbracoUser2app);

				string sqlDelUserUmbracoUser2NodeNotify = "DELETE FROM umbracoUser2NodeNotify WHERE userId IN (" + gvIDs.Substring(0,gvIDs.LastIndexOf(",")) + ")";
				sqlExecuteCleanup(sqlDelUserUmbracoUser2NodeNotify);

				string sqlDelUserUmbracoUser2NodePermission = "DELETE FROM umbracoUser2NodePermission WHERE userId IN (" + gvIDs.Substring(0,gvIDs.LastIndexOf(",")) + ")";
				sqlExecuteCleanup(sqlDelUserUmbracoUser2NodePermission);

				string sqlDelUserUmbracoUser2UserGroup = "DELETE FROM umbracoUser2UserGroup WHERE [user] IN (" + gvIDs.Substring(0,gvIDs.LastIndexOf(",")) + ")";
				sqlExecuteCleanup(sqlDelUserUmbracoUser2UserGroup);

				string sqlDelUserUmbracoUser = "DELETE FROM umbracoUser WHERE id IN (" + gvIDs.Substring(0,gvIDs.LastIndexOf(",")) + ")";
				sqlExecuteCleanup(sqlDelUserUmbracoUser);

				if (iErr == 0)
				{
					//ltrlUsers.Text = "<span style='color: Red; font-weight: bold;'>All selected users have been deleted.</span>";
					ltrlUsers.Text = "<span style='color: Red; font-weight: bold;'>" + getDictionaryItem("users_UsersDeleted") + "</span>";
				}

				// Refresh gridview
				gvUsers.DataBind();
			}
		}

		/// <summary>
		/// Cleanup SQL
		/// </summary>
		/// <param name="strSQL"></param>
		private void sqlExecuteCleanup(string strSQL)
		{
			SqlConnection cn = new SqlConnection(SqlDSUsers.ConnectionString);

			// Execute SQL Query only if checkboxes are checked to avoid any error with initial null string
			try
			{
				SqlCommand cmd = new SqlCommand(strSQL,cn);
				cn.Open();
				cmd.ExecuteNonQuery();

				iErr = 0;
			}
			catch (SqlException err)
			{
				iErr = 1;

				ltrlUsers.Text += "<span style='color: Red; font-weight: bold;'>" + err.Message.ToString() + "<br />" + strSQL + "</span><br />";
			}
			finally
			{
				cn.Close();
			}

		}

		/// <summary>
		/// Gridview RowDataBound
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		protected void gvUsers_RowDataBound(object sender,GridViewRowEventArgs e)
		{
			if (e.Row.RowType == DataControlRowType.DataRow)
			{
				Label lblUserDisabled = (Label)e.Row.FindControl("lblUserDisabled");

				if (lblUserDisabled.Text == "True")
				{
					e.Row.ForeColor = System.Drawing.Color.Gray;
				}
			}
		}

		/// <summary>
		/// Function to retrieve User Language Id
		/// </summary>
		protected void GetUserLanguage()
		{
			foreach (var language in Language.GetAllAsList())
			{
				if (iUserLanguageId == 0)
				{
					iUserLanguageId = language.id;
				}

				if (language.CultureAlias.Substring(0, 2) == userCurrent.Language.ToString())
				{
					iUserLanguageId = language.id;
					break;
				}
			}
		}

		/// <summary>
		/// Function to get dictionary item by user language
		/// </summary>
		/// <param name="strDictionaryItem"></param>
		/// <returns></returns>
		protected string getDictionaryItem(string strDictionaryItem)
		{
			return new Dictionary.DictionaryItem(strDictionaryItem).Value(iUserLanguageId);
		}

	}
}