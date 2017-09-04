// FALM
using FALM.Housekeeping.Models;
// LOG4NET
using log4net.Appender;
// SYSTEM
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web.Hosting;
// UMBRACO
using Umbraco.Core;

namespace FALM.Housekeeping.Services
{
    /// <summary>
    /// LogsService
    /// </summary>
    public class LogsService
    {
        private static string _baseTraceLogPath = string.Empty;
        private static string _defaultTraceLogPath = "~/App_Data/Logs/";
        private static string _baseTraceLogFilename = string.Empty;
        private static string _defautlTraceLogFileNamePattern = "Umbraco(TraceLog)?";
        private static string dateFormat = @"(?<date>\d{4}-\d{2}-\d{2})";
        private static string datePattern; // matches date pattern in log file name
        private static string machinePattern;
        private static string filePattern; // matches valid log file name      
        private readonly Regex filePatternRegex;

        /// <summary>
        /// Logs Service
        /// </summary>
        public LogsService()
        {
            datePattern = @"((" + dateFormat + ".txt)$|(txt." + dateFormat + ")$)";
            machinePattern = @"(?<machine>((?!" + dateFormat + @").*))";
            filePattern = @"(?<path>.*)" +
                          @"(?<file>" + GetBaseTraceLogFileName() + @")\." +
                          @"(" + machinePattern + @"\.)?" +
                          @"(" + datePattern + "|txt$)";

            filePatternRegex = new Regex(filePattern, RegexOptions.IgnoreCase | RegexOptions.ExplicitCapture);
        }

        /// <summary>
        /// Get Base Trace Log Path
        /// </summary>
        /// <returns>string</returns>
        public static string GetBaseTraceLogPath()
        {
            var loggerRepo = log4net.LogManager.GetRepository();
            if (loggerRepo != null)
            {
                var appender = loggerRepo.GetAppenders().FirstOrDefault(a => "rollingFile".InvariantEquals(a.Name)) as RollingFileAppender;

                if (appender != null)
                {
                    return Path.GetDirectoryName(appender.File);
                }
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

            if (logRepository != null)
            {
                var logAppender = logRepository.GetAppenders().FirstOrDefault(a => "rollingFile".InvariantEquals(a.Name)) as RollingFileAppender;

                if (logAppender != null)
                {
                    var _fileName = Path.GetFileName(logAppender.File);
                    return _fileName.Split('.')[0];
                }
            }

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

            var TraceLogFiles = Directory.GetFiles(_baseTraceLogPath, _baseTraceLogFilename + ".*");

            if (TraceLogFiles == null)
            {
                throw new ArgumentNullException("allTLFiles");
            };

            List<TraceLogFileModel> tlFileList = new List<TraceLogFileModel>();

            foreach (var TraceLogFile in TraceLogFiles)
            {
                string machineName = null;

                Match fileMatch = filePatternRegex.Match(TraceLogFile);

                if (fileMatch.Success)
                {
                    var logDate = DateTime.Now;
                    var date = fileMatch.Groups["date"].Value;

                    if (!string.IsNullOrWhiteSpace(date) && !DateTime.TryParse(date, out logDate))
                    {
                        continue;
                    }

                    var machineGroup = fileMatch.Groups["machine"].Value;
                    machineName = string.IsNullOrWhiteSpace(machineGroup) ? null : machineGroup;

                    TraceLogFileModel tlFile = new TraceLogFileModel();
                    tlFile.LogDate = logDate.Date;
                    tlFile.LogFileName = TraceLogFile;
                    tlFile.LogMachineName = machineName;

                    tlFileList.Add(tlFile);
                }
            }

            var sortedFiles = tlFileList.OrderByDescending(x => x.LogDate);

            return sortedFiles;
        }
    }
}