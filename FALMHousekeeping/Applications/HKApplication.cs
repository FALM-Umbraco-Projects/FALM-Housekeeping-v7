// FALM
using FALM.Housekeeping.Constants;
// UMBRACO
using umbraco.businesslogic;
using umbraco.interfaces;

namespace FALM.Housekeeping
{
    /// <summary>
    /// Application(HKConstants.Application.Alias, HKConstants.Application.Name, HKConstants.Application.Icon, 50)
    /// HKApplication
    /// </summary>
    [Application(HKConstants.Application.Alias, HKConstants.Application.Name, HKConstants.Application.Icon, 50)]
    public class HKApplication : IApplication
    {

    }
}