using FALM.Housekeeping.Helpers;
using FALM.Housekeeping.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Umbraco.Core;
using Umbraco.Core.Logging;
using Umbraco.Core.Models.Membership;
using Umbraco.Core.Persistence;
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

            var allUsers = UserService.GetAll(0, int.MaxValue, totalRecords: out int totalUsers);
            string userGroups = string.Empty;

            return (from user in allUsers
                    where user.Id != 0
                    select new HKUsersModel
                    {
                        Selected = false,
                        Id = user.Id.ToString(),
                        Name = user.Name,
                        Username = user.Username,
                        Email = user.Email,
                        UserType = GetAllUserGroups(user.Groups)
                    }).ToList();
        }

        /// <summary>
        /// Retrieve all groups of a user
        /// </summary>
        /// <param name="userGroups"></param>
        /// <returns></returns>
        protected string GetAllUserGroups(IEnumerable<IReadOnlyUserGroup> userGroups)
        {
            string allUserGroupsAlias = string.Empty;

            foreach (var g in userGroups)
            {
                allUserGroupsAlias += g.Name;

                if (g != userGroups.Last())
                {
                    allUserGroupsAlias += ", ";
                }
            }

            return allUserGroupsAlias;
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
                    var _dbContext = ApplicationContext.Current.DatabaseContext;
                    var _dbHelper = new DatabaseSchemaHelper(_dbContext.Database, LoggerResolver.Current.Logger, _dbContext.SqlSyntax);

                    foreach (var user in selectedUsersToDelete)
                    {
                        if (user.Selected)
                        {
                            // All documents related to selected user(s) will change to the administrator
                            var sqlDelChangeUmbracoNodeUser = "UPDATE umbracoNode SET nodeUser = 0 WHERE nodeUser IN (" + user.Id + ");";
                            db.Execute(sqlDelChangeUmbracoNodeUser);

                            var sqlDelChangeCmsDocumentUser = "UPDATE cmsDocument SET documentUser = 0 WHERE documentUser IN (" + user.Id + ");";
                            db.Execute(sqlDelChangeCmsDocumentUser);

                            // Delete all selected user(s) from cmsTask (for Umbraco all verions)
                            if (_dbHelper.TableExist("cmsTask"))
                            {
                                //var sqlDelLogCmsTask = "DELETE FROM cmsTask WHERE UserId IN (" + user.Id + ") OR parentUserID IN (" + user.Id + ");";
                                //db.Execute(sqlDelLogCmsTask);
                                _dbContext.Database.Execute("DELETE FROM cmsTask WHERE UserId IN (@0) OR parentUserID IN (@0);", user.Id);
                            }

                            // Delete all selected user(s) from umbracoUser2app (for Umbraco < v7.7)
                            if (_dbHelper.TableExist("umbracoUser2app"))
                            {
                                //var sqlDelUserUmbracoUser2App = "DELETE FROM umbracoUser2app WHERE [user] IN (" + user.Id + ");";
                                //db.Execute(sqlDelUserUmbracoUser2App);
                                _dbContext.Database.Execute("DELETE FROM umbracoUser2app WHERE [user] IN (@0);", user.Id);
                            }

                            // Delete all selected user(s) from umbracoUser2NodeNotify (for Umbraco all verions)
                            if (_dbHelper.TableExist("umbracoUser2NodeNotify"))
                            {
                                //var sqlDelUserUmbracoUser2NodeNotify = "DELETE FROM umbracoUser2NodeNotify WHERE userId IN (" + user.Id + ");";
                                //db.Execute(sqlDelUserUmbracoUser2NodeNotify);
                                _dbContext.Database.Execute("DELETE FROM umbracoUser2NodeNotify WHERE [userId] IN (@0);", user.Id);
                            }

                            // Delete all selected user(s) from umbracoUser2NodePermission (for Umbraco < v7.7)
                            if (_dbHelper.TableExist("umbracoUser2NodePermission"))
                            {
                                //var sqlDelUserUmbracoUser2NodePermission = "DELETE FROM umbracoUser2NodePermission WHERE userId IN (" + user.Id + ");";
                                //db.Execute(sqlDelUserUmbracoUser2NodePermission);
                                _dbContext.Database.Execute("DELETE FROM umbracoUser2NodePermission WHERE [userId] IN (@0);", user.Id);
                            }

                            // Delete all selected user(s) from umbracoUser2UserGroup (for Umbraco v7.7+)
                            if (_dbHelper.TableExist("umbracoUser2UserGroup"))
                            {
                                //var sqlDelUserumbracoUser2UserGroup = "DELETE FROM umbracoUser2UserGroup WHERE userId IN (" + user.Id + ");";
                                //db.Execute(sqlDelUserumbracoUser2UserGroup);
                                _dbContext.Database.Execute("DELETE FROM umbracoUser2UserGroup WHERE [userId] IN (@0);", user.Id);
                            }

                            // Delete all selected user(s) by Id
                            UserService.Delete(UserService.GetUserById(int.Parse(user.Id)), true);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LogHelper.Error<Exception>(ex.Message, ex);

                return false;
            }

            return true;
        }
    }
}