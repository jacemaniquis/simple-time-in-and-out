using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Security;
using CloudERP.Context;

namespace CloudERP.Web.Providers
{
    public class CloudERPRoleProvider : RoleProvider
    {
        public override bool IsUserInRole(string username, string roleName)
        {
            using (var context = new CloudERPDbContext())
            {
                var user = context.HR_UserRoles.FirstOrDefault(p => p.HR_Users.Username == username && p.To == null);
                if (user != null)
                {
                    var role = context.HR_Roles.FirstOrDefault(p => p.Id == user.HR_Roles.Id);
                    if (role != null)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public override string[] GetRolesForUser(string username)
        {
            using (var context = new CloudERPDbContext())
            {
                var user = context.HR_UserRoles.FirstOrDefault(p => p.HR_Users.Username == username && p.To == null);
                if (user != null)
                {
                    return new string[] { user.HR_Roles.RoleName };
                }
            }
            return new string[] { };
        }

        public override void CreateRole(string roleName)
        {
            throw new NotImplementedException();
        }

        public override bool DeleteRole(string roleName, bool throwOnPopulatedRole)
        {
            throw new NotImplementedException();
        }

        public override bool RoleExists(string roleName)
        {
            throw new NotImplementedException();
        }

        public override void AddUsersToRoles(string[] usernames, string[] roleNames)
        {
            throw new NotImplementedException();
        }

        public override void RemoveUsersFromRoles(string[] usernames, string[] roleNames)
        {
            throw new NotImplementedException();
        }

        public override string[] GetUsersInRole(string roleName)
        {
            throw new NotImplementedException();
        }

        public override string[] GetAllRoles()
        {
            throw new NotImplementedException();
        }

        public override string[] FindUsersInRole(string roleName, string usernameToMatch)
        {
            throw new NotImplementedException();
        }

        public override string ApplicationName
        {
            get { throw new NotImplementedException(); }
            set { throw new NotImplementedException(); }
        }
    }
}