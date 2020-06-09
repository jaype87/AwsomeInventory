// <copyright file="RegisterServiceAttribute.cs" company="Zizhen Li">
// Copyright (c) 2019 - 2020 Zizhen Li. All rights reserved.
// Licensed under the LGPL-3.0-only license. See LICENSE.md file in the project root for full license information.
// </copyright>

using System;
using Verse;

namespace AwesomeInventory
{
    /// <summary>
    /// Register service to <see cref="AwesomeInventoryServiceProvider"/>.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class RegisterServiceAttribute : StaticConstructorOnStartup
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RegisterServiceAttribute"/> class.
        /// </summary>
        /// <param name="base"> Type of service. </param>
        /// <param name="service"> Instance of service. </param>
        public RegisterServiceAttribute(Type @base, Type service)
        {
            ValidateArg.NotNull(@base, nameof(@base));
            ValidateArg.NotNull(service, nameof(service));

            if (!@base.IsAssignableFrom(service))
                AIDebug.Logger.WriteError($"Service {service} is not of type {@base}.");

            if (AwesomeInventoryServiceProvider.TryGetService(@base, out object value) && !service.IsSubclassOf(value.GetType()))
                return;

            AwesomeInventoryServiceProvider.AddService(@base, Activator.CreateInstance(service));
        }
    }
}
