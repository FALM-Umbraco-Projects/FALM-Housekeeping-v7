using System.Collections.Generic;
using System.Globalization;
using Umbraco.Core.Models;
using Umbraco.Web.Models;

namespace FALM.Housekeeping.Models
{
    /// <summary>
    /// List of all content of the Cache folders on File System
    /// </summary>
    public class HKCacheContentModel
    {
        /// <summary>Total dimension of Cache directory on File Sytem</summary>
        public long CacheDirectoryTotalDimension { get; set; }
        /// <summary>Content of the Cache folder on File Sytem</summary>
        public List<HKCacheModel> ListCacheContent { get; set; }
    }

    /// <summary>
    /// Cache Model
    /// </summary>
    public class HKCacheModel
    {
        /// <summary>Name of the object</summary>
        public string Entry { get; set; }
        /// <summary>Type of the object (file or folder)</summary>
        public string Type { get; set; }
        /// <summary>Dimension of the object</summary>
        public string Dimension { get; set; }
    }

    /// <summary>
    /// Items to be deleted
    /// </summary>
    public class HKCachePageModel : RenderModel
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="content"></param>
        public HKCachePageModel(IPublishedContent content) : base(content) { }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="content"></param>
        /// <param name="culture"></param>
        public HKCachePageModel(IPublishedContent content, CultureInfo culture) : base(content, culture) { }

        /// <summary></summary>
        public bool IsCacheDirectoryCleaned { get; set; }
    }
}