// <copyright file="Thing_RPGI_Patch.cs" company="Zizhen Li">
// Copyright (c) 2019 - 2020 Zizhen Li. All rights reserved.
// Licensed under the LGPL-3.0-only license. See LICENSE.md file in the project root for full license information.
// </copyright>

using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using AwesomeInventory.Loadout;
using HarmonyLib;
using Verse;

namespace AwesomeInventory.Common.HarmonyPatches
{
    /// <summary>
    /// Patch <see cref="Thing.SplitOff(int)"/>, so to synchronize with <see cref="AwesomeInventoryLoadout"/>.
    /// </summary>
    [StaticConstructorOnStartup]
    public static class Thing_RPGI_Patch
    {
        static Thing_RPGI_Patch()
        {
            MethodInfo original = AccessTools.Method(typeof(Thing), "SplitOff");
            MethodInfo postfix = AccessTools.Method(typeof(Thing_RPGI_Patch), "SplitOff_Postfix");
            Utility.Harmony.Patch(original, null, new HarmonyMethod(postfix));
        }

        /// <summary>
        /// Patch <see cref="Thing.SplitOff(int)"/>, so to synchronize with <see cref="AwesomeInventoryLoadout"/>.
        /// </summary>
        /// <param name="__instance"> Thing that is being splitted. </param>
        /// <param name="count"> Number of thing that is splitted. </param>
        [SuppressMessage("Design", "CA1062:Validate arguments of public methods", Justification = "Harmony Patch")]
        public static void SplitOff_Postfix(Thing __instance, int count)
        {
            if (__instance.ParentHolder?.ParentHolder is Pawn pawn)
            {
                if (pawn.TryGetComp<CompAwesomeInventoryLoadout>() is CompAwesomeInventoryLoadout compRPGI)
                {
                    compRPGI.NotifiedSplitOff(__instance, count);
                }
            }
        }
    }
}
