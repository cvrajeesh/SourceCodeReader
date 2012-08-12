using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.Caching;
using System.Web;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;

namespace SourceCodeReader.Web.Infrastructure
{
    /// <summary>
    /// Reference: http://www.strathweb.com/2012/05/output-caching-in-asp-net-web-api/
    /// </summary>
    public class WebApiOutputCacheAttribute : ActionFilterAttribute
    {
        // Cache length in seconds
        private int timespan;

        // Client cache length in seconds
        private int clientTimeSpan;

        // Cache key
        private string cachekey;

        // cache repository
        private static readonly ObjectCache WebApiCache = MemoryCache.Default;

        public WebApiOutputCacheAttribute(int timespan, int clientTimeSpan)
        {
            this.timespan = timespan;
            this.clientTimeSpan = clientTimeSpan;
        }


        private bool IsCacheable(HttpActionContext context)
        {
            if (this.timespan > 0 && this.clientTimeSpan > 0)
            {
                if (context.Request.Method == HttpMethod.Get) return true;
            }
            else
            {
                throw new InvalidOperationException("Wrong Arguments");
            }
            return false;
        }

        private CacheControlHeaderValue SetClientCache()
        {
            var cachecontrol = new CacheControlHeaderValue();
            cachecontrol.MaxAge = TimeSpan.FromSeconds(this.clientTimeSpan);
            cachecontrol.MustRevalidate = true;
            return cachecontrol;
        }

        public override void OnActionExecuting(HttpActionContext context)
        {
            if (context != null)
            {
                if (this.IsCacheable(context))
                {
                    this.cachekey = string.Join(":", new string[] { context.Request.RequestUri.AbsolutePath, context.Request.Headers.Accept.FirstOrDefault().ToString() });
                    if (WebApiCache.Contains(this.cachekey))
                    {
                        var val = (string)WebApiCache.Get(this.cachekey);
                        if (val != null)
                        {
                            context.Response = context.Request.CreateResponse();
                            context.Response.Content = new StringContent(val);
                            var contenttype = (MediaTypeHeaderValue)WebApiCache.Get(this.cachekey + ":response-ct");
                            if (contenttype == null)
                                contenttype = new MediaTypeHeaderValue(this.cachekey.Split(':')[1]);
                            context.Response.Content.Headers.ContentType = contenttype;
                            context.Response.Headers.CacheControl = this.SetClientCache();
                            return;
                        }
                    }
                }
            }
            else
            {
                throw new ArgumentNullException("context");
            }
        }

        public override void OnActionExecuted(HttpActionExecutedContext context)
        {
            if (!(WebApiCache.Contains(this.cachekey)))
            {
                var body = context.Response.Content.ReadAsStringAsync().Result;
                WebApiCache.Add(this.cachekey, body, DateTime.Now.AddSeconds(this.timespan));
                WebApiCache.Add(this.cachekey + ":response-ct", context.Response.Content.Headers.ContentType, DateTime.Now.AddSeconds(this.timespan));
            }

            if (this.IsCacheable(context.ActionContext))
            {
                context.ActionContext.Response.Headers.CacheControl = this.SetClientCache();
            }
        }
    }
}