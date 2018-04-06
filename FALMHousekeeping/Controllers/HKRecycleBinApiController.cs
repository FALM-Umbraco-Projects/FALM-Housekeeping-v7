using FALM.Housekeeping.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Umbraco.Core;
using Umbraco.Core.Logging;
using Umbraco.Core.Models;
using Umbraco.Core.Models.EntityBase;
using Umbraco.Core.Services;
using Umbraco.Web.Models;
using Umbraco.Web.Mvc;
using Umbraco.Web.WebApi;

namespace FALM.Housekeeping.Controllers
{
    /// <summary>
    /// PluginController("FALMHousekeeping")
    /// HkRecycleBinApiController
    /// </summary>
    [PluginController("FALMHousekeeping")]
    public class HkRecycleBinApiController : UmbracoApiController
    {
        /// <summary>
        /// Get all items in all Umbraco Recycle Bins
        /// </summary>
        /// <returns>HKRecycleBinModel</returns>
        [HttpGet]
        public HKRecycleBinModel GetAllItemsInRecycleBins()
        {
            var RecycleBinModel = new HKRecycleBinModel();
            var ListItemsInTheBin = new List<ItemsInRecycleBinsModel>();
            ItemsInRecycleBinsModel ItemsInRecycleBinModel = null;

            try
            {
                ItemsInRecycleBinModel = new ItemsInRecycleBinsModel
                {
                    Type = "content",
                    Value = ApplicationContext.Current.Services.ContentService.GetContentInRecycleBin().Count().ToString()
                };
                ListItemsInTheBin.Add(ItemsInRecycleBinModel);

                ItemsInRecycleBinModel = new ItemsInRecycleBinsModel
                {
                    Type = "media",
                    Value = ApplicationContext.Current.Services.MediaService.GetMediaInRecycleBin().Count().ToString()
                };
                ListItemsInTheBin.Add(ItemsInRecycleBinModel);

                RecycleBinModel.ListItemsInRecycleBins = ListItemsInTheBin;

                return RecycleBinModel;
            }
            catch (Exception ex)
            {
                ItemsInRecycleBinModel = new ItemsInRecycleBinsModel
                {
                    Type = "error",
                    Value = ex.Message
                };
                ListItemsInTheBin.Add(ItemsInRecycleBinModel);

                RecycleBinModel.ListItemsInRecycleBins = ListItemsInTheBin;

                LogHelper.Error<Exception>("FALM Housekeeping - " + ex.Message, ex);

                return RecycleBinModel;
            }
        }

        /// <summary>
        /// Empty Content Recycle Bin
        /// </summary>
        /// <returns>HKRecycleBinModel</returns>
        [HttpDelete]
        [HttpPost]
        public HKRecycleBinModel PostEmptyContentRecycleBin()
        {
            var RecycleBinModel = new HKRecycleBinModel();
            var ListItemsInTheBin = new List<ItemsInRecycleBinsModel>();
            ItemsInRecycleBinsModel ItemsInRecycleBinModel = null;

            try
            {
                ApplicationContext.Current.Services.ContentService.EmptyRecycleBin();
                ItemsInRecycleBinModel = new ItemsInRecycleBinsModel
                {
                    Type = "content",
                    Value = ApplicationContext.Current.Services.ContentService.GetContentInRecycleBin().Count().ToString()
                };
                ListItemsInTheBin.Add(ItemsInRecycleBinModel);

                RecycleBinModel.ListItemsInRecycleBins = ListItemsInTheBin;

                return RecycleBinModel;
            }
            catch (Exception ex)
            {
                ItemsInRecycleBinModel = new ItemsInRecycleBinsModel
                {
                    Type = "error",
                    Value = ex.Message
                };
                ListItemsInTheBin.Add(ItemsInRecycleBinModel);

                RecycleBinModel.ListItemsInRecycleBins = ListItemsInTheBin;

                LogHelper.Error<Exception>("FALM Housekeeping - " + ex.Message, ex);

                return RecycleBinModel;
            }
        }

        /// <summary>
        /// Empty Media Recycle Bin
        /// </summary>
        /// <returns>HKRecycleBinModel</returns>
        [HttpDelete]
        [HttpPost]
        public HKRecycleBinModel PostEmptyMediaRecycleBin()
        {
            var RecycleBinModel = new HKRecycleBinModel();
            var ListItemsInTheBin = new List<ItemsInRecycleBinsModel>();
            ItemsInRecycleBinsModel ItemsInRecycleBinModel = null;

            try
            {
                ApplicationContext.Current.Services.MediaService.EmptyRecycleBin();
                ItemsInRecycleBinModel = new ItemsInRecycleBinsModel
                {
                    Type = "media",
                    Value = ApplicationContext.Current.Services.MediaService.GetMediaInRecycleBin().Count().ToString()
                };
                ListItemsInTheBin.Add(ItemsInRecycleBinModel);

                RecycleBinModel.ListItemsInRecycleBins = ListItemsInTheBin;

                return RecycleBinModel;
            }
            catch (Exception ex)
            {
                ItemsInRecycleBinModel = new ItemsInRecycleBinsModel
                {
                    Type = "error",
                    Value = ex.Message
                };
                ListItemsInTheBin.Add(ItemsInRecycleBinModel);

                RecycleBinModel.ListItemsInRecycleBins = ListItemsInTheBin;

                LogHelper.Error<Exception>("FALM Housekeeping - " + ex.Message, ex);

                return RecycleBinModel;
            }
        }

        /// <summary>
        /// Empty Both Recycle Bins
        /// </summary>
        /// <returns>HKRecycleBinModel</returns>
        [HttpDelete]
        [HttpPost]
        public HKRecycleBinModel PostEmptyBothRecycleBins()
        {
            var RecycleBinModel = new HKRecycleBinModel();
            var ListItemsInTheBin = new List<ItemsInRecycleBinsModel>();
            ItemsInRecycleBinsModel ItemsInRecycleBinModel = null;

            try
            {
                ApplicationContext.Current.Services.ContentService.EmptyRecycleBin();
                ItemsInRecycleBinModel = new ItemsInRecycleBinsModel
                {
                    Type = "content",
                    Value = ApplicationContext.Current.Services.ContentService.GetContentInRecycleBin().Count().ToString()
                };
                ListItemsInTheBin.Add(ItemsInRecycleBinModel);

                ApplicationContext.Current.Services.MediaService.EmptyRecycleBin();
                ItemsInRecycleBinModel = new ItemsInRecycleBinsModel
                {
                    Type = "media",
                    Value = ApplicationContext.Current.Services.MediaService.GetMediaInRecycleBin().Count().ToString()
                };
                ListItemsInTheBin.Add(ItemsInRecycleBinModel);

                RecycleBinModel.ListItemsInRecycleBins = ListItemsInTheBin;

                return RecycleBinModel;
            }
            catch (Exception ex)
            {
                ItemsInRecycleBinModel = new ItemsInRecycleBinsModel
                {
                    Type = "error",
                    Value = ex.Message
                };
                ListItemsInTheBin.Add(ItemsInRecycleBinModel);

                RecycleBinModel.ListItemsInRecycleBins = ListItemsInTheBin;

                LogHelper.Error<Exception>("FALM Housekeeping - " + ex.Message, ex);

                return RecycleBinModel;
            }
        }

        /// <summary>
        /// Create the Auto Empty Recycle Bins Service Page
        /// </summary>
        /// <returns>bool</returns>
        [HttpGet]
        public bool GetCreateServicePage()
        {
            List<ContentTypeSort> listAllowedContentTypes = null;
            List<ITemplate> listAllowedTemplates = null;
            EntityContainer dtFalmContainer = null;
            IContentType dtFalmHKRecycleBinsCleanup = null;
            Template falmTemplate = null;
            IContent falmServiceFolderNode = null;
            IContent falmHKRecycleBinsCleanupNode = null;

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

                // Check existing DOCUMENT TYPE "falmHKRecycleBinsCleanup"
                if (contentTypeService.GetContentType("falmHKRecycleBinsCleanup") == null)
                {
                    // Create Docuement Type "falmHKRecycleBinsCleanup"
                    var createNewDocumentType = new ContentType(dtFalmContainer.Id)
                    {
                        Alias = "falmHKRecycleBinsCleanup",
                        Name = "FALM Housekeeping - Recycle Bins Cleanup",
                        AllowedAsRoot = false,
                        Icon = "icon-trash color-orange"
                    };
                    contentTypeService.Save(createNewDocumentType);

                    LogHelper.Info<string>("FALM Housekeeping - Document Type 'FALM Housekeeping - Recycle Bins Cleanup' successfully created");
                }
                else
                {
                    LogHelper.Info<string>("FALM Housekeeping - Document Type 'FALM Housekeeping - Recycle Bins Cleanup' already exist");
                }

                // Check existing TEMPLATE "falmHKRecycleBinsCleanup"
                falmTemplate = (Template)fileService.GetTemplate("falmHKRecycleBinsCleanup");

                if(falmTemplate == null)
                {
                    // Get Template "falmHKRecycleBinsCleanup" and Set the Content
                    falmTemplate = new Template("FALM Housekeeping - Recycle Bins Cleanup", "falmHKRecycleBinsCleanup")
                    {
                        Content = "@inherits Umbraco.Web.Mvc.UmbracoTemplatePage" + System.Environment.NewLine + "@{ Layout = null; }" + System.Environment.NewLine + "@inherits Umbraco.Web.Mvc.UmbracoViewPage<FALM.Housekeeping.Models.HKRecycleBinPageModel>" + System.Environment.NewLine + "<html>" + System.Environment.NewLine + "<head>" + System.Environment.NewLine + "<title>FALM Housekeeping - Recycle Bins Cleanup</title>" + System.Environment.NewLine + "</head>" + System.Environment.NewLine + "<body>" + System.Environment.NewLine + "<h1>FALM Housekeeping - Recycle Bins Cleanup</h1>" + System.Environment.NewLine + "<div>Both Recycle Bins (content/ media) has been emptied</div>" + System.Environment.NewLine + "</body>" + System.Environment.NewLine + "</html>"
                    };
                    fileService.SaveTemplate(falmTemplate);

                    LogHelper.Info<string>("FALM Housekeeping - Template 'FALM Housekeeping - Recycle Bins Cleanup' successfully created");
                }
                else
                {
                    LogHelper.Info<string>("FALM Housekeeping - Template 'FALM Housekeeping - Recycle Bins Cleanup' already exist");
                }

                listAllowedTemplates = new List<ITemplate>
                {
                    falmTemplate
                };

                // Set allowed and default template
                dtFalmHKRecycleBinsCleanup = contentTypeService.GetContentType("falmHKRecycleBinsCleanup");
                dtFalmHKRecycleBinsCleanup.AllowedTemplates = listAllowedTemplates;
                dtFalmHKRecycleBinsCleanup.SetDefaultTemplate(listAllowedTemplates.FirstOrDefault());
                contentTypeService.Save(dtFalmHKRecycleBinsCleanup);

                // Check existing DOCUMENT TYPE "falmServiceFolder"
                if (contentTypeService.GetContentType("falmServiceFolder") == null)
                {
                    listAllowedContentTypes = new List<ContentTypeSort>
                    {
                        new ContentTypeSort(dtFalmHKRecycleBinsCleanup.Id, 0)
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

                // Check existing "falmHKRecycleBinsCleanup" nodes
                var listHKRecycleBinsCleanupNodes = contentService.GetContentOfContentType(contentTypeService.GetContentType("falmHKRecycleBinsCleanup").Id);
                if ((listHKRecycleBinsCleanupNodes.Count() == 0) || (listHKRecycleBinsCleanupNodes.First(n => n.Name == "Housekeeping - Cleanup Recycle Bins") == null))
                {
                    // Create Recycle Bins Cleanup node
                    falmHKRecycleBinsCleanupNode = contentService.CreateContent("Housekeeping - Cleanup Recycle Bins", falmServiceFolderNode.Id, "falmHKRecycleBinsCleanup");
                    falmHKRecycleBinsCleanupNode.Template = dtFalmHKRecycleBinsCleanup.DefaultTemplate;
                    contentService.SaveAndPublishWithStatus(falmHKRecycleBinsCleanupNode);
                    contentService.RebuildXmlStructures(falmHKRecycleBinsCleanupNode.Id);

                    LogHelper.Info<string>("FALM Housekeeping - Content Node 'Housekeeping - Cleanup Recycle Bins' successfully created");
                }
                else
                {
                    LogHelper.Info<string>("FALM Housekeeping - Content Node 'Housekeeping - Cleanup Recycle Bins' already exist");
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
    /// falmHKRecycleBinsCleanupController is the controller used for service purposes
    /// </summary>
    public class falmHKRecycleBinsCleanupController : RenderMvcController
    {
        /// <summary>
        /// This Action is a service page that auto empty both Content and Media Recycle Bins
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpGet]
        public override ActionResult Index(RenderModel model)
        {
            try
            {
                HKRecycleBinPageModel HKRecycleBinPageModel = new HKRecycleBinPageModel(model.Content, model.CurrentCulture);

                // Empty Content Recycle Bin
                ApplicationContext.Current.Services.ContentService.EmptyRecycleBin();

                // Empty Media Recycle Bin
                ApplicationContext.Current.Services.MediaService.EmptyRecycleBin();

                HKRecycleBinPageModel.IsBothRecycleBinsCleaned = true;

                LogHelper.Info<string>("FALM Housekeeping - Cleanup Recycle Bins successfully completed");

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