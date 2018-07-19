using System;
using System.Collections.Generic;

namespace FALM.Housekeeping.Models
{
    /// <summary>
    /// List of DB Log
    /// </summary>
    public class HKDBLogsModel
    {
        /// <summary></summary>
        public List<DBLogModel> ListDBLogs { get; set; }
    }

    /// <summary>
    /// DB Log
    /// </summary>
    public class DBLogModel
    {
        /// <summary></summary>
        public int LogId { get; set; }
        /// <summary></summary>
        public int UserId { get; set; }
        /// <summary></summary>
        public string UserName { get; set; }
        /// <summary></summary>
        public string UserLogin { get; set; }
        /// <summary></summary>
        public int NodeId { get; set; }
        /// <summary></summary>
        public string NodeName { get; set; }
        /// <summary></summary>
        public DateTime LogDate { get; set; }
        /// <summary></summary>
        public string LogHeader { get; set; }
        /// <summary></summary>
        public string LogComment { get; set; }
    }

    /// <summary>
    /// List of Trace Log
    /// </summary>
    public class TraceLogsModel
    {
        /// <summary></summary>
        public List<TraceLogDataModel> ListTraceLogs { get; set; }
    }

    /// <summary>
    /// Trace Log File
    /// </summary>
    public class TraceLogFileModel
    {
        /// <summary></summary>
        public DateTime LogDate { get; set; }
        /// <summary></summary>
        public string LogFileName { get; set; }
        /// <summary></summary>
        public string LogMachineName { get; set; }
    }

    /// <summary>
    /// Trace Log Data
    /// </summary>
    public class TraceLogDataModel
    {
        // Trace Log Data Format
        // 
        // 2017-06-05 09:57:03,761 [P5076/D2/T1] INFO Umbraco.Core.CoreBootManager - Umbraco 7.5.14 application starting on BA-FABRI
        // DateTime - Process/Domain/Thread - Type - Logger - Message

        /// <summary></summary>
        public int LogId { get; set; }
        /// <summary></summary>
        public DateTime LogDate { get; set; }
        /// <summary></summary>
        public string LogProcess { get; set; }
        /// <summary></summary>
        public string LogDomain { get; set; }
        /// <summary></summary>
        public string LogThread { get; set; }
        /// <summary></summary>
        public string LogLevel { get; set; }
        /// <summary></summary>
        public string LogLogger { get; set; }
        /// <summary></summary>
        public string LogMessage { get; set; }
    }
}