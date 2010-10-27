﻿using System;
using System.ComponentModel.Composition;
using TicketDesk.Domain.Models;
using TicketDesk.Domain.Repositories;

namespace TicketDesk.Domain.Services
{
    [Export(typeof(ISecurityService))]
    public class SecurityService : ISecurityService
    {

        /// <summary>
        /// Initializes a new instance of the <see cref="TdSecurityService"/> class.
        /// </summary>
        /// <param name="securityRepository">The security repository.</param>
        /// <param name="userName">Name of the user.</param>
        /// <param name="staffRoleName">Name of the staff role.</param>
        /// <param name="submitterRoleName">Name of the submitter role.</param>
        /// <param name="adminRoleName">Name of the admin role.</param>
        [ImportingConstructor]
        public SecurityService
            (
                [ImportMany(typeof(ISecurityRepository))]ISecurityRepository[] repositories,
                [Import("RuntimeSecurityMode")]Func<string> getSecurityModeMethod,
                [Import("CurrentUserNameMethod")]Func<string> getCurrentUserNameMethod,
                [Import("StaffRoleName")]string staffRoleName,
                [Import("SubmitterRoleName")]string submitterRoleName,
                [Import("AdminRoleName")]string adminRoleName
            )
        {
            Repositories = repositories;
            GetCurrentUserName = getCurrentUserNameMethod;
            GetSecurityMode = getSecurityModeMethod;
            TdStaffRoleName = staffRoleName;
            TdSubmittersRoleName = submitterRoleName;
            TdAdminRoleName = adminRoleName;
        }

        private Func<string> GetCurrentUserName { get; set; }
        private Func<string> GetSecurityMode { get; set; }




        public ISecurityRepository[] Repositories { get; private set; }
        public ISecurityRepository Repository
        {
            get
            {
                ISecurityRepository repos = null;
                var sec = GetSecurityMode();
                foreach (var r in Repositories)
                {
                    foreach (Attribute attribute in r.GetType().GetCustomAttributes(false))
                    {
                        ExportMetadataAttribute emAttribute = attribute as ExportMetadataAttribute;
                        if (emAttribute != null && emAttribute.Name == "SecurityMode" && emAttribute.Value.Equals(sec))
                        {
                            repos = r;
                            break;
                        }
                    }
                    if (repos != null)
                    {
                        break;
                    }
                }
                return repos;
            }
        }


        private string TdStaffRoleName { get; set; }
        private string TdSubmittersRoleName { get; set; }
        private string TdAdminRoleName { get; set; }

        #region ISecurityService Members

        /// <summary>
        /// Gets the submitter users.
        /// </summary>
        /// <returns></returns>
        public UserInfo[] GetTdSubmitterUsers()
        {
            return Repository.GetUsersInRole(TdSubmittersRoleName);
        }

        /// <summary>
        /// Gets the staff users.
        /// </summary>
        /// <returns></returns>
        public UserInfo[] GetTdStaffUsers()
        {
            return Repository.GetUsersInRole(TdStaffRoleName);
        }

        /// <summary>
        /// Gets the admin users.
        /// </summary>
        /// <returns></returns>
        public UserInfo[] GetTdAdminUsers()
        {
            return Repository.GetUsersInRole(TdAdminRoleName);
        }

        /// <summary>
        /// Determines whether user is in the staff role.
        /// </summary>
        /// <returns>
        /// 	<c>true</c> if user in staff role; otherwise, <c>false</c>.
        /// </returns>
        public bool IsTdStaff()
        {
            return IsTdStaff(CurrentUserName);
        }

        /// <summary>
        /// Determines whether user is in the staff role.
        /// </summary>
        /// <param name="userName">Name of the user.</param>
        /// <returns>
        /// 	<c>true</c> if user in staff role; otherwise, <c>false</c>.
        /// </returns>
        public bool IsTdStaff(string userName)
        {
            return Repository.IsUserInRoleName(userName, TdStaffRoleName);
        }

        /// <summary>
        /// Determines whether user is in the submitter role.
        /// </summary>
        /// <returns>
        /// 	<c>true</c> if user in submitter role; otherwise, <c>false</c>.
        /// </returns>
        public bool IsTdSubmitter()
        {
            return IsTdSubmitter(CurrentUserName);
        }

        /// <summary>
        /// Determines whether user is in the submitter role.
        /// </summary>
        /// <param name="userName">Name of the user.</param>
        /// <returns>
        /// 	<c>true</c> if user in submitter role; otherwise, <c>false</c>.
        /// </returns>
        public bool IsTdSubmitter(string userName)
        {
            return Repository.IsUserInRoleName(userName, TdSubmittersRoleName);
        }

        /// <summary>
        /// Determines whether user is in the admin role.
        /// </summary>
        /// <returns>
        /// 	<c>true</c> if user is in admin role; otherwise, <c>false</c>.
        /// </returns>
        public bool IsTdAdmin()
        {
            return IsTdAdmin(CurrentUserName);
        }

        /// <summary>
        /// Determines whether user is in the admin role.
        /// </summary>
        /// <param name="userName">Name of the user.</param>
        /// <returns>
        /// 	<c>true</c> if user is in admin role; otherwise, <c>false</c>.
        /// </returns>
        public bool IsTdAdmin(string userName)
        {
            return Repository.IsUserInRoleName(userName, TdAdminRoleName);
        }

        /// <summary>
        /// Gets the display name of the current user.
        /// </summary>
        /// <returns></returns>
        public string GetUserDisplayName()
        {
            return GetUserDisplayName(CurrentUserName);
        }

        /// <summary>
        /// Gets the formatted name of the current user.
        /// </summary>
        /// <value>The name of the current user.</value>
        public string CurrentUserName
        {
            get
            {
                return Repository.FormatUserName(GetCurrentUserName());
            }
        }


        /// <summary>
        /// Gets the display name of the specified user.
        /// </summary>
        /// <param name="userName">Name of the user.</param>
        /// <returns></returns>
        public string GetUserDisplayName(string userName)
        {
            return Repository.GetUserDisplayName(userName);
        }


        /// <summary>
        /// Determines whether user is in a valid td role.
        /// </summary>
        /// <returns>
        /// 	<c>true</c> if user is in valid td role; otherwise, <c>false</c>.
        /// </returns>
        public bool IsInValidTdUserRole()
        {
            return (IsTdAdmin() || IsTdStaff() || IsTdSubmitter());
        }

        /// <summary>
        /// Determines whether user is in a valid td role.
        /// </summary>
        /// <param name="userName">Name of the user.</param>
        /// <returns>
        /// 	<c>true</c> if user is in valid td role; otherwise, <c>false</c>.
        /// </returns>
        public bool IsInValidTdUserRole(string userName)
        {
            return (IsTdAdmin(userName) || IsTdStaff(userName) || IsTdSubmitter(userName));

        }

        #endregion

        #region ISecurityService Members


        public string GetUserEmailAddress(string userName)
        {
            return Repository.GetUserEmailAddress(userName);
        }

        #endregion
    }
}