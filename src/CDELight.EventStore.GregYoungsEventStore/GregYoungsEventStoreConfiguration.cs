using System;
using System.Collections.Generic;
using System.Text;

namespace CQELight.EventStore.GregYoungsEventStore
{
    public class GregYoungsEventStoreConfiguration
    {
        /// <summary>
        /// Constructeur
        /// </summary>
        /// <param name="uri">Connection Uri</param>
        /// <param name="credentialsUserName">Credential user name</param>
        /// <param name="credentialsUserPassword">Credential password</param>
        /// <param name="sslConnectionTargetHost">SSL connection target host</param>
        /// <param name="sslConnectionValidateServer">Controls whether the connection validates the server certificate</param>
        /// <param name="clusterDiscoveryPolicy">Cluster discovery policy</param>
        public GregYoungsEventStoreConfiguration(Uri uri, string credentialsUserName, string credentialsUserPassword, string sslConnectionTargetHost, bool sslConnectionValidateServer = false, ClusterDiscovery? clusterDiscoveryPolicy = null)
        {
            Uri = uri;
            CredentialsUserName = credentialsUserName;
            CredentialsUserPassword = credentialsUserPassword;
            SslConnectionTargetHost = sslConnectionTargetHost;
            SslConnectionValidateServer = sslConnectionValidateServer;
            ClusterDiscoveryPolicy = clusterDiscoveryPolicy;
        }

        public Uri Uri { get; private set; }

        public string CredentialsUserName { get;private set; }
        public string CredentialsUserPassword { get; private set; }

        public string SslConnectionTargetHost { get; private set; }
        public bool SslConnectionValidateServer { get; private set; }

        public ClusterDiscovery? ClusterDiscoveryPolicy { get; private set; }
        
        public enum ClusterDiscovery
        {
            Dns,
            GossipSeeds
        }
    }
}
