// <copyright file="AwesomeInventoryServiceProvider.cs" company="Zizhen Li">
// Copyright (c) 2019 - 2020 Zizhen Li. All rights reserved.
// Licensed under the LGPL-3.0-only license. See LICENSE.md file in the project root for full license information.
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace AwesomeInventory
{
    /// <summary>
    /// Service provider for AwesomeInventory.
    /// </summary>
    public static class AwesomeInventoryServiceProvider
    {
        private static Dictionary<Type, object> _services = new Dictionary<Type, object>();
        private static Dictionary<Type, Type> _typeDictionary = new Dictionary<Type, Type>();
        private static Dictionary<int, object> _pluginService = new Dictionary<int, object>();

        /// <summary>
        /// Gets available plugins.
        /// </summary>
        public static Dictionary<int, object> Plugins => _pluginService;

        /// <summary>
        /// Get service of <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T"> type of service. </typeparam>
        /// <returns> Instance of <typeparamref name="T"/>. </returns>
        public static T GetService<T>()
        {
            return (T)_services[typeof(T)];
        }

        /// <summary>
        /// Try get service of <paramref name="type"/>.
        /// </summary>
        /// <param name="type"> Service type. </param>
        /// <param name="service"> Instance of <paramref name="type"/>, default value if not found. </param>
        /// <returns> Returns true if <paramref name="service"/> is found. </returns>
        public static bool TryGetService(Type type, out object service)
        {
            if (_services.ContainsKey(type))
            {
                service = _services[type];
                return true;
            }

            service = default;
            return false;
        }

        /// <summary>
        /// Try get service that implements <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T"> Type to implement. </typeparam>
        /// <param name="service"> Instance of implementation. </param>
        /// <returns> Returns true if service is found. </returns>
        public static bool TryGetImplementation<T>(out T service)
        {
            Type type = typeof(T);
            foreach (Type key in _services.Keys)
            {
                if (type.IsAssignableFrom(key))
                {
                    service = (T)_services[type];
                    return true;
                }
            }

            service = default;
            return false;
        }

        /// <summary>
        /// Add service to service provider.
        /// </summary>
        /// <param name="type"> Service type. </param>
        /// <param name="servie"> Service to add. </param>
        public static void AddService(Type type, object servie)
        {
            ValidateArg.NotNull(type, nameof(type));
            ValidateArg.NotNull(servie, nameof(servie));

            _services[type] = servie;
        }

        /// <summary>
        /// Add <paramref name="baseType"/> and <paramref name="derivedType"/> as a <see cref="KeyValuePair{TKey, TValue}"/> to a type dictionary.
        /// </summary>
        /// <param name="baseType"> Typs as a key. </param>
        /// <param name="derivedType"> Type as a value. </param>
        public static void AddType(Type baseType, Type derivedType)
        {
            ValidateArg.NotNull(baseType, nameof(baseType));
            ValidateArg.NotNull(derivedType, nameof(derivedType));

            if (_typeDictionary.TryGetValue(baseType, out Type value))
            {
                if (derivedType.IsSubclassOf(value))
                    _typeDictionary[baseType] = derivedType;
            }
            else
            {
                _typeDictionary[baseType] = derivedType;
            }
        }

        /// <summary>
        /// Create instance of a certain type, which is a result after querying a type dictionary with <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T"> A <see cref="Type"/> used for querying a type dictionary. </typeparam>
        /// <param name="ctorArgs"> Arguments passed to constructor.</param>
        /// <returns> Instance created after querying a type dictionary. </returns>
        public static T MakeInstanceOf<T>(params object[] ctorArgs)
        {
            Type concreteType = _typeDictionary[typeof(T)];
            return (T)Activator.CreateInstance(concreteType, ctorArgs);
        }

        /// <summary>
        /// Add plugins to service provider.
        /// </summary>
        /// <param name="plugin"> Plugin to add. </param>
        public static void AddPlugIn(Plugin plugin)
        {
            ValidateArg.NotNull(plugin, nameof(plugin));

            _pluginService[plugin.ID] = plugin;
        }

        /// <summary>
        /// Get plugin of type <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T"> Type of the plugin. </typeparam>
        /// <param name="id"> ID of the plugin. </param>
        /// <returns> Instance of type <typeparamref name="T"/>. </returns>
        public static T GetPlugin<T>(int id)
        {
            return (T)_pluginService[id];
        }

        /// <summary>
        /// Get a list of plugins of <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T"> Type of plugins. </typeparam>
        /// <returns> A list of plugins. </returns>
        public static IEnumerable<T> GetPluginsOfType<T>()
        {
            return _pluginService.Values.OfType<T>();
        }

        /// <summary>
        /// Gets next available plugin ID.
        /// </summary>
        /// <returns> ID for plugin. </returns>
        public static int GetNextAvailablePluginID()
        {
            int counter = 0;
            foreach (int id in _pluginService.OrderBy(pair => pair.Key).Select(pair => pair.Key))
            {
                if (counter == id)
                {
                    counter++;
                    continue;
                }
                else
                {
                    break;
                }
            }

            return counter;
        }
    }
}
