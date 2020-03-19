// <copyright file="Pawn_CarryTracker_CarriedThing_RPGI_Patch.cs" company="Zizhen Li">
// Copyright (c) Zizhen Li. All rights reserved.
// Licensed under the GPL-3.0-only license. See LICENSE.md file in the project root for full license information.
// </copyright>

using System.Reflection;
using AwesomeInventory.Jobs;
using AwesomeInventory.Utilities;
using HarmonyLib;
using Verse;
using Verse.AI;

namespace AwesomeInventory.Common.HarmonyPatches
{
    [StaticConstructorOnStartup]
    public class Pawn_CarryTracker_CarriedThing_RPGI_Patch
    {
        static Pawn_CarryTracker_CarriedThing_RPGI_Patch()
        {
            MethodInfo original = AccessTools.Method(typeof(Pawn_CarryTracker), "get_CarriedThing");
            MethodInfo postfix = AccessTools.Method(typeof(Pawn_CarryTracker_CarriedThing_RPGI_Patch), "Postfix");
            Utility.Harmony.Patch(original, null, new HarmonyMethod(postfix));
        }

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
