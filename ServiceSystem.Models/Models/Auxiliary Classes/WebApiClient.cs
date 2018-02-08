using System;
using System.Net.Http;

namespace ServiceSystem.Models.Auxiliary_Classes
{
    public static class WebApiClient
    {
        public static HttpClient InitializeClient(string baseAddress)
        {
            HttpClient client = new HttpClient();

            client.DefaultRequestHeaders.Clear();
            client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

            client.BaseAddress = new Uri(baseAddress);

            return client;
        }

        public static HttpClient InitializeAuthorizedClient(string baseAddress, string authType, string credentials)
        {
            HttpClient client = InitializeClient(baseAddress);
            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue(authType, credentials);

            return client;
        }
    }
}