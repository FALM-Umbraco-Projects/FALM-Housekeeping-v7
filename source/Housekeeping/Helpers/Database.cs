// UMBRACO
using Umbraco.Core;
using Umbraco.Core.Persistence;

namespace FALM.Housekeeping.Helpers
{
    public class DbHelper
    {
        public static Database Database;

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