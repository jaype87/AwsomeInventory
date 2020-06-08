// <copyright file="VanillaServicesRegistration.cs" company="Zizhen Li">
// Copyright (c) 2019 - 2020 Zizhen Li. All rights reserved.
// Licensed under the LGPL-3.0-only license. See LICENSE.md file in the project root for full license information.
// </copyright>

using AwesomeInventory.Loadout;
using AwesomeInventory.UI;
using Verse;

namespace AwesomeInventory
{
    /// <summary>
    /// Register services for the vanilla mod.
    /// </summary>
    [StaticConstructorOnStartup]
    public class VanillaServicesRegistration : ServiceRegistration
    {
        static VanillaServicesRegistration()
        {
            new VanillaServicesRegistration().RegisterAllServies();
        }

        /// <inheritdoc/>
        protected override void RegisterAllServies()
        {
            ServiceRegistration.RegisterIInventoryHelper(new InventoryHelper());
            ServiceRegistration.RegisterIDrawHelper(new DrawHelper());
            ServiceRegistration.RegisterAwesomeInventoryTabBase(new AwesomeInventoryTab());
            ServiceRegistration.RegisterSingleThingSelector<SingleThingSelector>();
            ServiceRegistration.RegisterGenericThingSelector<GenericThingSelector>();
            ServiceRegistration.RegisterDialogManageLoadout<Dialog_ManageLoadouts>();
        }
    }
}
