// <copyright file="PawnUtility.cs" company="Zizhen Li">
// Copyright (c) 2019 - 2020 Zizhen Li. All rights reserved.
// Licensed under the LGPL-3.0-only license. See LICENSE.md file in the project root for full license information.
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AwesomeInventory.Loadout;
using Verse;

namespace AwesomeInventory
{
    /// <summary>
    /// Extension functions for pawn.
    /// </summary>
    public static class PawnUtility
    {
        /// <summary>
        /// Check if pawn uses loadout.
        /// </summary>
        /// <param name="pawn"> Pawn to check. </param>
        /// <param name="comp"> The comp on <paramref name="pawn"/>, if found. </param>
        /// <returns> Returns true if <paramref name="pawn"/> uses loadout. </returns>
        public static bool UseLoadout(this Pawn pawn, out CompAwesomeInventoryLoadout comp)
        {
            comp = pawn.TryGetComp<CompAwesomeInventoryLoadout>();
            return comp?.Loadout != null && AwesomeInventoryMod.Settings.UseLoadout;
        }
    }
}
