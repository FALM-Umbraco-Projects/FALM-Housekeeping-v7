using System.Collections.Generic;

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
    public class HKRecycleBinPageModel
    {
        /// <summary></summary>
        public bool IsBothRecycleBinsCleaned { get; set; }
    }
}