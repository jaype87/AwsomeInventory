// <copyright file="Thing_GetInspectTabs_Patch.cs" company="Zizhen Li">
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
    /// Replace ITab_Pawn_Gear on all pawns even if they are modded races as long as they are colonists.
    /// </summary>
    [StaticConstructorOnStartup]
    public static class Thing_GetInspectTabs_Patch
    {
        static Thing_GetInspectTabs_Patch()
        {
            MethodInfo original = AccessTools.Method(typeof(Thing), "GetInspectTabs");
            MethodInfo postfix = AccessTools.Method(typeof(Thing_GetInspectTabs_Patch), "Postfix");
            Utility.Harmony.Patch(original, null, new HarmonyMethod(postfix));
        }

        /// <summary>
        /// Replace ITab_Pawn_Gear on all pawns even if they are modded races as long as they are colonists.
        /// </summary>
        /// <param name="tabBases"> TabBases used by <paramref name="__instance"/>. </param>
        /// <param name="__instance"> Thing to inspect. </param>
        /// <returns> A modified list of <see cref="InspectTabBase"/>s. </returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1062:Validate arguments of public methods", Justification = "Stylecop bug")]
        public static IEnumerable<InspectTabBase> Postfix(IEnumerable<InspectTabBase> tabBases, Thing __instance)
        {
            if (tabBases.EnumerableNullOrEmpty())
                yield break;

            if (AwesomeInventoryMod.Settings.PatchAllRaces && __instance is Pawn pawn && pawn.IsColonist)
            {
                foreach (InspectTabBase tabBase in tabBases)
                {
                    if (tabBase is ITab_Pawn_Gear)
                    {
                        yield return InspectTabManager.GetSharedInstance(AwesomeInventoryServiceProvider.GetService<AwesomeInventoryTabBase>().GetType());
                    }
                    else
                    {
                        yield return tabBase;
                    }
                }
            }
            else
            {
                foreach (var tabBase in tabBases)
                    yield return tabBase;
            }
        }
    }
}
