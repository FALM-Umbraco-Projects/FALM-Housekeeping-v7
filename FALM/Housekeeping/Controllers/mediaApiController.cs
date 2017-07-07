// FALM
using FALM.Housekeeping.Helpers;
using FALM.Housekeeping.Models;
//SYSTEM
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Web.Mvc;
//UMBRACO
using Umbraco.Core;
using Umbraco.Core.Configuration;
using Umbraco.Core.Logging;
using Umbraco.Core.Persistence;
using Umbraco.Core.Services;
using Umbraco.Web.Mvc;
using Umbraco.Web.WebApi;

namespace FALMHousekeeping.Controllers
{
    /// <summary>
    /// PluginController("FALMHousekeeping")
    /// MediaApiController
    /// </summary>
    [PluginController("FALMHousekeeping")]
    public class MediaApiController : UmbracoApiController
    {
        protected IMediaService mediaService = ApplicationContext.Current.Services.MediaService;
        protected MediaModel mediaModel = new MediaModel();
        protected List<MediaWarningModel> listMediaWarnings = new List<MediaWarningModel>();
        protected List<MediaToDeleteModel> listMediaToDelete = new List<MediaToDeleteModel>();

        /// <summary>
        /// Show media to delete
        /// </summary>
        /// <returns>MediaModel</returns>
        [HttpGet]
        public MediaModel GetMediaToDelete(string userLocale)
        {
            listMediaWarnings = new List<MediaWarningModel>();
            listMediaToDelete = new List<MediaToDeleteModel>();

            var currentUserCultureInfo = CultureInfo.GetCultureInfo(userLocale);

            try
            {
                // Find all media to be deleted and relative warning messages
                string _filePath = System.Web.HttpContext.Current.Server.MapPath(umbraco.GlobalSettings.Path + "/../media/");
            
                // Check if the files are stored in the /media folder root with a unique ID prefixed to the filename
			    if (UmbracoConfig.For.UmbracoSettings().Content.UploadAllowDirectories)
			    {
                    var strSQLGetMedia   = "SELECT SUBSTRING(cmsPropertyData.dataNvarchar, 8, CHARINDEX('/', cmsPropertyData.dataNvarchar, 8) - 8) AS pId ";
                    strSQLGetMedia      += "FROM cmsPropertyData INNER JOIN cmsPropertyType ON cmsPropertyData.propertytypeid = cmsPropertyType.id ";
                    strSQLGetMedia      += "WHERE (dataNvarchar LIKE '%media/%') AND (cmsPropertyType.Alias = 'umbracoFile') ";
                    strSQLGetMedia      += "UNION ";
                    strSQLGetMedia      += "SELECT SUBSTRING(";
	                strSQLGetMedia      += "    SUBSTRING(";
		            strSQLGetMedia      += "        cmsPropertyData.dataNtext, ";
		            strSQLGetMedia      += "        CHARINDEX('/media/', cmsPropertyData.dataNtext) + 7, ";
		            strSQLGetMedia      += "        LEN(RTRIM(CAST(cmsPropertyData.dataNtext as NVARCHAR(4000))))";
	                strSQLGetMedia      += "    ), ";
	                strSQLGetMedia      += "    0, ";
	                strSQLGetMedia      += "    CHARINDEX('/', ";
		            strSQLGetMedia      += "        SUBSTRING(";
			        strSQLGetMedia      += "            cmsPropertyData.dataNtext, ";
			        strSQLGetMedia      += "            CHARINDEX('/media/', cmsPropertyData.dataNtext) + 7, ";
			        strSQLGetMedia      += "            LEN(RTRIM(CAST(cmsPropertyData.dataNtext as NVARCHAR(4000))))";
		            strSQLGetMedia      += "        )";
	                strSQLGetMedia      += "    )";
                    strSQLGetMedia      += ")";
                    strSQLGetMedia      += "FROM cmsPropertyData INNER JOIN cmsPropertyType ON cmsPropertyData.propertytypeid = cmsPropertyType.id ";
                    strSQLGetMedia      += "WHERE (dataNtext LIKE '%media/%') AND (cmsPropertyType.Alias = 'umbracoFile') ";
                    strSQLGetMedia      += "ORDER BY pId";
				
                    // Show orphan directories
                    using (var db = DbHelper.ResolveDatabase())
                    {
                        MediaWarningModel mediaWarningModel = new MediaWarningModel();
                        MediaToDeleteModel mediaToDeleteModel = new MediaToDeleteModel();

                        List<MediaPIdModel> allMediaPId = db.Fetch<MediaPIdModel>(strSQLGetMedia);
                        int pId;

                        
                            // Create an array with the list of media directories
				            DirectoryInfo dir = new DirectoryInfo(_filePath);
				            DirectoryInfo[] subDirs = dir.GetDirectories();
                            int iDirectoryName = 0;
                            List<int> mediaMatched = new List<int>();
                            List<string> mediaToSkip = new List<string>();

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

                            foreach (DirectoryInfo subDir in subDirs)
                            {
                                iDirectoryName = 0;

                                // Do check only if the folder have a number as a name (STANDARD FOLDER)
                                if (int.TryParse(subDir.Name, out iDirectoryName))
                                {
                                    bool mediaAlreadyAddedToDeleteList = false;

                                if (allMediaPId.Count > 0)
                                {

                                    foreach (var media in allMediaPId)
                                    {
                                        if (int.TryParse(media.pId, out pId))
                                        {
                                            if (!mediaToSkip.Contains(pId.ToString()))
                                            {
                                                if (iDirectoryName == pId)
                                                {
                                                    mediaToSkip.Add(subDir.Name);
                                                    break;
                                                }
                                                else if (iDirectoryName < pId)
                                                {
                                                    // Check if the folder is used by data type that not store image informations (like Image Cropper)
                                                    int mediaCount = 0;

                                                    mediaCount += GetCountFromCmsContentXml(db, subDir.Name);

                                                    mediaCount += GetCountFromCmsPropertyData(db, subDir.Name);

                                                    // The Media is deletable if it isn't used
                                                    if ((mediaCount == 0) && (!mediaAlreadyAddedToDeleteList))
                                                    {
                                                        // MEDIA FOUND IN FILE SYSTEM BUT NO CORRISPONDATION INTO DB OR BY DATATYPE UPLOAD
                                                        // ### DELETEBLE ###
                                                        mediaToDeleteModel = new MediaToDeleteModel();
                                                        mediaToDeleteModel.Entry = subDir.Name;
                                                        mediaToDeleteModel.Message = Services.TextService.Localize("FALM/MediaManager.Cleanup.Contains", currentUserCultureInfo) + " " + subDir.GetFileSystemInfos().Length + " " + Services.TextService.Localize("FALM/MediaManager.Cleanup.Items", currentUserCultureInfo);
                                                        listMediaToDelete.Add(mediaToDeleteModel);
                                                        mediaToSkip.Add(subDir.Name);
                                                        mediaAlreadyAddedToDeleteList = true;
                                                    }
                                                    else
                                                    {
                                                        if (!mediaAlreadyAddedToDeleteList)
                                                        {
                                                            mediaWarningModel = new MediaWarningModel();
                                                            mediaWarningModel.Entry = subDir.Name;
                                                            mediaWarningModel.Message = "###" + Services.TextService.Localize("FALM/MediaManager.Cleanup.FolderFoundInFileSystemAndUploadedTroughDatatype", currentUserCultureInfo);
                                                            listMediaWarnings.Add(mediaWarningModel);
                                                            mediaToSkip.Add(subDir.Name);
                                                            break;
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                        else
                                        {
                                            // MEDIA IGNORED - NO MATCH STANDARD FORMAT INTO DB
                                            mediaWarningModel = new MediaWarningModel();
                                            mediaWarningModel.Entry = pId.ToString();
                                            mediaWarningModel.Message = Services.TextService.Localize("FALM/MediaManager.Cleanup.DBNotMatchFormat", currentUserCultureInfo) + " '/media/&lt;propertyid&gt;/'";
                                            listMediaWarnings.Add(mediaWarningModel);
                                        }
                                    }

                                    }

                                    if (!mediaToSkip.Contains(subDir.Name))
                                    {
                                        // Check if the folder is used by data type that not store image informations (like Image Cropper)
                                        int mediaCount = 0;

                                        mediaCount += GetCountFromCmsContentXml(db, subDir.Name);

                                        mediaCount += GetCountFromCmsPropertyData(db, subDir.Name);

                                        // If the media is not used...it is deletable
                                        if (mediaCount == 0)
                                        {
                                            // MEDIA FOUND IN FILE SYSTEM BUT NO CORRISPONDATION INTO DB OR BY DATATYPE UPLOAD
                                            // ### DELETEBLE ###
                                            mediaToDeleteModel = new MediaToDeleteModel();
                                            mediaToDeleteModel.Entry = subDir.Name;
                                            mediaToDeleteModel.Message = Services.TextService.Localize("FALM/MediaManager.Cleanup.Contains", currentUserCultureInfo) + " " + subDir.GetFileSystemInfos().Length + " " + Services.TextService.Localize("FALM/MediaManager.Cleanup.Items", currentUserCultureInfo);
                                            listMediaToDelete.Add(mediaToDeleteModel);
                                        }
                                        else
                                        {
                                            // MEDIA FOUND IN FILE SYSTEM AND UPLOADED TROUGH DATATYPE
                                            mediaWarningModel = new MediaWarningModel();
                                            mediaWarningModel.Entry = subDir.Name;
                                            mediaWarningModel.Message = Services.TextService.Localize("FALM/MediaManager.Cleanup.FoundInFileSystemAndUploadedTroughDatatype", currentUserCultureInfo);
                                            listMediaWarnings.Add(mediaWarningModel);
                                        }
                                    }
                                }
                                else
                                {
                                    // MEDIA IGNORED - NON STANDARD FOLDER
                                    mediaWarningModel = new MediaWarningModel();
                                    mediaWarningModel.Entry = subDir.Name;
                                    mediaWarningModel.Message = Services.TextService.Localize("FALM/MediaManager.Cleanup.FolderNameNotNumber", currentUserCultureInfo);
                                    listMediaWarnings.Add(mediaWarningModel);
                                }
                            }
                        //}
                        //else
                        //{
                        //    return null;
                        //}
                    }
                }

                mediaModel.ListMediaToDelete = listMediaToDelete;
                mediaModel.ListMediaWarnings = listMediaWarnings;
            }
            catch (Exception ex)
            {
                LogHelper.Error<Exception>(ex.Message, ex);
                return mediaModel;
            }

            return mediaModel;
        }

        /// <summary>
        /// Delete media orphans
        /// </summary>
        /// <param name="mediaOrphansToDelete"></param>
        /// <returns>MediaModel</returns>
        [HttpPost]
        public MediaModel PostDeleteMediaOrphans(List<MediaToDeleteModel> mediaOrphansToDelete)
        {
            string _filePath = System.Web.HttpContext.Current.Server.MapPath(umbraco.GlobalSettings.Path + "/../media/");
			string _dirPathToDelete = string.Empty;

            listMediaWarnings = new List<MediaWarningModel>();
            listMediaToDelete = new List<MediaToDeleteModel>();

            MediaWarningModel mediaWarningModel = new MediaWarningModel();
            MediaToDeleteModel mediaDeletedModel = new MediaToDeleteModel();

            foreach (MediaToDeleteModel mediaOrphan in mediaOrphansToDelete)
            {
                _dirPathToDelete = _filePath + mediaOrphan.Entry + "\\";

                if (Directory.Exists(_dirPathToDelete))
                {
                    Directory.Delete(_dirPathToDelete, true);

                    // ### MEDIA DELETED ###
                    mediaDeletedModel = new MediaToDeleteModel();
                    mediaDeletedModel.Entry = mediaOrphan.Entry;
                    mediaDeletedModel.Message = Services.TextService.Localize("FALM/MediaManager.Cleanup.Deleted");
                    listMediaToDelete.Add(mediaDeletedModel);
                }
                else
                {
                    // MEDIA IGNORED - NON STANDARD FOLDER
                    mediaWarningModel = new MediaWarningModel();
                    mediaWarningModel.Entry = mediaOrphan.Entry;
                    mediaWarningModel.Message = Services.TextService.Localize("FALM/MediaManager.Cleanup.FolderNotFound");
                    listMediaWarnings.Add(mediaWarningModel);
                }
            }

            mediaModel.ListMediaToDelete = listMediaToDelete;
            mediaModel.ListMediaWarnings = listMediaWarnings;

            return mediaModel;
        }

        /// <summary>
        /// Check media forlder in cmsContentXml
        /// </summary>
        /// <param name="db"></param>
        /// <param name="subDirName"></param>
        /// <returns>int</returns>
        protected int GetCountFromCmsContentXml(Database db, string subDirName)
        {
            var strSQLCheckMedia = "SELECT COUNT(nodeId) As Count ";
            strSQLCheckMedia += "FROM cmsContentXml ";
            strSQLCheckMedia += "WHERE xml LIKE '%/media/" + subDirName + "/%' ";

            List<int> countMediaInCmsContentXml = db.Fetch<int>(strSQLCheckMedia);
            if (countMediaInCmsContentXml.Count > 0)
            {
                return countMediaInCmsContentXml[0];
            }

            return 0;
        }

        /// <summary>
        /// Check media forlder in cmsPropertyData
        /// </summary>
        /// <param name="db"></param>
        /// <param name="subDirName"></param>
        /// <returns>int</returns>
        protected int GetCountFromCmsPropertyData(Database db, string subDirName)
        {
            var strSQLCheckMedia = "SELECT COUNT(id) As Count ";
            strSQLCheckMedia += "FROM cmsPropertyData ";
            strSQLCheckMedia += "WHERE [dataNtext] LIKE '%/media/" + subDirName + "/%' ";
            strSQLCheckMedia += "AND [dataNvarchar] LIKE '%/media/" + subDirName + "/%' ";

            List<int> countMediaInCmsPropertyData = db.Fetch<int>(strSQLCheckMedia);
            if (countMediaInCmsPropertyData.Count > 0)
            {
                return countMediaInCmsPropertyData[0];
            }

            return 0;
        }
    }
}