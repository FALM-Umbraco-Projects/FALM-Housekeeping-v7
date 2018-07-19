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
    /// HkLogsService
    /// </summary>
    public class HkLogsService
    {
        /// <summary></summary>
        private readonly UmbracoDatabase db;
        /// <summary></summary>
        private readonly IRuntimeCacheProvider cache;
        /// <summary></summary>
        private static string _baseTraceLogPath = string.Empty;
        /// <summary></summary>
        private static string _defaultTraceLogPath = "~/App_Data/Logs/";
        /// <summary></summary>
        private static string _baseTraceLogFilename = string.Empty;
        /// <summary></summary>
        private static string _defautlTraceLogFileNamePattern = "Umbraco(TraceLog)?";
        /// <summary></summary>
        private static string _dateFormat = @"(?<date>\d{4}-\d{2}-\d{2})";
        /// <summary></summary>
        private static string _datePattern;
        /// <summary></summary>
        private static string _machinePattern;
        /// <summary></summary>
        private static string _filePattern;
        /// <summary></summary>
        private readonly Regex _filePatternRegex;
        /// <summary></summary>
        protected const string CombinedLogEntryPattern = @"\d{4}-\d{2}-\d{2} \d{2}:\d{2}:\d{2},\d{3}\s(\[(?<PROCESS1>.+)\]|\s) (?<LEVEL>\w+) {1,5}(?<LOGGER>.+?) -(\s\[(?<PROCESS2>[A-Z]\d{1,6}/[A-Z]\d{1,6}/[A-Z]\d{1,6}|Thread \d.?)\]\s|\s)(?<MESSAGE>.+)";
        /// <summary></summary>
        protected readonly Regex LogEntryRegex = new Regex(CombinedLogEntryPattern, RegexOptions.Singleline | RegexOptions.Compiled);
        /// <summary></summary>
        protected const string ThreadProcessPattern = @"T(?<THREAD>\d+)|D(?<DOMAIN>\d+)|P(?<PROCESS>\d+)|Thread (?<THREADOLD>\d+)";
        /// <summary></summary>
        protected static readonly Regex ThreadProcessRegex = new Regex(ThreadProcessPattern, RegexOptions.IgnoreCase | RegexOptions.Compiled);

        /// <summary>
        /// Logs Service
        /// </summary>
        public HkLogsService()
        {
            _datePattern = @"((" + _dateFormat + ".txt)$|(txt." + _dateFormat + ")$)";
            _machinePattern = @"(?<machine>((?!" + _dateFormat + @").*))";
            _filePattern = @"(?<path>.*)" +
                          @"(?<file>" + GetBaseTraceLogFileName() + @")\." +
                          @"(" + _machinePattern + @"\.)?" +
                          @"(" + _datePattern + "|txt$)";

            _filePatternRegex = new Regex(_filePattern, RegexOptions.IgnoreCase | RegexOptions.ExplicitCapture);
        }

        /// <summary>
        /// Instantiates the log service with the Umbraco database and a caching provider
        /// </summary>
        /// <param name="db">The Umbraco database</param>
        /// <param name="cache">A caching provider</param>
        public HkLogsService(UmbracoDatabase db, IRuntimeCacheProvider cache)
        {
            if (db == null)
                throw new ArgumentNullException("db");

            this.db = db;
            this.cache = cache;
        }

        /// <summary>
        /// Get DB Logs
        /// </summary>
        /// <returns>IEnumerable of TraceLogFileModel</returns>
        public Page<DBLogModel> GetDBLog(HKDBLogsModel request)
        {
            var sqlLog = "SELECT umbracoLog.id AS Id, umbracoLog.userId AS UserId, umbracoUser.userName AS UserName, umbracoUser.userLogin AS UserLogin, umbracoLog.NodeId AS NodeId, umbracoNode.text AS NodeName, umbracoLog.DateStamp AS Date, umbracoLog.logHeader AS Header, umbracoLog.logComment AS Comment ";
            
            sqlLog += "FROM umbracoLog INNER JOIN umbracoUser ON umbracoLog.userId = umbracoUser.id LEFT OUTER JOIN umbracoNode ON umbracoLog.NodeId = umbracoNode.id ";

            if (!String.IsNullOrEmpty(request.Search))
            {
                //WHERE CONTAINS((co1, col2, col3, col4), 'term1')
                sqlLog += "WHERE (umbracoLog.logHeader LIKE '%" + request.Search.ToLower() + "%') OR ";
                sqlLog += "(umbracoUser.userName LIKE '%" + request.Search.ToLower() + "%') OR ";
                sqlLog += "(umbracoNode.text LIKE '%" + request.Search.ToLower() + "%') OR ";
                sqlLog += "(umbracoLog.logComment LIKE '%" + request.Search.ToLower() + "%') ";
            }

            sqlLog += "ORDER BY umbracoLog.DateStamp DESC";

            return db.Page<DBLogModel>(request.CurrentPage, request.ItemsPerPage, sqlLog);
        }

        /// <summary>
        /// Get Trace Logs
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public List<TraceLogDataModel> GetTraceLog(HKTraceLogsModel request) 
        {
            try
            {
                var ListTraceLogs = new List<TraceLogDataModel>();

                if (String.IsNullOrEmpty(request.Search))
                {
                    ListTraceLogs = GetTraceLogFromFile(request.FileName);    
                }
                else
                {
                    ListTraceLogs = FilterTraceLog(request.ListAllTraceLogs, request.Search);
                }

                return ListTraceLogs;
            }
            catch (Exception ex)
            {
                LogHelper.Error<Exception>(ex.Message, ex);
                return null;
            }
        }

        /// <summary>
        /// Get all trace logs from files
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns>List of Logs</returns>
        protected List<TraceLogDataModel> GetTraceLogFromFile(string fileName)
        {
            var ListTraceLogs = new List<TraceLogDataModel>();

            var currentTraceLogFile = Path.Combine(HkLogsService.GetBaseTraceLogPath(), fileName);

            if (System.IO.File.Exists(currentTraceLogFile))
            {
                var logFile = System.IO.File.ReadAllLines(currentTraceLogFile);

                var logList = new List<string>(logFile);

                foreach (var logEntry in logList)
                {
                    var match = LogEntryRegex.Match(logEntry);

                    TraceLogDataModel traceLogItem;

                    if (match.Success)
                    {
                        // Get Log Entry Date Format yyyy-MM-dd HH:mm:ss (first 19 chars)
                        var logEntryData = logEntry.Trim().Substring(0, 19);

                        var date = DateTime.Parse(logEntryData);

                        var threadProcess = match.Groups["PROCESS2"].Value;

                        if (string.IsNullOrEmpty(threadProcess))
                        {
                            threadProcess = match.Groups["PROCESS1"].Value;
                        }

                        string threadId = null;
                        string processId = null;
                        string domainId = null;

                        if (!string.IsNullOrEmpty(threadProcess))
                        {
                            var procMatches = ThreadProcessRegex.Matches(threadProcess);

                            foreach (Match procMatch in procMatches)
                            {
                                if (!procMatch.Success) continue;
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

                                if (threadId != null) continue;
                                grp = procMatch.Groups["THREADOLD"];
                                if (grp.Success)
                                {
                                    threadId = grp.Value;
                                }
                            }
                        }

                        traceLogItem = new TraceLogDataModel
                        {
                            Date = date,
                            Process = processId,
                            Domain = domainId,
                            Thread = threadId,
                            Level = match.Groups["LEVEL"].Value,
                            Logger = match.Groups["LOGGER"].Value,
                            Message = match.Groups["MESSAGE"].Value
                        };

                        ListTraceLogs.Add(traceLogItem);
                    }
                    else
                    {
                        if (ListTraceLogs.Count > 0)
                        {
                            traceLogItem = new TraceLogDataModel
                            {
                                Message = string.Concat(logEntry)
                            };
                            ListTraceLogs.Add(traceLogItem);
                        }
                    }

                    ListTraceLogs.Sort((x, y) => y.Date.CompareTo(x.Date));
                }

                return ListTraceLogs;
            }
            else
            {
                var fileNotFoundException = new FileNotFoundException("The requested trace log file '" + Path.GetFileName(currentTraceLogFile) + "' could not be found", currentTraceLogFile);
                LogHelper.Error<Exception>(fileNotFoundException.Message, fileNotFoundException);
                return null;
            }
        }

        /// <summary>
        /// Get all trace logs from files
        /// </summary>
        /// <param name="allTracelogs"></param>
        /// <param name="searchString"></param>
        /// <returns>List of Logs</returns>
        private List<TraceLogDataModel> FilterTraceLog(List<TraceLogDataModel> allTracelogs, string searchString)
        {
            var ListTraceLogs = new List<TraceLogDataModel>();

            var filteredTL = allTracelogs.Where(tl => tl.Date != null && tl.Date.ToShortDateString().Contains(searchString) || 
                                                      !String.IsNullOrEmpty(tl.Level) && tl.Level.ToLower().Contains(searchString.ToLower()) || 
                                                      !String.IsNullOrEmpty(tl.Logger) && tl.Logger.ToLower().Contains(searchString.ToLower()) || 
                                                      !String.IsNullOrEmpty(tl.Message) && tl.Message.ToLower().Contains(searchString.ToLower()));
            
            if (filteredTL.ToList().Count > 0) ListTraceLogs.AddRange(filteredTL);

            ListTraceLogs.Sort((df, dl) => dl.Date.CompareTo(df.Date));

            return ListTraceLogs;
        }

        /// <summary>
        /// Get Base Trace Log Path
        /// </summary>
        /// <returns>string</returns>
        public static string GetBaseTraceLogPath()
        {
            var loggerRepo = log4net.LogManager.GetRepository();
            var appender = loggerRepo?.GetAppenders().FirstOrDefault(a => "rollingFile".InvariantEquals(a.Name)) as RollingFileAppender;

            if (appender != null)
            {
                return Path.GetDirectoryName(appender.File);
            }
            return HostingEnvironment.MapPath(_defaultTraceLogPath);
        }

        /// <summary>
        /// Get Base Trace Log File Name
        /// </summary>
        /// <returns>string</returns>
        public static string GetBaseTraceLogFileName()
        {
            var logRepository = log4net.LogManager.GetRepository();

            var logAppender = logRepository?.GetAppenders().FirstOrDefault(a => "rollingFile".InvariantEquals(a.Name)) as RollingFileAppender;

            var fileName = Path.GetFileName(logAppender?.File);
            if (fileName != null) return fileName.Split('.')[0];

            return _defautlTraceLogFileNamePattern;
        }

        /// <summary>
        /// Get Trace Log Files
        /// </summary>
        /// <returns>IEnumerable of TraceLogFileModel</returns>
        public IEnumerable<TraceLogFileModel> GetTraceLogFiles()
        {
            _baseTraceLogPath = GetBaseTraceLogPath();
            _baseTraceLogFilename = GetBaseTraceLogFileName();

            var traceLogFiles = Directory.GetFiles(_baseTraceLogPath, _baseTraceLogFilename + ".*");

            if (traceLogFiles == null)
            {
                throw new ArgumentNullException("allTLFiles");
            };

            List<TraceLogFileModel> tlFileList = new List<TraceLogFileModel>();

            foreach (var traceLogFile in traceLogFiles)
            {
                Match fileMatch = _filePatternRegex.Match(traceLogFile);

                if (fileMatch.Success)
                {
                    var logDate = DateTime.Now;
                    var date = fileMatch.Groups["date"].Value;

                    if (!string.IsNullOrWhiteSpace(date) && !DateTime.TryParse(date, out logDate))
                    {
                        continue;
                    }

                    var machineGroup = fileMatch.Groups["machine"].Value;
                    var machineName = string.IsNullOrWhiteSpace(machineGroup) ? null : machineGroup;

                    var tlFile = new TraceLogFileModel();
                    tlFile.Date = logDate.Date;
                    tlFile.FileName = traceLogFile;
                    tlFile.MachineName = machineName;

                    tlFileList.Add(tlFile);
                }
            }

            var sortedFiles = tlFileList.OrderByDescending(x => x.Date);

            return sortedFiles;
        }
    }
}