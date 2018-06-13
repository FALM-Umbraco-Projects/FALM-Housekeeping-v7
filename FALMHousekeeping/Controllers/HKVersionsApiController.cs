using System;
using System.Collections.Generic;
using System.Web;
using System.Web.Mvc;
using FALM.Housekeeping.Helpers;
using FALM.Housekeeping.Models;
using Umbraco.Core;
using Umbraco.Core.Logging;
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
        public HKVersionsModel GetPublishedNodes()
        {
            try
            {
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
                    CurrentPublishedVersionsModel.ListCurrentPublishedVersions = ListCurrentPublishedVersions;
                }
            }
            catch (Exception ex)
            {
                LogHelper.Error<Exception>(ex.Message, ex);
                return CurrentPublishedVersionsModel;
            }

            return CurrentPublishedVersionsModel;
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
                            Result = _dbContext.Database.Execute("DELETE FROM cmsPreviewXml WHERE VersionId IN (SELECT FALMtmp1.VersionId FROM (SELECT nodeId, published, documentUser, versionId, text, releaseDate, expireDate, updateDate, templateId, newest FROM cmsDocument WHERE versionID NOT IN (SELECT D.versionId FROM cmsDocument D WHERE D.versionId IN (SELECT versionId FROM (SELECT TOP(1000000000000) DD.nodeId, CV.versionId, DD.published, DD.newest, COUNT (CV.versionDate) RowNum FROM(cmsContentVersion CV JOIN cmsDocument DD ON CV.versionId = DD.versionId), (cmsContentVersion CV2 JOIN cmsDocument DD2 ON CV2.versionId = DD2.versionId) WHERE DD.nodeId = D.nodeId AND CV.versionDate <= CV2.versionDate GROUP BY DD.nodeId, CV.versionId, DD.published, DD.newest, CV.versionDate ORDER BY DD.nodeId, CV.versionDate DESC) AS tmp WHERE tmp.RowNum <= @0 OR tmp.published = 1 OR tmp.newest = 1))) AS FALMtmp1 WHERE FALMtmp1.published = 0 AND FALMtmp1.newest = 0);", versionsToKeep)
                    };
                        cleanupSummary.Add(cleanupResult);
                    }
                    
                    // Delete versions from cmsContentVersion
                    if (_dbHelper.TableExist("cmsContentVersion"))
                    {
                        cleanupResult = new CleanupResultModel
                        {
                            Type = "cmsContentVersion",
                            Result = _dbContext.Database.Execute("DELETE FROM cmsContentVersion WHERE VersionId IN (SELECT FALMtmp1.VersionId FROM (SELECT nodeId, published, documentUser, versionId, text, releaseDate, expireDate, updateDate, templateId, newest FROM cmsDocument WHERE versionID NOT IN (SELECT D.versionId FROM cmsDocument D WHERE D.versionId IN (SELECT versionId FROM (SELECT TOP(1000000000000) DD.nodeId, CV.versionId, DD.published, DD.newest, COUNT (CV.versionDate) RowNum FROM (cmsContentVersion CV JOIN cmsDocument DD ON CV.versionId = DD.versionId), (cmsContentVersion CV2 JOIN cmsDocument DD2 ON CV2.versionId = DD2.versionId) WHERE DD.nodeId = D.nodeId AND CV.versionDate <= CV2.versionDate GROUP BY DD.nodeId, CV.versionId, DD.published, DD.newest, CV.versionDate ORDER BY DD.nodeId, CV.versionDate DESC) AS tmp WHERE tmp.RowNum <= @0 OR tmp.published = 1 OR tmp.newest = 1))) AS FALMtmp1 WHERE FALMtmp1.published = 0 AND FALMtmp1.newest = 0);", versionsToKeep)
                        };

                        cleanupSummary.Add(cleanupResult);
                    }

                    // Delete all properties data of each versions to delete from cmsPropertyData
                    if (_dbHelper.TableExist("cmsPropertyData"))
                    {
                        cleanupResult = new CleanupResultModel
                        {
                            Type = "cmsPropertyData",
                            Result = _dbContext.Database.Execute("DELETE FROM cmsPropertyData WHERE VersionId IN (SELECT FALMtmp1.VersionId FROM (SELECT nodeId, published, documentUser, versionId, text, releaseDate, expireDate, updateDate, templateId, newest FROM cmsDocument WHERE versionID NOT IN (SELECT D.versionId FROM cmsDocument D WHERE D.versionId IN (SELECT versionId FROM (SELECT TOP(1000000000000) DD.nodeId, CV.versionId, DD.published, DD.newest, COUNT (CV.versionDate) RowNum FROM (cmsContentVersion CV JOIN cmsDocument DD ON CV.versionId = DD.versionId), (cmsContentVersion CV2 JOIN cmsDocument DD2 ON CV2.versionId = DD2.versionId) WHERE DD.nodeId = D.nodeId AND CV.versionDate <= CV2.versionDate GROUP BY DD.nodeId, CV.versionId, DD.published, DD.newest, CV.versionDate ORDER BY DD.nodeId, CV.versionDate DESC) AS tmp WHERE tmp.RowNum <= @0 OR tmp.published = 1 OR tmp.newest = 1))) AS FALMtmp1 WHERE FALMtmp1.published = 0 AND FALMtmp1.newest = 0);", versionsToKeep)
                        };

                        cleanupSummary.Add(cleanupResult);
                    }

                    // Delete versions from cmsDocument
                    if (_dbHelper.TableExist("cmsDocument"))
                    {
                        cleanupResult = new CleanupResultModel
                        {
                            Type = "cmsDocument",
                            Result = _dbContext.Database.Execute("DELETE FROM cmsDocument WHERE VersionId IN (SELECT FALMtmp1.VersionId FROM (SELECT nodeId, published, documentUser, versionId, text, releaseDate, expireDate, updateDate, templateId, newest FROM cmsDocument WHERE versionID NOT IN (SELECT D.versionId FROM cmsDocument D WHERE D.versionId IN (SELECT versionId FROM (SELECT TOP(1000000000000) DD.nodeId, CV.versionId, DD.published, DD.newest, COUNT (CV.versionDate) RowNum FROM (cmsContentVersion CV JOIN cmsDocument DD ON CV.versionId = DD.versionId), (cmsContentVersion CV2 JOIN cmsDocument DD2 ON CV2.versionId = DD2.versionId) WHERE DD.nodeId = D.nodeId AND CV.versionDate <= CV2.versionDate GROUP BY DD.nodeId, CV.versionId, DD.published, DD.newest, CV.versionDate ORDER BY DD.nodeId, CV.versionDate DESC) AS tmp WHERE tmp.RowNum <= @0 OR tmp.published = 1 OR tmp.newest = 1))) AS FALMtmp1 WHERE FALMtmp1.published = 0 AND FALMtmp1.newest = 0);", versionsToKeep)
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
                            Result = _dbContext.Database.Execute("DELETE FROM cmsPreviewXml WHERE VersionId IN (SELECT FALMtmp1.VersionId FROM (SELECT nodeId, published, documentUser, versionId, text, releaseDate, expireDate, updateDate, templateId, newest FROM cmsDocument WHERE nodeId = @0 AND versionID NOT IN (SELECT D.versionId FROM cmsDocument D WHERE D.versionId IN (SELECT versionId FROM (SELECT TOP(1000000000000) DD.nodeId, CV.versionId, DD.published, DD.newest, COUNT (CV.versionDate) RowNum FROM(cmsContentVersion CV JOIN cmsDocument DD ON CV.versionId = DD.versionId), (cmsContentVersion CV2 JOIN cmsDocument DD2 ON CV2.versionId = DD2.versionId) WHERE DD.nodeId = D.nodeId AND CV.versionDate <= CV2.versionDate GROUP BY DD.nodeId, CV.versionId, DD.published, DD.newest, CV.versionDate ORDER BY DD.nodeId, CV.versionDate DESC) AS tmp WHERE tmp.RowNum <= @1 OR tmp.published = 1 OR tmp.newest = 1))) AS FALMtmp1 WHERE FALMtmp1.published = 0 AND FALMtmp1.newest = 0);", publishedNodeId, versionsToKeep)
                        };
                        cleanupSummary.Add(cleanupResult);
                    }

                    // Delete versions from cmsContentVersion
                    if (_dbHelper.TableExist("cmsContentVersion"))
                    {
                        cleanupResult = new CleanupResultModel
                        {
                            Type = "cmsContentVersion",
                            Result = _dbContext.Database.Execute("DELETE FROM cmsContentVersion WHERE VersionId IN (SELECT FALMtmp1.VersionId FROM (SELECT nodeId, published, documentUser, versionId, text, releaseDate, expireDate, updateDate, templateId, newest FROM cmsDocument WHERE nodeId = @0 AND versionID NOT IN (SELECT D.versionId FROM cmsDocument D WHERE D.versionId IN (SELECT versionId FROM (SELECT TOP(1000000000000) DD.nodeId, CV.versionId, DD.published, DD.newest, COUNT (CV.versionDate) RowNum FROM (cmsContentVersion CV JOIN cmsDocument DD ON CV.versionId = DD.versionId), (cmsContentVersion CV2 JOIN cmsDocument DD2 ON CV2.versionId = DD2.versionId) WHERE DD.nodeId = D.nodeId AND CV.versionDate <= CV2.versionDate GROUP BY DD.nodeId, CV.versionId, DD.published, DD.newest, CV.versionDate ORDER BY DD.nodeId, CV.versionDate DESC) AS tmp WHERE tmp.RowNum <= @1 OR tmp.published = 1 OR tmp.newest = 1))) AS FALMtmp1 WHERE FALMtmp1.published = 0 AND FALMtmp1.newest = 0);", publishedNodeId, versionsToKeep)
                        };

                        cleanupSummary.Add(cleanupResult);
                    }

                    // Delete all properties data of each versions to delete from cmsPropertyData
                    if (_dbHelper.TableExist("cmsPropertyData"))
                    {
                        cleanupResult = new CleanupResultModel
                        {
                            Type = "cmsPropertyData",
                            Result = _dbContext.Database.Execute("DELETE FROM cmsPropertyData WHERE VersionId IN (SELECT FALMtmp1.VersionId FROM (SELECT nodeId, published, documentUser, versionId, text, releaseDate, expireDate, updateDate, templateId, newest FROM cmsDocument WHERE nodeId = @0 AND versionID NOT IN (SELECT D.versionId FROM cmsDocument D WHERE D.versionId IN (SELECT versionId FROM (SELECT TOP(1000000000000) DD.nodeId, CV.versionId, DD.published, DD.newest, COUNT (CV.versionDate) RowNum FROM (cmsContentVersion CV JOIN cmsDocument DD ON CV.versionId = DD.versionId), (cmsContentVersion CV2 JOIN cmsDocument DD2 ON CV2.versionId = DD2.versionId) WHERE DD.nodeId = D.nodeId AND CV.versionDate <= CV2.versionDate GROUP BY DD.nodeId, CV.versionId, DD.published, DD.newest, CV.versionDate ORDER BY DD.nodeId, CV.versionDate DESC) AS tmp WHERE tmp.RowNum <= @1 OR tmp.published = 1 OR tmp.newest = 1))) AS FALMtmp1 WHERE FALMtmp1.published = 0 AND FALMtmp1.newest = 0);", publishedNodeId, versionsToKeep)
                        };

                        cleanupSummary.Add(cleanupResult);
                    }

                    // Delete versions from cmsDocument
                    if (_dbHelper.TableExist("cmsDocument"))
                    {
                        cleanupResult = new CleanupResultModel
                        {
                            Type = "cmsDocument",
                            Result = _dbContext.Database.Execute("DELETE FROM cmsDocument WHERE VersionId IN (SELECT FALMtmp1.VersionId FROM (SELECT nodeId, published, documentUser, versionId, text, releaseDate, expireDate, updateDate, templateId, newest FROM cmsDocument WHERE nodeId = @0 AND versionID NOT IN (SELECT D.versionId FROM cmsDocument D WHERE D.versionId IN (SELECT versionId FROM (SELECT TOP(1000000000000) DD.nodeId, CV.versionId, DD.published, DD.newest, COUNT (CV.versionDate) RowNum FROM (cmsContentVersion CV JOIN cmsDocument DD ON CV.versionId = DD.versionId), (cmsContentVersion CV2 JOIN cmsDocument DD2 ON CV2.versionId = DD2.versionId) WHERE DD.nodeId = D.nodeId AND CV.versionDate <= CV2.versionDate GROUP BY DD.nodeId, CV.versionId, DD.published, DD.newest, CV.versionDate ORDER BY DD.nodeId, CV.versionDate DESC) AS tmp WHERE tmp.RowNum <= @1 OR tmp.published = 1 OR tmp.newest = 1))) AS FALMtmp1 WHERE FALMtmp1.published = 0 AND FALMtmp1.newest = 0);", publishedNodeId, versionsToKeep)
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
    }
}