using FALM.Housekeeping.Models;
using FALM.Housekeeping.Helpers;
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
using Umbraco.Web.Models;

namespace FALM.Housekeeping.Controllers
{
    /// <summary>
    /// PluginController("FALMHousekeeping")
    /// HkCacheApiController
    /// </summary>
    [PluginController("FALMHousekeeping")]
    public class HkCacheApiController : UmbracoApiController
    {
        /// <summary>Cache directory</summary>
        protected static DirectoryInfo cacheDI = new DirectoryInfo(HttpContext.Current.Server.MapPath(GlobalSettings.Path + "/../App_Data/cache/"));

        /// <summary>
        /// Get Cache content summary
        /// </summary>
        /// <returns>HKCacheContentModel</returns>
        [HttpGet]
        public HKCacheContentModel GetCacheContent()
        {
            var cachePath = HttpContext.Current.Server.MapPath(GlobalSettings.Path + "/../App_Data/cache/");

            if (Directory.Exists(cachePath))
            {
                List<HKCacheModel> cacheContentList = new List<HKCacheModel>();

                var cacheObjects = cacheDI.GetDirectories();

                foreach (var dir in cacheObjects)
                {
                    HKCacheModel cacheModel = new HKCacheModel
                    {
                        Entry = dir.Name,
                        Type = "directory",
                        Dimension = HkFunctionsHelper.SizeSuffix(HkFunctionsHelper.DirSize(dir))
                    };
                    cacheContentList.Add(cacheModel);
                }

                HKCacheContentModel cacheContent = new HKCacheContentModel
                {
                    ListCacheContent = cacheContentList
                };

                return cacheContent;
            }

            return null;
        }

        /// <summary>
        /// Post Empty Cache directory
        /// </summary>
        /// <returns>true/false</returns>
        [HttpDelete]
        [HttpPost]
        public bool PostEmptyCacheDirectory()
        {
            try
            {
                // Delete cache folder recursively
                HkFunctionsHelper.DeleteFolderRecursive(cacheDI, cacheDI);

                return true;
            }
            catch (IOException ex)
            {
                return false;
            }
        }

        /// <summary>
        /// Create the Auto Clean Cache Service Page
        /// </summary>
        /// <returns>true/false</returns>
        [HttpGet]
        public bool GetCreateServicePage()
        {
            List<ContentTypeSort> listAllowedContentTypes = null;
            List<ITemplate> listAllowedTemplates = null;
            EntityContainer dtFalmContainer = null;
            IContentType dtFalmHKCacheDirectoryCleanup = null;
            Template falmTemplate = null;
            IContent falmServiceFolderNode = null;
            IContent falmHKCacheDirectoryCleanupNode = null;

            var contentTypeService = ApplicationContext.Current.Services.ContentTypeService;
            var fileService = ApplicationContext.Current.Services.FileService;
            var contentService = ApplicationContext.Current.Services.ContentService;

            try
            {
                // Check existing DOCUMENT TYPE CONTAINER "FALM"
                var dtFalmContainers = contentTypeService.GetContentTypeContainers("FALM", 1);

                if (dtFalmContainers.Count() == 0)
                {
                    var falmFolderCreationResult = contentTypeService.CreateContentTypeContainer(-1, "FALM");

                    dtFalmContainer = contentTypeService.GetContentTypeContainer(falmFolderCreationResult.Result.Entity.Id);

                    if (!falmFolderCreationResult.Success)
                    {
                        LogHelper.Error<Exception>("FALM Housekeeping - " + falmFolderCreationResult.Exception.Message, falmFolderCreationResult.Exception);
                    }
                    else
                    {
                        LogHelper.Info<string>("FALM Housekeeping - Document Type Container 'FALM' successfully created");
                    }
                }
                else
                {
                    dtFalmContainer = contentTypeService.GetContentTypeContainer(dtFalmContainers.FirstOrDefault().Id);
                    LogHelper.Info<string>("FALM Housekeeping - Document Type Container 'FALM' already exist");
                }

                // Check existing DOCUMENT TYPE "falmHKCacheCleanup"
                if (contentTypeService.GetContentType("falmHKCacheCleanup") == null)
                {
                    // Create Document Type "falmHKCacheCleanup"
                    var createNewDocumentType = new ContentType(dtFalmContainer.Id)
                    {
                        Alias = "falmHKCacheCleanup",
                        Name = "FALM Housekeeping - Cache Directory Cleanup",
                        AllowedAsRoot = false,
                        Icon = "icon-trash color-orange"
                    };
                    contentTypeService.Save(createNewDocumentType);

                    LogHelper.Info<string>("FALM Housekeeping - Document Type 'FALM Housekeeping - Cache Directory Cleanup' successfully created");
                }
                else
                {
                    LogHelper.Info<string>("FALM Housekeeping - Document Type 'FALM Housekeeping - Cache Directory Cleanup' already exist");
                }

                // Check existing TEMPLATE "falmHKCacheCleanup"
                falmTemplate = (Template)fileService.GetTemplate("falmHKCacheCleanup");

                if(falmTemplate == null)
                {
                    // Get Template "falmHKCacheCleanup" and Set the Content
                    falmTemplate = new Template("FALM Housekeeping - Cache Directory Cleanup", "falmHKCacheCleanup")
                    {                        
                        Content = "@inherits Umbraco.Web.Mvc.UmbracoTemplatePage" + System.Environment.NewLine + "@{ Layout = null; }" + System.Environment.NewLine + "@inherits Umbraco.Web.Mvc.UmbracoViewPage<FALM.Housekeeping.Models.HKCachePageModel>" + System.Environment.NewLine + "<html>" + System.Environment.NewLine + "<head>" + System.Environment.NewLine + "<title>FALM Housekeeping - Cache Directory Cleanup</title>" + System.Environment.NewLine + "</head>" + System.Environment.NewLine + "<body>" + System.Environment.NewLine + "<h1>FALM Housekeeping - Cache Directory Cleanup</h1>" + System.Environment.NewLine + "<div>The 'App_Data/cache' Directory has been emptied</div>" + System.Environment.NewLine + "</body>" + System.Environment.NewLine + "</html>"
                    };
                    fileService.SaveTemplate(falmTemplate);

                    LogHelper.Info<string>("FALM Housekeeping - Template 'FALM Housekeeping - Cache Directory Cleanup' successfully created");
                }
                else
                {
                    LogHelper.Info<string>("FALM Housekeeping - Template 'FALM Housekeeping - Cache Directory Cleanup' already exist");
                }

                listAllowedTemplates = new List<ITemplate>
                {
                    falmTemplate
                };

                // Set allowed and default template
                dtFalmHKCacheDirectoryCleanup = contentTypeService.GetContentType("falmHKCacheCleanup");
                dtFalmHKCacheDirectoryCleanup.AllowedTemplates = listAllowedTemplates;
                dtFalmHKCacheDirectoryCleanup.SetDefaultTemplate(listAllowedTemplates.FirstOrDefault());
                contentTypeService.Save(dtFalmHKCacheDirectoryCleanup);

                // Check existing DOCUMENT TYPE "falmServiceFolder"
                if (contentTypeService.GetContentType("falmServiceFolder") == null)
                {
                    listAllowedContentTypes = new List<ContentTypeSort>
                    {
                        new ContentTypeSort(dtFalmHKCacheDirectoryCleanup.Id, 0)
                    };

                    // Create Docuement Type "falmServiceFolder"
                    var createNewDocumentType = new ContentType(dtFalmContainer.Id)
                    {
                        Alias = "falmServiceFolder",
                        Name = "FALM - Service Folder",
                        AllowedAsRoot = true,
                        AllowedContentTypes = listAllowedContentTypes,
                        Icon = "icon-speed-gauge color-orange"
                    };
                    contentTypeService.Save(createNewDocumentType);

                    LogHelper.Info<string>("FALM Housekeeping - Document Type 'FALM - Service Folder' successfully created");
                }
                else
                {
                    LogHelper.Info<string>("FALM Housekeeping - Document Type 'FALM - Service Folder' already exist");
                }


                // CONTENT

                // Check existing "falmServiceFolder" node
                var listFalmServiceFolderNodes = contentService.GetContentOfContentType(contentTypeService.GetContentType("falmServiceFolder").Id);
                if ((listFalmServiceFolderNodes.Count() == 0) || (listFalmServiceFolderNodes.First(n => n.Name == "FALM - Service Folder") == null))
                {
                    // Create Service Folder node
                    falmServiceFolderNode = contentService.CreateContent("FALM - Service Folder", -1, "falmServiceFolder");
                    contentService.SaveAndPublishWithStatus(falmServiceFolderNode);
                    contentService.RebuildXmlStructures(falmServiceFolderNode.Id);

                    LogHelper.Info<string>("FALM Housekeeping - Content Node 'FALM - Service Folder' successfully created");
                }
                else
                {
                    falmServiceFolderNode = listFalmServiceFolderNodes.First(n => n.Name == "FALM - Service Folder");

                    LogHelper.Info<string>("FALM Housekeeping - Content Node 'FALM - Service Folder' already exist");
                }

                // Check existing "falmHKCacheCleanup" nodes
                var listHKCacheDirectoryCleanupNodes = contentService.GetContentOfContentType(contentTypeService.GetContentType("falmHKCacheCleanup").Id);
                if ((listHKCacheDirectoryCleanupNodes.Count() == 0) || (listHKCacheDirectoryCleanupNodes.First(n => n.Name == "Housekeeping - Cleanup Cache Directory") == null))
                {
                    // Create Cache Directory Cleanup node
                    falmHKCacheDirectoryCleanupNode = contentService.CreateContent("Housekeeping - Cleanup Cache Directory", falmServiceFolderNode.Id, "falmHKCacheCleanup");
                    falmHKCacheDirectoryCleanupNode.Template = dtFalmHKCacheDirectoryCleanup.DefaultTemplate;
                    contentService.SaveAndPublishWithStatus(falmHKCacheDirectoryCleanupNode);
                    contentService.RebuildXmlStructures(falmHKCacheDirectoryCleanupNode.Id);

                    LogHelper.Info<string>("FALM Housekeeping - Content Node 'Housekeeping - Cleanup Cache Directory' successfully created");
                }
                else
                {
                    LogHelper.Info<string>("FALM Housekeeping - Content Node 'Housekeeping - Cleanup Cache Directory' already exist");
                }

                // Re order FALM Service Node Children by Name
                var x = falmServiceFolderNode.Children().OrderBy(n => n.Name);
                int iSortOrder = 0;
                foreach (var y in x)
                {
                    y.SortOrder = iSortOrder;

                    if (y.Published)
                    {
                        contentService.SaveAndPublishWithStatus(y);
                        contentService.RebuildXmlStructures(y.Id);
                    }
                    else
                    {
                        contentService.Save(y);
                    }

                    iSortOrder++;
                }

                LogHelper.Info<string>("FALM Housekeeping - Content FALM Nodes successfully re-ordered");

                return true;
            }
            catch (Exception ex)
            {
                LogHelper.Error<Exception>("FALM Housekeeping - " + ex.Message, ex);

                return false;
            }
        }
    }

    /// <summary>
    /// falmHKCacheCleanup is the controller used for service purposes
    /// </summary>
    public class falmHKCacheCleanupController : RenderMvcController
    {
        /// <summary>
        /// This Action is a service page that auto empty Cache Directory on File System
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpGet]
        public override ActionResult Index(RenderModel model)
        {
            try
            {
                HKCachePageModel HKRecycleBinPageModel = new HKCachePageModel(model.Content, model.CurrentCulture);

                // Delete cache folder recursively
                DirectoryInfo cacheDI = new DirectoryInfo(HttpContext.Server.MapPath(GlobalSettings.Path + "/../App_Data/cache/"));
                HkFunctionsHelper.DeleteFolderRecursive(cacheDI, cacheDI);

                HKRecycleBinPageModel.IsCacheDirectoryCleaned = true;

                LogHelper.Info<string>("FALM Housekeeping - Cleanup Cache Directory successfully completed");

                return CurrentTemplate(HKRecycleBinPageModel);
            }
            catch (Exception ex)
            {
                LogHelper.Error<Exception>("FALM Housekeeping - " + ex.Message, ex);

                throw new Exception(ex.Message);
            }
        }
    }
}