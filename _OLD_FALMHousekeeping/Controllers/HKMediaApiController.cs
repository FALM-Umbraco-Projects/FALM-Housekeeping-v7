using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Web;
using System.Web.Mvc;
using FALM.Housekeeping.Helpers;
using FALM.Housekeeping.Models;
using umbraco;
using Umbraco.Core;
using Umbraco.Core.Configuration;
using Umbraco.Core.Logging;
using Umbraco.Core.Persistence;
using Umbraco.Core.Services;
using Umbraco.Web.Mvc;
using Umbraco.Web.WebApi;

namespace FALM.Housekeeping.Controllers
{
    /// <summary>
    /// PluginController("FALMHousekeeping")
    /// HkMediaApiController
    /// </summary>
    [PluginController("FALMHousekeeping")]
    public class HkMediaApiController : UmbracoApiController
    {
        /// <summary></summary>
        protected IMediaService MediaService = ApplicationContext.Current.Services.MediaService;
        /// <summary></summary>
        protected HKMediaModel MediaModel = new HKMediaModel();
        /// <summary></summary>
        protected List<MediaWarningModel> ListMediaWarnings = new List<MediaWarningModel>();
        /// <summary></summary>
        protected List<MediaToDeleteModel> ListMediaToDelete = new List<MediaToDeleteModel>();

        /// <summary>
        /// Show media to delete
        /// </summary>
        /// <returns>MediaModel</returns>
        [HttpGet]
        public HKMediaModel GetMediaToDelete(string userLocale)
        {
            ListMediaWarnings = new List<MediaWarningModel>();
            ListMediaToDelete = new List<MediaToDeleteModel>();

            // Map of media folders to the count of information within them to minimize database calls
            var mediaCountMap = new Dictionary<string, int>();

            var currentUserCultureInfo = CultureInfo.GetCultureInfo(userLocale);

            try
            {
                // Find all media to be deleted and relative warning messages
                var filePath = HttpContext.Current.Server.MapPath(GlobalSettings.Path + "/../media/");

                // Check if the files are stored in the /media folder root with a unique ID prefixed to the filename
                if (UmbracoConfig.For.UmbracoSettings().Content.UploadAllowDirectories)
                {
                    var strSqlGetMedia = "SELECT SUBSTRING(cmsPropertyData.dataNvarchar, 8, CHARINDEX('/', cmsPropertyData.dataNvarchar, 8) - 8) AS pId ";
                    strSqlGetMedia += "FROM cmsPropertyData INNER JOIN cmsPropertyType ON cmsPropertyData.propertytypeid = cmsPropertyType.id ";
                    strSqlGetMedia += "WHERE (dataNvarchar LIKE '%media/%') AND (cmsPropertyType.Alias = 'umbracoFile') ";
                    strSqlGetMedia += "UNION ";
                    strSqlGetMedia += "SELECT SUBSTRING(";
                    strSqlGetMedia += "    SUBSTRING(";
                    strSqlGetMedia += "        cmsPropertyData.dataNtext, ";
                    strSqlGetMedia += "        CHARINDEX('/media/', cmsPropertyData.dataNtext) + 7, ";
                    strSqlGetMedia += "        LEN(RTRIM(CAST(cmsPropertyData.dataNtext as NVARCHAR(4000))))";
                    strSqlGetMedia += "    ), ";
                    strSqlGetMedia += "    0, ";
                    strSqlGetMedia += "    CHARINDEX('/', ";
                    strSqlGetMedia += "        SUBSTRING(";
                    strSqlGetMedia += "            cmsPropertyData.dataNtext, ";
                    strSqlGetMedia += "            CHARINDEX('/media/', cmsPropertyData.dataNtext) + 7, ";
                    strSqlGetMedia += "            LEN(RTRIM(CAST(cmsPropertyData.dataNtext as NVARCHAR(4000))))";
                    strSqlGetMedia += "        )";
                    strSqlGetMedia += "    )";
                    strSqlGetMedia += ")";
                    strSqlGetMedia += "FROM cmsPropertyData INNER JOIN cmsPropertyType ON cmsPropertyData.propertytypeid = cmsPropertyType.id ";
                    strSqlGetMedia += "WHERE (dataNtext LIKE '%media/%') AND (cmsPropertyType.Alias = 'umbracoFile') ";
                    strSqlGetMedia += "ORDER BY pId";

                    // Show orphan directories
                    using (var db = HkDbHelper.ResolveDatabase())
                    {
                        var allMediaPId = db.Fetch<MediaPIdModel>(strSqlGetMedia);

                        // Create an array with the list of media directories
                        var dir = new DirectoryInfo(filePath);
                        var subDirs = dir.GetDirectories();

                        var mediaToSkip = new List<string>();

                        // Sort Directories by name
                        Array.Sort(subDirs, delegate (DirectoryInfo d1, DirectoryInfo d2)
                        {
                            int n1, n2;

                            if (int.TryParse(d1.Name, out n1) && int.TryParse(d2.Name, out n2))
                            {
                                return n1 - n2;
                            }
                            return string.CompareOrdinal(d1.Name, d2.Name);
                        });

                        int mediaCount = -1;

                        foreach (var subDir in subDirs)
                        {
                            // Do check only if the folder have a number as a name (STANDARD FOLDER)
                            MediaWarningModel mediaWarningModel;

                            int iDirectoryName, pId;

                            if (int.TryParse(subDir.Name, out iDirectoryName))
                            {
                                var mediaAlreadyAddedToDeleteList = false;

                                MediaToDeleteModel mediaToDeleteModel;
                                if (allMediaPId.Count > 0)
                                {

                                    foreach (var media in allMediaPId)
                                    {
                                        if (int.TryParse(media.PId, out pId))
                                        {
                                            if (!mediaToSkip.Contains(pId.ToString()))
                                            {
                                                if (iDirectoryName == pId)
                                                {
                                                    mediaToSkip.Add(subDir.Name);
                                                    break;
                                                }
                                                if (iDirectoryName < pId)
                                                {
                                                    // Check if the folder is used by data type that not store image informations (like Image Cropper)
                                                    //int mediaCount = 0;

                                                    //mediaCount += GetCountFromCmsContentXml(db, subDir.Name);

                                                    //mediaCount += GetCountFromCmsPropertyData(db, subDir.Name);

                                                    if (!mediaCountMap.ContainsKey(subDir.Name))
                                                    {
                                                        mediaCountMap[subDir.Name] = GetCountFromCmsContentXml(db, subDir.Name) + GetCountFromCmsPropertyData(db, subDir.Name);
                                                    }

                                                    mediaCount = mediaCountMap[subDir.Name];

                                                    // The Media is deletable if it isn't used
                                                    if ((mediaCount == 0) && (!mediaAlreadyAddedToDeleteList))
                                                    {
                                                        // MEDIA FOUND IN FILE SYSTEM BUT NO CORRISPONDATION INTO DB OR BY DATATYPE UPLOAD
                                                        // ### DELETEBLE ###
                                                        mediaToDeleteModel = new MediaToDeleteModel
                                                        {
                                                            Entry = subDir.Name,
                                                            Message = Services.TextService.Localize("FALM/MediaManager.Cleanup.Contains", currentUserCultureInfo) + " " + subDir.GetFileSystemInfos().Length + " " + Services.TextService.Localize("FALM/MediaManager.Cleanup.Items", currentUserCultureInfo)
                                                        };
                                                        ListMediaToDelete.Add(mediaToDeleteModel);
                                                        mediaToSkip.Add(subDir.Name);
                                                        mediaAlreadyAddedToDeleteList = true;
                                                    }
                                                    else
                                                    {
                                                        if (!mediaAlreadyAddedToDeleteList)
                                                        {
                                                            mediaWarningModel = new MediaWarningModel
                                                            {
                                                                Entry = subDir.Name,
                                                                Message = "###" + Services.TextService.Localize("FALM/MediaManager.Cleanup.FolderFoundInFileSystemAndUploadedTroughDatatype", currentUserCultureInfo)
                                                            };
                                                            ListMediaWarnings.Add(mediaWarningModel);
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
                                            mediaWarningModel = new MediaWarningModel
                                            {
                                                Entry = pId.ToString(),
                                                Message = Services.TextService.Localize("FALM/MediaManager.Cleanup.DBNotMatchFormat", currentUserCultureInfo) + " '/media/&lt;propertyid&gt;/'"
                                            };
                                            ListMediaWarnings.Add(mediaWarningModel);
                                        }
                                    }

                                }

                                if (!mediaToSkip.Contains(subDir.Name))
                                {
                                    // Check if the folder is used by data type that not store image informations (like Image Cropper)
                                    //var mediaCount = 0;

                                    //mediaCount += GetCountFromCmsContentXml(db, subDir.Name);

                                    //mediaCount += GetCountFromCmsPropertyData(db, subDir.Name);

                                    if (!mediaCountMap.ContainsKey(subDir.Name))
                                    {
                                        mediaCountMap[subDir.Name] = GetCountFromCmsContentXml(db, subDir.Name) + GetCountFromCmsPropertyData(db, subDir.Name);
                                    }

                                    mediaCount = mediaCountMap[subDir.Name];

                                    // If the media is not used...it is deletable
                                    if (mediaCount == 0)
                                    {
                                        // MEDIA FOUND IN FILE SYSTEM BUT NO CORRISPONDATION INTO DB OR BY DATATYPE UPLOAD
                                        // ### DELETEBLE ###
                                        mediaToDeleteModel = new MediaToDeleteModel
                                        {
                                            Entry = subDir.Name,
                                            Message = Services.TextService.Localize("FALM/MediaManager.Cleanup.Contains", currentUserCultureInfo) + " " + subDir.GetFileSystemInfos().Length + " " + Services.TextService.Localize("FALM/MediaManager.Cleanup.Items", currentUserCultureInfo)
                                        };
                                        ListMediaToDelete.Add(mediaToDeleteModel);
                                    }
                                    else
                                    {
                                        // MEDIA FOUND IN FILE SYSTEM AND UPLOADED TROUGH DATATYPE
                                        mediaWarningModel = new MediaWarningModel
                                        {
                                            Entry = subDir.Name,
                                            Message = Services.TextService.Localize("FALM/MediaManager.Cleanup.FoundInFileSystemAndUploadedTroughDatatype", currentUserCultureInfo)
                                        };
                                        ListMediaWarnings.Add(mediaWarningModel);
                                    }
                                }
                            }
                            else
                            {
                                // MEDIA IGNORED - NON STANDARD FOLDER
                                mediaWarningModel = new MediaWarningModel
                                {
                                    Entry = subDir.Name,
                                    Message = Services.TextService.Localize("FALM/MediaManager.Cleanup.FolderNameNotNumber", currentUserCultureInfo)
                                };
                                ListMediaWarnings.Add(mediaWarningModel);
                            }
                        }
                        //}
                        //else
                        //{
                        //    return null;
                        //}
                    }
                }

                MediaModel.ListMediaToDelete = ListMediaToDelete;
                MediaModel.ListMediaWarnings = ListMediaWarnings;
            }
            catch (Exception ex)
            {
                LogHelper.Error<Exception>(ex.Message, ex);
                return MediaModel;
            }

            return MediaModel;
        }

        /// <summary>
        /// Delete media orphans
        /// </summary>
        /// <param name="mediaOrphansToDelete"></param>
        /// <returns>MediaModel</returns>
        [HttpPost]
        public HKMediaModel PostDeleteMediaOrphans(List<MediaToDeleteModel> mediaOrphansToDelete)
        {
            string filePath = HttpContext.Current.Server.MapPath(GlobalSettings.Path + "/../media/");

            ListMediaWarnings = new List<MediaWarningModel>();
            ListMediaToDelete = new List<MediaToDeleteModel>();

            foreach (MediaToDeleteModel mediaOrphan in mediaOrphansToDelete)
            {
                var dirPathToDelete = filePath + mediaOrphan.Entry + "\\";

                if (Directory.Exists(dirPathToDelete))
                {
                    Directory.Delete(dirPathToDelete, true);

                    // ### MEDIA DELETED ###
                    var mediaDeletedModel = new MediaToDeleteModel
                    {
                        Entry = mediaOrphan.Entry,
                        Message = Services.TextService.Localize("FALM/MediaManager.Cleanup.Deleted")
                    };
                    ListMediaToDelete.Add(mediaDeletedModel);
                }
                else
                {
                    // MEDIA IGNORED - NON STANDARD FOLDER
                    var mediaWarningModel = new MediaWarningModel
                    {
                        Entry = mediaOrphan.Entry,
                        Message = Services.TextService.Localize("FALM/MediaManager.Cleanup.FolderNotFound")
                    };
                    ListMediaWarnings.Add(mediaWarningModel);
                }
            }

            MediaModel.ListMediaToDelete = ListMediaToDelete;
            MediaModel.ListMediaWarnings = ListMediaWarnings;

            return MediaModel;
        }

        /// <summary>
        /// Check media forlder in cmsContentXml
        /// </summary>
        /// <param name="db"></param>
        /// <param name="subDirName"></param>
        /// <returns>int</returns>
        protected int GetCountFromCmsContentXml(Database db, string subDirName)
        {
            var strSqlCheckMedia = "SELECT COUNT(nodeId) As Count ";
            strSqlCheckMedia += "FROM cmsContentXml ";
            strSqlCheckMedia += "WHERE xml LIKE '%/media/" + subDirName + "/%' ";

            List<int> countMediaInCmsContentXml = db.Fetch<int>(strSqlCheckMedia);
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
            var strSqlCheckMedia = "SELECT COUNT(id) As Count ";
            strSqlCheckMedia += "FROM cmsPropertyData ";
            strSqlCheckMedia += "WHERE [dataNtext] LIKE '%/media/" + subDirName + "/%' ";
            strSqlCheckMedia += "AND [dataNvarchar] LIKE '%/media/" + subDirName + "/%' ";

            List<int> countMediaInCmsPropertyData = db.Fetch<int>(strSqlCheckMedia);
            if (countMediaInCmsPropertyData.Count > 0)
            {
                return countMediaInCmsPropertyData[0];
            }

            return 0;
        }
    }
}