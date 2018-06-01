using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace CQELight.Buses.AzureServiceBus
{
    public static class BootstrapperExt
    {

        #region Public static methods

        public static Bootstrapper UseAzureServiceBus(this Bootstrapper bootstrapper, string connectionString)
        {
            if (string.IsNullOrWhiteSpace(connectionString))
            {
                throw new ArgumentException("Bootstrapper.UseAzureServiceBus() : Connection string should be provided.", nameof(connectionString));
            }

            return bootstrapper;
        }

        #endregion

    }
}