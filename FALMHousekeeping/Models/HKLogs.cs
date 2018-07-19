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
        /// <summary></summary>
        public string Search { get; set; }
        /// <summary></summary>
        public int CurrentPage { get; set; }
        /// <summary></summary>
        public int ItemsPerPage { get; set; }
        /// <summary></summary>
        public int TotalItems { get; set; }
        /// <summary></summary>
        public int TotalPages { get; set; }
    }

    /// <summary>
    /// DB Log
    /// </summary>
    public class DBLogModel
    {
        /// <summary></summary>
        public int Id { get; set; }
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
        public DateTime Date { get; set; }
        /// <summary></summary>
        public string Header { get; set; }
        /// <summary></summary>
        public string Comment { get; set; }
    }

    /// <summary>
    /// List of Trace Log
    /// </summary>
    public class HKTraceLogsModel
    {
        /// <summary></summary>
        public string FileName { get; set; }
        /// <summary></summary>
        public List<TraceLogDataModel> ListAllTraceLogs { get; set; }
        /// <summary></summary>
        public List<TraceLogDataModel> ListAllFilteredTraceLogs { get; set; }
        /// <summary></summary>
        public List<TraceLogDataModel> ListTraceLogs { get; set; }
        /// <summary></summary>
        public string Search { get; set; }
        /// <summary></summary>
        public string PreviousSearch { get; set; }
        /// <summary></summary>
        public int ItemsPerPage { get; set; }
        /// <summary></summary>
        public int CurrentPage { get; set; }
        /// <summary></summary>
        public int TotalItems { get; set; }
        /// <summary></summary>
        public int TotalPages { get; set; }
    }

    /// <summary>
    /// Trace Log File
    /// </summary>
    public class TraceLogFileModel
    {
        /// <summary></summary>
        public DateTime Date { get; set; }
        /// <summary></summary>
        public string FileName { get; set; }
        /// <summary></summary>
        public string MachineName { get; set; }
    }

    /// <summary>
    /// Trace Log Data
    /// </summary>
    public class TraceLogDataModel
    {
        // Trace Log Data Format
        // 
        // 2017-06-05 09:57:03,761 [P5076/D2/T1] INFO Umbraco.Core.CoreBootManager - Umbraco 7.5.14 application starting on BA-XXXXX
        // DateTime - Process/Domain/Thread - Type - Logger - Message

        /// <summary></summary>
        public int Id { get; set; }
        /// <summary></summary>
        public DateTime Date { get; set; }
        /// <summary></summary>
        public string Process { get; set; }
        /// <summary></summary>
        public string Domain { get; set; }
        /// <summary></summary>
        public string Thread { get; set; }
        /// <summary></summary>
        public string Level { get; set; }
        /// <summary></summary>
        public string Logger { get; set; }
        /// <summary></summary>
        public string Message { get; set; }
    }
}