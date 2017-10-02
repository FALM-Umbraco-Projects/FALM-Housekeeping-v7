using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using FALM.Housekeeping.Helpers;
using FALM.Housekeeping.Models;
using Umbraco.Core;
using Umbraco.Core.Logging;
using Umbraco.Core.Services;
using Umbraco.Web.Mvc;
using Umbraco.Web.WebApi;

namespace FALM.Housekeeping.Controllers
{
    /// <summary>
    /// PluginController("FALMHousekeeping")
    /// HkUsersApiController
    /// </summary>
    [PluginController("FALMHousekeeping")]
    public class HkUsersApiController : UmbracoApiController
    {
        /// <summary></summary>
        protected IUserService UserService = ApplicationContext.Current.Services.UserService;

        /// <summary>
        /// Return all user excluding the administrator
        /// </summary>
        /// <returns>List of UsersModel</returns>
        [HttpGet]
        public List<HKUsersModel> GetAllUsers()
        {
            int totalUsers;

            var allUsers = UserService.GetAll(0, int.MaxValue, totalRecords: out totalUsers);

            return (from user in allUsers
                    where user.Id != 0
                    select new HKUsersModel
                    {
                        Selected = false,
                        Id = user.Id.ToString(),
                        Name = user.Name,
                        Username = user.Username,
                        Email = user.Email,
                        UserType = user.UserType.Alias.First().ToString().ToUpper() + user.UserType.Alias.Substring(1)
                    }).ToList();
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
                using (var db = HkDbHelper.ResolveDatabase())
                {
                    foreach (var user in selectedUsersToDelete)
                    {
                        if (user.Selected)
                        {
                            // All documents related to selected user(s) will change to the administrator
                            var sqlDelChangeUmbracoNodeUser = "UPDATE umbracoNode SET nodeUser = 0 WHERE nodeUser IN (" + user.Id + ")";
                            db.Execute(sqlDelChangeUmbracoNodeUser);

                            var sqlDelChangeCmsDocumentUser = "UPDATE cmsDocument SET documentUser = 0 WHERE documentUser IN (" + user.Id + ")";
                            db.Execute(sqlDelChangeCmsDocumentUser);

                            // Delete user from cmsTask
                            var sqlDelLogCmsTask = "DELETE FROM cmsTask WHERE UserId IN (" + user.Id + ") OR parentUserID IN (" + user.Id + ")";
                            db.Execute(sqlDelLogCmsTask);

                            // Delete all selected user(s) references
                            var sqlDelUserUmbracoUser2App = "DELETE FROM umbracoUser2app WHERE [user] IN (" + user.Id + ")";
                            db.Execute(sqlDelUserUmbracoUser2App);

                            // Delete user from umbracoUser2NodeNotify
                            var sqlDelUserUmbracoUser2NodeNotify = "DELETE FROM umbracoUser2NodeNotify WHERE userId IN (" + user.Id + ")";
                            db.Execute(sqlDelUserUmbracoUser2NodeNotify);

                            // Delete user from umbracoUser2NodePermission
                            var sqlDelUserUmbracoUser2NodePermission = "DELETE FROM umbracoUser2NodePermission WHERE userId IN (" + user.Id + ")";
                            db.Execute(sqlDelUserUmbracoUser2NodePermission);

                            // Delete user by Id
                            UserService.Delete(UserService.GetUserById(int.Parse(user.Id)), true);
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