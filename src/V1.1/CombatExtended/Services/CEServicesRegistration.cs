// <copyright file="CEServicesRegistration.cs" company="Zizhen Li">
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
using Verse;

namespace AwesomeInventory
{
    /// <summary>
    /// Register service for CE.
    /// </summary>
    [StaticConstructorOnStartup]
    public class CEServicesRegistration : ServiceRegistration
    {
        static CEServicesRegistration()
        {
            new CEServicesRegistration().RegisterAllServies();
        }

        /// <inheritdoc/>
        protected override void RegisterAllServies()
        {
            ServiceRegistration.RegisterIInventoryHelper(new InventoryHelper());
            ServiceRegistration.RegisterIDrawHelper(new DrawHelperCE());
            ServiceRegistration.RegisterAwesomeInventoryTabBase(new AwesomeInventoryTab());
            ServiceRegistration.RegisterSingleThingSelector<SingleThingSelector>();
            ServiceRegistration.RegisterGenericThingSelector<GenericThingSelector>();
            ServiceRegistration.RegisterDialogManageLoadout<Dialog_ManageLoadoutCE>();
        }
    }
}
