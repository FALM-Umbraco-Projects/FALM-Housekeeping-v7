using System;
using System.Collections.Generic;

namespace FALM.Housekeeping.Models
{
    /// <summary>
    /// List of Versions
    /// </summary>
    public class HKVersionsModel
    {
        /// <summary></summary>
        public List<CurrentPublishedVersionModel> ListCurrentPublishedVersions { get; set; }
    }

    /// <summary>
    /// Current Published Version
    /// </summary>
    public class CurrentPublishedVersionModel
    {
        /// <summary></summary>
        public int NodeId { get; set; }
        /// <summary></summary>
        public string NodeName { get; set; }
        /// <summary></summary>
        public string NodeUser { get; set; }
        /// <summary></summary>
        public DateTime PublishedDate { get; set; }
        /// <summary></summary>
        public int VersionsCount { get; set; }
    }

    /// <summary>
    /// List of History Versions
    /// </summary>
    public class HistoryVersionsModel
    {
        /// <summary></summary>
        public List<HistoryVersionModel> ListNodeVersions { get; set; }
    }

    /// <summary>
    /// History Version
    /// </summary>
    public class HistoryVersionModel
    {
        /// <summary></summary>
        public string VersionGuid { get; set; }
        /// <summary></summary>
        public DateTime VersionDate { get; set; }
        /// <summary></summary>
        public int Published { get; set; }
        /// <summary></summary>
        public int Newest { get; set; }
    }

    /// <summary>
    /// Cleanup Result
    /// </summary>
    public class CleanupResultModel
    {
        /// <summary></summary>
        public string Type { get; set; }
        /// <summary></summary>
        public int Result { get; set; }
    }
}