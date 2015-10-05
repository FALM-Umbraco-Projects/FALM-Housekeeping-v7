using System;
using umbraco.BusinessLogic;
using umbraco.cms.businesslogic;
using umbraco.cms.businesslogic.language;
using umbraco.cms.businesslogic.web;
using umbraco.DataLayer;
using System.Data.SqlClient;
using umbraco.DataLayer.SqlHelpers.SqlServer;

namespace FALMHousekeepingVersionsManager
{
	public partial class cleanupVersionsByCount : System.Web.UI.Page
	{
		/// <summary>
		/// Initialize an SQL Helper Interface
		/// </summary>
		protected static ISqlHelper SqlHelper
		{
			get
			{
				return umbraco.BusinessLogic.Application.SqlHelper;
			}
		}

		/// <summary>
		/// Retrieve all umbraco's unique id nodes
		/// </summary>
		private int[] nodeIds = Document.getAllUniqueNodeIdsFromObjectType(new Guid("C66BA18E-EAF3-4CFF-8A22-41B16D66A972"));

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

			// Page Title
			PanelCleanupVersionsByCount.Text = getDictionaryItem("versions_CleanupByCount_PageTitle");

			// Page SubTitle
			if (getDictionaryItem("versions_CleanupByCount_PageSubTitle") != string.Empty)
			{
				lblSubTitle.Text = "<p>" + getDictionaryItem("versions_CleanupByCount_PageSubTitle") + "</p>";
			}

			// Page Warning
			if (getDictionaryItem("versions_CleanupByCount_PageWarning") != string.Empty)
			{
				lblWarning.Text = "<p><strong><em>" + getDictionaryItem("versions_CleanupByCount_PageWarning") + "</em></strong></p>";
			}

			// Cleanup Info Message
			ltrlInfoMessage.Text = "<p><strong><em>" + getDictionaryItem("versions_Common_InfoMessage") + "</em></strong></p>";

			// Set Labels
			lblNumberOfVersionsToKeep.Text = getDictionaryItem("versions_CleanupByCount_NumberOfVersionsToKeep");

			// Button Show Logs text
			btnClearVersions.Text = getDictionaryItem("versions_CleanupByCount_Button_ClearVersions");

			if (!IsPostBack)
			{
				if (txtNVer.Text == "")
				{
					txtNVer.Text = "0";
				}
			}
		}

		/// <summary>
		/// Function to clear Versions by Count
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		protected void btnClearVersions_Click(object sender, EventArgs e)
		{
			Server.ScriptTimeout = 100000;

			string strSQLGetVersions = string.Empty;

			if (txtNVer.Text == "")
			{
				txtNVer.Text = "0";
			}

			strSQLGetVersions = @"CREATE TABLE #tmp
									(
										nodeId int,
										published bit,
										documentUser int,
										versionId uniqueidentifier,
										text nvarchar(255),
										releaseDate datetime,
										expireDate datetime,
										updateDate datetime,
										templateId int,
										newest bit
									);

									INSERT INTO #tmp(nodeId, published, documentUser, versionId, text, releaseDate, expireDate, updateDate, templateId, newest)
									SELECT nodeId, published, documentUser, versionId, text, releaseDate, expireDate, updateDate, templateId, newest FROM cmsDocument WHERE versionID NOT IN (SELECT D.versionId FROM cmsDocument D WHERE D.versionId IN (SELECT versionId FROM (SELECT CV.versionId, published, newest, RANK() OVER(ORDER BY CV.versionDate DESC) RowNum FROM cmsContentVersion CV JOIN cmsDocument DD ON CV.versionId = DD.versionId WHERE DD.nodeId = D.nodeId) AS tmp WHERE tmp.RowNum <= @versionsToKeep OR tmp.published = 1 OR tmp.newest = 1));
									
									DELETE FROM cmsPreviewXml WHERE VersionId IN (SELECT #tmp.VersionId FROM #tmp WHERE #tmp.published = 0 AND #tmp.newest = 0);
									
									DELETE FROM cmsContentVersion WHERE VersionId IN (SELECT #tmp.VersionId FROM #tmp WHERE #tmp.published = 0 AND #tmp.newest = 0);
									
									DELETE FROM cmsPropertyData WHERE VersionId IN (SELECT #tmp.VersionId FROM #tmp WHERE #tmp.published = 0 AND #tmp.newest = 0);
									
									DELETE FROM cmsDocument WHERE VersionId IN (SELECT #tmp.VersionId FROM #tmp WHERE #tmp.published = 0 AND #tmp.newest = 0);
									
									DROP TABLE #tmp;";

			// cleanup versions v1 - This version go in timeout (after 30 seconds) with large amount of history versions.
			//int iVersionsCount = SqlHelper.ExecuteNonQuery(strSQLGetVersions, SqlHelper.CreateParameter("@versionsToKeep", txtNVer.Text));

			// cleanup versions v2 - This version don't go in timeout
			var sqlConnection = new SqlConnection(SqlHelper.ConnectionString);
			sqlConnection.Open();
			var command = new SqlCommand(strSQLGetVersions, sqlConnection);
			command.CommandTimeout = 100000;
			command.Parameters.AddWithValue("@versionsToKeep", txtNVer.Text);
			int iVersionsCount = command.ExecuteNonQuery();
			command.Dispose();
			sqlConnection.Close();
			sqlConnection.Dispose();

			// Print result
			ltrlVersions.Text = getDictionaryItem("versions_Label_NumberOfVersionsDeleted") + " " + iVersionsCount;
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
