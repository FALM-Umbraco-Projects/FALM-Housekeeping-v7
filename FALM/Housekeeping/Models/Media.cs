// SYSTEM
using System.Collections.Generic;

namespace FALM.Housekeeping.Models
{
    public class MediaModel
    {
        public List<MediaWarningModel> ListMediaWarnings { get; set; }
        public List<MediaToDeleteModel> ListMediaToDelete { get; set; }
    }

    public class MediaWarningModel
    {
        public string Entry { get; set; }
        public string Message { get; set; }
    }

    public class MediaToDeleteModel
    {
        public string Entry { get; set; }
        public string Message { get; set; }
    }

    public class MediaPIdModel
    {
        public string pId { get; set; }
    }

    public class AllXMLMediaModel
    {
        public int count { get; set; }
    }

    public class AllMediaModel
    {
        public int count { get; set; }
    }
}