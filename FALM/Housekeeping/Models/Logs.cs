// SYSTEM
using System;
using System.Collections.Generic;

namespace FALM.Housekeeping.Models
{
    public class DBLogsModel
    {
        public List<DBLogModel> ListDBLogs { get; set; }
    }

    public class DBLogModel
    {
        public int      LogId       { get; set; }
        public int      UserId      { get; set; }
        public string   UserName    { get; set; }
        public string   UserLogin   { get; set; }
        public int      NodeId      { get; set; }
        public string   NodeName    { get; set; }
        public DateTime LogDate     { get; set; }
        public string   LogHeader   { get; set; }
        public string   LogComment  { get; set; }
    }

    public class TraceLogFileModel
    {
        public DateTime LogDate         { get; set; }
        public string   LogFileName     { get; set; }
        public string   LogMachineName  { get; set; }
    }

    public class TraceLogsModel
    {
        public List<TraceLogDataModel> ListTraceLogs { get; set; }
    }

    public class TraceLogDataModel
    {
        // Trace Log Data Format
        // 
        // 2017-06-05 09:57:03,761 [P5076/D2/T1] INFO Umbraco.Core.CoreBootManager - Umbraco 7.5.14 application starting on BA-FABRI
        // DateTime - Process/Domain/Thread - Type - Logger - Message

        public int      LogId       { get; set; }
        public DateTime LogDate     { get; set; }
        public string   LogProcess  { get; set; }
        public string   LogDomain   { get; set; }
        public string   LogThread   { get; set; }
        public string   LogLevel    { get; set; }
        public string   LogLogger   { get; set; }
        public string   LogMessage  { get; set; }
    }
}