// UMBRACO
using Umbraco.Core;

namespace FALM.Housekeeping.Handlers
{
    /// <summary>
    /// HKRegisterEvents
    /// </summary>
    public class HKRegisterEvents : ApplicationEventHandler
    {
        /// <summary>
        /// Check and Install Languages on application started
        /// </summary>
        /// <param name="umbracoApplication"></param>
        /// <param name="applicationContext"></param>
        protected override void ApplicationStarted(UmbracoApplicationBase umbracoApplication, ApplicationContext applicationContext)
        {
            Helper.HKLanguageInstaller.CheckAndInstallLanguageActions();
        }
    }
}