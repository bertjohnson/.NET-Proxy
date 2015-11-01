using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

/// <summary>
/// ASP.NET and JavaScript proxies for accessing external content.
/// </summary>
namespace netProxy
{
    /// <summary>
    /// Handle proxying on default page load.
    /// </summary>
    public partial class Default : System.Web.UI.Page
    {
        /// <summary>
        /// Parse GET and POST objects, proxy request, handle caching, and return response.
        /// </summary>
        protected void Page_Load(object sender, EventArgs e)
        {
            // Check for a buffer size override.
            int bufferSize = 1048576;
            if (ConfigurationManager.AppSettings["NetProxyBufferSize"] != null)
                int.TryParse(ConfigurationManager.AppSettings["NetProxyBufferSize"], out bufferSize);

            // Check if a cache duration has been specified.
            int cacheDuration = 0;
            if (ConfigurationManager.AppSettings["NetProxyCacheDuration"] != null)
                int.TryParse(ConfigurationManager.AppSettings["NetProxyCacheDuration"], out cacheDuration);

            #region Optional Filtering
            // Check if supported protocols have been specified.
            string[] supportedProtocols = new string[] { "http", "https" };
            if (ConfigurationManager.AppSettings["NetProxyProtocols"] != null)
                supportedProtocols = ConfigurationManager.AppSettings["NetProxyProtocols"].ToString().ToUpper().Split(',');

            // Check if supported referers have been specified.
            string[] referers = new string[] {};
            if (ConfigurationManager.AppSettings["NetProxyReferers"] != null)
            {
                if (!string.IsNullOrEmpty(ConfigurationManager.AppSettings["NetProxyReferers"]) && ConfigurationManager.AppSettings["NetProxyReferers"] != "*")
                    referers = ConfigurationManager.AppSettings["NetProxyReferers"].ToString().ToUpper().Split(',');
            }

            // Check if domains referers have been specified.
            string[] domains = new string[] {};
            if (ConfigurationManager.AppSettings["NetProxyDomains"] != null)
                domains = ConfigurationManager.AppSettings["NetProxyDomains"].ToString().ToUpper().Split(',');
            #endregion Optional Filtering

            if (Request.QueryString.Count > 0)
            {
                #region Optional Filtering
                Uri remoteUrl = new Uri(Server.UrlDecode(Request.QueryString.ToString()));

                // Check if the remote protocol is supported.
                if (supportedProtocols.Length > 0)
                {
                    string remoteUrlProtocol = "";
                    int remoteUrlFirstColon = remoteUrl.ToString().IndexOf(":");
                    if (remoteUrlFirstColon > -1)
                        remoteUrlProtocol = remoteUrl.ToString().Substring(0, remoteUrlFirstColon).ToUpper();

                    bool matchedProtocol = false;
                    foreach (string protocol in supportedProtocols)
                    {
                        if (protocol == remoteUrlProtocol)
                            matchedProtocol = true;
                    }

                    if (!matchedProtocol)
                    {
                        // Return an error related to the protocol requested.
                        throw new ArgumentException("Protocol \"" + remoteUrlProtocol + "\" not supported.");
                    }
                }

                // Check if the referer is supported.
                if (referers.Length > 0)
                {
                    string pageReferer = "";
                    if (HttpContext.Current.Request.UrlReferrer != null)
                        pageReferer = HttpContext.Current.Request.UrlReferrer.Host.ToUpper();

                    if (string.IsNullOrEmpty(pageReferer))
                    {
                        // Return an error related to the referer provided.
                        throw new ArgumentException("A referer must be specified.");
                    }

                    bool matchedReferer = false;
                    foreach (string referer in referers)
                    {
                        if (referer == pageReferer)
                            matchedReferer = true;
                    }

                    if (!matchedReferer)
                    {
                        // Return an error related to the referer provided.
                        throw new ArgumentException("Referer \"" + pageReferer + "\" not supported.");
                    }
                }

                // Check if the destination domain is supported.
                if (domains.Length > 0)
                {
                    string destinationDomain = remoteUrl.Host.ToUpper();

                    bool matchedDomain = false;
                    foreach (string domain in domains)
                    {
                        if (domain.StartsWith("*") && destinationDomain.EndsWith(domain.Substring(1)))
                            matchedDomain = true;
                        else if (domain == destinationDomain)
                            matchedDomain = true;
                    }

                    if (!matchedDomain)
                    {
                        // Return an error related to the destination domain requested.
                        throw new ArgumentException("Domain \"" + destinationDomain + "\" not supported.");
                    }
                }
                #endregion Optional Filtering

                // If this is a GET request and the result exists in cache, return it as-is.
                string cacheKey = "NetProxy-" + remoteUrl;
                if (Request.HttpMethod.ToUpper() == "GET" && Cache[cacheKey] != null)
                {
                    Response.BinaryWrite((byte[])Cache[cacheKey]);
                    return;
                }

                // Prepare the request object.
                HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(remoteUrl);
                webRequest.Method = Request.HttpMethod;
                webRequest.ContentType = Request.ContentType;

                // Read and pass POST data.
                if (Request.HttpMethod.ToUpper() == "POST")
                {
                    int postDataLength = (int)Request.InputStream.Length;
                    webRequest.ContentLength = Request.ContentLength;
                    byte[] postData = new byte[postDataLength];

                    Request.InputStream.Read(postData, 0, postDataLength);

                    Stream postStream = webRequest.GetRequestStream();
                    postStream.Write(postData, 0, postDataLength);
                    postStream.Flush();
                }

                // Cycle through and return output.
                HttpWebResponse webResponse = (HttpWebResponse)webRequest.GetResponse();
                using (Stream fileStream = webResponse.GetResponseStream())
                {
                    // If caching, prepare the buffer.
                    MemoryStream cacheBuffer = null;
                    if (cacheDuration > 0)
                        cacheBuffer = new MemoryStream(bufferSize);

                    // Loop through response, cache result and emit output to client.
                    Byte[] buffer = new byte[bufferSize];
                    int bytesRead = 1;
                    while (bytesRead > 0)
                    {
                        bytesRead = fileStream.Read(buffer, 0, bufferSize);
                        if (bytesRead == bufferSize)
                        {
                            Response.BinaryWrite(buffer);

                            if (cacheDuration > 0)
                                cacheBuffer.Write(buffer, 0, bytesRead);
                        }
                        else if (bytesRead > 0)
                        {
                            byte[] endBuffer = new byte[bytesRead];
                            Array.Copy(buffer, endBuffer, bytesRead);
                            Response.BinaryWrite(endBuffer);

                            if (cacheDuration > 0)
                                cacheBuffer.Write(buffer, 0, bytesRead);
                        }
                    }

                    // Cache the results.
                    if (cacheDuration > 0)
                    {
                        Cache.Add(cacheKey, cacheBuffer.ToArray(), null, System.Web.Caching.Cache.NoAbsoluteExpiration, new TimeSpan(0, 0, cacheDuration), System.Web.Caching.CacheItemPriority.Normal, null);
                        cacheBuffer.Dispose();
                    }
                }
            }
            else
            {
                // Return a blank response for malformed queries.
                throw new ArgumentException("A URL must be provided on the query string.");
            }
        }
    }
}