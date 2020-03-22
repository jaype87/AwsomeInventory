// <copyright file="ServiceRegistration.cs" company="Zizhen Li">
// Copyright (c) 2019 - 2020 Zizhen Li. All rights reserved.
// Licensed under the LGPL-3.0-only license. See LICENSE.md file in the project root for full license information.
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AwesomeInventory.UI;
using AwesomeInventory.Utilities;

namespace AwesomeInventory
{
    /// <summary>
    /// Register services provided either by the vanilla or the CE assembly.
    /// It provides a template of services for vanilla and CE implementation.
    /// </summary>
    public abstract class ServiceRegistration
    {
        /// <summary>
        /// Register <see cref="IInventoryHelper"/>.
        /// </summary>
        /// <param name="inventoryHelper"> Instance of <paramref name="inventoryHelper"/> to register. </param>
        protected static void RegisterIInventoryHelper(IInventoryHelper inventoryHelper)
        {
            AwesomeInventoryServiceProvider.AddService(typeof(IInventoryHelper), inventoryHelper);
        }

        /// <summary>
        /// Register <see cref="IDrawHelper"/>.
        /// </summary>
        /// <param name="drawHelper"> Instance of <paramref name="drawHelper"/> to register. </param>
        protected static void RegisterIDrawHelper(IDrawHelper drawHelper)
        {
            AwesomeInventoryServiceProvider.AddService(typeof(IDrawHelper), drawHelper);
        }

        /// <summary>
        /// Register <see cref="AwesomeInventoryTabBase"/>.
        /// </summary>
        /// <param name="awesomeInventoryTabBase"> Implementation of <paramref name="awesomeInventoryTabBase"/>. </param>
        protected static void RegisterAwesomeInventoryTabBase(AwesomeInventoryTabBase awesomeInventoryTabBase)
        {
            AwesomeInventoryServiceProvider.AddService(typeof(AwesomeInventoryTabBase), awesomeInventoryTabBase);
        }

        /// <summary>
        /// Register all services needed for Awesome Inventory.
        /// </summary>
        protected abstract void RegisterAllServies();
    }
}
