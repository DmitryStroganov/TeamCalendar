using System;
using System.Collections.Generic;
using System.DirectoryServices.AccountManagement;
using System.Web;
using System.Web.Hosting;
using TeamCalendar.Data;

namespace TeamCalendar.Common
{
    public sealed class AdCommon : IDisposable, ITeamCalendarUserDataResolver
    {
        private readonly PrincipalContext AdPrincipalContext;

        public AdCommon(string adDomain, string adContainer)
        {
            AdPrincipalContext = new PrincipalContext(ContextType.Domain, adDomain, adContainer);
        }

        public void Dispose()
        {
            AdPrincipalContext.Dispose();
            GC.SuppressFinalize(this);
        }

        public string ResolveEmailByUsername(string userName)
        {
            var byIdentity = UserPrincipal.FindByIdentity(AdPrincipalContext, IdentityType.Name, userName);
            return byIdentity?.EmailAddress;
        }

        public string ResolveAccountDisplayNameByEmail(string email)
        {
            var user = new UserPrincipal(AdPrincipalContext)
            {
                EmailAddress = email
            };
            var pS = new PrincipalSearcher(user);
            var result = pS.FindOne();
            return result?.Name;
        }

        public string ResolveUserNameByID(string userID)
        {
            var byIdentity = UserPrincipal.FindByIdentity(AdPrincipalContext, userID);
            return byIdentity?.DisplayName;
        }

        public UserPrincipal GetUserPrincipalByID(string userID)
        {
            return UserPrincipal.FindByIdentity(AdPrincipalContext, userID);
        }

        internal AdUserData GetUserData(string userID, params string[] adProps)
        {
            var adUserData = new AdUserData();
            UserPrincipal userPrincipal = null;

            if (HttpContext.Current != null)
            {
                using (HostingEnvironment.Impersonate())
                {
                    userPrincipal = GetUserPrincipalByID(userID);
                }
            }
            else
            {
                userPrincipal = GetUserPrincipalByID(userID);
            }

            if (userPrincipal == null)
            {
                return null;
            }

            adUserData.FirstName = userPrincipal.GivenName;
            adUserData.LastName = userPrincipal.Surname;
            adUserData.Email = userPrincipal.EmailAddress;
            adUserData.Company = userPrincipal.GetCompany();

            if (adProps == null)
            {
                adUserData.JobTitle = userPrincipal.GetProperty("title");
                adUserData.Department = userPrincipal.GetProperty("department");
                adUserData.Office = userPrincipal.GetProperty("physicalDeliveryOfficeName");
            }
            else
            {
                var list = new List<KeyValuePair<string, string>>();
                foreach (var str in adProps)
                {
                    list.Add(new KeyValuePair<string, string>(str, userPrincipal.GetProperty(str)));
                }
                adUserData.AdProps = list.ToArray();
            }

            return adUserData;
        }

        internal bool ValidateAccount(string userName, string userPassword)
        {
            return AdPrincipalContext.ValidateCredentials(userName, userPassword);
        }
    }
}