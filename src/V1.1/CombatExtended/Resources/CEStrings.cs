// <copyright file="CEStrings.cs" company="Zizhen Li">
// Copyright (c) 2019 - 2020 Zizhen Li. All rights reserved.
// Licensed under the LGPL-3.0-only license. See LICENSE.md file in the project root for full license information.
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AwesomeInventory
{
    /// <summary>
    /// String resources for CE.
    /// </summary>
    public static class CEStrings
    {
        /// <summary>
        /// Prefix for generic ammo def of a specified gun in CE.
        /// </summary>
        public const string GenericAmmoPrefix = "GenericAmmo-";

        /// <summary>
        /// Description for generic ammo.
        /// </summary>
        public const string AmmoDescription = "Generic Loadout ammo for {0}. Intended for generic collection of ammo for given gun.";

        /// <summary>
        /// Label for generic ammo.
        /// </summary>
        public const string AmmoLabel = "CE_Generic_Ammo";
    }
}
