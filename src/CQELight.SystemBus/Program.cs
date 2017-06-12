using CQELight.SystemBus.DAL;
using Microsoft.EntityFrameworkCore;
using System;

namespace CQELight.SystemBus
{

#pragma warning disable RCS1102 // Mark class as static.
    class Program
#pragma warning restore RCS1102 // Mark class as static.
    {

        #region Static methods

        /// <summary>
        /// Main entry point of app.
        /// </summary>
        /// <param name="args">Command line arguments.</param>
        static void Main(string[] args)
        {
            using (var ctx = new SystemBusContext())
            {
                ctx.Database.Migrate();
            }
            var server = new Server();
            server.Run().GetAwaiter().GetResult();
            Console.Read();
        }

        #endregion

    }
}