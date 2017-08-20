using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Web;

namespace ServiceSystem.Models.Auxiliary_Classes
{
    public static class ApplicationUserManagerExtension
    {
        public static IdentityResult AddSingleLoginAsync(this ApplicationUserManager manager, string mail, UserLoginInfo login)
        {
            using (SqlConnection connection = new SqlConnection(System.Web.Configuration.WebConfigurationManager.ConnectionStrings["DBCS"].ConnectionString))
            {
                string cmdString = "SELECT Id FROM AspNetUsers WHERE Email=@mail";

                SqlCommand cmd = new SqlCommand(cmdString, connection);

                cmd.Parameters.AddWithValue("@mail", mail);

                try
                {
                    connection.Open();

                    string userId = (string)cmd.ExecuteScalar();

                    if (userId != null)
                    {
                        cmdString = "INSERT INTO AspNetUserLogins VALUES(@provider, @key, @id);";

                        cmd = new SqlCommand(cmdString, connection);

                        cmd.Parameters.AddWithValue("@provider", login.LoginProvider);
                        cmd.Parameters.AddWithValue("@key", login.ProviderKey);
                        cmd.Parameters.AddWithValue("@id", userId);

                        cmd.ExecuteNonQuery();

                        return new IdentityResult();
                    }
                    else
                    {
                        return new IdentityResult("Internal server error");
                    }
                }
                catch (Exception ex)
                {
                    return new IdentityResult(ex.Message);
                }
            }
        }
    }
}