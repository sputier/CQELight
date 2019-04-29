using CQELight.Abstractions.DDD;
using CQELight.TestFramework;
using FluentAssertions;
using Geneao.Domain;
using Geneao.Events;
using Geneao.Identity;
using System;
using System.Linq;
using Xunit;

namespace Geneao.Tests
{
    public class FamilleTests : BaseUnitTestClass
    {
        private static bool s_Init = false;
        public FamilleTests()
        {
            if(!s_Init)
            {
                Famille.CreerFamille("UnitTest");
                s_Init = true;
            }
        }

        [Fact]
        public void Ajouter_Personne_Should_Create_Event_PersonneAjoute()
        {
            var famille = new Famille("UnitTest");

            var result = famille.AjouterPersonne("First",
                new InfosNaissance("Paris", new DateTime(1965, 12, 03)));

            result.IsSuccess.Should().BeTrue();
            famille.DomainEvents.Should().HaveCount(1);
            famille.DomainEvents.First().Should().BeOfType<PersonneAjoutee>();
            var evt = famille.DomainEvents.First().As<PersonneAjoutee>();
            evt.Prenom.Should().Be("First");
            evt.DateNaissance.Should().BeSameDateAs(new DateTime(1965, 12, 03));
            evt.LieuNaissance.Should().Be("Paris");

        }

        [Fact]
        public void Ajouter_Personne_Already_Exists_Should_Returns_Result_Fail()
        {
            var famille = new Famille("UnitTest");

            famille.AjouterPersonne("First",
                new InfosNaissance("Paris", new DateTime(1965, 12, 03)));

            var result = famille.AjouterPersonne("First",
                new InfosNaissance("Paris", new DateTime(1965, 12, 03)));

            result.IsSuccess.Should().BeFalse();
            result.Should().BeOfType<Result<PersonneNonAjouteeCar>>();

            var raison = result.As<Result<PersonneNonAjouteeCar>>().Value;
            raison.Should().Be(PersonneNonAjouteeCar.PersonneExistante);
        }

    }
}
