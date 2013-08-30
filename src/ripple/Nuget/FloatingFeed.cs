using System;
using System.Collections.Generic;
using System.Net;
using System.Xml;

namespace ripple.Nuget
{
    public class FloatingFeed : NugetFeed, IFloatingFeed
    {
        public const string FindAllLatestCommand =
            "/Packages()?$filter=IsAbsoluteLatestVersion&$orderby=DownloadCount%20desc,Id&$skip=0&$top=1000";

		private readonly Lazy<XmlDocument> _feed;

        public FloatingFeed(string url, NugetStability stability) 
            : base(url, stability)
        {
            _feed = new Lazy<XmlDocument>(loadLatestFeed);
        }

        private XmlDocument loadLatestFeed()
        {
            var document = new XmlDocument();
            try
            {
                var url = Url + FindAllLatestCommand;

                string proxyserver = System.Configuration.ConfigurationManager.AppSettings["proxyserver"];
                string[] bypass = System.Configuration.ConfigurationManager.AppSettings["bypass"].Split(',');
                IWebProxy proxy = new WebProxy(proxyserver, true, bypass); ;
                proxy.Credentials = CredentialCache.DefaultCredentials;
                var client = new WebClient();
                client.Proxy = proxy;

                var text = client.DownloadString(url);
            
                document.LoadXml(text);            
            }
            catch (Exception e)
            {
                RippleLog.Info(e.Message);
                RippleLog.Info(e.StackTrace);                
            }
            return document;
        }

        public IEnumerable<IRemoteNuget> GetLatest()
        {
            var feed = new NugetXmlFeed(_feed.Value);
            return feed.ReadAll(this);
        }
    }
}