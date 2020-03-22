// <copyright file="OutfitDatabase_TryDelete.cs" company="Zizhen Li">
// Copyright (c) 2019 - 2020 Zizhen Li. All rights reserved.
// Licensed under the LGPL-3.0-only license. See LICENSE.md file in the project root for full license information.
// </copyright>

using System.Reflection;
using AwesomeInventory.Loadout;
using AwesomeInventory.Utilities;
using HarmonyLib;
using RimWorld;
using Verse;

namespace AwesomeInventory.Common.HarmonyPatches
{
    /// <summary>
    /// Patch into <see cref="OutfitDatabase.TryDelete(Outfit)"/>, so to synchronize with <see cref="LoadoutManager"/>.
    /// </summary>
    [StaticConstructorOnStartup]
    public static class OutfitDatabase_TryDelete
    {
        static OutfitDatabase_TryDelete()
        {
            MethodInfo original = AccessTools.Method(typeof(OutfitDatabase), "TryDelete");
            MethodInfo postfix = AccessTools.Method(typeof(OutfitDatabase_TryDelete), "Postfix");
            Utility.Harmony.Patch(original, null, new HarmonyMethod(postfix));
        }

        /// <summary>
        /// Patch into <see cref="OutfitDatabase.TryDelete(Outfit)"/>, so to synchronize with <see cref="LoadoutManager"/>.
        /// </summary>
        /// <param name="outfit"> Outfit to remove. </param>
        /// <param name="__result"> Result from <see cref="OutfitDatabase.TryDelete(Outfit)"/>. </param>
        public static void Postfix(Outfit outfit, AcceptanceReport __result)
        {
            if (__result.Accepted)
            {
                if (outfit is AILoadout loadout)
                {
                    LoadoutManager.TryRemoveLoadout(loadout, true);
                }
            }
        }
    }
}
