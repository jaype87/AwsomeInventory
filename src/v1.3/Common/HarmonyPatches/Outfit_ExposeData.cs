// <copyright file="Outfit_ExposeData.cs" company="Zizhen Li">
// Copyright (c) 2019 - 2020 Zizhen Li. All rights reserved.
// Licensed under the LGPL-3.0-only license. See LICENSE.md file in the project root for full license information.
// </copyright>

using System.Reflection;
using AwesomeInventory.Loadout;
using HarmonyLib;
using RimWorld;
using Verse;

namespace AwesomeInventory.HarmonyPatches
{
    /// <summary>
    /// Patch <see cref="Outfit.ExposeData"/>, so that <see cref="AwesomeInventoryLoadout"/> is also saved.
    /// </summary>
    [StaticConstructorOnStartup]
    public static class Outfit_ExposeData
    {
        static Outfit_ExposeData()
        {
            MethodInfo original = AccessTools.Method(typeof(Outfit), "ExposeData");
            MethodInfo postfix = AccessTools.Method(typeof(Outfit_ExposeData), "Postfix");
            Utility.Harmony.Patch(original, null, new HarmonyMethod(postfix));
        }

        /// <summary>
        /// Patch <see cref="Outfit.ExposeData"/>, so that <see cref="AwesomeInventoryLoadout"/> is also saved.
        /// </summary>
        /// <param name="__instance"> instance of <see cref="AwesomeInventoryLoadout"/>. </param>
        public static void Postfix(Outfit __instance)
        {
            if (__instance is AwesomeInventoryLoadout loadout)
            {
                loadout.ExposeData();
            }
        }
    }
}
