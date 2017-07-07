// UMBRACO
using Umbraco.Core;
using Umbraco.Core.Persistence;

namespace FALM.Housekeeping.Helpers
{
    /// <summary>
    /// DbHelper
    /// </summary>
    public class DbHelper
    {
        public static Database Database;

        /// <summary>
        /// Resolve Database connection
        /// </summary>
        /// <returns>Database</returns>
        internal static Database ResolveDatabase()
        {
            if (Database == null)
            {
                return ApplicationContext.Current.DatabaseContext.Database;
            }

            return Database;
        }
    }
}