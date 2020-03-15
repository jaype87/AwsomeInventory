using System;
using System.Linq;
using System.Reflection;
using Verse;

namespace AwesomeInventory.Common.Services
{
    /// <summary>
    /// Resovle service for different versions of the game.
    /// </summary>
    [StaticConstructorOnStartup]
    internal static class ServiceResolver
    {
        private const string _serviceProviderAssembly = "AISeriveProvider";

        /// <summary>
        /// Get service provider from current appdomain.
        /// </summary>
        /// <returns> A service provider suitable for current version. </returns>
        public static IServiceProvider GetServiceProvider()
        {
            Assembly serviceAssem = AppDomain.CurrentDomain
                                        .GetAssemblies()
                                        .First(
                                            assem =>
                                                assem.GetName().Name == _serviceProviderAssembly);
            return serviceAssem.GetTypes()
                        .First(
                            t => typeof(IServiceProvider).IsAssignableFrom(t))
                        as IServiceProvider;
        }
    }
}
