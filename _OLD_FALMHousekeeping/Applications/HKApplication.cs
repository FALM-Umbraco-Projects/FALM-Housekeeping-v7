using FALM.Housekeeping.Constants;
using umbraco.businesslogic;
using umbraco.interfaces;

namespace FALM.Housekeeping.Applications
{
    /// <summary>
    /// Application(HkConstants.Application.Alias, HkConstants.Application.Name, HkConstants.Application.Icon, 50)
    /// HkApplication
    /// </summary>
    [Application(HkConstants.Application.Alias, HkConstants.Application.Name, HkConstants.Application.Icon, 50)]
    public class HkApplication : IApplication
    { }
}