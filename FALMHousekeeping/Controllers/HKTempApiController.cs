using FALM.Housekeeping.Helpers;
using FALM.Housekeeping.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using umbraco;
using Umbraco.Core;
using Umbraco.Core.Logging;
using Umbraco.Core.Models;
using Umbraco.Web.Mvc;
using Umbraco.Web.WebApi;

namespace FALM.Housekeeping.Controllers
{
    /// <summary>
    /// PluginController("FALMHousekeeping")
    /// HkTempApiController
    /// </summary>
    [PluginController("FALMHousekeeping")]
    public class HkTempApiController : UmbracoApiController
    {
        /// <summary>Cache directory</summary>
        protected static DirectoryInfo tempDI = new DirectoryInfo(HttpContext.Current.Server.MapPath(GlobalSettings.Path + "/../App_Data/TEMP/"));

        /// <summary>
        /// Get Cache and TEMP content summary
        /// </summary>
        /// <returns>HKCacheTempContentModel</returns>
        [HttpGet]
        public List<HKTempModel> GetTempContent()
        {
            var tempPath = HttpContext.Current.Server.MapPath(GlobalSettings.Path + "/../App_Data/TEMP/");

            if (Directory.Exists(tempPath))
            {
                List<HKTempModel> tempContentList = new List<HKTempModel>();

                var tempDirObjects = tempDI.GetDirectories();
                foreach (var dir in tempDirObjects)
                {
                    HKTempModel tempModel = new HKTempModel
                    {
                        Entry = dir.Name,
                        Type = "directory",
                        Dimension = HkFunctionsHelper.SizeSuffix(HkFunctionsHelper.DirSize(dir))
                    };
                    tempContentList.Add(tempModel);
                }

                var tempFilesObjects = tempDI.GetFiles();
                foreach (var file in tempFilesObjects)
                {
                    HKTempModel tempModel = new HKTempModel
                    {
                        Entry = file.Name,
                        Type = "file",
                        Dimension = HkFunctionsHelper.SizeSuffix(file.Length)
                    };
                    tempContentList.Add(tempModel);
                }

                return tempContentList;
            }

            return null;
        }

        /// <summary>
        /// Post Empty TEMP directory
        /// </summary>
        /// <returns>true/false</returns>
        [HttpDelete]
        [HttpPost]
        public bool PostEmptySelectedTempDirectories(List<HKTempModel> selectedTempItemsToDelete)
        {
            try
            {
                foreach (var tempItem in selectedTempItemsToDelete)
                {
                    if (tempItem.Selected)
                    {
                        if (tempItem.Type == "directory")
                        {
                            DirectoryInfo selectedDirectory = new DirectoryInfo(HttpContext.Current.Server.MapPath(GlobalSettings.Path + "/../App_Data/TEMP/" + tempItem.Entry + "/"));
                            // Delete cache folder recursively
                            HkFunctionsHelper.DeleteFolderRecursive(tempDI, selectedDirectory);
                        }
                        else
                        {
                            FileInfo selectedFile = new FileInfo(HttpContext.Current.Server.MapPath(GlobalSettings.Path + "/../App_Data/TEMP/" + tempItem.Entry));
                            HkFunctionsHelper.DeleteFile(tempDI, selectedFile);
                        }
                    }
                }

                return true;
            }
            catch (IOException ex)
            {
                return false;
            }
        }
    }
}