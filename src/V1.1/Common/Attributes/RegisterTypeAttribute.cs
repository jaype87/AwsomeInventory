// <copyright file="RegisterTypeAttribute.cs" company="Zizhen Li">
// Copyright (c) 2019 - 2020 Zizhen Li. All rights reserved.
// Licensed under the LGPL-3.0-only license. See LICENSE.md file in the project root for full license information.
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace AwesomeInventory
{
    /// <summary>
    /// Register this type to <see cref="AwesomeInventoryServiceProvider"/>.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class RegisterTypeAttribute : StaticConstructorOnStartup
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RegisterTypeAttribute"/> class.
        /// </summary>
        /// <param name="base"> Base type. </param>
        /// <param name="derived"> Derived type, usually is the type this attribute is adorned on. </param>
        public RegisterTypeAttribute(Type @base, Type derived)
        {
            ValidateArg.NotNull(@base, nameof(@base));

            if (!@base.IsAssignableFrom(derived))
                AIDebug.Logger.WriteError($"{derived} is not a derived type of {@base}.");

            AwesomeInventoryServiceProvider.AddType(@base, derived);
        }
    }
}
