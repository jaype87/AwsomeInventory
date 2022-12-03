// <copyright file="ThingOwner_RPGI_Patch.cs" company="Zizhen Li">
// Copyright (c) Zizhen Li. All rights reserved.
// Licensed under the GPL-3.0-only license. See LICENSE.md file in the project root for full license information.
// </copyright>

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using AwesomeInventory.Loadout;
using HarmonyLib;
using RimWorld;
using Verse;

namespace AwesomeInventory.HarmonyPatches
{
    [StaticConstructorOnStartup]
    public class ThingOwner_RPGI_Patch
    {
        static ThingOwner_RPGI_Patch()
        {
            MethodInfo original = AccessTools.Method(typeof(ThingOwner), "NotifyAdded");
            MethodInfo postfix = AccessTools.Method(typeof(ThingOwner_RPGI_Patch), "NotifyAdded_Postfix");
            Utility.Harmony.Patch(original, null, new HarmonyMethod(postfix));

            MethodInfo original1 = AccessTools.Method(typeof(ThingOwner), "NotifyAddedAndMergedWith");
            MethodInfo postfix1 = AccessTools.Method(typeof(ThingOwner_RPGI_Patch), "NotifyAddedAndMergedWith_Postfix");
            Utility.Harmony.Patch(original1, null, new HarmonyMethod(postfix1));

            MethodInfo original2 = AccessTools.Method(typeof(ThingOwner), "NotifyRemoved");
            MethodInfo postfix2 = AccessTools.Method(typeof(ThingOwner_RPGI_Patch), "NotifyRemoved_Postfix");
            Utility.Harmony.Patch(original2, null, new HarmonyMethod(postfix2));
        }

        [SuppressMessage("Design", "CA1062:Validate arguments of public methods", Justification = "<Pending>")]
        public static void NotifyAdded_Postfix(ThingOwner __instance, Thing item)
        {
            (__instance.Owner as Pawn_InventoryTracker)?.pawn?.TryGetComp<CompAwesomeInventoryLoadout>()?.NotifiedAdded(item);
            (__instance.Owner as Pawn_ApparelTracker)?.pawn?.TryGetComp<CompAwesomeInventoryLoadout>()?.NotifiedAdded(item);
            (__instance.Owner as Pawn_EquipmentTracker)?.pawn?.TryGetComp<CompAwesomeInventoryLoadout>()?.NotifiedAdded(item);
        }

        [SuppressMessage("Design", "CA1062:Validate arguments of public methods", Justification = "<Pending>")]
        public static void NotifyAddedAndMergedWith_Postfix(ThingOwner __instance, Thing item, int mergedCount)
        {
            (__instance.Owner as Pawn_InventoryTracker)?.pawn?.TryGetComp<CompAwesomeInventoryLoadout>()?.NotifiedAddedAndMergedWith(item, mergedCount);
            (__instance.Owner as Pawn_ApparelTracker)?.pawn?.TryGetComp<CompAwesomeInventoryLoadout>()?.NotifiedAddedAndMergedWith(item, mergedCount);
            (__instance.Owner as Pawn_EquipmentTracker)?.pawn?.TryGetComp<CompAwesomeInventoryLoadout>()?.NotifiedAddedAndMergedWith(item, mergedCount);
        }

        [SuppressMessage("Design", "CA1062:Validate arguments of public methods", Justification = "<Pending>")]
        public static void NotifyRemoved_Postfix(ThingOwner __instance, Thing item)
        {
            (__instance.Owner as Pawn_InventoryTracker)?.pawn?.TryGetComp<CompAwesomeInventoryLoadout>()?.NotifiedRemoved(item);
            (__instance.Owner as Pawn_ApparelTracker)?.pawn?.TryGetComp<CompAwesomeInventoryLoadout>()?.NotifiedRemoved(item);
            (__instance.Owner as Pawn_EquipmentTracker)?.pawn?.TryGetComp<CompAwesomeInventoryLoadout>()?.NotifiedRemoved(item);
        }
    }
}