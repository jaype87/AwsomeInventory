// <copyright file="AwesomeInventoryServiceProvider.cs" company="Zizhen Li">
// Copyright (c) Zizhen Li. All rights reserved.
// Licensed under the GPL-3.0-only license. See LICENSE.md file in the project root for full license information.
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;
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
            _services[type] = servie;
        }
    }
}
