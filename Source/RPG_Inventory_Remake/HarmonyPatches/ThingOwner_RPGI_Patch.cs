using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using HarmonyLib;
using Verse;
using RimWorld;
using System.Reflection;
using RPG_Inventory_Remake_Common;
using RPG_Inventory_Remake.Loadout;
using System.Diagnostics.CodeAnalysis;

namespace RPG_Inventory_Remake
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
            (__instance.Owner as Pawn_InventoryTracker)?.pawn?.TryGetComp<CompRPGILoadout>()?.NotifiedAdded(item);
            (__instance.Owner as Pawn_ApparelTracker)?.pawn?.TryGetComp<CompRPGILoadout>()?.NotifiedAdded(item);
            (__instance.Owner as Pawn_EquipmentTracker)?.pawn?.TryGetComp<CompRPGILoadout>()?.NotifiedAdded(item);
        }

        [SuppressMessage("Design", "CA1062:Validate arguments of public methods", Justification = "<Pending>")]
        public static void NotifyAddedAndMergedWith_Postfix(ThingOwner __instance, Thing item, int mergedCount)
        {
            (__instance.Owner as Pawn_InventoryTracker)?.pawn?.TryGetComp<CompRPGILoadout>()?.NotifiedAddedAndMergedWith(item, mergedCount);
            (__instance.Owner as Pawn_ApparelTracker)?.pawn?.TryGetComp<CompRPGILoadout>()?.NotifiedAddedAndMergedWith(item, mergedCount);
            (__instance.Owner as Pawn_EquipmentTracker)?.pawn?.TryGetComp<CompRPGILoadout>()?.NotifiedAddedAndMergedWith(item, mergedCount);
        }

        [SuppressMessage("Design", "CA1062:Validate arguments of public methods", Justification = "<Pending>")]
        public static void NotifyRemoved_Postfix(ThingOwner __instance, Thing item)
        {
            (__instance.Owner as Pawn_InventoryTracker)?.pawn?.TryGetComp<CompRPGILoadout>()?.NotifiedRemoved(item);
            (__instance.Owner as Pawn_ApparelTracker)?.pawn?.TryGetComp<CompRPGILoadout>()?.NotifiedRemoved(item);
            (__instance.Owner as Pawn_EquipmentTracker)?.pawn?.TryGetComp<CompRPGILoadout>()?.NotifiedRemoved(item);
        }
    }
}