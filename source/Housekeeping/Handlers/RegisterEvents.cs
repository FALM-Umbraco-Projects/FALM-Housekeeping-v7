// UMBRACO
using Umbraco.Core;

namespace FALM.Housekeeping
{
    public class HKRegisterEvents : ApplicationEventHandler
    {
        protected override void ApplicationStarted(UmbracoApplicationBase umbracoApplication, ApplicationContext applicationContext)
        {
            Helper.HKLanguageInstaller.CheckAndInstallLanguageActions();
        }
    }
}