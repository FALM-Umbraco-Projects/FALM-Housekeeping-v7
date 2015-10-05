using System;
using System.Data;
using System.Web.UI.WebControls;
using umbraco.BusinessLogic;
using umbraco.cms.businesslogic;
using umbraco.cms.businesslogic.language;
using umbraco.DataLayer;

namespace FALMHousekeepingLogsManager
{
	public partial class cleanupLogs : System.Web.UI.Page
	{
		/// <summary>
		/// Initialize an SQL Helper Interface
		/// </summary>
		protected static ISqlHelper SqlHelper
		{
			get { return umbraco.BusinessLogic.Application.SqlHelper; }
		}

		/// <summary>
		/// Initialize last days variable
		/// </summary>
		private int iLastDays = 19;

		/// <summary>
		/// Initialize an SQL Check log variable
		/// </summary>
		string sqlCheckLog = string.Empty;
		
		/// <summary>
		/// Initialize an SQL Delete log variable
		/// </summary>
		string sqlDeleteLog = string.Empty;

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
		protected void Page_Load(object sender, EventArgs e)
		{
			// Retrieve User Language
			GetUserLanguage();

			Server.ScriptTimeout = 100000;

			// Page Title
			PanelCleanupLogs.Text			= getDictionaryItem("logs_Cleanup_PageTitle");

			// Page SubTitle
			if (getDictionaryItem("logs_Cleanup_PageSubTitle") != string.Empty)
			{
				lblSubTitle.Text			= "<p>" + getDictionaryItem("logs_Cleanup_PageSubTitle") + "</p>";
			}

			// Set Filter Labels
			lblFilterByNodeId.Text			= getDictionaryItem("logs_Label_FilterByNodeId");
			lblFilterByUser.Text			= getDictionaryItem("logs_Label_FilterByUser");
			lblFilterByLogType.Text			= getDictionaryItem("logs_Label_FilterByLogType");
			lblFilterByDateRangeFrom.Text	= getDictionaryItem("logs_Label_FilterByDateRangeFrom");
			lblFilterByDateRangeTo.Text		= getDictionaryItem("logs_Label_FilterByDateRangeTo");

			// Button Confirm Deletion text
			btnDelete.Text					= getDictionaryItem("logs_Cleanup_Button_Delete");

			// Button Show Logs text
			btnShowLogs.Text = getDictionaryItem("logs_Cleanup_Button_ShowLogs");
			
			if (!IsPostBack)
			{
				string[] strLogTypes = Enum.GetNames(typeof(LogTypes));

				Array.Sort(strLogTypes);

				ddlLogTypes.DataSource = strLogTypes;
				ddlLogTypes.DataBind();

				ddlUsers.Items.Insert(0, new ListItem(getDictionaryItem("logs_DropDown_Any"), "any"));
				ddlUsers.SelectedIndex = 0;

				ddlLogTypes.Items.Insert(0, new ListItem(getDictionaryItem("logs_DropDown_Any"), "any"));
				ddlLogTypes.SelectedIndex = 0;
			}
		}

		/// <summary>
		/// Show Logs
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		protected void btnShowLogs_Click(object sender, EventArgs e)
		{
			// Check logs
			CheckLogs();
		}

		/// <summary>
		/// Delete Logs
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		protected void btnDelete_Click(object sender, EventArgs e)
		{
			sqlDeleteLog = "DELETE FROM umbracoLog WHERE (";

			if ((dtpckrDateFrom.Text.Equals(string.Empty)) && (dtpckrDateTo.Text.Equals(string.Empty)))
			{
				dtpckrDateFrom.DateTime = DateTime.Now.Subtract(new TimeSpan(iLastDays, 0, 0, 0));
				dtpckrDateTo.DateTime = DateTime.Now.Subtract(new TimeSpan(0, 0, 0, 0));

				sqlDeleteLog += "DateStamp >= CONVERT(DATETIME, '" + dtpckrDateFrom.DateTime.ToString("yyyy.MM.dd") + " 00:00:00', 102) AND DateStamp <= CONVERT(DATETIME, '" + dtpckrDateTo.DateTime.ToString("yyyy.MM.dd") + " 23:59:00', 102) ";
			}
			else if ((dtpckrDateFrom.Text.Equals(string.Empty)) && (!dtpckrDateTo.Text.Equals(string.Empty)))
			{
				sqlDeleteLog += "DateStamp <= CONVERT(DATETIME, '" + dtpckrDateTo.DateTime.ToString("yyyy.MM.dd") + " 23:59:00', 102) ";
			}
			else if ((!dtpckrDateFrom.Text.Equals(string.Empty)) && (dtpckrDateTo.Text.Equals(string.Empty)))
			{
				sqlDeleteLog += "DateStamp >= CONVERT(DATETIME, '" + dtpckrDateFrom.DateTime.ToString("yyyy.MM.dd") + " 00:00:00', 102) ";
			}
			else if (dtpckrDateFrom.DateTime <= dtpckrDateTo.DateTime)
			{
				sqlDeleteLog += "DateStamp >= CONVERT(DATETIME, '" + dtpckrDateFrom.DateTime.ToString("yyyy.MM.dd") + " 00:00:00', 102) AND DateStamp <= CONVERT(DATETIME, '" + dtpckrDateTo.DateTime.ToString("yyyy.MM.dd") + " 23:59:00', 102) ";
			}
			else
			{
				string strDateFrom = dtpckrDateFrom.Text;
				string strDateSince = dtpckrDateTo.Text;

				dtpckrDateFrom.DateTime = DateTime.Parse(strDateSince);
				dtpckrDateTo.DateTime = DateTime.Parse(strDateFrom);

				sqlDeleteLog += "DateStamp >= CONVERT(DATETIME, '" + dtpckrDateFrom.DateTime.ToString("yyyy.MM.dd") + " 00:00:00', 102) AND DateStamp <= CONVERT(DATETIME, '" + dtpckrDateTo.DateTime.ToString("yyyy.MM.dd") + " 23:59:00', 102) ";

				ltrlLogInfo.Text = getDictionaryItem("logs_StartDateGreaterThanEndDate");
				ltrlLogInfo.Visible = true;
			}

			if (!txtbNodeID.Text.Equals(string.Empty))
			{
				sqlDeleteLog += "AND NodeId = " + txtbNodeID.Text + " ";
			}

			if (ddlLogTypes.SelectedValue.ToString() != "any")
			{
				sqlDeleteLog += "AND logHeader LIKE '" + ddlLogTypes.SelectedValue.ToString() + "' ";
			}

			if (ddlUsers.SelectedValue.ToString() != "any")
			{
				sqlDeleteLog += "AND UserId = " + ddlUsers.SelectedValue + " ";
			}

			sqlDeleteLog += ")";

			// Loop through DataReader
			IRecordsReader irLogs = SqlHelper.ExecuteReader(sqlDeleteLog);

			ltrlLogTotal.Text = string.Empty;
			btnDelete.Visible = false;
			gvLogTypesList.Visible = false;
			ltrlLogInfo.Text = "<span style='color: #990000; font-weight: bold;'>" + getDictionaryItem("logs_LogsDeleted") + "</span>";
		}

		/// <summary>
		/// Check Logs
		/// </summary>
		protected void CheckLogs()
		{
			sqlCheckLog = "SELECT logHeader, COUNT(logHeader) AS logCount FROM umbracoLog WHERE (";

			if ((dtpckrDateFrom.Text.Equals(string.Empty)) && (dtpckrDateTo.Text.Equals(string.Empty)))
			{
				dtpckrDateFrom.DateTime = DateTime.Now.Subtract(new TimeSpan(iLastDays, 0, 0, 0));
				dtpckrDateTo.DateTime = DateTime.Now.Subtract(new TimeSpan(0, 0, 0, 0));

				sqlCheckLog += "DateStamp >= CONVERT(DATETIME, '" + dtpckrDateFrom.DateTime.ToString("yyyy.MM.dd") + " 00:00:00', 102) AND DateStamp <= CONVERT(DATETIME, '" + dtpckrDateTo.DateTime.ToString("yyyy.MM.dd") + " 23:59:00', 102) ";
			}
			else if ((dtpckrDateFrom.Text.Equals(string.Empty)) && (!dtpckrDateTo.Text.Equals(string.Empty)))
			{
				sqlCheckLog += "DateStamp <= CONVERT(DATETIME, '" + dtpckrDateTo.DateTime.ToString("yyyy.MM.dd") + " 23:59:00', 102) ";
			}
			else if ((!dtpckrDateFrom.Text.Equals(string.Empty)) && (dtpckrDateTo.Text.Equals(string.Empty)))
			{
				sqlCheckLog += "DateStamp >= CONVERT(DATETIME, '" + dtpckrDateFrom.DateTime.ToString("yyyy.MM.dd") + " 00:00:00', 102) ";
			}
			else if (dtpckrDateFrom.DateTime <= dtpckrDateTo.DateTime)
			{
				sqlCheckLog += "DateStamp >= CONVERT(DATETIME, '" + dtpckrDateFrom.DateTime.ToString("yyyy.MM.dd") + " 00:00:00', 102) AND DateStamp <= CONVERT(DATETIME, '" + dtpckrDateTo.DateTime.ToString("yyyy.MM.dd") + " 23:59:00', 102) ";
			}
			else
			{
				string strDateFrom = dtpckrDateFrom.Text;
				string strDateSince = dtpckrDateTo.Text;

				dtpckrDateFrom.DateTime = DateTime.Parse(strDateSince);
				dtpckrDateTo.DateTime = DateTime.Parse(strDateFrom);

				sqlCheckLog += "DateStamp >= CONVERT(DATETIME, '" + dtpckrDateFrom.DateTime.ToString("yyyy.MM.dd") + " 00:00:00', 102) AND DateStamp <= CONVERT(DATETIME, '" + dtpckrDateTo.DateTime.ToString("yyyy.MM.dd") + " 23:59:00', 102) ";

				ltrlLogInfo.Text = getDictionaryItem("logs_StartDateGreaterThanEndDate");
				ltrlLogInfo.Visible = true;
			}

			if (!txtbNodeID.Text.Equals(string.Empty))
			{
				sqlCheckLog += "AND NodeId = " + int.Parse(txtbNodeID.Text) + " ";
			}

			if (ddlLogTypes.SelectedValue.ToString() != "any")
			{
				sqlCheckLog += "AND logHeader LIKE '" + ddlLogTypes.SelectedValue.ToString() + "' ";
			}

			if (ddlUsers.SelectedValue.ToString() != "any")
			{
				sqlCheckLog += "AND UserId = " + ddlUsers.SelectedValue + " ";
			}

			sqlCheckLog += ") GROUP BY logHeader ";

			sqlCheckLog += "ORDER BY logHeader";

			// Create DataTable
			DataTable DataReaderTable = new DataTable();
			DataColumn dc1 = new DataColumn("Log Type", typeof(string));
			DataColumn dc2 = new DataColumn("Log Count", typeof(int));

			DataReaderTable.Columns.Add(dc1);
			DataReaderTable.Columns.Add(dc2);

			// Loop through DataReader
			IRecordsReader irLogs = SqlHelper.ExecuteReader(sqlCheckLog);

			while (irLogs.Read())
			{
				// Set up DataRow object
				DataRow dr = DataReaderTable.NewRow();

				dr[0] = irLogs.GetString("logHeader");
				dr[1] = irLogs.GetInt("logCount");

				// Add rows to existing DataTable
				DataReaderTable.Rows.Add(dr);
			}

			// Create DataView to support our column sorting
			DataView Source = DataReaderTable.DefaultView;

			gvLogTypesList.DataSource = Source;
			gvLogTypesList.DataBind();
			gvLogTypesList.Visible = true;
			
			//Loop through DataReader
			if (Source.Table.Rows.Count != 0)
			{
				ltrlLogInfo.Text = getDictionaryItem("logs_Cleanup_TableSummaryDescription") + "<br />";

				btnDelete.Visible = true;
			}
			else
			{
				btnDelete.Visible = false;
			}
		}

		/// <summary>
		/// Check for empty datasource
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		protected void gvLogTypesList_RowDataBound(object sender, GridViewRowEventArgs e)
		{
			// Set Text for no logs to show
			if (e.Row.RowType.Equals(DataControlRowType.EmptyDataRow))
			{
				Label lblNoLogsToShow = (Label)e.Row.FindControl("lblNoLogsToShow");
				lblNoLogsToShow.Text = getDictionaryItem("logs_Label_NoLogsToShow") + " '" + dtpckrDateFrom.DateTime.ToShortDateString() + "'";
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
