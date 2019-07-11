IoC avec Autofac
================

`Autofac <https://autofac.org/>`_ est une libraire d'IoC très puissante et très activement maintenue et évoluée par la communauté. Le web regorge de documentation, d'exemples et d'informations à son sujet.

Pour utiliser Autofac, il faut ajouter le package ``CQELight.IoC.Autofac`` à votre projet. Le package est disponible sur NuGet.

Cette extension s'utilise comme toutes les autres, et s'utilise en appelant la méthode d'extension dédiée sur le bootstrapper. Il y a deux overloads de cette méthode:

- La première prends directement votre instance de ``ContainerBuilder``, dans lequel vous aurez défini tous vos enregistrements et y ajoutera les types du système pouvant être utilisé par lui, et construira le Container en fin de ``Bootstrapp()``
- La seconde prends en paramètre une action d'enregistrement sur un ``ContainerBuilder`` qui sera créé par le système, afin que vous puissiez ajouter à celui-ci vos propres enregistrements

De même, comme chaque module d'IoC, il faut pouvoir traiter les interfaces d'enregistrements automatique ``IAutoRegisterType`` et ``IAutoRegisterTypeSingleInstance``. Le module Autofac va rechercher dans l'ensemble des types du système, c'est pourquoi un paramètre permet d'exclure des assemblies de la recherche.

::

    // With lambda registration
    new Bootstrapper().UseAutofacAsIoC(
        containerBuilder => { containerBuilder.RegisterType<MyClass>().AsImplementedInterfaces(); }
    // Excluding DLLs from searching to enhance performances (it's a contains searching)
        "CQELight", "Microsoft", "System")
    
    // With container
    var containerBuilder = new ContainerBuilder();
    containerBuilder.RegisterType<MyClass>().AsImplementedInterfaces(); 
    new Bootsrapper().UseAutofacAsIoC(containerBuilder);

Autofac est configuré afin d'être le plus puissant possible. Par exemple, la recherche des constructeurs cherche également ceux qui sont privés/protégés/internals, et ceci afin de ne pas se retrouver bloqué pour mettre en place les bonnes portées sur vos objets.