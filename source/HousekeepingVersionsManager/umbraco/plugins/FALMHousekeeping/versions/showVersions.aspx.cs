using System;
using System.Web.UI.WebControls;
using umbraco.BusinessLogic;
using umbraco.cms.businesslogic;
using umbraco.cms.businesslogic.language;
using umbraco.DataLayer;

namespace FALMHousekeepingVersionsManager
{
	public partial class showVersions : System.Web.UI.Page
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
		/// Page Load
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		protected void Page_Load(object sender,EventArgs e)
		{
			// Retrieve User Language
			GetUserLanguage();

			// Page Title
			PanelShowVersions.Text = getDictionaryItem("versions_Show_PageTitle");

			// Page SubTitle
			if (getDictionaryItem("versions_Show_PageSubTitle") != string.Empty)
			{
				lblSubTitle.Text = "<p>" + getDictionaryItem("versions_Show_PageSubTitle") + "</p>";
			}

			// Set Filter Labels
			lblFilterByNodeId.Text = getDictionaryItem("versions_Show_FilterByNodeId");

			lblFilterByDateRangeFrom.Text = getDictionaryItem("versions_Show_FilterByDateRangeFrom");

			lblFilterByDateRangeTo.Text = getDictionaryItem("versions_Show_FilterByDateRangeTo");

			// Button Show Logs text
			btnShowVersions.Text = getDictionaryItem("versions_Show_Button_ShowVersions");

			if (!Page.IsPostBack)
			{
				gvCurVer.Visible = false;
			}
		}

		/// <summary>
		/// This function will show the version history
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		protected void btnShowVersions_Click(object sender,EventArgs e)
		{
			// Reset GridView Page to the top
			gvCurVer.PageIndex = 0;

			SqlDSCurrentVersion.SelectCommand = GetSQLCurrentVersion();
			gvCurVer.Visible = true;
		}

		/// <summary>
		///  This function return the SQLCommand of the current version published nodes
		/// </summary>
		/// <returns>strSQLCurrentVersion</returns>
		protected string GetSQLCurrentVersion()
		{
			string strSQLCurrentVersion = string.Empty;

			strSQLCurrentVersion = "SELECT DISTINCT CurDoc.nodeId, CurDoc.text, CurDoc.updateDate ";
			strSQLCurrentVersion += "FROM cmsDocument AS CurDoc INNER JOIN cmsDocument AS HistDoc ON CurDoc.nodeId = HistDoc.nodeId ";
			strSQLCurrentVersion += "WHERE (CurDoc.published = 1) ";
			if (!txtbNodeID.Text.Equals(string.Empty))
			{
				strSQLCurrentVersion += "AND CurDoc.nodeId = " + int.Parse(txtbNodeID.Text) + " ";
			}
			strSQLCurrentVersion += GetDateRangeFilter();
			strSQLCurrentVersion += "ORDER BY CurDoc.nodeId ASC";

			return strSQLCurrentVersion;
		}

		/// <summary>
		/// This function return the SQLCommand to shows all history versions for each node
		/// </summary>
		/// <param name="NodeId"></param>
		/// <returns></returns>
		protected string GetSQLHistoryVersions()
		{
			string strSQLHistoryVersion = string.Empty;
			strSQLHistoryVersion = "SELECT documentUser, versionId, updateDate, text, published, newest ";
			strSQLHistoryVersion += "FROM cmsDocument ";
			strSQLHistoryVersion += "WHERE nodeId = @NodeId ";
			strSQLHistoryVersion += GetDateRangeFilter().Replace("HistDoc.updateDate","updateDate");
			strSQLHistoryVersion += "ORDER BY updateDate DESC";

			return strSQLHistoryVersion;
		}

		/// <summary>
		/// This function return the date filter for SQLCommand
		/// </summary>
		/// <returns>strSQLDateRangeFilter</returns>
		protected string GetDateRangeFilter()
		{
			string strSQLDateRangeFilter = string.Empty;

			if ((dtpckrDateFrom.Text.Equals(string.Empty)) && (dtpckrDateTo.Text.Equals(string.Empty)))
			{
				dtpckrDateTo.DateTime = DateTime.Now.Subtract(new TimeSpan(0,0,0,0));

				strSQLDateRangeFilter += "AND HistDoc.updateDate <= CONVERT(DATETIME, '" + dtpckrDateTo.DateTime.ToString("yyyy.MM.dd") + " 23:59:00', 102) ";
			}
			else if ((dtpckrDateFrom.Text.Equals(string.Empty)) && (!dtpckrDateTo.Text.Equals(string.Empty)))
			{
				strSQLDateRangeFilter += "AND HistDoc.updateDate <= CONVERT(DATETIME, '" + dtpckrDateTo.DateTime.ToString("yyyy.MM.dd") + " 23:59:00', 102) ";
			}
			else if ((!dtpckrDateFrom.Text.Equals(string.Empty)) && (dtpckrDateTo.Text.Equals(string.Empty)))
			{
				strSQLDateRangeFilter += "AND HistDoc.updateDate >= CONVERT(DATETIME, '" + dtpckrDateFrom.DateTime.ToString("yyyy.MM.dd") + " 00:00:00', 102) ";
			}
			else if (dtpckrDateFrom.DateTime <= dtpckrDateTo.DateTime)
			{
				strSQLDateRangeFilter += "AND HistDoc.updateDate >= CONVERT(DATETIME, '" + dtpckrDateFrom.DateTime.ToString("yyyy.MM.dd") + " 00:00:00', 102) AND HistDoc.updateDate <= CONVERT(DATETIME, '" + dtpckrDateTo.DateTime.ToString("yyyy.MM.dd") + " 23:59:00', 102) ";
			}
			else
			{
				string strDateFrom = dtpckrDateFrom.Text;
				string strDateSince = dtpckrDateTo.Text;

				dtpckrDateFrom.DateTime = DateTime.Parse(strDateSince);
				dtpckrDateTo.DateTime = DateTime.Parse(strDateFrom);

				strSQLDateRangeFilter += "AND HistDoc.updateDate >= CONVERT(DATETIME, '" + dtpckrDateFrom.DateTime.ToString("yyyy.MM.dd") + " 00:00:00', 102) AND HistDoc.updateDate <= CONVERT(DATETIME, '" + dtpckrDateTo.DateTime.ToString("yyyy.MM.dd") + " 23:59:00', 102) ";
			}

			return strSQLDateRangeFilter;
		}

		/// <summary>
		/// This function shows all version history for each current node
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		protected void gvCurVer_RowDataBound(object sender,GridViewRowEventArgs e)
		{
			// Set Text for no versions to show
			if (e.Row.RowType.Equals(DataControlRowType.EmptyDataRow))
			{
				Label lblNoVersionsToShow = (Label)e.Row.FindControl("lblNoVersionsToShow");
				lblNoVersionsToShow.Text = getDictionaryItem("versions_Label_NoLogsToShow") + " '" + dtpckrDateFrom.DateTime.ToShortDateString() + "'";
			}

			if (e.Row.RowType == DataControlRowType.DataRow)
			{
				Label lblNodeId = (Label)e.Row.FindControl("lblNodeId");
				Literal ltrlHistVer = (Literal)e.Row.FindControl("ltrlHistVer");

				using (IRecordsReader dr = SqlHelper.ExecuteReader(GetSQLHistoryVersions(),SqlHelper.CreateParameter("@nodeId",int.Parse(lblNodeId.Text))))
				{
					while (dr.Read())
					{
						ltrlHistVer.Text += dr.GetString("text") + " <small>(Created: " + dr.GetDateTime("updateDate").ToShortDateString() + " " + dr.GetDateTime("updateDate").ToShortTimeString() + ") ";
						if (dr.GetBoolean("published"))
						{
							ltrlHistVer.Text += "(<span style='color: green;'>published</span>)";
						}

						if (dr.GetBoolean("newest"))
						{
							ltrlHistVer.Text += "(<span style='color: navy;'>newest</span>)";
						}

						ltrlHistVer.Text += "</small><br />";
					}
				}
			}
		}

		/// <summary>
		/// This function manage the gridview paging
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		protected void gvCurVer_PageIndexChanging(object sender,GridViewPageEventArgs e)
		{
			gvCurVer.PageIndex = e.NewPageIndex;
			SqlDSCurrentVersion.SelectCommand = GetSQLCurrentVersion();
		}

		/// <summary>
		/// This function convert dates to Short Date Time
		/// </summary>
		/// <param name="dDate"></param>
		/// <returns></returns>
		protected string convertToShortDateTime(string dDate)
		{
			string convertedDate = DateTime.Parse(dDate).ToShortDateString() + " " + DateTime.Parse(dDate).ToShortTimeString();

			return convertedDate;
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