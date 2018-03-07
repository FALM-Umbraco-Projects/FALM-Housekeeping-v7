using FALM.Housekeeping.Controllers;
using System;
using System.Linq;
using System.Web.Mvc;
using System.Web.Routing;
using System.Web.Security;
using Umbraco.Core;
using Umbraco.Core.Models;
using Umbraco.Web;
using Umbraco.Web.Models;
using Umbraco.Web.Mvc;

namespace FALM.Housekeeping.Handlers
{
    /// <summary>
    /// HkRegisterEvents
    /// </summary>
    public class HkRegisterEvents : ApplicationEventHandler
    {
        /// <summary>
        /// Create the FALM custom routes on application started
        /// </summary>
        /// <param name="umbracoApplication"></param>
        /// <param name="applicationContext"></param>
        protected override void ApplicationStarting(UmbracoApplicationBase umbracoApplication, ApplicationContext applicationContext) { }
    }
}