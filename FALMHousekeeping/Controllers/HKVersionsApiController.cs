// FALM
using FALM.Housekeeping.Helpers;
using FALM.Housekeeping.Models;
// SYSTEM
using System;
using System.Collections.Generic;
using System.Web.Mvc;
// UMBRACO
using Umbraco.Core.Logging;
using Umbraco.Web.Mvc;
using Umbraco.Web.WebApi;

namespace FALM.Housekeeping.Controllers
{
    /// <summary>
    /// PluginController("FALMHousekeeping")
    /// VersionsApiController
    /// </summary>
    [PluginController("FALMHousekeeping")]
    public class HKVersionsApiController : UmbracoApiController
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
                string sqlVersions   = "SELECT CurDoc.nodeId, CurDoc.text AS NodeName, umbracoUser.userName AS NodeUser, CurDoc.updateDate AS PublishedDate, HistDoc.VersionsCount AS VersionsCount ";
                sqlVersions         += "FROM cmsDocument AS CurDoc ";
                sqlVersions         += "INNER JOIN umbracoUser ON CurDoc.documentUser = umbracoUser.id ";
                sqlVersions         += "LEFT OUTER JOIN (";
                sqlVersions         += "SELECT COUNT(1) as VersionsCount, nodeId ";
                sqlVersions         += "FROM cmsDocument ";
                sqlVersions         += "WHERE (published = 0) ";
                sqlVersions         += "GROUP BY nodeid ";
                sqlVersions         += ") AS HistDoc ON CurDoc.nodeId = HistDoc.nodeId ";
                sqlVersions         += "WHERE (CurDoc.published = 1 AND curdoc.nodeid = curdoc.nodeid) ";
                sqlVersions         += "ORDER BY CurDoc.nodeId; ";

                using (var db = HKDbHelper.ResolveDatabase())
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
                string sqlVersions   = "SELECT CAST(versionId AS NVARCHAR(50)) AS VersionGUID, updateDate AS VersionDate, CAST(published AS INT) AS Published, CAST(newest AS INT) AS Newest ";
                sqlVersions         += "FROM CMSDocument ";
                sqlVersions         += "WHERE (nodeId = " + publishedNodeId + ") ";
                sqlVersions         += "ORDER BY VersionDate DESC, published DESC, newest DESC, VersionGUID DESC; ";

                using (var db = HKDbHelper.ResolveDatabase())
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
                List<CleanupResultModel> cleanupSummary = new List<CleanupResultModel>();
                CleanupResultModel cleanupResult;

                using (var db = HKDbHelper.ResolveDatabase())
                {
                    // Begin Transaction
                    db.BeginTransaction();

                    // Delete versions from cmsPreviewXml
                    string sqlDeletePreviewXml = @"DELETE FROM cmsPreviewXml WHERE VersionId IN (SELECT FALMtmp1.VersionId FROM (SELECT nodeId, published, documentUser, versionId, text, releaseDate, expireDate, updateDate, templateId, newest FROM cmsDocument WHERE versionID NOT IN (SELECT D.versionId FROM cmsDocument D WHERE D.versionId IN (SELECT versionId FROM (SELECT CV.versionId, published, newest, RANK() OVER(ORDER BY CV.versionDate DESC) RowNum FROM cmsContentVersion CV JOIN cmsDocument DD ON CV.versionId = DD.versionId WHERE DD.nodeId = D.nodeId) AS tmp WHERE tmp.RowNum <= @versionsToKeep OR tmp.published = 1 OR tmp.newest = 1))) AS FALMtmp1 WHERE FALMtmp1.published = 0 AND FALMtmp1.newest = 0);";
                    var dbComm = db.CreateCommand(db.Connection, sqlDeletePreviewXml, new { versionsToKeep = versionsToKeep });
                    dbComm.CommandTimeout = 100000;
                    int iResultCount = dbComm.ExecuteNonQuery();
                    cleanupResult = new CleanupResultModel
                    {
                        Type = "cmsPreviewXml",
                        Result = iResultCount
                    };
                    cleanupSummary.Add(cleanupResult);
                    dbComm.Dispose();

                    // Delete versions from cmsContentVersion
                    string sqlDeleteContentVersions = @"DELETE FROM cmsContentVersion WHERE VersionId IN (SELECT FALMtmp1.VersionId FROM (SELECT nodeId, published, documentUser, versionId, text, releaseDate, expireDate, updateDate, templateId, newest FROM cmsDocument WHERE versionID NOT IN (SELECT D.versionId FROM cmsDocument D WHERE D.versionId IN (SELECT versionId FROM (SELECT CV.versionId, published, newest, RANK() OVER(ORDER BY CV.versionDate DESC) RowNum FROM cmsContentVersion CV JOIN cmsDocument DD ON CV.versionId = DD.versionId WHERE DD.nodeId = D.nodeId) AS tmp WHERE tmp.RowNum <= @versionsToKeep OR tmp.published = 1 OR tmp.newest = 1))) AS FALMtmp1 WHERE FALMtmp1.published = 0 AND FALMtmp1.newest = 0);";
                    dbComm = db.CreateCommand(db.Connection, sqlDeleteContentVersions, new { versionsToKeep = versionsToKeep });
                    dbComm.CommandTimeout = 100000;
                    iResultCount = dbComm.ExecuteNonQuery();
                    cleanupResult = new CleanupResultModel
                    {
                        Type = "cmsContentVersion",
                        Result = iResultCount
                    };
                    cleanupSummary.Add(cleanupResult);
                    dbComm.Dispose();

                    // Delete all properties data of each versions to delete from cmsPropertyData
                    string sqlDeletePropertyData = @"DELETE FROM cmsPropertyData WHERE VersionId IN (SELECT FALMtmp1.VersionId FROM (SELECT nodeId, published, documentUser, versionId, text, releaseDate, expireDate, updateDate, templateId, newest FROM cmsDocument WHERE versionID NOT IN (SELECT D.versionId FROM cmsDocument D WHERE D.versionId IN (SELECT versionId FROM (SELECT CV.versionId, published, newest, RANK() OVER(ORDER BY CV.versionDate DESC) RowNum FROM cmsContentVersion CV JOIN cmsDocument DD ON CV.versionId = DD.versionId WHERE DD.nodeId = D.nodeId) AS tmp WHERE tmp.RowNum <= @versionsToKeep OR tmp.published = 1 OR tmp.newest = 1))) AS FALMtmp1 WHERE FALMtmp1.published = 0 AND FALMtmp1.newest = 0);";
                    dbComm = db.CreateCommand(db.Connection, sqlDeletePropertyData, new { versionsToKeep = versionsToKeep });
                    dbComm.CommandTimeout = 100000;
                    iResultCount = dbComm.ExecuteNonQuery();
                    cleanupResult = new CleanupResultModel
                    {
                        Type = "cmsPropertyData",
                        Result = iResultCount
                    };
                    cleanupSummary.Add(cleanupResult);
                    dbComm.Dispose();

                    // Delete versions from cmsDocument
                    string sqlDeleteDocument = @"DELETE FROM cmsDocument WHERE VersionId IN (SELECT FALMtmp1.VersionId FROM (SELECT nodeId, published, documentUser, versionId, text, releaseDate, expireDate, updateDate, templateId, newest FROM cmsDocument WHERE versionID NOT IN (SELECT D.versionId FROM cmsDocument D WHERE D.versionId IN (SELECT versionId FROM (SELECT CV.versionId, published, newest, RANK() OVER(ORDER BY CV.versionDate DESC) RowNum FROM cmsContentVersion CV JOIN cmsDocument DD ON CV.versionId = DD.versionId WHERE DD.nodeId = D.nodeId) AS tmp WHERE tmp.RowNum <= @versionsToKeep OR tmp.published = 1 OR tmp.newest = 1))) AS FALMtmp1 WHERE FALMtmp1.published = 0 AND FALMtmp1.newest = 0);";
                    dbComm = db.CreateCommand(db.Connection, sqlDeleteDocument, new { versionsToKeep = versionsToKeep });
                    dbComm.CommandTimeout = 100000;
                    iResultCount = dbComm.ExecuteNonQuery();
                    cleanupResult = new CleanupResultModel
                    {
                        Type = "cmsDocument",
                        Result = iResultCount
                    };
                    cleanupSummary.Add(cleanupResult);
                    dbComm.Dispose();

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
                List<CleanupResultModel> cleanupSummary = new List<CleanupResultModel>();
                CleanupResultModel cleanupResult;

                using (var db = HKDbHelper.ResolveDatabase())
                {
                    // Begin Transaction
                    db.BeginTransaction();

                    // Delete versions from cmsPreviewXml
                    string sqlDeletePreviewXml = @"DELETE FROM cmsPreviewXml WHERE VersionId IN (SELECT FALMtmp1.VersionId FROM (SELECT nodeId, published, documentUser, versionId, text, releaseDate, expireDate, updateDate, templateId, newest FROM cmsDocument WHERE nodeId = @publishedNodeId AND versionID NOT IN (SELECT D.versionId FROM cmsDocument D WHERE D.versionId IN (SELECT versionId FROM (SELECT CV.versionId, published, newest, RANK() OVER(ORDER BY CV.versionDate DESC) RowNum FROM cmsContentVersion CV JOIN cmsDocument DD ON CV.versionId = DD.versionId WHERE DD.nodeId = D.nodeId) AS tmp WHERE tmp.RowNum <= @versionsToKeep OR tmp.published = 1 OR tmp.newest = 1))) AS FALMtmp1 WHERE FALMtmp1.published = 0 AND FALMtmp1.newest = 0);";
                    var dbComm = db.CreateCommand(db.Connection, sqlDeletePreviewXml, new { publishedNodeId = publishedNodeId, versionsToKeep = versionsToKeep });
                    dbComm.CommandTimeout = 100000;
                    int iResultCount = dbComm.ExecuteNonQuery();
                    cleanupResult = new CleanupResultModel
                    {
                        Type = "cmsPreviewXml",
                        Result = iResultCount
                    };
                    cleanupSummary.Add(cleanupResult);
                    dbComm.Dispose();

                    // Delete versions from cmsContentVersion
                    string sqlDeleteContentVersions = @"DELETE FROM cmsContentVersion WHERE VersionId IN (SELECT FALMtmp1.VersionId FROM (SELECT nodeId, published, documentUser, versionId, text, releaseDate, expireDate, updateDate, templateId, newest FROM cmsDocument WHERE nodeId = @publishedNodeId AND versionID NOT IN (SELECT D.versionId FROM cmsDocument D WHERE D.versionId IN (SELECT versionId FROM (SELECT CV.versionId, published, newest, RANK() OVER(ORDER BY CV.versionDate DESC) RowNum FROM cmsContentVersion CV JOIN cmsDocument DD ON CV.versionId = DD.versionId WHERE DD.nodeId = D.nodeId) AS tmp WHERE tmp.RowNum <= @versionsToKeep OR tmp.published = 1 OR tmp.newest = 1))) AS FALMtmp1 WHERE FALMtmp1.published = 0 AND FALMtmp1.newest = 0);";
                    dbComm = db.CreateCommand(db.Connection, sqlDeleteContentVersions, new { publishedNodeId = publishedNodeId, versionsToKeep = versionsToKeep });
                    dbComm.CommandTimeout = 100000;
                    iResultCount = dbComm.ExecuteNonQuery();
                    cleanupResult = new CleanupResultModel
                    {
                        Type = "cmsContentVersion",
                        Result = iResultCount
                    };
                    cleanupSummary.Add(cleanupResult);
                    dbComm.Dispose();

                    // Delete all properties data of each versions to delete from cmsPropertyData
                    string sqlDeletePropertyData = @"DELETE FROM cmsPropertyData WHERE VersionId IN (SELECT FALMtmp1.VersionId FROM (SELECT nodeId, published, documentUser, versionId, text, releaseDate, expireDate, updateDate, templateId, newest FROM cmsDocument WHERE nodeId = @publishedNodeId AND versionID NOT IN (SELECT D.versionId FROM cmsDocument D WHERE D.versionId IN (SELECT versionId FROM (SELECT CV.versionId, published, newest, RANK() OVER(ORDER BY CV.versionDate DESC) RowNum FROM cmsContentVersion CV JOIN cmsDocument DD ON CV.versionId = DD.versionId WHERE DD.nodeId = D.nodeId) AS tmp WHERE tmp.RowNum <= @versionsToKeep OR tmp.published = 1 OR tmp.newest = 1))) AS FALMtmp1 WHERE FALMtmp1.published = 0 AND FALMtmp1.newest = 0);";
                    dbComm = db.CreateCommand(db.Connection, sqlDeletePropertyData, new { publishedNodeId = publishedNodeId, versionsToKeep = versionsToKeep });
                    dbComm.CommandTimeout = 100000;
                    iResultCount = dbComm.ExecuteNonQuery();
                    cleanupResult = new CleanupResultModel
                    {
                        Type = "cmsPropertyData",
                        Result = iResultCount
                    };
                    cleanupSummary.Add(cleanupResult);
                    dbComm.Dispose();

                    // Delete versions from cmsDocument
                    string sqlDeleteDocument = @"DELETE FROM cmsDocument WHERE VersionId IN (SELECT FALMtmp1.VersionId FROM (SELECT nodeId, published, documentUser, versionId, text, releaseDate, expireDate, updateDate, templateId, newest FROM cmsDocument WHERE nodeId = @publishedNodeId AND versionID NOT IN (SELECT D.versionId FROM cmsDocument D WHERE D.versionId IN (SELECT versionId FROM (SELECT CV.versionId, published, newest, RANK() OVER(ORDER BY CV.versionDate DESC) RowNum FROM cmsContentVersion CV JOIN cmsDocument DD ON CV.versionId = DD.versionId WHERE DD.nodeId = D.nodeId) AS tmp WHERE tmp.RowNum <= @versionsToKeep OR tmp.published = 1 OR tmp.newest = 1))) AS FALMtmp1 WHERE FALMtmp1.published = 0 AND FALMtmp1.newest = 0);";
                    dbComm = db.CreateCommand(db.Connection, sqlDeleteDocument, new { publishedNodeId = publishedNodeId, versionsToKeep = versionsToKeep });
                    dbComm.CommandTimeout = 100000;
                    iResultCount = dbComm.ExecuteNonQuery();
                    cleanupResult = new CleanupResultModel
                    {
                        Type = "cmsDocument",
                        Result = iResultCount
                    };
                    cleanupSummary.Add(cleanupResult);
                    dbComm.Dispose();

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