// <copyright file="ServiceRegistration.cs" company="Zizhen Li">
// Copyright (c) 2019 - 2020 Zizhen Li. All rights reserved.
// Licensed under the LGPL-3.0-only license. See LICENSE.md file in the project root for full license information.
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AwesomeInventory.Loadout;
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
        /// Register <see cref="DrawHelper"/>.
        /// </summary>
        /// <param name="drawHelper"> Instance of <paramref name="drawHelper"/> to register. </param>
        protected static void RegisterIDrawHelper(DrawHelper drawHelper)
        {
            AwesomeInventoryServiceProvider.AddService(typeof(DrawHelper), drawHelper);
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
        /// Register derived type of <see cref="SingleThingSelector"/>.
        /// </summary>
        /// <typeparam name="T"> Derived Type of <see cref="SingleThingSelector"/>. </typeparam>
        protected static void RegisterSingleThingSelector<T>()
            where T : SingleThingSelector
                => AwesomeInventoryServiceProvider.AddType(typeof(SingleThingSelector), typeof(T));

        /// <summary>
        /// Register derived type of <see cref="GenericThingSelector"/>.
        /// </summary>
        /// <typeparam name="T"> Derived Type of <see cref="GenericThingSelector"/>. </typeparam>
        protected static void RegisterGenericThingSelector<T>()
            where T : GenericThingSelector
                => AwesomeInventoryServiceProvider.AddType(typeof(GenericThingSelector), typeof(T));

        /// <summary>
        /// Register derived type of <see cref="ThingGroupSelector"/>.
        /// </summary>
        /// <typeparam name="T"> Derived Type of <see cref="ThingGroupSelector"/>. </typeparam>
        protected static void RegisterThingGroupSelector<T>()
            where T : ThingGroupSelector
                => AwesomeInventoryServiceProvider.AddType(typeof(ThingGroupSelector), typeof(T));

        /// <summary>
        /// Register derived type of <see cref="Dialog_ManageLoadouts"/>.
        /// </summary>
        /// <typeparam name="T"> Derived type of <see cref="Dialog_ManageLoadouts"/>. </typeparam>
        protected static void RegisterDialogManageLoadout<T>()
            where T : Dialog_ManageLoadouts
                => AwesomeInventoryServiceProvider.AddType(typeof(Dialog_ManageLoadouts), typeof(T));

        /// <summary>
        /// Register all services needed for Awesome Inventory.
        /// </summary>
        protected abstract void RegisterAllServies();
    }
}
