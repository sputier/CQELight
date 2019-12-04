Intégration avec ASP.NET Core
=============================

CQELight s'intègre avec ASP.NET Core en peu d'efforts. Pour le faire, il faut installer le package ``CQELight.AspCore`` sur votre projet ASP.NET Core.
Ensuite, selon votre version, référez vous au guide ci-dessous pour l'intégration en un rien de temps !

ASP.NET Core 2.x
^^^^^^^^^^^^^^^^
L'intégration avec ASP.NET Core 2.x est moins naturelle qu'en 3.x. En premier lieu, il convient de savoir que les packages du framework pour cette version sont tous en netstandard2.0, la version 2.1 n'étant pas compatible avec cette version.

Pour pouvoir utiliser CQELight, il faut modifier la classe Startup.cs. La méthode ``ConfigureService`` doit dorénavant renvoyer un ``IServiceProvider``, la signature change donc de 

::

    public void ConfigureServices(IServiceCollection services)
	
à

::
    
	public IServiceProvider ConfigureServices(IServiceCollection services)
	
Suite à cela, configurez votre application comme vous le souhaitez et, pour satisfaire la nouvelle signature, renvoyez ``services.ConfigureCQELight(b => ...)``. 
Cette méthode est l'appel vers le Bootstrapper, et nous vous renvoyons vers la documentation dédiée pour savoir comment le configurer : :doc:`../cqelight/bootstrapper`

Retrouvez un exemple complet de cette configuration à cette adresse : https://github.com/cdie/CQELight/tree/master/samples/web/CQELight_ASPNETCore2_1

ASP.NET Core 3.x
^^^^^^^^^^^^^^^^
L'intégration avec ASP.NET Core 3.x est plus aisée qu'en 2.x. En premier lieu, il convient de savoir que les packages du framwork pour cette version sont tous en netstandard2.1 afin de profiter de toutes les améliorations qu'offre C#8 et .NET Core 3.

Pour pouvoir utiliser CQELight, il faut modifier la classe Program.cs. Il suffit d'appeler la méthode d'extension ``ConfigureCQELight(b => ...)`` sur l'objet ``IHostBuilder`` créé par la méthode ``Host.CreateDefaultBuilder(args)``. Cette méthode est l'appel vers le Bootstrapper, et nous vous renvoyons vers la documentation dédiée pour savoir comment le configurer : :doc:`../cqelight/bootstrapper`

Retrouvez un exemple complet de cette configuration à cette adresse : https://github.com/cdie/CQELight/tree/master/samples/web/CQELight_ASPNETCore3