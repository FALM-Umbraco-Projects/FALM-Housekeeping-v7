// FALM
using FALM.Housekeeping.Helpers;
using FALM.Housekeeping.Models;
using FALM.Housekeeping.Services;
//SYSTEM
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Web.Mvc;
//UMBRACO
using Umbraco.Core.Logging;
using Umbraco.Web.Mvc;
using Umbraco.Web.WebApi;

namespace FALM.Housekeeping.Controllers
{
    [PluginController("FALMHousekeeping")]
    public class LogsApiController : UmbracoApiController
    {
        protected DBLogsModel DBLogsModel = new DBLogsModel();
        protected List<DBLogModel> ListDBLogs = new List<DBLogModel>();

        protected TraceLogsModel traceLogsModel = new TraceLogsModel();
        protected List<TraceLogDataModel> ListTraceLogs = new List<TraceLogDataModel>();

        protected const string CombinedLogEntryPattern = @"\d{4}-\d{2}-\d{2} \d{2}:\d{2}:\d{2},\d{3}\s(\[(?<PROCESS1>.+)\]|\s) (?<LEVEL>\w+) {1,5}(?<LOGGER>.+?) -(\s\[(?<PROCESS2>[A-Z]\d{1,6}/[A-Z]\d{1,6}/[A-Z]\d{1,6}|Thread \d.?)\]\s|\s)(?<MESSAGE>.+)";
        protected readonly Regex LogEntryRegex = new Regex(CombinedLogEntryPattern, RegexOptions.Singleline | RegexOptions.Compiled);
        
        protected const string ThreadProcessPattern = @"T(?<THREAD>\d+)|D(?<DOMAIN>\d+)|P(?<PROCESS>\d+)|Thread (?<THREADOLD>\d+)";
        protected static readonly Regex ThreadProcessRegex = new Regex(ThreadProcessPattern, RegexOptions.IgnoreCase | RegexOptions.Compiled);

        /// <summary>
        /// Show logs
        /// </summary>
        /// <returns>DBLogsModel</returns>
        [HttpGet]
        public DBLogsModel GetDBLogs()
        {
            ListDBLogs = new List<DBLogModel>();

            try
            {
                string sqlLog    = "SELECT TOP 30 umbracoLog.id AS logId, umbracoLog.userId AS UserId, umbracoUser.userName AS UserName, umbracoUser.userLogin AS UserLogin, umbracoLog.NodeId AS NodeId, umbracoNode.text AS NodeName, umbracoLog.DateStamp AS LogDate, umbracoLog.logHeader AS LogHeader, umbracoLog.logComment AS LogComment ";
                sqlLog          += "FROM umbracoLog INNER JOIN umbracoUser ON umbracoLog.userId = umbracoUser.id LEFT OUTER JOIN umbracoNode ON umbracoLog.NodeId = umbracoNode.id ";
                sqlLog          += "ORDER BY umbracoLog.DateStamp DESC";

                using (var db = DbHelper.ResolveDatabase())
                {
                    ListDBLogs = db.Fetch<DBLogModel>(sqlLog);
                    DBLogsModel.ListDBLogs = ListDBLogs;
                }
            }
            catch (Exception ex)
            {
                LogHelper.Error<Exception>(ex.Message, ex);
                return DBLogsModel;
            }

            return DBLogsModel;
        }

        /// <summary>
        /// Delete filtered logs
        /// The method name MUST START with Post, because is an HttpPost method
        /// </summary>
        /// <param name="selectedUsersToDelete"></param>
        /// <returns></returns>
        [HttpPost]
        public bool PostDeleteDBLogs(List<DBLogModel> LogsToDelete)
        {
            try
            {
                using (var db = DbHelper.ResolveDatabase())
                {
                    string sqlDeleteLog = "DELETE FROM umbracoLog WHERE umbracoLog.id in (";

                    var iCount = 1;

                    foreach (DBLogModel logItem in LogsToDelete)
                    {
                        sqlDeleteLog += logItem.LogId.ToString();
                        sqlDeleteLog += iCount < LogsToDelete.Count ? ", " : string.Empty;

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
        /// Show Trace Logs
        /// </summary>
        /// <returns>TraceLogsModel</returns>
        [HttpGet]
        public TraceLogsModel GetTraceLogs(string filename)
        {
            try
            {
                ListTraceLogs = new List<TraceLogDataModel>();
                TraceLogDataModel traceLogItem = new TraceLogDataModel();

                LogsService logsService = new LogsService();
            
                var currentTraceLogFile = Path.Combine(LogsService.getBaseTraceLogPath(), filename);

                if (File.Exists(currentTraceLogFile))
                {
                    TextReader traceLogEventsReader = File.OpenText(currentTraceLogFile);

                    var logFile = File.ReadAllLines(currentTraceLogFile);
                    List<string> LogList = new List<string>(logFile);

                    foreach (var logEntry in LogList)
                    {
                        var match = LogEntryRegex.Match(logEntry);

                        if (match.Success)
                        {
                            logEntry.Trim();
                            
                            // Get Log Entry Date Format yyyy-MM-dd HH:mm:ss (first 19 chars)
                            string logEntryData = logEntry.Substring(0, 19);

                            var date = DateTime.Parse(logEntryData);

                            string threadProcess = match.Groups["PROCESS2"].Value;

                            if (String.IsNullOrEmpty(threadProcess))
                            {
                                threadProcess = match.Groups["PROCESS1"].Value;
                            }

                            string threadId = null;
                            string processId = null;
                            string domainId = null;

                            if (!String.IsNullOrEmpty(threadProcess))
                            {
                                var procMatches = ThreadProcessRegex.Matches(threadProcess);

                                foreach (Match procMatch in procMatches)
                                {
                                    if (procMatch.Success)
                                    {
                                        var grp = procMatch.Groups["THREAD"];
                                        if (grp.Success)
                                        {
                                            threadId = grp.Value;
                                        }

                                        grp = procMatch.Groups["PROCESS"];
                                        if (grp.Success)
                                        {
                                            processId = grp.Value;
                                        }

                                        grp = procMatch.Groups["DOMAIN"];
                                        if (grp.Success)
                                        {
                                            domainId = grp.Value;
                                        }

                                        if (threadId == null)
                                        {
                                            grp = procMatch.Groups["THREADOLD"];
                                            if (grp.Success)
                                            {
                                                threadId = grp.Value;
                                            }
                                        }
                                    }
                                }
                            }

                            traceLogItem = new TraceLogDataModel();

                            traceLogItem.LogDate    = date;
                            traceLogItem.LogProcess = processId;
                            traceLogItem.LogDomain  = domainId;
                            traceLogItem.LogThread  = threadId;
                            traceLogItem.LogLevel   = match.Groups["LEVEL"].Value;
                            traceLogItem.LogLogger  = match.Groups["LOGGER"].Value;
                            traceLogItem.LogMessage = match.Groups["MESSAGE"].Value;

                            ListTraceLogs.Add(traceLogItem);
                        }
                        else
                        {
                            if (ListTraceLogs.Count > 0)
                            {
                                traceLogItem = new TraceLogDataModel();
                                traceLogItem.LogMessage = string.Concat(logEntry);
                                ListTraceLogs.Add(traceLogItem);
                            }
                        }

                        ListTraceLogs.Sort((x, y) => y.LogDate.CompareTo(x.LogDate));
                        traceLogsModel.ListTraceLogs = ListTraceLogs;
                    }
                }
                else
                {
                    throw new FileNotFoundException("The requested trace log file '" + Path.GetFileName(currentTraceLogFile) + "' could not be found", currentTraceLogFile);
                }
            }
            catch (Exception ex)
            {
                LogHelper.Error<Exception>(ex.Message, ex);
                return traceLogsModel;
            }
            return traceLogsModel;
        }
    }
}