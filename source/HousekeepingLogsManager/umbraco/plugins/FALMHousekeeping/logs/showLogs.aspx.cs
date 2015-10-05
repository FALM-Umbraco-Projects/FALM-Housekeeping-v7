using System;
using System.Data;
using System.Web.UI.WebControls;
using umbraco.BusinessLogic;
using umbraco.cms.businesslogic;
using umbraco.cms.businesslogic.language;
using umbraco.DataLayer;

namespace FALMHousekeepingLogsManager
{
	public partial class showLogs : System.Web.UI.Page
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
		/// Initialize DataView
		/// </summary>
		private DataView dgCache = new DataView();

		/// <summary>
		/// Initialize an SQL Read Log variable
		/// </summary>
		string sqlReadLog = string.Empty;

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
			PanelShowLogs.Text	= getDictionaryItem("logs_ShowLogs_PageTitle");

			// Page SubTitle
			if (getDictionaryItem("logs_ShowLogs_PageSubTitle") != string.Empty)
			{
				lblSubTitle.Text = "<p>" + getDictionaryItem("logs_ShowLogs_PageSubTitle") + "</p>";
			}

			// Set Filter Labels
			lblFilterByNodeId.Text = getDictionaryItem("logs_Label_FilterByNodeId");
			lblFilterByUser.Text = getDictionaryItem("logs_Label_FilterByUser");
			lblFilterByLogType.Text	= getDictionaryItem("logs_Label_FilterByLogType");
			lblFilterByDateRangeFrom.Text = getDictionaryItem("logs_Label_FilterByDateRangeFrom");
			lblFilterByDateRangeTo.Text	= getDictionaryItem("logs_Label_FilterByDateRangeTo");

			// Button Show Logs text
			btnShowLogs.Text = getDictionaryItem("logs_ShowLogs_Button_ShowLogs");

			if (!IsPostBack)
			{
				string[] strLogTypes = Enum.GetNames(typeof(LogTypes));

				Array.Sort(strLogTypes);

				ddlUsers.Items.Insert(0, new ListItem(getDictionaryItem("logs_DropDown_Any"), "any"));
				ddlUsers.SelectedIndex = 0;

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
		protected void btnShowLogs_Click(object sender,EventArgs e)
		{
			// Reset cache
			Cache.Remove("dgCache");

			// Reset viewstate sort order
			ViewState["SortOrder"] = null;

			// Reset GridView Page to the top
			gvLogTypesList.PageIndex = 0;

			// Assign default column sort order
			GetLogs("DateStamp desc");
		}

		/// <summary>
		/// Get Logs
		/// </summary>
		/// <param name="ColumnOrder"></param>
		protected void GetLogs(string ColumnOrder)
		{
			sqlReadLog = "SELECT umbracoLog.userId, umbracoUser.userName, umbracoUser.userLogin, umbracoLog.NodeId, umbracoNode.text AS nodeName, umbracoLog.DateStamp, umbracoLog.logHeader, umbracoLog.logComment ";
			sqlReadLog += "FROM umbracoLog INNER JOIN umbracoUser ON umbracoLog.userId = umbracoUser.id LEFT OUTER JOIN umbracoNode ON umbracoLog.NodeId = umbracoNode.id ";
			sqlReadLog += "WHERE (";

			if ((dtpckrDateFrom.Text.Equals(string.Empty)) && (dtpckrDateTo.Text.Equals(string.Empty)))
			{
				dtpckrDateFrom.DateTime = DateTime.Now.Subtract(new TimeSpan(iLastDays,0,0,0));
				dtpckrDateTo.DateTime = DateTime.Now.Subtract(new TimeSpan(0,0,0,0));

				sqlReadLog += "umbracoLog.DateStamp >= CONVERT(DATETIME, '" + dtpckrDateFrom.DateTime.ToString("yyyy.MM.dd") + " 00:00:00', 102) AND umbracoLog.DateStamp <= CONVERT(DATETIME, '" + dtpckrDateTo.DateTime.ToString("yyyy.MM.dd") + " 23:59:00', 102) ";
			}
			else if ((dtpckrDateFrom.Text.Equals(string.Empty)) && (!dtpckrDateTo.Text.Equals(string.Empty)))
			{
				sqlReadLog += "umbracoLog.DateStamp <= CONVERT(DATETIME, '" + dtpckrDateTo.DateTime.ToString("yyyy.MM.dd") + " 23:59:00', 102) ";
			}
			else if ((!dtpckrDateFrom.Text.Equals(string.Empty)) && (dtpckrDateTo.Text.Equals(string.Empty)))
			{
				sqlReadLog += "umbracoLog.DateStamp >= CONVERT(DATETIME, '" + dtpckrDateFrom.DateTime.ToString("yyyy.MM.dd") + " 00:00:00', 102) ";
			}
			else if (dtpckrDateFrom.DateTime <= dtpckrDateTo.DateTime)
			{
				sqlReadLog += "umbracoLog.DateStamp >= CONVERT(DATETIME, '" + dtpckrDateFrom.DateTime.ToString("yyyy.MM.dd") + " 00:00:00', 102) AND umbracoLog.DateStamp <= CONVERT(DATETIME, '" + dtpckrDateTo.DateTime.ToString("yyyy.MM.dd") + " 23:59:00', 102) ";
			}
			else
			{
				string strDateFrom = dtpckrDateFrom.Text;
				string strDateTo = dtpckrDateTo.Text;

				dtpckrDateFrom.DateTime = DateTime.Parse(strDateTo);
				dtpckrDateTo.DateTime = DateTime.Parse(strDateFrom);

				sqlReadLog += "umbracoLog.DateStamp >= CONVERT(DATETIME, '" + dtpckrDateFrom.DateTime.ToString("yyyy.MM.dd") + " 00:00:00', 102) AND umbracoLog.DateStamp <= CONVERT(DATETIME, '" + dtpckrDateTo.DateTime.ToString("yyyy.MM.dd") + " 23:59:00', 102) ";

				ltrlLogInfo.Text = getDictionaryItem("logs_StartDateGreaterThanEndDate");
				ltrlLogInfo.Visible = true;
			}

			if (!txtbNodeID.Text.Equals(string.Empty))
			{
				sqlReadLog += "AND umbracoLog.NodeId = " + int.Parse(txtbNodeID.Text) + " ";
			}

			if (ddlLogTypes.SelectedValue.ToString() != "any")
			{
				sqlReadLog += "AND logHeader LIKE '" + ddlLogTypes.SelectedValue.ToString() + "' ";
			}

			if (ddlUsers.SelectedValue.ToString() != "any")
			{
				sqlReadLog += "AND UserId = " + ddlUsers.SelectedValue;
			}

			sqlReadLog += ") ORDER BY umbracoLog.DateStamp DESC";

			//Set up Cache Object and determine if it exists
			dgCache = (DataView)Cache.Get("dgCache");

			//Assign ColumnOrder to ViewState
			ViewState["SortOrder"] = ColumnOrder;

			if (dgCache == null)
			{
				//Create DataTable
				DataTable DataReaderTable = new DataTable();
				DataColumn dc1 = new DataColumn("UserName",typeof(string));
				DataColumn dc2 = new DataColumn("NodeId",typeof(int));
				DataColumn dc3 = new DataColumn("NodeName",typeof(string));
				DataColumn dc4 = new DataColumn("DateStamp",typeof(DateTime));
				DataColumn dc5 = new DataColumn("logHeader",typeof(string));
				DataColumn dc6 = new DataColumn("logComment",typeof(string));

				DataReaderTable.Columns.Add(dc1);
				DataReaderTable.Columns.Add(dc2);
				DataReaderTable.Columns.Add(dc3);
				DataReaderTable.Columns.Add(dc4);
				DataReaderTable.Columns.Add(dc5);
				DataReaderTable.Columns.Add(dc6);

				//Loop through DataReader
				IRecordsReader irLogs = SqlHelper.ExecuteReader(sqlReadLog);

				while (irLogs.Read())
				{
					//Set up DataRow object
					DataRow dr = DataReaderTable.NewRow();

					dr[0] = irLogs.GetString("userName");
					dr[1] = irLogs.GetInt("NodeId");
					dr[2] = irLogs.GetString("NodeName");
					dr[3] = irLogs.GetDateTime("DateStamp");
					dr[4] = irLogs.GetString("logHeader");
					dr[5] = irLogs.GetString("logComment");

					//Add rows to existing DataTable
					DataReaderTable.Rows.Add(dr);
				}

				//Create DataView to support our column sorting
				DataView Source = DataReaderTable.DefaultView;

				//Assign column sort order for DataView
				Source.Sort = ColumnOrder;

				//Insert DataTable into Cache object
				Cache.Insert("dgCache",Source);

				//Bind DataGrid from DataView
				gvLogTypesList.DataSource = Source;
			}
			else
			{
				//Assign Cached DataView new sort order
				dgCache.Sort = ViewState["SortOrder"].ToString();

				//Bind DataGrid from Cached DataView
				gvLogTypesList.DataSource = dgCache;
			}

			gvLogTypesList.Style.Add("width","100%");
			gvLogTypesList.DataBind();
		}

		/// <summary>
		/// Sort Order
		/// </summary>
		/// <param name="strField"></param>
		/// <returns></returns>
		protected string SortOrder(string strField)
		{
			string strSortOrder = string.Empty;

			if (strField == ViewState["SortOrder"].ToString())
			{
				strSortOrder = strField.Replace("asc","desc");
			}
			else
			{
				strSortOrder = strField.Replace("desc","asc");
			}

			return strSortOrder;
		}

		/// <summary>
		/// Manage Paging
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		protected void gvLogTypesList_PageIndexChanging(object sender,GridViewPageEventArgs e)
		{
			gvLogTypesList.PageIndex = e.NewPageIndex;
			GetLogs(ViewState["SortOrder"].ToString());
		}

		/// <summary>
		/// Mange Sorting order
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		protected void gvLogTypesList_Sorting(object sender,GridViewSortEventArgs e)
		{
			gvLogTypesList.PageIndex = 0;
			GetLogs(SortOrder(e.SortExpression.ToString()));
		}

		/// <summary>
		/// Convert Date to Short Date Time String
		/// </summary>
		/// <param name="dDate"></param>
		/// <returns></returns>
		protected string convertToShortDateTime(string dDate)
		{
			string convertedDate = DateTime.Parse(dDate).ToShortDateString() + "<br />" + DateTime.Parse(dDate).ToLongTimeString();

			return convertedDate;
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