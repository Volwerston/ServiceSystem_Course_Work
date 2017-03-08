using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Runtime.Caching;
using System.Threading;
using System.Threading.Tasks;
using System.Web;

namespace ServiceSystem.Facebook
{
    public class FacebookBackChannelHandler : HttpClientHandler
    {
        protected override async Task<HttpResponseMessage>
            SendAsync(HttpRequestMessage request,
                      CancellationToken cancellationToken)
        {
            if (!request.RequestUri.AbsolutePath.Contains("/oauth"))
            {

                try
                {
                    string token = request.RequestUri.AbsoluteUri.Split('?').Where(x => x.Length > 12 && x.Substring(0, 12) == "access_token").Single();

                    token = token.Substring(13);
                    ObjectCache cache = MemoryCache.Default;

                    CacheItemPolicy policy = new CacheItemPolicy();
                    policy.AbsoluteExpiration =
                        DateTimeOffset.Now.AddHours(1);

                    cache.Set("access_token", token, policy);

                    request.RequestUri = new Uri(
                        request.RequestUri.AbsoluteUri.Replace("?access_token", "&access_token"));
                }
                catch(Exception ex)
                {

                }
            }
            return await base.SendAsync(request, cancellationToken);
        }
    }
}