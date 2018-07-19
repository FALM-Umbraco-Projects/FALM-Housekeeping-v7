using System.Collections.Generic;

namespace FALM.Housekeeping.Models
{
    ///// <summary>
    ///// List of all content of the Temp folder on File System
    ///// </summary>
    //public class HKTempContentModel
    //{
    //    /// <summary>Total dimension of Cache directory on File Sytem</summary>
    //    public long TempDirectoryTotalDimension { get; set; }
    //    /// <summary>Content of the Cache folder on File Sytem</summary>
    //    public List<HKTempModel> ListTempContent { get; set; }
    //}

    /// <summary>
    /// Temp Model
    /// </summary>
    public class HKTempModel
    {
        /// <summary></summary>
        public bool Selected { get; set; }
        /// <summary>Name of the object</summary>
        public string Entry { get; set; }
        /// <summary>Type of the object (file or folder)</summary>
        public string Type { get; set; }
        /// <summary>Dimension of the object</summary>
        public string Dimension { get; set; }
    }
}