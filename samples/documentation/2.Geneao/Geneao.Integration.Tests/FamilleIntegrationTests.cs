using CQELight;
using CQELight.Dispatcher;
using CQELight.TestFramework;
using FluentAssertions;
using Geneao.Common.Commands;
using Geneao.Domain;
using Geneao.Events;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Geneao.Integration.Tests
{
    public class FamilleIntegrationTests : BaseUnitTestClass
    {
        [Fact]
        public async Task Ajouter_Personne_Should_Publish_Event_PersonneAjoute()
        {
            new Bootstrapper()
                .UseInMemoryEventBus()
                .UseInMemoryCommandBus()
                .UseAutofacAsIoC(_ => { })
                .UseEFCoreAsEventStore(
                    new CQELight.EventStore.EFCore.EFEventStoreOptions(
                        c => c.UseSqlite("FileName=events_tests.db", opts => opts.MigrationsAssembly(typeof(FamilleIntegrationTests).Assembly.GetName().Name)),
                        archiveBehavior: CQELight.EventStore.SnapshotEventsArchiveBehavior.Delete))
                .Bootstrapp();

            await CoreDispatcher.DispatchCommandAsync(new CreerFamilleCommand("UnitTest"));

            var command = new AjouterPersonneCommand("UnitTest", "First", "Paris", new DateTime(1965, 12, 03));

            var evt = await Test.WhenAsync(() => CoreDispatcher.DispatchCommandAsync(command))
                .ThenEventShouldBeRaised<PersonneAjoutee>();

            evt.Prenom.Should().Be("First");
            evt.DateNaissance.Should().BeSameDateAs(new DateTime(1965, 12, 03));
            evt.LieuNaissance.Should().Be("Paris");

        }
    }
}
