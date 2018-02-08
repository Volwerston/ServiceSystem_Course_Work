using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace ServiceSystem.Controllers
{
    public class CategoryController : ApiController
    {
        public HttpResponseMessage Get()
        {
            using (SqlConnection connection = new SqlConnection(System.Web.Configuration.WebConfigurationManager.ConnectionStrings["DBCS"].ConnectionString))
            {
                string cmdString = "SELECT * FROM Categories;";

                SqlDataAdapter da = new SqlDataAdapter(cmdString, connection);

                DataSet ds = new DataSet();

                try
                {
                    da.Fill(ds);

                    Dictionary<string, string> toReturn = new Dictionary<string, string>();

                    foreach(DataRow row in ds.Tables[0].Rows)
                    {
                        string key = row["UA_NAME"].ToString();
                        string value = row["EN_NAME"].ToString();

                        toReturn.Add(key, value);
                    }

                    return Request.CreateResponse(HttpStatusCode.OK, toReturn);
                }
                catch(Exception ex)
                {
                    return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message);
                }
            }
        }
    }
}