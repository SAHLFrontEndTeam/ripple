using System.Collections.Generic;
using System.Linq;
using System.Net;
using FubuCore;
using NuGet;
using ripple.Model;
using System;

namespace ripple.Nuget
{
    public class NugetFeed : NugetFeedBase
    {
        private readonly IPackageRepository _repository;
        private readonly string _url;
        private readonly NugetStability _stability;

        public NugetFeed(string url, NugetStability stability)
        {
            _url = url;
            _stability = stability;
            _repository = new PackageRepositoryFactory().CreateRepository(_url);
        }

        public string Url
        {
            get { return _url; }
        }

        public override bool IsOnline()
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
                    using (var stream = client.OpenRead(_url))
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
        }

        protected override IRemoteNuget find(Dependency query)
		{
			IVersionSpec versionSpec;

            if (query.Mode == UpdateMode.Fixed)
            {
                SemanticVersion version;
                if (!SemanticVersion.TryParse(query.Version, out version))
                {
                    RippleLog.Debug("Could not find exact for " + query);
                    return null;
                }

                versionSpec = new VersionSpec(version);
            }
            else
            {
                versionSpec = query.VersionSpec;
            }
            var package = _repository.FindPackages(query.Name, versionSpec, query.DetermineStability(_stability) == NugetStability.Anything, true).FirstOrDefault();
            if (package == null)
            {
	            return null;
            }
            
            return new RemoteNuget(package);
        }

        public override IEnumerable<IRemoteNuget> FindLatestByName(string idPart)
        {
            return _repository.GetPackages()
                .Where(package => package.Id.Contains(idPart) && package.IsLatestVersion)
                .ToArray()
                .Select(package => new RemoteNuget(package));
        }

        protected override IRemoteNuget findLatest(Dependency query)
        {
			RippleLog.Debug("Searching for {0} from {1}".ToFormat(query, _url));
            var candidates = _repository.Search(query.Name, query.DetermineStability(_stability) == NugetStability.Anything)
                                        .Where(x => query.Name == x.Id).OrderBy(x => x.Id).ToList();

            var candidate = candidates.FirstOrDefault(x => x.IsAbsoluteLatestVersion)
                            ?? candidates.FirstOrDefault(x => x.IsLatestVersion);

            if (candidate == null)
            {
                // If both absolute and latest are false, then we order in descending order (by version) and take the top
                candidate = candidates
                    .OrderByDescending(x => x.Version)
                    .FirstOrDefault();

                if (candidate == null) return null;
            }

            return new RemoteNuget(candidate);
        }

		public override IPackageRepository Repository { get { return _repository; } }

        public override string ToString()
        {
            return _url;
        }
    }
}