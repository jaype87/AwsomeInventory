// <copyright file="Pawn_DraftController_Drafted.cs" company="Zizhen Li">
// Copyright (c) 2019 - 2020 Zizhen Li. All rights reserved.
// Licensed under the LGPL-3.0-only license. See LICENSE.md file in the project root for full license information.
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using AwesomeInventory.Loadout;
using HarmonyLib;
using RimWorld;
using Verse;

namespace AwesomeInventory.HarmonyPatches
{
    /// <summary>
    /// Swap loadout to the one before pawn is drafted.
    /// </summary>
    [StaticConstructorOnStartup]
    public static class Pawn_DraftController_Drafted
    {
        static Pawn_DraftController_Drafted()
        {
            MethodInfo original = AccessTools.Property(typeof(Pawn_DraftController), "Drafted").GetSetMethod();
            MethodInfo postfix = AccessTools.Method(typeof(Pawn_DraftController_Drafted), "Postfix");
            Utility.Harmony.Patch(original, null, new HarmonyMethod(postfix));
        }

        /// <summary>
        /// When a pawn becomes undrafted, it will change to the loadout before hotswap.
        /// </summary>
        /// <param name="value"> Indicates if a pawn is drafted or not. </param>
        /// <param name="__instance"> Instance of <see cref="Pawn_DraftController"/> which is a member of <see cref="Pawn"/>. </param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1062:Validate arguments of public methods", Justification = "Harmony Patch")]
        public static void Postfix(bool value, Pawn_DraftController __instance)
        {
            ValidateArg.NotNull(__instance, nameof(__instance));

            if (!value)
            {
                CompAwesomeInventoryLoadout comp = __instance.pawn.TryGetComp<CompAwesomeInventoryLoadout>();
                if (comp != null && comp.HotswapState != CompAwesomeInventoryLoadout.HotSwapState.Inactive && comp.HotSwapCostume != null && comp.Loadout == comp.HotSwapCostume)
                {
                    if (comp.LoadoutBeforeHotSwap != null && comp.LoadoutBeforeHotSwap != comp.HotSwapCostume)
                    {
                        ApparelOptionUtility.StopDressingJobs(__instance.pawn);
                        __instance.pawn.SetLoadout(comp.LoadoutBeforeHotSwap);
                        comp.LoadoutBeforeHotSwap = null;
                        comp.HotswapState = CompAwesomeInventoryLoadout.HotSwapState.Inactive;
                    }
                }
            }
        }
    }
}
