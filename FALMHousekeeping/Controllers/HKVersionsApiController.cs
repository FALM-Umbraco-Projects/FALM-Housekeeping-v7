using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using FALM.Housekeeping.Helpers;
using FALM.Housekeeping.Models;
using FALM.Housekeeping.Services;
using Umbraco.Core;
using Umbraco.Core.Cache;
using Umbraco.Core.Logging;
using Umbraco.Core.Models;
using Umbraco.Core.Persistence;
using Umbraco.Web.Mvc;
using Umbraco.Web.WebApi;

namespace FALM.Housekeeping.Controllers
{
    /// <summary>
    /// PluginController("FALMHousekeeping")
    /// HkVersionsApiController
    /// </summary>
    [PluginController("FALMHousekeeping")]
    public class HkVersionsApiController : UmbracoApiController
    {
        /// <summary></summary>
        private readonly UmbracoDatabase db;
        /// <summary></summary>
        private readonly IRuntimeCacheProvider cache;
        /// <summary></summary>
        protected HkVersionsService versionsService;
        /// <summary></summary>
        protected HKVersionsModel VersionsModel = new HKVersionsModel();
        /// <summary></summary>
        protected HKVersionsModel CurrentPublishedVersionsModel = new HKVersionsModel();
        /// <summary></summary>
        protected List<CurrentPublishedVersionModel> ListCurrentPublishedVersions = new List<CurrentPublishedVersionModel>();
        /// <summary></summary>
        protected HistoryVersionsModel HistoryVersionsModel = new HistoryVersionsModel();
        /// <summary></summary>
        protected List<HistoryVersionModel> ListHistoryVersions = new List<HistoryVersionModel>();

        /// <summary>
        /// Get all published versions
        /// </summary>
        /// <returns>VersionsModel</returns>
        [HttpGet]
        public HKVersionsModel GetPublishedNodes(string search = "", int itemsPerPage = 10, int pageNumber = 1)
        {
            try { 
                var request = new HKVersionsModel()
                {
                    Search = search,
                    ItemsPerPage = itemsPerPage,
                    CurrentPage = pageNumber,
                };

                var sqlVersions = "SELECT CurDoc.nodeId, CurDoc.text AS NodeName, umbracoUser.userName AS NodeUser, CurDoc.updateDate AS PublishedDate, HistDoc.VersionsCount AS VersionsCount ";
                sqlVersions += "FROM cmsDocument AS CurDoc ";
                sqlVersions += "INNER JOIN umbracoUser ON CurDoc.documentUser = umbracoUser.id ";
                sqlVersions += "LEFT OUTER JOIN (";
                sqlVersions += "SELECT COUNT(1) as VersionsCount, nodeId ";
                sqlVersions += "FROM cmsDocument ";
                sqlVersions += "WHERE (published = 0) ";
                sqlVersions += "GROUP BY nodeid ";
                sqlVersions += ") AS HistDoc ON CurDoc.nodeId = HistDoc.nodeId ";
                sqlVersions += "WHERE (CurDoc.published = 1 AND curdoc.nodeid = curdoc.nodeid) ";
                sqlVersions += "ORDER BY CurDoc.nodeId; ";

                using (var db = HkDbHelper.ResolveDatabase())
                {
                    ListCurrentPublishedVersions = db.Fetch<CurrentPublishedVersionModel>(sqlVersions);
                }

                if (!string.IsNullOrEmpty(request.Search))
                {
                    ListCurrentPublishedVersions = ListCurrentPublishedVersions.Where(tl => tl.NodeId > 0 && tl.NodeId.ToString().Contains(request.Search.ToLower()) ||
                                                          !String.IsNullOrEmpty(tl.NodeName) && tl.NodeName.ToLower().Contains(request.Search.ToLower()) ||
                                                          !String.IsNullOrEmpty(tl.NodeUser) && tl.NodeUser.ToLower().Contains(request.Search.ToLower())).ToList();
                }

                request.ListCurrentPublishedVersions = ListCurrentPublishedVersions;

                var paged = CreatePagination(request);

                //var paged = versionsService.GetVersions(request);

                VersionsModel.CurrentPage = int.Parse(paged.PageNumber.ToString());
                VersionsModel.ItemsPerPage = int.Parse(paged.PageSize.ToString());
                VersionsModel.ListCurrentPublishedVersions = paged.Items.ToList();
                VersionsModel.TotalItems = int.Parse(paged.TotalItems.ToString());
                VersionsModel.TotalPages = int.Parse(paged.TotalPages.ToString());

                return VersionsModel;
            }
            catch (Exception ex)
            {
                LogHelper.Error<Exception>(ex.Message, ex);
                return null;
            }
        }

        /// <summary>
        /// Get versions of a published node
        /// </summary>
        /// <param name="publishedNodeId"></param>
        /// <returns>HistoryVersionsModel</returns>
        [HttpGet]
        public HistoryVersionsModel GetVersionsByNodeId(int publishedNodeId)
        {
            try
            {
                string sqlVersions = "SELECT CAST(versionId AS NVARCHAR(50)) AS VersionGUID, updateDate AS VersionDate, CAST(published AS INT) AS Published, CAST(newest AS INT) AS Newest ";
                sqlVersions += "FROM CMSDocument ";
                sqlVersions += "WHERE (nodeId = " + publishedNodeId + ") ";
                sqlVersions += "ORDER BY VersionDate DESC, published DESC, newest DESC, VersionGUID DESC; ";

                using (var db = HkDbHelper.ResolveDatabase())
                {
                    ListHistoryVersions = db.Fetch<HistoryVersionModel>(sqlVersions);
                    HistoryVersionsModel.ListNodeVersions = ListHistoryVersions;
                }
            }
            catch (Exception ex)
            {
                LogHelper.Error<Exception>(ex.Message, ex);
                return HistoryVersionsModel;
            }

            return HistoryVersionsModel;
        }

        /// <summary>
        /// Delete all versions by count, except the published and the newest
        /// </summary>
        /// <param name="versionsToKeep"></param>
        /// <returns>List of CleanupResultModel</returns>
        [HttpPost]
        public List<CleanupResultModel> PostDeleteVersionsByCount(int versionsToKeep)
        {
            try
            {
                HttpContext.Current.Server.ScriptTimeout = 10000;

                var cleanupSummary = new List<CleanupResultModel>();

                using (var db = HkDbHelper.ResolveDatabase())
                {
                    db.CommandTimeout = 0;

                    var _dbContext = ApplicationContext.Current.DatabaseContext;
                    var _dbHelper = new DatabaseSchemaHelper(_dbContext.Database, LoggerResolver.Current.Logger, _dbContext.SqlSyntax);

                    CleanupResultModel cleanupResult = new CleanupResultModel();

                    // Begin Transaction
                    db.BeginTransaction();

                    // Delete versions from cmsPreviewXml
                    if (_dbHelper.TableExist("cmsPreviewXml"))
                    {
                        cleanupResult = new CleanupResultModel
                        {
                            Type = "cmsPreviewXml",
                            Result = _dbContext.Database.Execute("DELETE b FROM (SELECT FALMtmp1.VersionId FROM(SELECT published, versionId, newest FROM cmsDocument WHERE versionId NOT IN(SELECT VersionId FROM(SELECT TOP(1000000000000) DD.nodeId, CV.versionId, CV.versionDate, DD.published, DD.newest, ROW_NUMBER() OVER(PARTITION BY nodeId ORDER BY nodeId, versionDate DESC) RowNum FROM cmsContentVersion CV JOIN cmsDocument DD ON CV.versionId = DD.versionId GROUP BY DD.nodeId, CV.versionId, CV.versionDate, DD.published, DD.newest, CV.versionDate)tmp WHERE tmp.RowNum <= @0 OR tmp.published = 1 OR tmp.newest = 1)) AS FALMtmp1 WHERE FALMtmp1.published = 0 AND FALMtmp1.newest = 0) a INNER JOIN cmsPreviewXml b ON a.versionId = b.versionId;", versionsToKeep)
                    };
                        cleanupSummary.Add(cleanupResult);
                    }
                    
                    // Delete versions from cmsContentVersion
                    if (_dbHelper.TableExist("cmsContentVersion"))
                    {
                        cleanupResult = new CleanupResultModel
                        {
                            Type = "cmsContentVersion",
                            Result = _dbContext.Database.Execute("DELETE b FROM (SELECT FALMtmp1.VersionId FROM(SELECT published, versionId, newest FROM cmsDocument WHERE versionId NOT IN(SELECT VersionId FROM(SELECT TOP(1000000000000) DD.nodeId, CV.versionId, CV.versionDate, DD.published, DD.newest, ROW_NUMBER() OVER(PARTITION BY nodeId ORDER BY nodeId, versionDate DESC) RowNum FROM cmsContentVersion CV JOIN cmsDocument DD ON CV.versionId = DD.versionId GROUP BY DD.nodeId, CV.versionId, CV.versionDate, DD.published, DD.newest, CV.versionDate)tmp WHERE tmp.RowNum <= @0 OR tmp.published = 1 OR tmp.newest = 1)) AS FALMtmp1 WHERE FALMtmp1.published = 0 AND FALMtmp1.newest = 0) a INNER JOIN cmsContentVersion b ON a.versionId = b.versionId;", versionsToKeep)
                        };

                        cleanupSummary.Add(cleanupResult);
                    }

                    // Delete all properties data of each versions to delete from cmsPropertyData
                    if (_dbHelper.TableExist("cmsPropertyData"))
                    {
                        cleanupResult = new CleanupResultModel
                        {
                            Type = "cmsPropertyData",
                            Result = _dbContext.Database.Execute("DELETE b FROM (SELECT FALMtmp1.VersionId FROM(SELECT published, versionId, newest FROM cmsDocument WHERE versionId NOT IN(SELECT VersionId FROM(SELECT TOP(1000000000000) DD.nodeId, CV.versionId, CV.versionDate, DD.published, DD.newest, ROW_NUMBER() OVER(PARTITION BY nodeId ORDER BY nodeId, versionDate DESC) RowNum FROM cmsContentVersion CV JOIN cmsDocument DD ON CV.versionId = DD.versionId GROUP BY DD.nodeId, CV.versionId, CV.versionDate, DD.published, DD.newest, CV.versionDate)tmp WHERE tmp.RowNum <= @0 OR tmp.published = 1 OR tmp.newest = 1)) AS FALMtmp1 WHERE FALMtmp1.published = 0 AND FALMtmp1.newest = 0) a INNER JOIN cmsPropertyData b ON a.versionId = b.versionId;", versionsToKeep)
                        };

                        cleanupSummary.Add(cleanupResult);
                    }

                    // Delete versions from cmsDocument
                    if (_dbHelper.TableExist("cmsDocument"))
                    {
                        cleanupResult = new CleanupResultModel
                        {
                            Type = "cmsDocument",
                            Result = _dbContext.Database.Execute("DELETE b FROM (SELECT FALMtmp1.VersionId FROM(SELECT published, versionId, newest FROM cmsDocument WHERE versionId NOT IN(SELECT VersionId FROM(SELECT TOP(1000000000000) DD.nodeId, CV.versionId, CV.versionDate, DD.published, DD.newest, ROW_NUMBER() OVER(PARTITION BY nodeId ORDER BY nodeId, versionDate DESC) RowNum FROM cmsContentVersion CV JOIN cmsDocument DD ON CV.versionId = DD.versionId GROUP BY DD.nodeId, CV.versionId, CV.versionDate, DD.published, DD.newest, CV.versionDate)tmp WHERE tmp.RowNum <= @0 OR tmp.published = 1 OR tmp.newest = 1)) AS FALMtmp1 WHERE FALMtmp1.published = 0 AND FALMtmp1.newest = 0) a INNER JOIN cmsDocument b ON a.versionId = b.versionId;", versionsToKeep)
                        };

                        cleanupSummary.Add(cleanupResult);
                    }

                    // End Transaction
                    db.CompleteTransaction();
                }

                return cleanupSummary;
            }
            catch (Exception ex)
            {
                LogHelper.Error<Exception>(ex.Message, ex);
                return null;
            }
        }

        /// <summary>
        /// Delete all versions of specific nodeId, except the published and the newest
        /// </summary>
        /// <param name="publishedNodeId"></param>
        /// <param name="versionsToKeep"></param>
        /// <returns>List of CleanupResultModel</returns>
        [HttpPost]
        public List<CleanupResultModel> PostDeleteVersionsByNodeId(int publishedNodeId, int versionsToKeep)
        {
            try
            {
                HttpContext.Current.Server.ScriptTimeout = 10000;

                var cleanupSummary = new List<CleanupResultModel>();

                using (var db = HkDbHelper.ResolveDatabase())
                {
                    db.CommandTimeout = 0;

                    var _dbContext = ApplicationContext.Current.DatabaseContext;
                    var _dbHelper = new DatabaseSchemaHelper(_dbContext.Database, LoggerResolver.Current.Logger, _dbContext.SqlSyntax);

                    CleanupResultModel cleanupResult = new CleanupResultModel();

                    // Begin Transaction
                    db.BeginTransaction();

                    // Delete versions from cmsPreviewXml
                    if (_dbHelper.TableExist("cmsPreviewXml"))
                    {
                        cleanupResult = new CleanupResultModel
                        {
                            Type = "cmsPreviewXml",
                            Result = _dbContext.Database.Execute("DELETE b FROM (SELECT FALMtmp1.VersionId,FALMtmp1.nodeId FROM(SELECT published, versionId, newest, nodeId FROM cmsDocument WHERE versionId NOT IN(SELECT VersionId FROM(SELECT TOP(1000000000000) DD.nodeId, CV.versionId, CV.versionDate, DD.published, DD.newest, ROW_NUMBER() OVER(PARTITION BY nodeId ORDER BY nodeId, versionDate DESC) RowNum FROM cmsContentVersion CV JOIN cmsDocument DD ON CV.versionId = DD.versionId GROUP BY DD.nodeId, CV.versionId, CV.versionDate, DD.published, DD.newest, CV.versionDate)tmp WHERE tmp.RowNum <= @1 OR tmp.published = 1 OR tmp.newest = 1)) AS FALMtmp1 WHERE FALMtmp1.published = 0 AND FALMtmp1.newest = 0) a INNER JOIN cmsPreviewXml b ON a.versionId = b.versionId AND a.nodeId=@0;", publishedNodeId, versionsToKeep)
                        };
                        cleanupSummary.Add(cleanupResult);
                    }

                    // Delete versions from cmsContentVersion
                    if (_dbHelper.TableExist("cmsContentVersion"))
                    {
                        cleanupResult = new CleanupResultModel
                        {
                            Type = "cmsContentVersion",
                            Result = _dbContext.Database.Execute("DELETE b FROM (SELECT FALMtmp1.VersionId,FALMtmp1.nodeId FROM(SELECT published, versionId, newest, nodeId FROM cmsDocument WHERE versionId NOT IN(SELECT VersionId FROM(SELECT TOP(1000000000000) DD.nodeId, CV.versionId, CV.versionDate, DD.published, DD.newest, ROW_NUMBER() OVER(PARTITION BY nodeId ORDER BY nodeId, versionDate DESC) RowNum FROM cmsContentVersion CV JOIN cmsDocument DD ON CV.versionId = DD.versionId GROUP BY DD.nodeId, CV.versionId, CV.versionDate, DD.published, DD.newest, CV.versionDate)tmp WHERE tmp.RowNum <= @1 OR tmp.published = 1 OR tmp.newest = 1)) AS FALMtmp1 WHERE FALMtmp1.published = 0 AND FALMtmp1.newest = 0) a INNER JOIN cmsContentVersion b ON a.versionId = b.versionId AND a.nodeId=@0;", publishedNodeId, versionsToKeep)
                        };

                        cleanupSummary.Add(cleanupResult);
                    }

                    // Delete all properties data of each versions to delete from cmsPropertyData
                    if (_dbHelper.TableExist("cmsPropertyData"))
                    {
                        cleanupResult = new CleanupResultModel
                        {
                            Type = "cmsPropertyData",
                            Result = _dbContext.Database.Execute("DELETE b FROM (SELECT FALMtmp1.VersionId,FALMtmp1.nodeId FROM(SELECT published, versionId, newest, nodeId FROM cmsDocument WHERE versionId NOT IN(SELECT VersionId FROM(SELECT TOP(1000000000000) DD.nodeId, CV.versionId, CV.versionDate, DD.published, DD.newest, ROW_NUMBER() OVER(PARTITION BY nodeId ORDER BY nodeId, versionDate DESC) RowNum FROM cmsContentVersion CV JOIN cmsDocument DD ON CV.versionId = DD.versionId GROUP BY DD.nodeId, CV.versionId, CV.versionDate, DD.published, DD.newest, CV.versionDate)tmp WHERE tmp.RowNum <= @1 OR tmp.published = 1 OR tmp.newest = 1)) AS FALMtmp1 WHERE FALMtmp1.published = 0 AND FALMtmp1.newest = 0) a INNER JOIN cmsPropertyData b ON a.versionId = b.versionId AND a.nodeId=@0;", publishedNodeId, versionsToKeep)
                        };

                        cleanupSummary.Add(cleanupResult);
                    }

                    // Delete versions from cmsDocument
                    if (_dbHelper.TableExist("cmsDocument"))
                    {
                        cleanupResult = new CleanupResultModel
                        {
                            Type = "cmsDocument",
                            Result = _dbContext.Database.Execute("DELETE b FROM (SELECT FALMtmp1.VersionId,FALMtmp1.nodeId FROM(SELECT published, versionId, newest, nodeId FROM cmsDocument WHERE versionId NOT IN(SELECT VersionId FROM(SELECT TOP(1000000000000) DD.nodeId, CV.versionId, CV.versionDate, DD.published, DD.newest, ROW_NUMBER() OVER(PARTITION BY nodeId ORDER BY nodeId, versionDate DESC) RowNum FROM cmsContentVersion CV JOIN cmsDocument DD ON CV.versionId = DD.versionId GROUP BY DD.nodeId, CV.versionId, CV.versionDate, DD.published, DD.newest, CV.versionDate)tmp WHERE tmp.RowNum <= @1 OR tmp.published = 1 OR tmp.newest = 1)) AS FALMtmp1 WHERE FALMtmp1.published = 0 AND FALMtmp1.newest = 0) a INNER JOIN cmsDocument b ON a.versionId = b.versionId AND a.nodeId=@0;", publishedNodeId, versionsToKeep)
                        };

                        cleanupSummary.Add(cleanupResult);
                    }

                    // End Transaction
                    db.CompleteTransaction();
                }

                return cleanupSummary;
            }
            catch (Exception ex)
            {
                LogHelper.Error<Exception>(ex.Message, ex);
                return null;
            }
        }

        /// <summary>
        /// Create Pagination
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        private PagedResult<CurrentPublishedVersionModel> CreatePagination(HKVersionsModel request)
        {
            try
            {
                var startAt = (request.CurrentPage - 1) * request.ItemsPerPage;

                var PagedLogs = new PagedResult<CurrentPublishedVersionModel>(request.ListCurrentPublishedVersions.Count, request.CurrentPage, request.ItemsPerPage)
                {
                    Items = request.ListCurrentPublishedVersions.Skip(startAt).Take(request.ItemsPerPage).ToList<CurrentPublishedVersionModel>()
                };

                return PagedLogs;
            }
            catch (Exception ex)
            {
                LogHelper.Error<Exception>(ex.Message, ex);
                return null;
            }
        }
    }
}