using System;
using System.Collections.Generic;
using System.Net;
using FubuCore.Descriptions;
using ripple.Model;

namespace ripple.Nuget
{
    public class UrlNugetDownloader : INugetDownloader, DescribesItself
    {
        private readonly string _url;

        public UrlNugetDownloader(string url)
        {
            _url = url;
        }

        public string Url
        {
            get { return _url; }
        }

		public INugetFile DownloadTo(SolutionMode mode, string filename)
        {
            string proxyserver = System.Configuration.ConfigurationManager.AppSettings["proxyserver"];
            string[] bypass = System.Configuration.ConfigurationManager.AppSettings["bypass"].Split(',');
            IWebProxy proxy = new WebProxy(proxyserver, true, bypass); 
            try 
            {
                proxy.Credentials = CredentialCache.DefaultCredentials;            
                var client = new WebClient();
                client.Proxy = proxy;

                Console.WriteLine("Downloading {0} to {1}", Url, filename);
                client.DownloadFile(Url, filename);
                RippleLog.Info("UrlNugetDownloader: Ripple Writing file: " + filename);
            }
            catch (Exception e)
            {   
                RippleLog.Info("Exception proxy server: " + proxyserver);
                RippleLog.Info("Exception downloaded file url: " + Url);
                RippleLog.Info("Exception credentials: " + proxy.Credentials.ToString());
                RippleLog.Info("Exception downloaded file system path: " + filename);
                RippleLog.Info(e.Message);
                RippleLog.Info(e.StackTrace);
                RippleLog.Info(e.Message);
                RippleLog.Info(e.StackTrace);                
            }

            return new NugetFile(filename, mode);
        }

        private sealed class UrlEqualityComparer : IEqualityComparer<UrlNugetDownloader>
        {
            public bool Equals(UrlNugetDownloader x, UrlNugetDownloader y)
            {
                if (ReferenceEquals(x, y)) return true;
                if (ReferenceEquals(x, null)) return false;
                if (ReferenceEquals(y, null)) return false;
                if (x.GetType() != y.GetType()) return false;
                return string.Equals(x._url, y._url);
            }

            public int GetHashCode(UrlNugetDownloader obj)
            {
                return (obj._url != null ? obj._url.GetHashCode() : 0);
            }
        }

        private static readonly IEqualityComparer<UrlNugetDownloader> UrlComparerInstance = new UrlEqualityComparer();

        public static IEqualityComparer<UrlNugetDownloader> UrlComparer
        {
            get { return UrlComparerInstance; }
        }

	    public void Describe(Description description)
	    {
		    description.ShortDescription = "Download from " + _url;
	    }
    }
}