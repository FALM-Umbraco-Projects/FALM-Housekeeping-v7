// FALM
using FALM.Housekeeping.Helpers;
using FALM.Housekeeping.Models;
//SYSTEM
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
//UMBRACO
using Umbraco.Core;
using Umbraco.Core.Logging;
using Umbraco.Core.Models.Membership;
using Umbraco.Core.Services;
using Umbraco.Web.Mvc;
using Umbraco.Web.WebApi;

namespace FALM.Housekeeping.Controllers
{
    /// <summary>
    /// PluginController("FALMHousekeeping")
    /// UsersApiController
    /// </summary>
    [PluginController("FALMHousekeeping")]
    public class HKUsersApiController : UmbracoApiController
    {
        /// <summary></summary>
        protected IUserService userService = ApplicationContext.Current.Services.UserService;

        /// <summary>
        /// Return all user excluding the administrator
        /// </summary>
        /// <returns>List of UsersModel</returns>
        [HttpGet]
        public List<HKUsersModel> GetAllUsers()
        {
            var totalUsers = 0;
            var userService = ApplicationContext.Services.UserService;
            var allUsers = userService.GetAll(0, int.MaxValue, totalRecords: out totalUsers);

            List<HKUsersModel> allUsersWithoutAdmin = new List<HKUsersModel>();

            foreach (IUser user in allUsers)
            {
                if (user.Id != 0)
                {
                    var userToAdd = new HKUsersModel
                    {
                        Selected = false,
                        Id = user.Id.ToString(),
                        Name = user.Name,
                        Username = user.Username,
                        Email = user.Email,
                        UserType = user.UserType.Alias.First().ToString().ToUpper() + user.UserType.Alias.Substring(1)
                    };

                    allUsersWithoutAdmin.Add(userToAdd);
                }
            }

            return allUsersWithoutAdmin;
        }

        /// <summary>
        /// Delete all selected users
        /// </summary>
        /// <param name="selectedUsersToDelete"></param>
        /// <returns>bool</returns>
        [HttpPost]
        public bool PostDeleteSelectedUsers(List<HKUsersModel> selectedUsersToDelete)
        {
            try
            {
                using (var db = HKDbHelper.ResolveDatabase())
                {
                    foreach (HKUsersModel user in selectedUsersToDelete)
                    {
                        if (user.Selected)
                        {
                            // All documents related to selected user(s) will change to the administrator
                            string sqlDelChangeUmbracoNodeUser = "UPDATE umbracoNode SET nodeUser = 0 WHERE nodeUser IN (" + user.Id + ")";
                            db.Execute(sqlDelChangeUmbracoNodeUser);

                            string sqlDelChangeCmsDocumentUser = "UPDATE cmsDocument SET documentUser = 0 WHERE documentUser IN (" + user.Id + ")";
                            db.Execute(sqlDelChangeCmsDocumentUser);

                            // Delete user from cmsTask
                            string sqlDelLogCmsTask = "DELETE FROM cmsTask WHERE UserId IN (" + user.Id + ") OR parentUserID IN (" + user.Id + ")";
                            db.Execute(sqlDelLogCmsTask);

                            // Delete user from umbracoLog
                            //string sqlDelLogUmbracoLog = "DELETE FROM umbracoLog WHERE userId IN (" + user.Id + ")";
                            //db.Execute(sqlDelLogUmbracoLog);

                            // Delete all selected user(s) references
                            string sqlDelUserUmbracoUser2app = "DELETE FROM umbracoUser2app WHERE [user] IN (" + user.Id + ")";
                            db.Execute(sqlDelUserUmbracoUser2app);

                            // Delete user from umbracoUser2NodeNotify
                            string sqlDelUserUmbracoUser2NodeNotify = "DELETE FROM umbracoUser2NodeNotify WHERE userId IN (" + user.Id + ")";
                            db.Execute(sqlDelUserUmbracoUser2NodeNotify);

                            // Delete user from umbracoUser2NodePermission
                            string sqlDelUserUmbracoUser2NodePermission = "DELETE FROM umbracoUser2NodePermission WHERE userId IN (" + user.Id + ")";
                            db.Execute(sqlDelUserUmbracoUser2NodePermission);

                            // Delete user by Id
                            userService.Delete(userService.GetUserById(int.Parse(user.Id)), true);
                        }
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                LogHelper.Error<Exception>(ex.Message, ex);
                return false;
            }
        }
    }
}