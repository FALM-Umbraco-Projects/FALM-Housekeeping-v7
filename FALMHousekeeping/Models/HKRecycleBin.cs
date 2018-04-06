using System.Collections.Generic;
using System.Globalization;
using Umbraco.Core.Models;
using Umbraco.Web.Models;

namespace FALM.Housekeeping.Models
{
    /// <summary>
    /// UsersModel
    /// </summary>
    public class HKRecycleBinModel
    {
        /// <summary></summary>
        public List<ItemsInRecycleBinsModel> ListItemsInRecycleBins { get; set; }
    }

    /// <summary>
    /// Items to be deleted
    /// </summary>
    public class ItemsInRecycleBinsModel
    {
        /// <summary></summary>
        public string Type { get; set; }
        /// <summary></summary>
        public string Value { get; set; }
    }

    /// <summary>
    /// Items to be deleted
    /// </summary>
    public class HKRecycleBinPageModel : RenderModel
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="content"></param>
        public HKRecycleBinPageModel(IPublishedContent content) : base(content) { }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="content"></param>
        /// <param name="culture"></param>
        public HKRecycleBinPageModel(IPublishedContent content, CultureInfo culture) : base(content, culture) { }

        /// <summary></summary>
        public bool IsBothRecycleBinsCleaned { get; set; }
    }
}