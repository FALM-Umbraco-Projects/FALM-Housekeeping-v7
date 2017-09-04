// SYSTEM
using System.Collections.Generic;

namespace FALM.Housekeeping.Models
{
    /// <summary>
    /// List of Warnings and List of Media to delete
    /// </summary>
    public class MediaModel
    {
        /// <summary></summary>
        public List<MediaWarningModel> ListMediaWarnings { get; set; }
        /// <summary></summary>
        public List<MediaToDeleteModel> ListMediaToDelete { get; set; }
    }

    /// <summary>
    /// Media Warning
    /// </summary>
    public class MediaWarningModel
    {
        /// <summary></summary>
        public string Entry { get; set; }
        /// <summary></summary>
        public string Message { get; set; }
    }

    /// <summary>
    /// Media to delete
    /// </summary>
    public class MediaToDeleteModel
    {
        /// <summary></summary>
        public string Entry { get; set; }
        /// <summary></summary>
        public string Message { get; set; }
    }

    /// <summary>
    /// Media PID 
    /// </summary>
    public class MediaPIdModel
    {
        /// <summary></summary>
        public string PId { get; set; }
    }

    /// <summary>
    /// All XML Media
    /// </summary>
    public class AllXMLMediaModel
    {
        /// <summary></summary>
        public int Count { get; set; }
    }

    /// <summary>
    /// All Media
    /// </summary>
    public class AllMediaModel
    {
        /// <summary></summary>
        public int Count { get; set; }
    }
}