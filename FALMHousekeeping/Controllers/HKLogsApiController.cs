using FALM.Housekeeping.Helpers;
using FALM.Housekeeping.Models;
using FALM.Housekeeping.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web.Mvc;
using Umbraco.Core.Logging;
using Umbraco.Core.Models;
using Umbraco.Web.Mvc;
using Umbraco.Web.WebApi;

namespace FALM.Housekeeping.Controllers
{
    /// <summary>
    /// PluginController("FALMHousekeeping")
    /// HkLogsApiController
    /// </summary>
    [PluginController("FALMHousekeeping")]
    public class HkLogsApiController : UmbracoApiController
    {
        /// <summary></summary>
        protected HkLogsService logService;
        /// <summary></summary>
        protected HKDBLogsModel DbLogsModel = new HKDBLogsModel();
        /// <summary></summary>
        protected List<DBLogModel> ListDbLogs = new List<DBLogModel>();
        /// <summary></summary>
        protected HKTraceLogsModel TraceLogsModel = new HKTraceLogsModel();
        /// <summary></summary>
        protected List<TraceLogDataModel> ListTraceLogs = new List<TraceLogDataModel>();
        /// <summary></summary>
        protected const string CombinedLogEntryPattern = @"\d{4}-\d{2}-\d{2} \d{2}:\d{2}:\d{2},\d{3}\s(\[(?<PROCESS1>.+)\]|\s) (?<LEVEL>\w+) {1,5}(?<LOGGER>.+?) -(\s\[(?<PROCESS2>[A-Z]\d{1,6}/[A-Z]\d{1,6}/[A-Z]\d{1,6}|Thread \d.?)\]\s|\s)(?<MESSAGE>.+)";
        /// <summary></summary>
        protected readonly Regex LogEntryRegex = new Regex(CombinedLogEntryPattern, RegexOptions.Singleline | RegexOptions.Compiled);
        /// <summary></summary>
        protected const string ThreadProcessPattern = @"T(?<THREAD>\d+)|D(?<DOMAIN>\d+)|P(?<PROCESS>\d+)|Thread (?<THREADOLD>\d+)";
        /// <summary></summary>
        protected static readonly Regex ThreadProcessRegex = new Regex(ThreadProcessPattern, RegexOptions.IgnoreCase | RegexOptions.Compiled);

        /// <summary></summary>
        public HkLogsApiController()
        {
            this.logService = new HkLogsService(UmbracoContext.Application.DatabaseContext.Database, UmbracoContext.Application.ApplicationCache.RuntimeCache);
        }

        /// <summary>
        /// Show all DB logs
        /// </summary>
        /// <returns>DBLogsModel</returns>
        [HttpGet]
        public HKDBLogsModel GetDbLogs(string search = "", int itemsPerPage = 10, int pageNumber = 1)
        {
            try
            {
                var request = new HKDBLogsModel()
                {
                    Search = search,
                    ItemsPerPage = itemsPerPage,
                    CurrentPage = pageNumber,
                };

                var paged = logService.GetDBLog(request);

                DbLogsModel.CurrentPage     = int.Parse(paged.CurrentPage.ToString());
                DbLogsModel.ItemsPerPage    = int.Parse(paged.ItemsPerPage.ToString());
                DbLogsModel.ListDBLogs      = paged.Items;
                DbLogsModel.TotalItems      = int.Parse(paged.TotalItems.ToString());
                DbLogsModel.TotalPages      = int.Parse(paged.TotalPages.ToString());

                return DbLogsModel;
            }
            catch (Exception ex)
            {
                LogHelper.Error<Exception>(ex.Message, ex);
                return null;
            }
        }

        /// <summary>
        /// Delete filtered DB logs
        /// </summary>
        /// <param name="logsToDelete"></param>
        /// <returns>bool</returns>
        [HttpPost]
        public bool PostDeleteDbLogs(List<DBLogModel> logsToDelete)
        {
            try
            {
                using (var db = HkDbHelper.ResolveDatabase())
                {
                    var sqlDeleteLog = "DELETE FROM umbracoLog WHERE umbracoLog.id in (";

                    var iCount = 1;

                    foreach (var logItem in logsToDelete)
                    {
                        sqlDeleteLog += logItem.Id.ToString();
                        sqlDeleteLog += iCount < logsToDelete.Count ? ", " : string.Empty;

                        iCount++;
                    }

                    sqlDeleteLog += ");";

                    db.Execute(sqlDeleteLog);
                }

                return true;
            }
            catch (Exception ex)
            {
                LogHelper.Error<Exception>(ex.Message, ex);
                return false;
            }
        }

        /// <summary>
        /// Delete DB logs older than 6 months
        /// </summary>
        /// <returns>bool</returns>
        [HttpPost]
        public bool PostDeleteDbLogsBeforeMonths()
        {
            try
            {
                using (var db = HkDbHelper.ResolveDatabase())
                {
                    const string sqlDeleteLog = "DELETE FROM umbracoLog WHERE Datestamp < DATEADD(MONTH, -6, GETDATE())";

                    db.Execute(sqlDeleteLog);
                }

                return true;
            }
            catch (Exception ex)
            {
                LogHelper.Error<Exception>(ex.Message, ex);
                return false;
            }
        }

        /// <summary>
        /// Show selected Trace Log
        /// </summary>
        /// <param name="filename"></param>
        /// <param name="itemsPerPage"></param>
        /// <param name="pageNumber"></param>
        /// <returns>TraceLogsModel</returns>
        [HttpGet]
        public HKTraceLogsModel GetTraceLogs(string filename, int itemsPerPage = 10, int pageNumber = 1)
        {
            try
            {
                var request = new HKTraceLogsModel()
                {
                    FileName            = filename,
                    Search              = string.Empty,
                    ItemsPerPage        = itemsPerPage,
                    CurrentPage         = pageNumber
                };

                request.ListTraceLogs = ((request.ListAllTraceLogs == null) || request.ListAllTraceLogs.Count == 0) ? logService.GetTraceLog(request) : request.ListAllTraceLogs;

                var paged = CreatePagination(request);

                TraceLogsModel.ListAllTraceLogs = request.ListTraceLogs;
                TraceLogsModel.ListTraceLogs    = (List<TraceLogDataModel>)paged.Items;
                TraceLogsModel.Search           = request.Search;
                TraceLogsModel.FileName         = request.FileName;
                TraceLogsModel.CurrentPage      = int.Parse(paged.PageNumber.ToString());
                TraceLogsModel.ItemsPerPage     = int.Parse(paged.PageSize.ToString());
                TraceLogsModel.TotalItems       = int.Parse(paged.TotalItems.ToString());
                TraceLogsModel.TotalPages       = int.Parse(paged.TotalPages.ToString());

                return TraceLogsModel;
            }
            catch (Exception ex)
            {
                LogHelper.Error<Exception>(ex.Message, ex);
                return null;
            }
        }

        /// <summary>
        /// Filter trace logs
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        public HKTraceLogsModel PostFilterTraceLogs(HKTraceLogsModel request)
        {
            try
            {
                TraceLogsModel = new HKTraceLogsModel();
                
                request.ListTraceLogs = ((request.ListAllFilteredTraceLogs == null) || (!request.Search.ToLower().Equals(request.PreviousSearch.ToLower()))) ? logService.GetTraceLog(request) : request.ListAllFilteredTraceLogs;

                var paged = CreatePagination(request);

                TraceLogsModel.ListAllTraceLogs         = request.ListAllTraceLogs;
                TraceLogsModel.ListAllFilteredTraceLogs = request.ListTraceLogs;
                TraceLogsModel.ListTraceLogs            = (List<TraceLogDataModel>)paged.Items;
                TraceLogsModel.Search                   = request.Search.ToLower();
                TraceLogsModel.PreviousSearch           = ((request.ListAllFilteredTraceLogs == null) || !request.Search.ToLower().Equals(request.PreviousSearch.ToLower())) ? request.Search.ToLower() : request.PreviousSearch.ToLower();
                TraceLogsModel.FileName                 = request.FileName;
                TraceLogsModel.CurrentPage              = int.Parse(paged.PageNumber.ToString());
                TraceLogsModel.ItemsPerPage             = int.Parse(paged.PageSize.ToString());
                TraceLogsModel.TotalItems               = int.Parse(paged.TotalItems.ToString());
                TraceLogsModel.TotalPages               = int.Parse(paged.TotalPages.ToString());

                return TraceLogsModel;
            }
            catch (Exception ex)
            {
                LogHelper.Error<Exception>(ex.Message, ex);
                return null;
            }
        }

        /// <summary>
        /// Create Trace Logs Pagination
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        private PagedResult<TraceLogDataModel> CreatePagination(HKTraceLogsModel request)
        {
            try
            {
                var startAt = (request.CurrentPage - 1) * request.ItemsPerPage;

                var PagedLogs = new PagedResult<TraceLogDataModel>(request.ListTraceLogs.Count, request.CurrentPage, request.ItemsPerPage)
                {
                    Items = request.ListTraceLogs.Skip(startAt).Take(request.ItemsPerPage).ToList<TraceLogDataModel>()
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