// <copyright file="Thing_RPGI_Patch.cs" company="Zizhen Li">
// Copyright (c) Zizhen Li. All rights reserved.
// Licensed under the GPL-3.0-only license. See LICENSE.md file in the project root for full license information.
// </copyright>

using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using AwesomeInventory.Common.Loadout;
using HarmonyLib;
using Verse;

namespace AwesomeInventory.Common.HarmonyPatches
{
    [StaticConstructorOnStartup]
    public class Thing_RPGI_Patch
    {
        static Thing_RPGI_Patch()
        {
            MethodInfo original = AccessTools.Method(typeof(Thing), "SplitOff");
            MethodInfo postfix = AccessTools.Method(typeof(Thing_RPGI_Patch), "SplitOff_Postfix");
            Utility.Harmony.Patch(original, null, new HarmonyMethod(postfix));
        }

        [SuppressMessage("Design", "CA1062:Validate arguments of public methods", Justification = "Harmony Patch")]
        public static void SplitOff_Postfix(Thing __instance, int count)
        {
            if (__instance.ParentHolder is Pawn pawn)
            {
                if (pawn.TryGetComp<CompRPGILoadout>() is CompRPGILoadout compRPGI)
                {
                    compRPGI.NotifiedSplitOff(__instance, count);
                }
            }
        }
    }
}
