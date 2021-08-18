// <copyright file="ContextMenuUtility.cs" company="Zizhen Li">
// Copyright (c) 2019 - 2020 Zizhen Li. All rights reserved.
// Licensed under the LGPL-3.0-only license. See LICENSE.md file in the project root for full license information.
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AwesomeInventory.Jobs;
using AwesomeInventory.Loadout;
using AwesomeInventory.UI;
using RimWorld;
using Verse;
using Verse.Noise;

namespace AwesomeInventory
{
    /// <summary>
    /// Creates float menu options.
    /// </summary>
    public static class ContextMenuUtility
    {
        /*
        public static IEnumerable<FloatMenuOption> EquipAndAddToLoadout(Pawn pawn, Thing thing)
        {
            if (pawn.UseLoadout(out CompAwesomeInventoryLoadout comp))
            {
                if (comp.Loadout != null)
                {
                }
            }
        }
        */

        /*
        private static IEnumerable<FloatMenuOption> OptionForSpawnThing(Pawn pawn, Thing thing, CompAwesomeInventoryLoadout comp)
        {
            List<FloatMenuOption> options = new List<FloatMenuOption>();
            if (thing.def.IsWeapon)
            {
                var option = FloatMenuUtility.DecoratePrioritizedTask(
                    new FloatMenuOption(
                        "Equip, keep the replaced and add to loadout"
                        , () =>
                        {
                            EquipWeaponDialog(pawn, thing);
                        }
                        , MenuOptionPriority.High)
                    , pawn
                    , thing);
                options.Add(option);
            }
            else if (thing.def.IsApparel)
            {
                var option = FloatMenuUtility.DecoratePrioritizedTask(
                    new FloatMenuOption(
                        "Wear, keep the replaced and add to loadout"
                        , () =>
                        {
                            WearApparel(pawn, thing, false);
                            AddToLoadoutDialog(pawn, comp, thing, false);
                        }
                        , MenuOptionPriority.High)
                    , pawn
                    , thing);
                options.Add(option);
            }
            else
            {
                var option = FloatMenuUtility.DecoratePrioritizedTask(
                    new FloatMenuOption(
                        "Pick up and add to loadout"
                        , () =>
                        {
                            AddToLoadoutDialog(pawn, comp, thing, true);
                        }
                        , MenuOptionPriority.High)
                    , pawn
                    , thing);
                options.Add(option);
            }
        }
        */

        /// <summary>
        /// Create loadout options(add to loadout or remove from loadout) for <paramref name="thing"/> that <paramref name="pawn"/> carries.
        /// </summary>
        /// <param name="pawn"> Pawn who carries <paramref name="thing"/>. </param>
        /// <param name="thing"> Thing that is carried by <paramref name="pawn"/>. </param>
        /// <param name="comp"> Comp on <paramref name="pawn"/>. </param>
        /// <returns> Returns different options based on the condition that whether <paramref name="thing"/> is in <paramref name="pawn"/>'s loadout. </returns>
        public static FloatMenuOption OptionForThingOnPawn(Pawn pawn, Thing thing, CompAwesomeInventoryLoadout comp)
        {
            ValidateArg.NotNull(comp, nameof(comp));

            CompAwesomeInventoryLoadout.ThingGroupSelectorPool pool = comp.FindPotentialThingGroupSelectors(thing, comp.Loadout);
            if (pool.OrderedSelectorTuples.Any())
            {
                return MakeDecoratedOption(
                    UIText.RemoveFromLoadout.Translate(comp.Loadout.label)
                    , () => comp.Loadout.Remove(pool.OrderedSelectorTuples.First().Item2)
                    , pawn
                    , thing);
            }
            else
            {
                return MakeDecoratedOption(
                    UIText.AddToLoadout.Translate(comp.Loadout.label)
                    , () => AddToLoadoutDialog(pawn, comp, thing)
                    , pawn
                    , thing);
            }
        }

        /// <summary>
        /// Open loadout window, and if appropriately, open stuff window as well.
        /// </summary>
        /// <param name="pawn"> Pawn who interact with <paramref name="thing"/>. </param>
        /// <param name="comp"> Comp on <paramref name="pawn"/>. </param>
        /// <param name="thing"> Thing that <paramref name="pawn"/> interacts with. </param>
        public static void AddToLoadoutDialog(Pawn pawn, CompAwesomeInventoryLoadout comp, Thing thing)
        {
            ValidateArg.NotNull(comp, nameof(comp));

            ThingGroupSelector groupSelector = thing.MakeThingGrouopSelector();
            comp.Loadout.Add(groupSelector);
            OpenLoadoutWindow(pawn, comp, groupSelector);
        }

        public static FloatMenuOption MakeDecoratedWearApparelOption(Pawn pawn, Apparel apparel, string label, bool forceWear, Action action)
        {
            return MakeDecoratedOption(
                label
                , () =>
                {
                    WearApparel(pawn, apparel, forceWear);
                    action?.Invoke();
                }
                , pawn
                , apparel);
        }

        public static FloatMenuOption MakeDecoratedEquipOption(Pawn pawn, ThingWithComps equipment, string label, Action action)
        {
            return MakeDecoratedOption(
                label
                , () =>
                {
                    EquipWeaponDialog(pawn, equipment);
                    action?.Invoke();
                }
                , pawn
                , equipment);
        }

        private static void OpenLoadoutWindow(Pawn pawn, CompAwesomeInventoryLoadout comp, ThingGroupSelector groupSelector)
        {
            Find.WindowStack.Add(AwesomeInventoryServiceProvider.MakeInstanceOf<Dialog_ManageLoadouts>(new object[] { comp.Loadout, pawn, false }));
            ThingDef allowedThing = groupSelector.AllowedThing;
            if (allowedThing.MadeFromStuff || allowedThing.HasComp(typeof(CompQuality)) || allowedThing.useHitPoints)
            {
                Find.WindowStack.Add(new Dialog_StuffAndQuality(groupSelector));
            }
        }

        private static void WearApparel(Pawn pawn, Thing apparel, bool forceWear)
        {
            DressJob dressJob = SimplePool<DressJob>.Get();
            dressJob.def = AwesomeInventory_JobDefOf.AwesomeInventory_Dress;
            dressJob.targetA = apparel;
            dressJob.ForceWear = forceWear;

            apparel.SetForbidden(value: false);
            pawn.jobs.TryTakeOrderedJob(dressJob);
        }

        private static void EquipWeaponDialog(Pawn pawn, Thing equipment)
        {
            var equipWeaponConfirmationDialogText = ThingRequiringRoyalPermissionUtility.GetEquipWeaponConfirmationDialogText(equipment, pawn);
            var compBladelinkWeapon = equipment.TryGetComp<CompBladelinkWeapon>();
            if (compBladelinkWeapon != null && compBladelinkWeapon.CodedPawn != pawn)
            {
                if (!equipWeaponConfirmationDialogText.NullOrEmpty())
                {
                    equipWeaponConfirmationDialogText += "\n\n";
                }

                equipWeaponConfirmationDialogText += "BladelinkEquipWarning".Translate();
            }

            if (!equipWeaponConfirmationDialogText.NullOrEmpty())
            {
                equipWeaponConfirmationDialogText += "\n\n" + "RoyalWeaponEquipConfirmation".Translate();
                Find.WindowStack.Add(
                    new Dialog_MessageBox(
                        equipWeaponConfirmationDialogText,
                        "Yes".Translate(),
                        () =>
                        {
                            EquipWeapon(pawn, equipment);
                        },
                        "No".Translate()));
            }
            else
            {
                EquipWeapon(pawn, equipment);
            }
        }

        private static void EquipWeapon(Pawn pawn, Thing equipment)
        {
            equipment.SetForbidden(value: false);
            pawn.jobs.TryTakeOrderedJob(JobMaker.MakeJob(AwesomeInventory_JobDefOf.AwesomeInventory_MapEquip, equipment));
            FleckMaker.Static(equipment.DrawPos, equipment.Map, FleckDefOf.FeedbackEquip);
            // MoteMaker.MakeStaticMote(equipment.DrawPos, equipment.Map, ThingDefOf.Mote_FeedbackEquip);
            PlayerKnowledgeDatabase.KnowledgeDemonstrated(ConceptDefOf.EquippingWeapons, KnowledgeAmount.Total);
        }

        private static FloatMenuOption MakeDecoratedOption(string label, Action action, Pawn pawn, Thing thing)
        {
            return FloatMenuUtility.DecoratePrioritizedTask(
                new FloatMenuOption(
                    label
                    , action
                    , MenuOptionPriority.High)
                , pawn
                , thing);
        }
    }
}
