using Xamarin.Forms;
using GeneaoMobile.Views;
using System.IO;
using CQELight;
using Microsoft.EntityFrameworkCore;
using System;
using Autofac;
using Geneao.Common.Data.Repositories.Familles;

namespace GeneaoMobile
{
    public partial class App : Application
    {

        public App()
        {
            InitializeComponent();

            var filePath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "familles.json");
            if (!File.Exists(filePath))
            {
                File.WriteAllText(filePath, "[]");
            }
            var eventsDb = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "events.db");
            new Bootstrapper()
                .OnlyIncludeDLLsForTypeSearching("Geneao")
                .UseInMemoryEventBus()
                .UseInMemoryCommandBus()
                .UseAutofacAsIoC(c =>
                {
                    c.Register(_ => new FileFamilleRepository(new FileInfo(filePath))).As<IFamilleRepository>();
                })
                .UseEFCoreAsEventStore(
                new CQELight.EventStore.EFCore.EFEventStoreOptions(
                    c => c.UseSqlite($"FileName={eventsDb}", opts => opts.MigrationsAssembly(typeof(App).Assembly.GetName().Name)),
                    archiveBehavior: CQELight.EventStore.SnapshotEventsArchiveBehavior.Delete))
                .Bootstrapp();

            MainPage = new MainPage();
        }

        protected override void OnStart()
        {
            // Handle when your app starts
        }

        protected override void OnSleep()
        {
            // Handle when your app sleeps
        }

        protected override void OnResume()
        {
            // Handle when your app resumes
        }
    }
}
