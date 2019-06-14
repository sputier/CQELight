using CQELight.Abstractions.DDD;
using CQELight.Abstractions.Events.Interfaces;
using CQELight.Dispatcher;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;
using XamarinHelloWorld.Commands;
using XamarinHelloWorld.Events;

namespace XamarinHelloWorld
{
    // Learn more about making custom code visible in the Xamarin.Forms previewer
    // by visiting https://aka.ms/xamarinforms-previewer
    [DesignTimeVisible(false)]
    public partial class MainPage : ContentPage, IDomainEventHandler<HelloSaid>
    {
        public MainPage()
        {
            InitializeComponent();
            //CoreDispatcher.AddHandlerToDispatcher(this);
            SayHelloBtn.Clicked += async (s, e) =>
             {
                 await CoreDispatcher.DispatchCommandAsync(new SayHello());
             };
        }

        public Task<Result> HandleAsync(HelloSaid domainEvent, IEventContext context = null)
        {
            HelloLabel.Text = "Hello World!";
            return Result.Ok();
        }
    }
}
