using System;
using System.IO;
using umbraco.BusinessLogic;
using umbraco.cms.businesslogic;
using umbraco.cms.businesslogic.language;
using umbraco.DataLayer;

namespace FALMHousekeepingMediaManager
{
	public partial class cleanupMediaFS : System.Web.UI.Page
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
		/// Get Current Logged User
		/// </summary>
		protected User userCurrent = umbraco.BusinessLogic.User.GetCurrent();

		/// <summary>
		/// Backoffice User Language Id
		/// </summary>
		protected int iUserLanguageId = 0;

		/// <summary>
		/// Page_Load
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		protected void Page_Load(object sender, EventArgs e)
		{
			// Retrieve User Language
			GetUserLanguage();

			// Page Title
			PanelCleanupMediaFS.Text = getDictionaryItem("media_PageTitle");

			// Page SubTitle
			if (getDictionaryItem("media_PageSubTitle") != string.Empty)
			{
				lblSubTitle.Text = "<p>" + getDictionaryItem("media_PageSubTitle") + "</p>";
			}

			// Button Show Orphans text
			btnCheckOrphan.Text = getDictionaryItem("media_Button_ShowOrphans");

			// Button Confirm Deletion text
			btnDeleteOrphan.Text = getDictionaryItem("media_Button_Delete");
		}

		/// <summary>
		/// Function to check media orphans
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		protected void btnCheckOrphan_Click(object sender, EventArgs e)
		{
			Server.ScriptTimeout = 100000;

			ltrlDeletable.Text = string.Empty;
			ltrlWarning.Text = string.Empty;

			string strWarnings = string.Empty;
			string strMediaDeletable = "<tr style='background-color: rgb(247, 247, 222);'><td style='width: 0px; white-space: nowrap;'></td></tr>";

			string strSQLGetMedia;

            string _filePath = System.Web.HttpContext.Current.Server.MapPath(umbraco.GlobalSettings.Path + "/../media/");

			// Check if the files are stored in the /media folder root with a unique ID prefixed to the filename
			if (umbraco.UmbracoSettings.UploadAllowDirectories)
			{
				// Create an array with the list of media directories
				DirectoryInfo dir = new DirectoryInfo(_filePath);
				DirectoryInfo[] subDirs = dir.GetDirectories();

				// Sort Directories by name
				Array.Sort<DirectoryInfo>(subDirs, new Comparison<DirectoryInfo>(delegate(DirectoryInfo d1, DirectoryInfo d2)
				{
					int n1, n2;

					if (int.TryParse(d1.Name, out n1) && int.TryParse(d2.Name, out n2))
					{
						return n1 - n2;
					}
					else
					{
						return string.Compare(d1.Name, d2.Name);
					}
				}));

				strSQLGetMedia = "SELECT DISTINCT CONVERT(INT, SUBSTRING(cmsPropertyData.dataNvarchar, 8, CHARINDEX('/', cmsPropertyData.dataNvarchar, 8) - 8)) AS pId, cmsPropertyData.dataNvarchar ";
				strSQLGetMedia += "FROM cmsPropertyData INNER JOIN cmsPropertyType ON cmsPropertyData.propertytypeid = cmsPropertyType.id ";
				strSQLGetMedia += "WHERE     (cmsPropertyType.dataTypeId = - 90) ";
				strSQLGetMedia += "      AND (cmsPropertyData.dataNvarchar LIKE '/media%') ";
				strSQLGetMedia += "      AND (ISNUMERIC(SUBSTRING(cmsPropertyData.dataNvarchar, 8, CHARINDEX('/', cmsPropertyData.dataNvarchar, 8) - 8)) = 1) ";
				strSQLGetMedia += "ORDER BY pId";
				
				int iDirectoryName = 0;
				bool bEOF = false;
				bool bThereAreOrphans = false;

				// Show orphan directories
				using (IRecordsReader dr = SqlHelper.ExecuteReader(strSQLGetMedia))
				{
					if (!dr.Read())
					{
						bEOF = true;
					}

					foreach (DirectoryInfo subDir in subDirs)
					{
						// Do check only if the folder have a number as a name (STANDARD FOLDER)
						if (int.TryParse(subDir.Name, out iDirectoryName))
						{
							while (!bEOF && iDirectoryName > dr.GetInt("pId"))
							{
                                // 
								strWarnings += "<tr style='background-color: rgb(255, 255, 255);'><td style='width: 0px; white-space: nowrap;'>/media/" + dr.GetInt("pId") + "/</td><td style='width: 0px; white-space: nowrap;'>" + getDictionaryItem("media_MediaNotInFileSystem") + "</td></tr>";

								if (!dr.Read())
								{
									bEOF = true;
								}
							}

							if (bEOF || iDirectoryName < dr.GetInt("pId"))
							{
                                // Check if the folder is used by data type that not store image informations (like Image Cropper)
                                string strSQLCheckMedia;
                                int mediaCount = 0;

                                strSQLCheckMedia = "SELECT COUNT(nodeId) As Count ";
                                strSQLCheckMedia += "FROM cmsContentXml ";
                                strSQLCheckMedia += "WHERE xml LIKE '%/media/" + subDir.Name + "/%' ";
                                using (IRecordsReader drcheckmediaContentXML = SqlHelper.ExecuteReader(strSQLCheckMedia))
                                {
                                    if (drcheckmediaContentXML.Read())
                                    {
                                        mediaCount += drcheckmediaContentXML.GetInt("Count");
                                    }
                                }

                                strSQLCheckMedia = "SELECT COUNT(id) As Count ";
                                strSQLCheckMedia += "FROM cmsPropertyData ";
                                strSQLCheckMedia += "WHERE[dataNtext] LIKE '%/media/" + subDir.Name + "/%' ";
                                using (IRecordsReader drcheckmediaPropertyData = SqlHelper.ExecuteReader(strSQLCheckMedia))
                                {
                                    if (drcheckmediaPropertyData.Read())
                                    {
                                        mediaCount += drcheckmediaPropertyData.GetInt("Count");
                                    }
                                }

                                // If the media is not used...it is deletable
                                if (mediaCount == 0)
                                {
                                    // MEDIA DELETABLE
                                    strMediaDeletable += "<tr style='background-color: rgb(247, 247, 222);'><td style='width: 0px; white-space: nowrap;'>" + getDictionaryItem("media_Folder") + " '/media/" + subDir.Name + "/' " + getDictionaryItem("media_Contains") + " " + subDir.GetFileSystemInfos().Length + " " + getDictionaryItem("media_Items") + "</td></tr>";
                                    bThereAreOrphans = true;
                                }
                                else
                                {
                                    // else MEDIA IGNORED - STANDARD FOLDER BUT SAVED FROM DATA TYPE THAT NOT SAVE INFO INTO DB
                                    strWarnings += "<tr style='background-color: rgb(255, 255, 255);'><td style='width: 0px; white-space: nowrap;'>/media/" + subDir.Name + "/</td><td style='width: 0px; white-space: nowrap;'>" + getDictionaryItem("media_FolderNotTracked") + "</td></tr>";
                                }
                            }
                            else
							{
								if (!dr.Read())
								{
									bEOF = true;
								}
							}
						}
						else
						{
                            // MEDIA IGNORED - NON STANDARD FOLDER
							strWarnings += "<tr style='background-color: rgb(255, 255, 255);'><td style='width: 0px; white-space: nowrap;'>/media/" + subDir.Name + "/</td><td style='width: 0px; white-space: nowrap;'>" + getDictionaryItem("media_FolderNameNotNumber") + "</td></tr>";
						}
					}
				}

				// Show all non standard folder existing into DB
				strSQLGetMedia = "SELECT DISTINCT cmsPropertyData.dataNvarchar ";
				strSQLGetMedia += "FROM cmsPropertyData INNER JOIN cmsPropertyType ON cmsPropertyData.propertytypeid = cmsPropertyType.id ";
				strSQLGetMedia += "WHERE     (cmsPropertyType.dataTypeId = - 90) ";
				strSQLGetMedia += "      AND (cmsPropertyData.dataNvarchar LIKE '/media%') ";
				strSQLGetMedia += "      AND (ISNUMERIC(SUBSTRING(cmsPropertyData.dataNvarchar, 8, CHARINDEX('/', cmsPropertyData.dataNvarchar, 8) - 8)) = 0) ";
				strSQLGetMedia += "      OR  (cmsPropertyType.dataTypeId = - 90) ";
				strSQLGetMedia += "      AND (cmsPropertyData.dataNvarchar <> '') ";
				strSQLGetMedia += "      AND (cmsPropertyData.dataNvarchar NOT LIKE '/media%') ";
				strSQLGetMedia += "ORDER BY cmsPropertyData.dataNvarchar";

				using (IRecordsReader drNSM = SqlHelper.ExecuteReader(strSQLGetMedia))
				{
					if (drNSM.Read())
					{
						while (drNSM.Read())
						{
							strWarnings += "<tr style='background-color: rgb(255, 255, 255);'><td style='width: 0px; white-space: nowrap;'>" + drNSM.GetString("dataNvarchar") + "</td><td style='width: 0px; white-space: nowrap;'>" + getDictionaryItem("media_DBNotMatchFormat") + " '/media/&lt;propertyid&gt;/'</td></tr>";
						}
					}
				}

				// TABLE WITH WARNINGS - Show Warnings
				if (strWarnings != "")
				{
					string strMediaTableWarnings = string.Empty;

					strMediaTableWarnings = "<p><strong>" + getDictionaryItem("media_IgnoredEntries") + ":</strong></p>";
					strMediaTableWarnings += "<table style='color: Black; background-color: White; border: 1px solid rgb(222, 223, 222); width: 100%; border-collapse: collapse;' border='1' cellpadding='4' cellspacing='0'";
					strMediaTableWarnings += "<tbody>";
					strMediaTableWarnings += "<tr style='color: White; background-color: rgb(107, 105, 107); font-weight: bold; white-space: nowrap;' align='center'>";
					strMediaTableWarnings += "<th scope='col' style='width: 0px; white-space: nowrap;'>" + getDictionaryItem("media_Entry") + "</th>";
					strMediaTableWarnings += "<th scope='col' style='width: 0px; white-space: nowrap;'>" + getDictionaryItem("media_WarningMessages") + "</th>";
					strMediaTableWarnings += "</tr>";
					strMediaTableWarnings += strWarnings;
					strMediaTableWarnings += "</tbody>";
					strMediaTableWarnings += "</table>";

					ltrlWarning.Text += strMediaTableWarnings;
				}

				// TABLE DELETABLES FOLDERS - Show Deletable Folders
				if (bThereAreOrphans)
				{
					string strMediaTableDeletable = string.Empty;

					strMediaTableDeletable = "<p><strong>" + getDictionaryItem("media_FoldersToDelete") + ":</strong></p>";
					strMediaTableDeletable += "<table style='color: Black; background-color: White; border: 1px solid rgb(222, 223, 222); border-collapse: collapse;' border='1' cellpadding='4' cellspacing='0'";
					strMediaTableDeletable += "<tbody>";
					strMediaTableDeletable += "<tr style='color: White; background-color: rgb(107, 105, 107); font-weight: bold; white-space: nowrap;' align='center'>";
					strMediaTableDeletable += "<th scope='col' style='width: 0px; white-space: nowrap;'>" + getDictionaryItem("media_Folder") + "</th>";
					strMediaTableDeletable += "</tr>";
					strMediaTableDeletable += strMediaDeletable;
					strMediaTableDeletable += "</tbody>";
					strMediaTableDeletable += "</table>";

					ltrlDeletable.Text += strMediaTableDeletable;

					btnDeleteOrphan.Visible = true;
				}
				else
				{
					ltrlDeletable.Text = "<p><strong>" + getDictionaryItem("media_NoFoldersToDelete") + "</strong></p>";
				}
			}
			else
			{
				//
			}
		}

		/// <summary>
		/// Function to delete media orphans
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		protected void btnDeleteOrphan_Click(object sender, EventArgs e)
		{
			Server.ScriptTimeout = 100000;
			Response.Buffer = false;

			ltrlDeletable.Text = "";
			ltrlWarning.Text = "";

			bool bThereAreOrphans = false;

			string strSQLGetMedia;
			string strWarnings = string.Empty;
			string strMediaDeletable = string.Empty;

			string _filePath = System.Web.HttpContext.Current.Server.MapPath(umbraco.GlobalSettings.Path + "/../media/");
			string _dirPathToDelete = string.Empty;

			if (umbraco.UmbracoSettings.UploadAllowDirectories)
			{
				// Create an array with the list of media directories
				DirectoryInfo dir = new DirectoryInfo(_filePath);
				DirectoryInfo[] subDirs = dir.GetDirectories();

				// Sort Directories by name
				Array.Sort<DirectoryInfo>(subDirs, new Comparison<DirectoryInfo>(delegate(DirectoryInfo d1, DirectoryInfo d2)
				{
					int n1, n2;

					if (int.TryParse(d1.Name, out n1) && int.TryParse(d2.Name, out n2))
					{
						return n1 - n2;
					}
					else
					{
						return string.Compare(d1.Name, d2.Name);
					}
				}));

				strSQLGetMedia = "SELECT DISTINCT CONVERT(INT, SUBSTRING(cmsPropertyData.dataNvarchar, 8, CHARINDEX('/', cmsPropertyData.dataNvarchar, 8) - 8)) AS pId, cmsPropertyData.dataNvarchar ";
				strSQLGetMedia += "FROM cmsPropertyData INNER JOIN cmsPropertyType ON cmsPropertyData.propertytypeid = cmsPropertyType.id ";
				strSQLGetMedia += "WHERE     (cmsPropertyType.dataTypeId = - 90) ";
				strSQLGetMedia += "      AND (cmsPropertyData.dataNvarchar LIKE '/media%') ";
				strSQLGetMedia += "      AND (ISNUMERIC(SUBSTRING(cmsPropertyData.dataNvarchar, 8, CHARINDEX('/', cmsPropertyData.dataNvarchar, 8) - 8)) = 1) ";
				strSQLGetMedia += "ORDER BY pId";

				int iDirectoryName = 0;
				bool bEOF = false;

				// Delete orphan directories
				using (IRecordsReader dr = SqlHelper.ExecuteReader(strSQLGetMedia))
				{
					if (!dr.Read())
					{
						bEOF = true;
					}

					foreach (DirectoryInfo subDir in subDirs)
					{
						// Do check only if the folder have a number as a name
						if (int.TryParse(subDir.Name, out iDirectoryName))
						{
							while (!bEOF && iDirectoryName > dr.GetInt("pId"))
							{
								if (!dr.Read())
								{
									bEOF = true;
								}
							}

							if (bEOF || iDirectoryName < dr.GetInt("pId"))
							{
                                // Check if the folder is used by data type that not store image informations (like Image Cropper)
                                string strSQLCheckMedia;
                                int mediaCount = 0;

                                strSQLCheckMedia = "SELECT COUNT(nodeId) As Count ";
                                strSQLCheckMedia += "FROM cmsContentXml ";
                                strSQLCheckMedia += "WHERE xml LIKE '%/media/" + subDir.Name + "/%' ";
                                using (IRecordsReader drcheckmediaContentXML = SqlHelper.ExecuteReader(strSQLCheckMedia))
                                {
                                    if (drcheckmediaContentXML.Read())
                                    {
                                        mediaCount += drcheckmediaContentXML.GetInt("Count");
                                    }
                                }

                                strSQLCheckMedia = "SELECT COUNT(id) As Count ";
                                strSQLCheckMedia += "FROM cmsPropertyData ";
                                strSQLCheckMedia += "WHERE[dataNtext] LIKE '%/media/" + subDir.Name + "/%' ";
                                using (IRecordsReader drcheckmediaPropertyData = SqlHelper.ExecuteReader(strSQLCheckMedia))
                                {
                                    if (drcheckmediaPropertyData.Read())
                                    {
                                        mediaCount += drcheckmediaPropertyData.GetInt("Count");
                                    }
                                }

                                // If the media is not used...it is deletable
                                if (mediaCount == 0)
                                {
                                    _dirPathToDelete = _filePath + subDir.Name + "\\";

                                    if (Directory.Exists(_dirPathToDelete))
                                    {
                                        Directory.Delete(_dirPathToDelete, true);

                                        strMediaDeletable += "<tr style='background-color: rgb(247, 247, 222);'><td style='width: 0px; white-space: nowrap;'>/media/" + subDir.Name + "/</td></tr>";

                                        bThereAreOrphans = true;
                                    }
                                    else
                                    {
                                        strWarnings += "<tr style='background-color: rgb(255, 255, 255);'><td style='width: 0px; white-space: nowrap;'>" + getDictionaryItem("media_Folder") + "/media/" + subDir.Name + "/ " + getDictionaryItem("media_FolderNotFound") + " </td></tr>";
                                    }
                                }
							}
							else
							{
								if (!dr.Read())
								{
									bEOF = true;
								}
							}
						}
					}
				}

				if (strWarnings != "")
				{
					string strMediaTableWarnings = string.Empty;

					strMediaTableWarnings = "<p><strong>" + getDictionaryItem("media_Ignored") + "</strong></p>";
					strMediaTableWarnings += "<table style='color: Black; background-color: White; border: 1px solid rgb(222, 223, 222); width: 100%; border-collapse: collapse;' border='1' cellpadding='4' cellspacing='0'";
					strMediaTableWarnings += "<tbody>";
					strMediaTableWarnings += "<tr style='color: White; background-color: rgb(107, 105, 107); font-weight: bold; white-space: nowrap;' align='center'>";
					strMediaTableWarnings += "<th scope='col' style='width: 0px; white-space: nowrap;'>" + getDictionaryItem("media_Entry") + "</th>";
					strMediaTableWarnings += "<th scope='col' style='width: 0px; white-space: nowrap;'>" + getDictionaryItem("media_WarningMessages") + "</th>";
					strMediaTableWarnings += "</tr>";
					strMediaTableWarnings += strWarnings;
					strMediaTableWarnings += "</tbody>";
					strMediaTableWarnings += "</table>";

					ltrlWarning.Text += strMediaTableWarnings;
				}

				if (bThereAreOrphans)
				{
					string strMediaTableDeletable = string.Empty;

					strMediaTableDeletable = "<p><strong>" + getDictionaryItem("media_FoldersDeleted") + ":</strong></p>";
					strMediaTableDeletable += "<table style='color: Black; background-color: White; border: 1px solid rgb(222, 223, 222); border-collapse: collapse;' border='1' cellpadding='4' cellspacing='0'";
					strMediaTableDeletable += "<tbody>";
					strMediaTableDeletable += "<tr style='color: White; background-color: rgb(107, 105, 107); font-weight: bold; white-space: nowrap;' align='center'>";
					strMediaTableDeletable += "<th scope='col' style='width: 0px; white-space: nowrap;'>" + getDictionaryItem("media_Folders") + "</th>";
					strMediaTableDeletable += "</tr>";
					strMediaTableDeletable += strMediaDeletable;
					strMediaTableDeletable += "</tbody>";
					strMediaTableDeletable += "</table>";

					ltrlDeletable.Text += strMediaTableDeletable;
					
				}
				else
				{
					ltrlDeletable.Text = "<p><strong>" + getDictionaryItem("media_NoFoldersToDelete") + "</strong></p>";
				}
			}
			else
			{

			}

			btnDeleteOrphan.Visible = false;
		}

        /// <summary>
		/// Function to check media orphans with image cropper
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		protected void btnCheckOrphanWithImageCropper_Click(object sender, EventArgs e)
        {
            Server.ScriptTimeout = 100000;

            ltrlDeletable.Text = string.Empty;
            ltrlWarning.Text = string.Empty;

            string strWarnings = string.Empty;
            string strMediaDeletable = string.Empty;

            string strSQLGetMedia;

            string _filePath = System.Web.HttpContext.Current.Server.MapPath(umbraco.GlobalSettings.Path + "/../media/");

            // Check if the files are stored in the /media folder root with a unique ID prefixed to the filename
            if (umbraco.UmbracoSettings.UploadAllowDirectories)
            {
                // Create an array with the list of media directories
                DirectoryInfo dir = new DirectoryInfo(_filePath);
                DirectoryInfo[] subDirs = dir.GetDirectories();

                // Sort Directories by name
                Array.Sort<DirectoryInfo>(subDirs, new Comparison<DirectoryInfo>(delegate (DirectoryInfo d1, DirectoryInfo d2)
                {
                    int n1, n2;

                    if (int.TryParse(d1.Name, out n1) && int.TryParse(d2.Name, out n2))
                    {
                        return n1 - n2;
                    }
                    else
                    {
                        return string.Compare(d1.Name, d2.Name);
                    }
                }));

                strSQLGetMedia = "SELECT DISTINCT CONVERT(INT, SUBSTRING(cmsPropertyData.dataNvarchar, 8, CHARINDEX('/', cmsPropertyData.dataNvarchar, 8) - 8)) AS pId, cmsPropertyData.dataNvarchar ";
                strSQLGetMedia += "FROM cmsPropertyData INNER JOIN cmsPropertyType ON cmsPropertyData.propertytypeid = cmsPropertyType.id ";
                strSQLGetMedia += "WHERE     (cmsPropertyType.dataTypeId = - 90) ";
                strSQLGetMedia += "      AND (cmsPropertyData.dataNvarchar LIKE '/media%') ";
                strSQLGetMedia += "      AND (ISNUMERIC(SUBSTRING(cmsPropertyData.dataNvarchar, 8, CHARINDEX('/', cmsPropertyData.dataNvarchar, 8) - 8)) = 1) ";
                strSQLGetMedia += "ORDER BY pId";

                int iDirectoryName = 0;
                bool bEOF = false;
                bool bThereAreOrphans = false;

                // Show orphan directories
                using (IRecordsReader dr = SqlHelper.ExecuteReader(strSQLGetMedia))
                {
                    if (!dr.Read())
                    {
                        bEOF = true;
                    }

                    foreach (DirectoryInfo subDir in subDirs)
                    {
                        // Do check only if the folder have a number as a name
                        if (int.TryParse(subDir.Name, out iDirectoryName))
                        {
                            while (!bEOF && iDirectoryName > dr.GetInt("pId"))
                            {
                                strWarnings += "<tr style='background-color: rgb(255, 255, 255);'><td style='width: 0px; white-space: nowrap;'>/media/" + dr.GetInt("pId") + "/</td><td style='width: 0px; white-space: nowrap;'>" + getDictionaryItem("media_MediaNotInFileSystem") + "</td></tr>";

                                if (!dr.Read())
                                {
                                    bEOF = true;
                                }
                            }

                            if (bEOF || iDirectoryName < dr.GetInt("pId"))
                            {
                                strMediaDeletable += "<tr style='background-color: rgb(247, 247, 222);'><td style='width: 0px; white-space: nowrap;'>" + getDictionaryItem("media_Folder") + " '/media/" + subDir.Name + "/' " + getDictionaryItem("media_Contains") + " " + subDir.GetFileSystemInfos().Length + " " + getDictionaryItem("media_Items") + "</td></tr>";
                                bThereAreOrphans = true;
                            }
                            else
                            {
                                if (!dr.Read())
                                {
                                    bEOF = true;
                                }
                            }
                        }
                        else
                        {
                            strWarnings += "<tr style='background-color: rgb(255, 255, 255);'><td style='width: 0px; white-space: nowrap;'>/media/" + subDir.Name + "/</td><td style='width: 0px; white-space: nowrap;'>" + getDictionaryItem("media_FolderNameNotNumber") + "</td></tr>";
                        }
                    }
                }

                // Show all non standard folder existing into DB
                strSQLGetMedia = "SELECT DISTINCT cmsPropertyData.dataNvarchar ";
                strSQLGetMedia += "FROM cmsPropertyData INNER JOIN cmsPropertyType ON cmsPropertyData.propertytypeid = cmsPropertyType.id ";
                strSQLGetMedia += "WHERE     (cmsPropertyType.dataTypeId = - 90) ";
                strSQLGetMedia += "      AND (cmsPropertyData.dataNvarchar LIKE '/media%') ";
                strSQLGetMedia += "      AND (ISNUMERIC(SUBSTRING(cmsPropertyData.dataNvarchar, 8, CHARINDEX('/', cmsPropertyData.dataNvarchar, 8) - 8)) = 0) ";
                strSQLGetMedia += "      OR  (cmsPropertyType.dataTypeId = - 90) ";
                strSQLGetMedia += "      AND (cmsPropertyData.dataNvarchar <> '') ";
                strSQLGetMedia += "      AND (cmsPropertyData.dataNvarchar NOT LIKE '/media%') ";
                strSQLGetMedia += "ORDER BY cmsPropertyData.dataNvarchar";

                using (IRecordsReader drNSM = SqlHelper.ExecuteReader(strSQLGetMedia))
                {
                    if (drNSM.Read())
                    {
                        while (drNSM.Read())
                        {
                            strWarnings += "<tr style='background-color: rgb(255, 255, 255);'><td style='width: 0px; white-space: nowrap;'>" + drNSM.GetString("dataNvarchar") + "</td><td style='width: 0px; white-space: nowrap;'>" + getDictionaryItem("media_DBNotMatchFormat") + " '/media/&lt;propertyid&gt;/'</td></tr>";
                        }
                    }
                }

                // Show Warnings
                if (strWarnings != "")
                {
                    string strMediaTableWarnings = string.Empty;

                    strMediaTableWarnings = "<p><strong>" + getDictionaryItem("media_IgnoredEntries") + ":</strong></p>";
                    strMediaTableWarnings += "<table style='color: Black; background-color: White; border: 1px solid rgb(222, 223, 222); width: 100%; border-collapse: collapse;' border='1' cellpadding='4' cellspacing='0'";
                    strMediaTableWarnings += "<tbody>";
                    strMediaTableWarnings += "<tr style='color: White; background-color: rgb(107, 105, 107); font-weight: bold; white-space: nowrap;' align='center'>";
                    strMediaTableWarnings += "<th scope='col' style='width: 0px; white-space: nowrap;'>" + getDictionaryItem("media_Entry") + "</th>";
                    strMediaTableWarnings += "<th scope='col' style='width: 0px; white-space: nowrap;'>" + getDictionaryItem("media_WarningMessages") + "</th>";
                    strMediaTableWarnings += "</tr>";
                    strMediaTableWarnings += strWarnings;
                    strMediaTableWarnings += "</tbody>";
                    strMediaTableWarnings += "</table>";

                    ltrlWarning.Text += strMediaTableWarnings;
                }

                // Show Deletable Folders
                if (bThereAreOrphans)
                {
                    string strMediaTableDeletable = string.Empty;

                    strMediaTableDeletable = "<p><strong>" + getDictionaryItem("media_FoldersToDelete") + ":</strong></p>";
                    strMediaTableDeletable += "<table style='color: Black; background-color: White; border: 1px solid rgb(222, 223, 222); border-collapse: collapse;' border='1' cellpadding='4' cellspacing='0'";
                    strMediaTableDeletable += "<tbody>";
                    strMediaTableDeletable += "<tr style='color: White; background-color: rgb(107, 105, 107); font-weight: bold; white-space: nowrap;' align='center'>";
                    strMediaTableDeletable += "<th scope='col' style='width: 0px; white-space: nowrap;'>" + getDictionaryItem("media_Folder") + "</th>";
                    strMediaTableDeletable += "</tr>";
                    strMediaTableDeletable += strMediaDeletable;
                    strMediaTableDeletable += "</tbody>";
                    strMediaTableDeletable += "</table>";

                    ltrlDeletable.Text += strMediaTableDeletable;

                    btnDeleteOrphan.Visible = true;
                }
                else
                {
                    ltrlDeletable.Text = "<p><strong>" + getDictionaryItem("media_NoFoldersToDelete") + "</strong></p>";
                }
            }
            else
            {
                //
            }
        }

        /// <summary>
        /// Function to delete media orphans with image cropper
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void btnDeleteOrphanWithImageCropper_Click(object sender, EventArgs e)
        {
            Server.ScriptTimeout = 100000;
            Response.Buffer = false;

            ltrlDeletable.Text = "";
            ltrlWarning.Text = "";

            bool bThereAreOrphans = false;

            string strSQLGetMedia;
            string strWarnings = string.Empty;
            string strMediaDeletable = string.Empty;

            string _filePath = System.Web.HttpContext.Current.Server.MapPath(umbraco.GlobalSettings.Path + "/../media/");
            string _dirPathToDelete = string.Empty;

            if (umbraco.UmbracoSettings.UploadAllowDirectories)
            {
                // Create an array with the list of media directories
                DirectoryInfo dir = new DirectoryInfo(_filePath);
                DirectoryInfo[] subDirs = dir.GetDirectories();

                // Sort Directories by name
                Array.Sort<DirectoryInfo>(subDirs, new Comparison<DirectoryInfo>(delegate (DirectoryInfo d1, DirectoryInfo d2)
                {
                    int n1, n2;

                    if (int.TryParse(d1.Name, out n1) && int.TryParse(d2.Name, out n2))
                    {
                        return n1 - n2;
                    }
                    else
                    {
                        return string.Compare(d1.Name, d2.Name);
                    }
                }));

                strSQLGetMedia = "SELECT DISTINCT CONVERT(INT, SUBSTRING(cmsPropertyData.dataNvarchar, 8, CHARINDEX('/', cmsPropertyData.dataNvarchar, 8) - 8)) AS pId, cmsPropertyData.dataNvarchar ";
                strSQLGetMedia += "FROM cmsPropertyData INNER JOIN cmsPropertyType ON cmsPropertyData.propertytypeid = cmsPropertyType.id ";
                strSQLGetMedia += "WHERE     (cmsPropertyType.dataTypeId = - 90) ";
                strSQLGetMedia += "      AND (cmsPropertyData.dataNvarchar LIKE '/media%') ";
                strSQLGetMedia += "      AND (ISNUMERIC(SUBSTRING(cmsPropertyData.dataNvarchar, 8, CHARINDEX('/', cmsPropertyData.dataNvarchar, 8) - 8)) = 1) ";
                strSQLGetMedia += "ORDER BY pId";

                int iDirectoryName = 0;
                bool bEOF = false;

                // Delete orphan directories
                using (IRecordsReader dr = SqlHelper.ExecuteReader(strSQLGetMedia))
                {
                    if (!dr.Read())
                    {
                        bEOF = true;
                    }

                    foreach (DirectoryInfo subDir in subDirs)
                    {
                        // Do check only if the folder have a number as a name
                        if (int.TryParse(subDir.Name, out iDirectoryName))
                        {
                            while (!bEOF && iDirectoryName > dr.GetInt("pId"))
                            {
                                if (!dr.Read())
                                {
                                    bEOF = true;
                                }
                            }

                            if (bEOF || iDirectoryName < dr.GetInt("pId"))
                            {
                                _dirPathToDelete = _filePath + subDir.Name + "\\";

                                if (Directory.Exists(_dirPathToDelete))
                                {
                                    Directory.Delete(_dirPathToDelete, true);

                                    strMediaDeletable += "<tr style='background-color: rgb(247, 247, 222);'><td style='width: 0px; white-space: nowrap;'>/media/" + subDir.Name + "/</td></tr>";

                                    bThereAreOrphans = true;
                                }
                                else
                                {
                                    strWarnings += "<tr style='background-color: rgb(255, 255, 255);'><td style='width: 0px; white-space: nowrap;'>" + getDictionaryItem("media_Folder") + "/media/" + subDir.Name + "/ " + getDictionaryItem("media_FolderNotFound") + " </td></tr>";
                                }
                            }
                            else
                            {
                                if (!dr.Read())
                                {
                                    bEOF = true;
                                }
                            }
                        }
                    }
                }

                if (strWarnings != "")
                {
                    string strMediaTableWarnings = string.Empty;

                    strMediaTableWarnings = "<p><strong>" + getDictionaryItem("media_Ignored") + "</strong></p>";
                    strMediaTableWarnings += "<table style='color: Black; background-color: White; border: 1px solid rgb(222, 223, 222); width: 100%; border-collapse: collapse;' border='1' cellpadding='4' cellspacing='0'";
                    strMediaTableWarnings += "<tbody>";
                    strMediaTableWarnings += "<tr style='color: White; background-color: rgb(107, 105, 107); font-weight: bold; white-space: nowrap;' align='center'>";
                    strMediaTableWarnings += "<th scope='col' style='width: 0px; white-space: nowrap;'>" + getDictionaryItem("media_Entry") + "</th>";
                    strMediaTableWarnings += "<th scope='col' style='width: 0px; white-space: nowrap;'>" + getDictionaryItem("media_WarningMessages") + "</th>";
                    strMediaTableWarnings += "</tr>";
                    strMediaTableWarnings += strWarnings;
                    strMediaTableWarnings += "</tbody>";
                    strMediaTableWarnings += "</table>";

                    ltrlWarning.Text += strMediaTableWarnings;
                }

                if (bThereAreOrphans)
                {
                    string strMediaTableDeletable = string.Empty;

                    strMediaTableDeletable = "<p><strong>" + getDictionaryItem("media_FoldersDeleted") + ":</strong></p>";
                    strMediaTableDeletable += "<table style='color: Black; background-color: White; border: 1px solid rgb(222, 223, 222); border-collapse: collapse;' border='1' cellpadding='4' cellspacing='0'";
                    strMediaTableDeletable += "<tbody>";
                    strMediaTableDeletable += "<tr style='color: White; background-color: rgb(107, 105, 107); font-weight: bold; white-space: nowrap;' align='center'>";
                    strMediaTableDeletable += "<th scope='col' style='width: 0px; white-space: nowrap;'>" + getDictionaryItem("media_Folders") + "</th>";
                    strMediaTableDeletable += "</tr>";
                    strMediaTableDeletable += strMediaDeletable;
                    strMediaTableDeletable += "</tbody>";
                    strMediaTableDeletable += "</table>";

                    ltrlDeletable.Text += strMediaTableDeletable;

                }
                else
                {
                    ltrlDeletable.Text = "<p><strong>" + getDictionaryItem("media_NoOrphansDeleted") + "</strong></p>";
                }
            }
            else
            {

            }

            btnDeleteOrphan.Visible = false;
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
