using System;
using System.Web;
using System.Collections.Specialized;

namespace Utility
{
    /// <summary>
    /// Summary description for WebAddress
    /// </summary>
    public class WebAddress
    {
        public WebAddress()
        {
            PostParams = new NameValueCollection();
            GetParams = new NameValueCollection();
            if (HttpContext.Current != null)
            {
                _bld = new UriBuilder(HttpContext.Current.Request.Url.ToString());
                GetParams = HttpUtility.ParseQueryString(_bld.Query);
            }
            else
            {
                _bld = new UriBuilder();
            }
        }

        public WebAddress(string Url)
        {
            PostParams = new NameValueCollection();
            GetParams = new NameValueCollection();

            _bld = new UriBuilder(Url);
            GetParams = HttpUtility.ParseQueryString(_bld.Query);
        }

        public NameValueCollection PostParams;
        public NameValueCollection GetParams;
        private UriBuilder _bld;

        public string Url
        {
            get
            {
                _bld.Query = NameValueCollectionToQueryString(GetParams);
                return _bld.Uri.ToString();
            }

            set
            {
                _bld = new UriBuilder(HttpContext.Current.Request.Url.ToString());
                GetParams = HttpUtility.ParseQueryString(_bld.Query);
                PostParams = new NameValueCollection();

            }
        }

        public string FileName
        {
            get
            {
                string filename = string.Empty;
                try
                {
                    filename = System.IO.Path.GetFileName(_bld.Uri.LocalPath);
                }
                catch (ArgumentException)
                { }
                return filename;
            }
        }

        public override string ToString()
        {
            return Url;
        }

        /// <summary>
        /// Change the WebAddress by a relative path
        /// </summary>
        /// <param name="relative_path"></param>
        public void TraverseRelative(string relative_path)
        {
            _bld = new UriBuilder(new Uri(_bld.Uri, relative_path));
            var p = HttpUtility.ParseQueryString(_bld.Query);
            foreach (var key in p.AllKeys)
            {
                GetParams[key] = p[key];
            }
        }

        /// <summary>
        /// Do a Response.Redirect to the WebAddress
        /// </summary>
        public void Redirect()
        {
            if (HttpContext.Current != null)
                HttpContext.Current.Response.Redirect(Url);
        }

        private string _agent = "DPW2.Misc.WebAddress";

        public string UserAgent
        {
            get { return _agent; }
            set { _agent = value; }
        }

        public int TimeOut;

        /// <summary>
        /// Return the results of a Get to the WebAddresss
        /// </summary>
        /// <returns></returns>
        public string Get()
        {
            string Url = this.Url;
            try
            {
                var webRequest = (System.Net.HttpWebRequest)System.Net.WebRequest.Create(Url);
                webRequest.UserAgent = _agent;
                return
                    new System.IO.StreamReader(webRequest.GetResponse().GetResponseStream(),
                                               System.Text.Encoding.Default).ReadToEnd();
            }
            catch (System.Net.WebException e)
            {
                string content = "";
                if (e.Response == null)
                    content = e.Message;
                else
                    content = (new System.IO.StreamReader(e.Response.GetResponseStream())).ReadToEnd();
                var ex = new System.Net.WebException("Error trying to GET " + Url + Environment.NewLine + content, e);
                throw ex;
            }
        }

        ///<summary>
        ///<para>
        ///Generates an HTTP call to the specified URL with the URL encoded parameters in postData
        ///</para>       
        ///</summary>
        private string PostDataToURL(string URL, string postData)
        {
            try
            {
                Byte[] data = (new System.Text.ASCIIEncoding()).GetBytes(postData);
                var webRequest = (System.Net.HttpWebRequest)System.Net.WebRequest.Create(URL);
                webRequest.UserAgent = _agent;
                webRequest.Method = "POST";
                webRequest.ContentType = "application/x-www-form-urlencoded";
                webRequest.ContentLength = data.Length;
                var newStream = webRequest.GetRequestStream();
                newStream.Write(data, 0, data.Length);
                newStream.Close();
                return
                    new System.IO.StreamReader(webRequest.GetResponse().GetResponseStream(),
                                               System.Text.Encoding.Default).ReadToEnd();
            }
            catch (System.Net.WebException e)
            {
                string content = "";
                if (e.Response == null)
                    content = e.Message;
                else
                    content = (new System.IO.StreamReader(e.Response.GetResponseStream())).ReadToEnd();

                var ex = new System.Net.WebException("Error trying to POST to " + URL + Environment.NewLine + content, e);
                throw ex;
            }
        }

        ///<summary>
        ///<para>
        ///Generates an HTTP call to the specified URL with the name value pairs specified in postData.
        /// postData is automatically URL encoded.
        ///</para>       
        ///</summary>
        private string PostDataToURL(string URL, NameValueCollection postData)
        {

            string postString = "";
            for (int i = 0; i < postData.Count; i++)
                postString += (i == 0 ? "" : "&") + postData.Keys[i] + "=" + HttpUtility.UrlEncode(postData[i]);
            return PostDataToURL(URL, postString);
        }

        /// <summary>
        /// Return the results of a Post to the WebAddresss
        /// </summary>
        /// <returns></returns>
        public string Post()
        {
            return PostDataToURL(Url, PostParams);
        }

        #region UriBuilderPassThrough

        /// <summary>
        /// Get or Set the IP address or DNS name of the server
        /// </summary>
        public string Host
        {
            get { return _bld.Host; }
            set { _bld.Host = value; }
        }

        public string Path
        {
            get { return _bld.Path; }
            set { _bld.Path = value; }
        }

        public int Port
        {
            get { return _bld.Port; }
            set { _bld.Port = value; }
        }

        public string Query
        {
            get { return _bld.Query; }
            set { _bld.Query = value; }
        }

        #endregion

        private static string NameValueCollectionToQueryString(NameValueCollection c)
        {
            var b = new System.Text.StringBuilder();
            int i = 0;
            foreach (string key in c.AllKeys)
            {
                //if (i == 0) b.Append("?"); //note that the question mark is not required on a UriBuilder query
                b.Append(key + "=" + System.Web.HttpUtility.UrlEncode(c[key]));
                if (i != c.AllKeys.Length - 1)
                    b.Append("&");
                i++;
            }
            return b.ToString();
        }


    }
}