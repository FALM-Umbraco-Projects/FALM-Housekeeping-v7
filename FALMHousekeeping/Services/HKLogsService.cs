using FALM.Housekeeping.Models;
using log4net.Appender;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web.Hosting;
using Umbraco.Core;

namespace FALM.Housekeeping.Services
{
    /// <summary>
    /// HkLogsService
    /// </summary>
    public class HkLogsService
    {
        private static string _baseTraceLogPath = string.Empty;
        private static string _defaultTraceLogPath = "~/App_Data/Logs/";
        private static string _baseTraceLogFilename = string.Empty;
        private static string _defautlTraceLogFileNamePattern = "Umbraco(TraceLog)?";
        private static string _dateFormat = @"(?<date>\d{4}-\d{2}-\d{2})";
        private static string _datePattern;
        private static string _machinePattern;
        private static string _filePattern;
        private readonly Regex _filePatternRegex;

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
                    tlFile.LogDate = logDate.Date;
                    tlFile.LogFileName = traceLogFile;
                    tlFile.LogMachineName = machineName;

                    tlFileList.Add(tlFile);
                }
            }

            var sortedFiles = tlFileList.OrderByDescending(x => x.LogDate);

            return sortedFiles;
        }
    }
}