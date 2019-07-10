using CQELight;
using System;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace XamarinHelloWorld
{
    public partial class App : Application
    {
        public App()
        {

            new Bootstrapper()
                .UseInMemoryEventBus()
                .UseInMemoryCommandBus()
                .Bootstrapp();

            InitializeComponent();

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
