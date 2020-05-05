// <copyright file="Pawn_JobTracker_CleanQueuedJobs.cs" company="Zizhen Li">
// Copyright (c) 2019 - 2020 Zizhen Li. All rights reserved.
// Licensed under the LGPL-3.0-only license. See LICENSE.md file in the project root for full license information.
// </copyright>

using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using AwesomeInventory.Jobs;
using AwesomeInventory.Loadout;
using HarmonyLib;
using Verse;
using Verse.AI;

namespace AwesomeInventory.HarmonyPatches
{
    /// <summary>
    /// Check if there is a hot-swap job in the queue.
    /// </summary>
    [StaticConstructorOnStartup]
    public static class Pawn_JobTracker_CleanQueuedJobs
    {
        private static FieldInfo pawnField = typeof(Pawn_JobTracker).GetField("pawn", BindingFlags.NonPublic | BindingFlags.Instance);

        static Pawn_JobTracker_CleanQueuedJobs()
        {
            MethodInfo original = AccessTools.Method(typeof(Pawn_JobTracker), "ClearQueuedJobs");
            MethodInfo prefix = AccessTools.Method(typeof(Pawn_JobTracker_CleanQueuedJobs), "Prefix");
            Utility.Harmony.Patch(original, new HarmonyMethod(prefix));
        }

        /// <summary>
        /// Check if there is a hot-swap job in the queue.
        /// </summary>
        /// <param name="__instance"> Instance of the patched class. </param>
        /// <returns> If true, continue to execute the rest of the method body. </returns>
        [SuppressMessage("Design", "CA1062:Validate arguments of public methods", Justification = "Harmony patch")]
        public static bool Prefix(Pawn_JobTracker __instance)
        {
            if (__instance.jobQueue.Any(qj => qj.job.def == AwesomeInventory_JobDefOf.AwesomeInventory_HotSwapStateChecker))
            {
                CompAwesomeInventoryLoadout comp = ((Pawn)pawnField.GetValue(__instance)).TryGetComp<CompAwesomeInventoryLoadout>();
                if (comp != null)
                {
                    comp.HotswapState = CompAwesomeInventoryLoadout.HotSwapState.Interuppted;
                }
            }

            return true;
        }
    }
}
