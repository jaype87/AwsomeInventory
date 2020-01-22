using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Harmony;
using Verse;
using RimWorld;
using System.Reflection;
using RPG_Inventory_Remake_Common;
using RPG_Inventory_Remake.Loadout;

namespace RPG_Inventory_Remake
{
    [StaticConstructorOnStartup]
    public class ThingOwner_RPGI_Patch
    {
        static ThingOwner_RPGI_Patch()
        {
            MethodInfo original = AccessTools.Method(typeof(ThingOwner), "NotifyAdded");
            MethodInfo postfix = AccessTools.Method(typeof(ThingOwner_RPGI_Patch), "NotifyAdded_Postfix");
            Utility._harmony.Patch(original, null, new HarmonyMethod(postfix));

            MethodInfo original1 = AccessTools.Method(typeof(ThingOwner), "NotifyAddedAndMergedWith");
            MethodInfo postfix1 = AccessTools.Method(typeof(ThingOwner_RPGI_Patch), "NotifyAddedAndMergedWith_Postfix");
            Utility._harmony.Patch(original1, null, new HarmonyMethod(postfix1));

            MethodInfo original2 = AccessTools.Method(typeof(ThingOwner), "NotifyRemoved");
            MethodInfo postfix2 = AccessTools.Method(typeof(ThingOwner_RPGI_Patch), "NotifyRemoved_Postfix");
            Utility._harmony.Patch(original2, null, new HarmonyMethod(postfix2));

            MethodInfo original3 = AccessTools.Method(typeof(ThingOwner), "Notify_ContainedItemDestroyed");
            MethodInfo postfix3 = AccessTools.Method(typeof(ThingOwner_RPGI_Patch), "Notify_ContainedItemDestroyed_Postfix");
            Utility._harmony.Patch(original3, null, new HarmonyMethod(postfix3));
        }

        public static void NotifyAdded_Postfix(ThingOwner __instance, Thing item)
        {
            (__instance.Owner as Pawn_InventoryTracker)?.pawn?.TryGetComp<compRPGILoudout>()?.NotifiedAdded(item);
            (__instance.Owner as Pawn_ApparelTracker)?.pawn?.TryGetComp<compRPGILoudout>()?.NotifiedAdded(item);
            (__instance.Owner as Pawn_EquipmentTracker)?.pawn?.TryGetComp<compRPGILoudout>()?.NotifiedAdded(item);
        }

        public static void NotifyAddedAndMergedWith_Postfix(ThingOwner __instance, Thing item, int mergedCount)
        {
            (__instance.Owner as Pawn_InventoryTracker)?.pawn?.TryGetComp<compRPGILoudout>()?.NotifiedAddedAndMergedWith(item, mergedCount);
            (__instance.Owner as Pawn_ApparelTracker)?.pawn?.TryGetComp<compRPGILoudout>()?.NotifiedAddedAndMergedWith(item, mergedCount);
            (__instance.Owner as Pawn_EquipmentTracker)?.pawn?.TryGetComp<compRPGILoudout>()?.NotifiedAddedAndMergedWith(item, mergedCount);
        }

        public static void NotifyRemoved_Postfix(ThingOwner __instance, Thing item)
        {
            (__instance.Owner as Pawn_InventoryTracker)?.pawn?.TryGetComp<compRPGILoudout>()?.NotifiedRemoved(item);
            (__instance.Owner as Pawn_ApparelTracker)?.pawn?.TryGetComp<compRPGILoudout>()?.NotifiedRemoved(item);
            (__instance.Owner as Pawn_EquipmentTracker)?.pawn?.TryGetComp<compRPGILoudout>()?.NotifiedRemoved(item);
        }

        public static void Notify_ContainedItemDestroyed_Postfix(ThingOwner __instance, Thing t)
        {
            (__instance.Owner as Pawn_InventoryTracker)?.pawn?.TryGetComp<compRPGILoudout>()?.NotifiedRemoved(t);
            (__instance.Owner as Pawn_ApparelTracker)?.pawn?.TryGetComp<compRPGILoudout>()?.NotifiedRemoved(t);
            (__instance.Owner as Pawn_EquipmentTracker)?.pawn?.TryGetComp<compRPGILoudout>()?.NotifiedRemoved(t);
        }
    }
}