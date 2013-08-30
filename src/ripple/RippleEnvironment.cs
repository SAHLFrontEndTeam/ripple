using System;
using System.Net;
using ripple.Model;

namespace ripple
{
    public class RippleEnvironment
    {
        private static Func<bool> _connected;

        private readonly static Lazy<bool> HasConnection;

        static RippleEnvironment()
        {
            HasConnection = new Lazy<bool>(() =>
            {
                try
                {
                    string proxyserver = System.Configuration.ConfigurationManager.AppSettings["proxyserver"];
                    string[] bypass = System.Configuration.ConfigurationManager.AppSettings["bypass"].Split(',');
                    IWebProxy proxy = new WebProxy(proxyserver, true, bypass); ;
                    proxy.Credentials = CredentialCache.DefaultCredentials; 
                    using (var client = new WebClient())
                    {
                        client.Proxy = proxy;
                        using (var stream = client.OpenRead(Feed.NuGetV2.Url))
                        {
                            return true;
                        }
                    }
                }
                catch(Exception e)
                {                     
                    RippleLog.Info(e.Message);
                    RippleLog.Info(e.StackTrace);                
                    return false;
                }
            });

            Live();
        }

        public static void Live()
        {
            _connected = () => HasConnection.Value;
        }

        public static void StubConnection(bool connected)
        {
            _connected = () => connected;
        }

        public static bool Connected()
        {
            return _connected();
        }
    }
}