// FALM
using FALM.Housekeeping.Services;
// SYSTEM
using System;
using System.Globalization;
using System.Net.Http.Formatting;
using System.Web;
// UMBRACO
using Umbraco.Core;
using Umbraco.Web;
using Umbraco.Web.Models.Trees;
using Umbraco.Web.Mvc;
using Umbraco.Web.Trees;

namespace FALM.Housekeeping
{
    /// <summary>
    /// Tree(HKConstants.Application.Alias, HKConstants.Tree.Alias, HKConstants.Tree.Title)
    /// PluginController(HKConstants.Controller.Alias)
    /// HKTreeController
    /// </summary>
    [Tree(HKConstants.Application.Alias, HKConstants.Tree.Alias, HKConstants.Tree.Title)]
    [PluginController(HKConstants.Controller.Alias)]
    public class HKTreeController : TreeController
    {
        /// <summary>
        /// GetTreeNodes(string id, FormDataCollection queryStrings)
        /// This method create the Base Tree of FALM custom section
        /// </summary>
        /// <param name="id"></param>
        /// <param name="queryStrings"></param>
        /// <returns>tree</returns>
        protected override TreeNodeCollection GetTreeNodes(string id, FormDataCollection queryStrings)
        {
            var tree = new TreeNodeCollection();
            var textService = ApplicationContext.Services.TextService;

            LogsService logsService = new LogsService();
            string currentMachineName = Environment.MachineName;
            int iCount = 1;

            // check if we're rendering the root node's children
            if (id == global::Umbraco.Core.Constants.System.Root.ToInvariantString())
            {
                tree = new TreeNodeCollection
            {
                CreateTreeNode("logs", "-1", queryStrings, textService.Localize("FALM/LogsManager.TreeSection", CultureInfo.CurrentCulture), "icon-list", true, FormDataCollectionExtensions.GetValue<string>(queryStrings, "application")),
                CreateTreeNode("media", "-1", queryStrings, textService.Localize("FALM/MediaManager.TreeSection", CultureInfo.CurrentCulture), "icon-umb-media", true, FormDataCollectionExtensions.GetValue<string>(queryStrings, "application")),
                CreateTreeNode("users", "-1", queryStrings, textService.Localize("FALM/UsersManager.TreeSection", CultureInfo.CurrentCulture), "icon-users", true, FormDataCollectionExtensions.GetValue<string>(queryStrings, "application")),
                CreateTreeNode("versions", "-1", queryStrings, textService.Localize("FALM/VersionsManager.TreeSection", CultureInfo.CurrentCulture), "icon-books", true, FormDataCollectionExtensions.GetValue<string>(queryStrings, "application"))
            };

                return tree;
            }
            else
            {
                switch (id)
                {
                    case "logs": // check if we're rendering Logs node's children
                        tree = new TreeNodeCollection {
                            CreateTreeNode("logs-dbmanager", id, queryStrings, textService.Localize("FALM/LogsManager.TreeActionManagerDB", CultureInfo.CurrentCulture), "icon-diagnostics color-green", false),
                            CreateTreeNode("logs-tlmanager", id, queryStrings, textService.Localize("FALM/LogsManager.TreeActionManagerTL", CultureInfo.CurrentCulture), "icon-folder", true, FormDataCollectionExtensions.GetValue<string>(queryStrings, "application"))
                        };
                        break;

                    case "logs-tlmanager": // check if we're rendering Logs node's children
                        tree = new TreeNodeCollection();

                        // Create TraceLog tree
                        foreach (var logFile in logsService.getTraceLogFiles())
                        {
                            string title = iCount == 1 ? textService.Localize("FALM/LogsManager.TreeActionManagerTL.Today", CultureInfo.CurrentCulture) : logFile.LogDate.ToString("yyyy-MM-dd");

                            if (logFile.LogMachineName != null && !logFile.LogMachineName.InvariantEquals(currentMachineName))
                            {
                                title += " (" + logFile.LogMachineName + ")";
                            }

                            string path = HttpUtility.UrlEncode(System.IO.Path.GetFileName(logFile.LogFileName));
                            string traceLogRoutePath = FormDataCollectionExtensions.GetValue<string>(queryStrings, "application") + "/housekeeping/edittl/" + path;

                            tree.Add(CreateTreeNode(path, id, queryStrings, title, "icon-calendar-alt color-green", false, traceLogRoutePath));

                            iCount++;
                        };
                        break;

                    case "media": //check if we're rendering Media node's children
                        var mediaPath = FormDataCollectionExtensions.GetValue<string>(queryStrings, "application") + "/" + this.TreeAlias + "/media-cleanup";
                        tree = new TreeNodeCollection {
                            CreateTreeNode("media-cleanup", id, queryStrings, textService.Localize("FALM/MediaManager.TreeActionCleanup", CultureInfo.CurrentCulture), "icon-delete color-red", false)
                        };
                        break;

                    case "users": //check if we're rendering Users node's children
                        tree = new TreeNodeCollection {
                            CreateTreeNode("users-cleanup", id, queryStrings, textService.Localize("FALM/UsersManager.TreeActionCleanup", CultureInfo.CurrentCulture), "icon-delete color-red", false)
                        };
                        break;

                    case "versions": //check if we're rendering Versions node's children
                        tree = new TreeNodeCollection {
                            CreateTreeNode("versions-manager", id, queryStrings, textService.Localize("FALM/VersionsManager.TreeActionManager", CultureInfo.CurrentCulture), "icon-diagnostics color-green", false),
                        };
                        break;
                }

                return tree;
            }

            //this tree doesn't suport rendering more than 1 level
            throw new NotSupportedException();
        }

        /// <summary>
        /// GetMenuForNode(string id, FormDataCollection queryStrings)
        /// This method create the actions on a single menu item (by pressing "..." symbol)
        /// </summary>
        /// <param name="id"></param>
        /// <param name="queryStrings"></param>
        /// <returns>menu</returns>
        protected override MenuItemCollection GetMenuForNode(string id, FormDataCollection queryStrings)
        {
            return null;
        }
    }
}