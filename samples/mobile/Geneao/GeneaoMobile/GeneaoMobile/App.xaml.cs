using Xamarin.Forms;
using GeneaoMobile.Views;
using System.IO;
using CQELight;
using Microsoft.EntityFrameworkCore;
using Autofac;
using Geneao.Common.Data.Repositories.Familles;
using System;

namespace GeneaoMobile
{
    public partial class App : Application
    {

        public App()
        {
            InitializeComponent();

            string filePath = "";
            if (Device.RuntimePlatform == Device.Android)
            {
                filePath = Path.Combine(
                   System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal), "familles.json");
            }
            else if (Device.RuntimePlatform == Device.iOS)
            {
                filePath = Path.Combine(
                   System.Environment.GetFolderPath(System.Environment.SpecialFolder.MyDocuments), "familles.json");
            }
            else
            {
                throw new NotImplementedException();
            }
            if (!File.Exists(filePath))
            {
                File.WriteAllText(filePath, "[]");
            }
            string eventsDb = "";
            if (Device.RuntimePlatform == Device.Android)
            {
                eventsDb = Path.Combine(
                   System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal), "events.db");
            }
            else if(Device.RuntimePlatform == Device.iOS)
            {
                eventsDb = Path.Combine(
                   System.Environment.GetFolderPath(System.Environment.SpecialFolder.MyDocuments), "events.db");
            }
            else
            {
                throw new NotImplementedException();
            }
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
