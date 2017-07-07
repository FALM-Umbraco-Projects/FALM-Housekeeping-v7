// SYSTEM
using System;
using System.Collections.Generic;

namespace FALM.Housekeeping.Models
{
    /// <summary>
    /// List of Versions
    /// </summary>
    public class VersionsModel
    {
        public List<CurrentPublishedVersionModel> ListCurrentPublishedVersions { get; set; }
    }

    /// <summary>
    /// Current Published Version
    /// </summary>
    public class CurrentPublishedVersionModel
    {
        public int          NodeId                      { get; set; }
        public string       NodeName                    { get; set; }
        public string       NodeUser                    { get; set; }
        public DateTime     PublishedDate               { get; set; }
        public int          VersionsCount               { get; set; }
    }

    /// <summary>
    /// List of History Versions
    /// </summary>
    public class HistoryVersionsModel
    {
        public List<HistoryVersionModel> ListNodeVersions { get; set; }
    }

    /// <summary>
    /// History Version
    /// </summary>
    public class HistoryVersionModel
    {
        public string       VersionGUID                 { get; set; }
        public DateTime     VersionDate                 { get; set; }
        public int          Published                   { get; set; }
        public int          Newest                      { get; set; }
    }

    /// <summary>
    /// Cleanup Result
    /// </summary>
    public class CleanupResultModel
    {
        public string       Type                        { get; set; }
        public int          Result                      { get; set; }
    }
}