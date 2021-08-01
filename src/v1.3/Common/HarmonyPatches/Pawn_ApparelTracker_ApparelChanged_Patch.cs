// <copyright file="Pawn_ApparelTracker_ApparelChanged_Patch.cs" company="Zizhen Li">
// Copyright (c) 2019 - 2020 Zizhen Li. All rights reserved.
// Licensed under the LGPL-3.0-only license. See LICENSE.md file in the project root for full license information.
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using AwesomeInventory.UI;
using HarmonyLib;
using RimWorld;
using Verse;

namespace AwesomeInventory.HarmonyPatches
{
    /// <summary>
    /// Notify gear tab when apparel is changed.
    /// </summary>
    [StaticConstructorOnStartup]
    public static class Pawn_ApparelTracker_ApparelChanged_Patch
    {
        static Pawn_ApparelTracker_ApparelChanged_Patch()
        {
            MethodInfo original = AccessTools.Method(typeof(Pawn_ApparelTracker), "Notify_ApparelChanged");
            MethodInfo postfix = AccessTools.Method(typeof(Pawn_ApparelTracker_ApparelChanged_Patch), "Postfix");
            Utility.Harmony.Patch(original, null, new HarmonyMethod(postfix));
        }

        /// <summary>
        /// Notify subscriber that apparel has changed.
        /// </summary>
        public static event Action<bool> ApparelChangedEvent;

        /// <summary>
        /// Notify <see cref="AwesomeInventoryTabBase"/> that apparel has changed.
        /// </summary>
        public static void Postfix()
        {
            ApparelChangedEvent?.Invoke(true);
        }
    }
}
