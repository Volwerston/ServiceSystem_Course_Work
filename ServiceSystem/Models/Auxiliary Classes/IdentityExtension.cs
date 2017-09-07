using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Security.Principal;
using System.Web;

namespace ServiceSystem.Models.Auxiliary_Classes
{
    public static class IdentityExtension
    {
        public static string GetFirstName(this IIdentity identity)
        {
            var claim = ((ClaimsIdentity)identity).FindFirst("FirstName");
            return (claim != null) ? claim.Value : string.Empty;
        }

        public static string GetLastName(this IIdentity identity)
        {
            var claim = ((ClaimsIdentity)identity).FindFirst("LastName");
            return (claim != null) ? claim.Value : string.Empty;
        }

        public static string GetFatherName(this IIdentity identity)
        {
            var claim = ((ClaimsIdentity)identity).FindFirst("FatherName");
            return (claim != null) ? claim.Value : string.Empty;
        }

        public static string GetOrganisation(this IIdentity identity)
        {
            var claim = ((ClaimsIdentity)identity).FindFirst("Organisation");
            return (claim != null) ? claim.Value : string.Empty;
        }
    }
}