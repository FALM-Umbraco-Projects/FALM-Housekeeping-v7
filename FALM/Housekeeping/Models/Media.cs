// SYSTEM
using System.Collections.Generic;

namespace FALM.Housekeeping.Models
{
    /// <summary>
    /// List of Warnings and List of Media to delete
    /// </summary>
    public class MediaModel
    {
        public List<MediaWarningModel> ListMediaWarnings { get; set; }
        public List<MediaToDeleteModel> ListMediaToDelete { get; set; }
    }

    /// <summary>
    /// Media Warning
    /// </summary>
    public class MediaWarningModel
    {
        public string Entry { get; set; }
        public string Message { get; set; }
    }

    /// <summary>
    /// Media to delete
    /// </summary>
    public class MediaToDeleteModel
    {
        public string Entry { get; set; }
        public string Message { get; set; }
    }

    /// <summary>
    /// Media PID 
    /// </summary>
    public class MediaPIdModel
    {
        public string pId { get; set; }
    }

    /// <summary>
    /// All XML Media
    /// </summary>
    public class AllXMLMediaModel
    {
        public int count { get; set; }
    }

    /// <summary>
    /// All Media
    /// </summary>
    public class AllMediaModel
    {
        public int count { get; set; }
    }
}