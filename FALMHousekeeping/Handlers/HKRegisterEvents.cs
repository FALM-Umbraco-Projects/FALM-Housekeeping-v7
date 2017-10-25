using FALM.Housekeeping.Helpers;
using Umbraco.Core;

namespace FALM.Housekeeping.Handlers
{
    /// <summary>
    /// HkRegisterEvents
    /// </summary>
    public class HkRegisterEvents : ApplicationEventHandler
    {
        /// <summary>
        /// Check and Install Languages on application started
        /// </summary>
        /// <param name="umbracoApplication"></param>
        /// <param name="applicationContext"></param>
        protected override void ApplicationStarted(UmbracoApplicationBase umbracoApplication, ApplicationContext applicationContext)
        {
            
        }
    }
}