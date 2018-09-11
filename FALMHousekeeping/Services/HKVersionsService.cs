using FALM.Housekeeping.Models;
using log4net.Appender;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web.Hosting;
using Umbraco.Core;
using Umbraco.Core.Cache;
using Umbraco.Core.Logging;
using Umbraco.Core.Models;
using Umbraco.Core.Persistence;

namespace FALM.Housekeeping.Services
{
    /// <summary>
    /// HkVersionsService
    /// </summary>
    public class HkVersionsService
    {
        /// <summary></summary>
        private readonly UmbracoDatabase db;
        /// <summary></summary>
        private readonly IRuntimeCacheProvider cache;

        /// <summary>
        /// Logs Service
        /// </summary>
        public HkVersionsService() { }

        /// <summary>
        /// Instantiates the versions service with the Umbraco database and a caching provider
        /// </summary>
        /// <param name="db">The Umbraco database</param>
        /// <param name="cache">A caching provider</param>
        public HkVersionsService(UmbracoDatabase db, IRuntimeCacheProvider cache)
        {
            if (db == null)
                throw new ArgumentNullException("db");

            this.db = db;
            this.cache = cache;
        }

        /// <summary>
        /// Get Versions
        /// </summary>
        /// <returns>Paged Versions Model</returns>
        public Page<CurrentPublishedVersionModel> GetVersions(HKVersionsModel request)
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

            return db.Page<CurrentPublishedVersionModel>(request.CurrentPage, request.ItemsPerPage, sqlVersions);
        }
    }
}