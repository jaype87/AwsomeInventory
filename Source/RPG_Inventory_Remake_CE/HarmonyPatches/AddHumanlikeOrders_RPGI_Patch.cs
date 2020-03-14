using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HarmonyLib;
using System.Reflection;
using Verse;
using Verse.AI;
using RimWorld;
using UnityEngine;
using CombatExtended;
using RPG_Inventory_Remake_Common;

namespace RPG_Inventory_Remake_CE
{
    [StaticConstructorOnStartup]
    public class AddHumanlikeOrders_RPGI_Patch
    {
        static AddHumanlikeOrders_RPGI_Patch()
        {
            MethodInfo original = AccessTools.Method(typeof(FloatMenuMakerMap), "AddHumanlikeOrders");
            MethodInfo postfix = AccessTools.Method(typeof(AddHumanlikeOrders_RPGI_Patch), "Postfix");
            Utility.Harmony.Patch(original, null, new HarmonyMethod(postfix));
        }

        public static void Postfix(Vector3 clickPos, Pawn pawn, List<FloatMenuOption> opts)
        {
            IntVec3 c = IntVec3.FromVector3(clickPos);
            CompInventory compInventory = pawn.TryGetComp<CompInventory>();
            if (pawn.equipment != null)
            {
                ThingWithComps equipment = null;
                List<Thing> thingList = c.GetThingList(pawn.Map);
                for (int i = 0; i < thingList.Count; i++)
                {
                    if (thingList[i].TryGetComp<CompEquippable>() != null)
                    {
                        equipment = (ThingWithComps)thingList[i];
                        break;
                    }
                }
                if (equipment != null)
                {
                    string labelShort = equipment.LabelShort;
                    FloatMenuOption menuOption;
                    int count = 1;
                    if (!(equipment.def.IsWeapon && pawn.story.WorkTagIsDisabled(WorkTags.Violent)
                        && !pawn.CanReach(equipment, PathEndMode.ClosestTouch, Danger.Deadly)
                        && !pawn.health.capacities.CapableOf(PawnCapacityDefOf.Manipulation)
                        && compInventory.CanFitInInventory(equipment, out count)
                        && equipment.IsBurning()))
                    {
                        string text5 = "Corgi_RPGI".Translate() + " " + "Equip".Translate(labelShort);
                        if (equipment.def.IsRangedWeapon && pawn.story != null && pawn.story.traits.HasTrait(TraitDefOf.Brawler))
                        {
                            text5 = text5 + " " + "EquipWarningBrawler".Translate();
                        }
                        menuOption = FloatMenuUtility.DecoratePrioritizedTask(new FloatMenuOption(text5, delegate
                        {
                            equipment.SetForbidden(value: false);
                            pawn.jobs.TryTakeOrderedJob(new Job(RPGI_JobDefOf.RPGI_Map_Equip, equipment) { count = count});
                            MoteMaker.MakeStaticMote(equipment.DrawPos, equipment.Map, ThingDefOf.Mote_FeedbackEquip);
                            PlayerKnowledgeDatabase.KnowledgeDemonstrated(ConceptDefOf.EquippingWeapons, KnowledgeAmount.Total);
                        }, MenuOptionPriority.High), pawn, equipment);
                        opts.Add(menuOption);
                    }
                }
            }
            if (pawn.apparel != null)
            {
                Apparel apparel = pawn.Map.thingGrid.ThingAt<Apparel>(c);
                if (apparel != null)
                {
                    if (pawn.CanReach(apparel, PathEndMode.ClosestTouch, Danger.Deadly)
                        && compInventory.CanFitInInventory(apparel, out _)
                        && !apparel.IsBurning()
                        && ApparelUtility.HasPartsToWear(pawn, apparel.def))
                    {
                        FloatMenuOption optionForced = FloatMenuUtility.DecoratePrioritizedTask(
                            new FloatMenuOption("Corgi_RPGI".Translate() + " " + "ForceWear".Translate(apparel.LabelShort, apparel) + " " + "Corgi_KeepOldApparel".Translate(), delegate
                            {
                                apparel.SetForbidden(value: false);
                                Job jobForced = new Job(RPGI_JobDefOf.RPGI_ApparelOptions, apparel) { count = 1 };
                                pawn.jobs.TryTakeOrderedJob(jobForced);
                            }, MenuOptionPriority.High), pawn, apparel);
                        opts.Add(optionForced);

                        FloatMenuOption option = FloatMenuUtility.DecoratePrioritizedTask(
                            new FloatMenuOption("Corgi_RPGI".Translate() + " " + "Corgi_Wear".Translate(apparel.LabelShort, apparel) + " " + "Corgi_KeepOldApparel".Translate(), delegate
                            {
                                apparel.SetForbidden(value: false);
                                Job jobForced = new Job(RPGI_JobDefOf.RPGI_ApparelOptions, apparel) { count = 0 };
                                pawn.jobs.TryTakeOrderedJob(jobForced);
                            }, MenuOptionPriority.High), pawn, apparel);
                        opts.Add(option);
                    }
                }
            }
        }
    }
}
