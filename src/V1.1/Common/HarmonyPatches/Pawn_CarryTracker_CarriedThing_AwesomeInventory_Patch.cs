// <copyright file="Pawn_CarryTracker_CarriedThing_AwesomeInventory_Patch.cs" company="Zizhen Li">
// Copyright (c) 2019 - 2020 Zizhen Li. All rights reserved.
// Licensed under the LGPL-3.0-only license. See LICENSE.md file in the project root for full license information.
// </copyright>

using System.Reflection;
using AwesomeInventory.Jobs;
using HarmonyLib;
using Verse;
using Verse.AI;

namespace AwesomeInventory.HarmonyPatches
{
    /// <summary>
    /// Trick <see cref="Pawn_CarryTracker"/> to think it has a carried thing when pawn is on an unload job.
    /// </summary>
    [StaticConstructorOnStartup]
    public static class Pawn_CarryTracker_CarriedThing_AwesomeInventory_Patch
    {
        static Pawn_CarryTracker_CarriedThing_AwesomeInventory_Patch()
        {
            MethodInfo original = AccessTools.Method(typeof(Pawn_CarryTracker), "get_CarriedThing");
            MethodInfo postfix = AccessTools.Method(typeof(Pawn_CarryTracker_CarriedThing_AwesomeInventory_Patch), "Postfix");
            Utility.Harmony.Patch(original, null, new HarmonyMethod(postfix));
        }

        /// <summary>
        /// Trick <see cref="Pawn_CarryTracker"/> to think it has a carried thing when pawn is on an unload job.
        /// </summary>
        /// <param name="__instance"> An instance of <see cref="Pawn_CarryTracker"/>. </param>
        /// <param name="__result"> Masquerade the target of an unload job as a carried thing. </param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1062:Validate arguments of public methods", Justification = "Harmony requirements")]
        public static void Postfix(Pawn_CarryTracker __instance, ref Thing __result)
        {
            Job job = __instance.pawn.CurJob;
            if (job != null)
            {
                if (job.def == AwesomeInventory_JobDefOf.AwesomeInventory_Unload)
                {
                    if (job.targetA.HasThing)
                    {
                        __result = job.targetA.Thing;
                    }
                }
            }
        }
    }
}
