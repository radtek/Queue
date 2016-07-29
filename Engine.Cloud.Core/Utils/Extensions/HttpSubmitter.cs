using System;
using System.Net.Cache;
using System.Text;
using System.Collections.Specialized;
using System.Net;
using System.IO;
using System.Web;

namespace Engine.Cloud.Core.Utils.Extensions
{
    public enum HttpSubmitterType
    {
        Get,
        Post,
        Put,
        Delete
    }

    public enum TimeOutMilisenconds
    {
        Default = 5000 * 60,
        Minimum = 1000 * 60,
        Maximum = 10000 * 60
    }

    public class HttpSubmitter
    {
        public string Url { get; set; }
        public NameValueCollection PostItems { get; set; }
        public HttpSubmitterType SubmitterType { get; set; }
        public WebHeaderCollection HeaderColletion { get; set; }
        public string CustomContentType { get; set; }
        public Encoding ResultEncoding { get; set; }
        public string CustomData { get; set; }
        public string CustomAccept { get; set; }
        public int? TimeOutMilliseconds { get; set; }
        public bool AllowAutoRedirect { get; set; }
        public ICredentials Credential { get; set; }
        private HttpWebRequest request;
        private CookieCollection cookieCollection;

        public HttpSubmitter()
        {
            this.CustomAccept = "application/xml";
            this.AllowAutoRedirect = true;
            request = null;
        }

        public HttpSubmitter(string url, NameValueCollection values, HttpSubmitterType type)
            : this()
        {
            Url = url;
            PostItems = new NameValueCollection(values);
            SubmitterType = type;
            this.AllowAutoRedirect = true;
        }

        public HttpSubmitter(string url, NameValueCollection values, HttpSubmitterType type, int? timeOutMilliseconds)
            : this()
        {
            Url = url;
            PostItems = new NameValueCollection(values);
            SubmitterType = type;
            TimeOutMilliseconds = timeOutMilliseconds;
            this.AllowAutoRedirect = true;
        }

        public string Post()
        {
            StringBuilder parameters = null;

            try
            {
                parameters = new StringBuilder();

                if (!string.IsNullOrEmpty(CustomData))
                    parameters.Append(CustomData);
                else
                {
                    for (int i = 0; i < PostItems.Count; i++)
                    {
                        EncodeAndAddItem(ref parameters, PostItems.GetKey(i), PostItems[i]);
                    }
                }

                return PostData(Url, parameters.ToString());
            }
            finally
            {
                if (parameters != null) { parameters = null; }
            }
        }

        public string PostSinat()
        {
            StringBuilder parameters = null;

            try
            {
                parameters = new StringBuilder();

                if (!string.IsNullOrEmpty(CustomData))
                    parameters.Append(CustomData);
                else
                {
                    for (int i = 0; i < PostItems.Count; i++)
                    {
                        EncodeAndAddItem(ref parameters, PostItems.GetKey(i), PostItems[i]);
                    }
                }

                return PostDataSinat(Url, parameters.ToString());
            }
            finally
            {
                if (parameters != null) { parameters = null; }
            }
        }

        private string PostDataSinat(string url, string postData)
        {
            try
            {
                string result = string.Empty;

                if (ResultEncoding == null)
                    ResultEncoding = Encoding.UTF8;

                if (SubmitterType == HttpSubmitterType.Post)
                {
                    UTF8Encoding encoding = new UTF8Encoding();
                    byte[] data = encoding.GetBytes(postData);

                    // Prepare web request...
                    HttpWebRequest myRequest = (HttpWebRequest)WebRequest.Create(url);
                    myRequest.Method = "POST";
                    myRequest.ContentType = "application/xml";
                    myRequest.ContentLength = data.Length;
                    Stream newStream = myRequest.GetRequestStream();

                    // Send the data.
                    newStream.Write(data, 0, data.Length);
                    newStream.Close();

                    using (HttpWebResponse response = (HttpWebResponse)myRequest.GetResponse())
                    {
                        using (Stream responseStream = response.GetResponseStream())
                        {
                            using (StreamReader readStream = new StreamReader(responseStream, ResultEncoding))
                            {
                                result = readStream.ReadToEnd();
                            }
                        }

                        this.HeaderColletion = response.Headers;
                    }
                }

                return result;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private string PostData(string url, string postData)
        {
            string result = string.Empty;

            if (ResultEncoding == null)
                ResultEncoding = Encoding.UTF8;

            try
            {
                if (SubmitterType == HttpSubmitterType.Post)
                {
                    #region POST
                    url = GetJessionId(url);
                    Uri uri = new Uri(url);
                    request = (HttpWebRequest)WebRequest.Create(uri);
                    request.Timeout = TimeOutMilliseconds != null ? TimeOutMilliseconds.Value : 3600000;
                    request.Method = "POST";
                    if (!string.IsNullOrEmpty(CustomContentType))
                        request.ContentType = CustomContentType;

                    if (!string.IsNullOrEmpty(CustomAccept))
                        request.Accept = CustomAccept;
                    else
                        request.Accept = "application/xml";

                    if (Credential != null)
                        request.Credentials = Credential;

                    // Mantendo Cookies da ultima execução
                    request.CookieContainer = new CookieContainer();
                    if (cookieCollection != null)
                    {
                        foreach (Cookie c in cookieCollection)
                        {
                            request.CookieContainer.Add(c);
                        }
                    }
                    if (!AllowAutoRedirect)
                        request.AllowAutoRedirect = false;

                    byte[] bytes = UTF8Encoding.UTF8.GetBytes(postData);
                    request.ContentLength = bytes.Length;

                    using (Stream writeStream = request.GetRequestStream())
                    {
                        writeStream.Write(bytes, 0, bytes.Length);
                    }
                    #endregion
                }
                else if (SubmitterType == HttpSubmitterType.Put)
                {
                    #region PUT
                    Uri uri = new Uri(url);
                    request = (HttpWebRequest)WebRequest.Create(uri);
                    request.Timeout = TimeOutMilliseconds != null ? TimeOutMilliseconds.Value : 3600000;
                    request.Method = "PUT";
                    request.ContentType = CustomContentType;
                    request.ContentLength = postData.Length;
                    if (!string.IsNullOrEmpty(CustomAccept))
                        request.Accept = CustomAccept;
                    if (Credential != null)
                        request.Credentials = Credential;

                    // Mantendo Cookies da ultima execução
                    request.CookieContainer = new CookieContainer();
                    if (cookieCollection != null)
                    {
                        foreach (Cookie c in cookieCollection)
                        {
                            request.CookieContainer.Add(c);
                        }
                    }

                    using (Stream writeStream = request.GetRequestStream())
                    {
                        UTF8Encoding encoding = new UTF8Encoding();
                        byte[] bytes = encoding.GetBytes(postData);
                        writeStream.Write(bytes, 0, bytes.Length);
                    }
                    #endregion
                }
                else if (SubmitterType == HttpSubmitterType.Delete)
                {
                    #region DELETE
                    Uri uri = new Uri(url);
                    request = (HttpWebRequest)WebRequest.Create(uri);
                    request.Timeout = TimeOutMilliseconds != null ? TimeOutMilliseconds.Value : 900000;
                    request.Method = "DELETE";
                    if (Credential != null)
                        request.Credentials = Credential;

                    // Mantendo Cookies da ultima execução
                    request.CookieContainer = new CookieContainer();
                    if (cookieCollection != null)
                    {
                        foreach (Cookie c in cookieCollection)
                        {
                            request.CookieContainer.Add(c);
                        }
                    }

                    using (Stream writeStream = request.GetRequestStream())
                    {
                        UTF8Encoding encoding = new UTF8Encoding();
                        byte[] bytes = encoding.GetBytes(postData);
                        writeStream.Write(bytes, 0, bytes.Length);
                    }
                    #endregion
                }
                else
                {
                    #region OTHER
                    Uri uri = null;
                    if (!string.IsNullOrEmpty(postData))
                    {
                        uri = new Uri(url + "?" + postData);
                    }
                    else
                    {
                        uri = new Uri(url);
                    }

                    request = (HttpWebRequest)WebRequest.Create(uri);
                    request.Timeout = TimeOutMilliseconds != null ? TimeOutMilliseconds.Value : 900000;
                    request.CachePolicy = new HttpRequestCachePolicy(HttpRequestCacheLevel.NoCacheNoStore);
                    request.Method = "GET";
                    if (Credential != null)
                        request.Credentials = Credential;
                    if (!string.IsNullOrEmpty(CustomAccept))
                        request.Accept = CustomAccept;

                    // Mantendo Cookies da ultima execução
                    request.CookieContainer = new CookieContainer();
                    if (cookieCollection != null)
                    {
                        foreach (Cookie c in cookieCollection)
                        {
                            request.CookieContainer.Add(c);
                        }
                    }

                    #endregion
                }

                using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                {
                    using (Stream responseStream = response.GetResponseStream())
                    {
                        using (StreamReader readStream = new StreamReader(responseStream, ResultEncoding))
                        {
                            result = readStream.ReadToEnd();
                        }
                    }

                    if (response.Headers.Count > 0)
                        this.HeaderColletion = response.Headers;
                    if (response.Cookies.Count > 0)
                    {
                        if (this.cookieCollection == null)
                        {
                            this.cookieCollection = response.Cookies;
                        }
                        else
                        {
                            foreach (Cookie cResp in response.Cookies)
                            {
                                var exists = false;
                                foreach (Cookie cLocal in this.cookieCollection)
                                {
                                    if (cResp.Name == cLocal.Name)
                                    {
                                        cLocal.Value = cResp.Value;
                                        break;
                                    }
                                }

                                if (!exists)
                                    this.cookieCollection.Add(cResp);
                            }
                        }
                    }
                }
                return result;
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {

                //if (request != null) { request = null; }
            }
        }

        private static void EncodeAndAddItem(ref StringBuilder baseRequest, string key, string dataItem)
        {
            if (baseRequest == null)
                baseRequest = new StringBuilder();

            if (baseRequest.Length != 0)
                baseRequest.Append("&");

            baseRequest.Append(key);
            baseRequest.Append("=");
            baseRequest.Append(HttpUtility.UrlEncode(dataItem));
        }

        private string GetJessionId(string url)
        {
            return url;
        }

        //public string Submit(String url, NameValueCollection nameValueCollection, Utility.HttpSubmitterType httpSubmitterType, int timeOutMiliseconds, string customContentType)
        //{
        //    var submit = new Utility.HttpSubmitter(url, nameValueCollection, httpSubmitterType) { TimeOutMilliseconds = timeOutMiliseconds };
        //    submit.ResultEncoding = Encoding.GetEncoding("UTF-8");
        //    submit.CustomContentType = "text/html; charset=UTF-8";
        //    var result = submit.Post();
        //    return result;
        //}

        //public string Submit(String url, NameValueCollection nameValueCollection, Utility.HttpSubmitterType httpSubmitterType, int timeOutMiliseconds, string customContentType, string customAccept)
        //{
        //    var submit = new Utility.HttpSubmitter(url, nameValueCollection, httpSubmitterType);
        //    submit.TimeOutMilliseconds = timeOutMiliseconds;

        //    if (string.IsNullOrEmpty(customContentType))
        //        submit.CustomContentType = "text/html; charset=UTF-8";
        //    else
        //        submit.CustomContentType = customContentType;

        //    if (string.IsNullOrEmpty(customAccept))
        //        submit.CustomAccept = "text/html; charset=UTF-8";
        //    else
        //        submit.CustomAccept = customAccept;

        //    var result = submit.Post();
        //    return result;
        //}
    }
}
