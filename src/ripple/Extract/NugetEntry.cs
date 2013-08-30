using System.Net;
using System.Xml.Linq;
using FubuCore;
using System.Configuration;
using System;

namespace ripple.Extract
{
    public class NugetEntry
    {
		private static readonly XNamespace atomNS = "http://www.w3.org/2005/Atom";
		private static readonly XNamespace mNS = "http://schemas.microsoft.com/ado/2007/08/dataservices/metadata";
		private static readonly XNamespace dNS = "http://schemas.microsoft.com/ado/2007/08/dataservices";

	    public NugetEntry(XElement entryNode)
	    {
			Version = "0.0.0";

		    var contentElement = entryNode.Element(atomNS + "content");
			if (contentElement != null) Url = contentElement.Attribute("src").Value;

		    var titleElement = entryNode.Element(atomNS + "title");
			if (titleElement != null) Name = titleElement.Value;

		    var propertyElement = entryNode.Element(mNS + "properties");
		    if (propertyElement != null)
		    {
			    var versionNode = propertyElement.Element(dNS + "Version");
			    if (versionNode != null) Version = versionNode.Value;
		    }

		    if (Url.IsEmpty() || Name.IsEmpty())
			{
				throw new RippleFatalError("Nuget entry did not match what I was expecting. \n\n" + entryNode.Document);
			}
	    }

	    public string Version { get; set; }
        public string Name { get; set; }
        public string Url { get; set; }

        public override string ToString()
        {
            return string.Format("Name: {0}, Version {1}, Url: {2}", Name, Version, Url);
        }

		private string getFileName()
		{
			return "{0}.{1}.nupkg".ToFormat(Name, Version);
		}

        public void DownloadTo(string directory)
        {
            var file = directory.AppendPath(getFileName());
            string proxyserver = System.Configuration.ConfigurationManager.AppSettings["proxyserver"];
            string[] bypass = System.Configuration.ConfigurationManager.AppSettings["bypass"].Split(',');
            IWebProxy proxy = new WebProxy(proxyserver, true, bypass); 
            try
            {                                
                proxy.Credentials = CredentialCache.DefaultCredentials;
                var client = new WebClient();
                client.Proxy = proxy;

                RippleLog.Info("Downloading {0} to {1}".ToFormat(Url, file));
                client.DownloadFile(Url, file);
                RippleLog.Info("Ripple Writing file: " + file);
            }
            catch (Exception e)
            {
                RippleLog.Info("Exception proxy server: " + proxyserver);
                RippleLog.Info("Exception downloaded file url: " + Url);
                RippleLog.Info("Exception credentials: " + proxy.Credentials.ToString());
                RippleLog.Info("Exception downloaded file system path: " + file);
                RippleLog.Info(e.Message);
                RippleLog.Info(e.StackTrace);
            }
        }
    }
}