// <copyright file="JobDriver_AwesomeInventory_Unload.cs" company="Zizhen Li">
// Copyright (c) Zizhen Li. All rights reserved.
// Licensed under the GPL-3.0-only license. See LICENSE.md file in the project root for full license information.
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AwesomeInventory.Common;
using RimWorld;
using Verse;
using Verse.AI;

namespace AwesomeInventory.Jobs
{
    /// <summary>
    /// Difference between this driver and the vanilla JobDriver_HaulToCell is
    /// pawn will first move to the reserved cell then unload while vanilla in reversed order.
    /// Check JobDriver_HaulToCell for more information.
    /// </summary>
    public class JobDriver_AwesomeInventory_Unload : JobDriver
    {
        private int duration;

        public override string GetReport()
        {
            Thing thing = null;
            if (job.def == AwesomeInventory_JobDefOf.AwesomeInventory_Fake)
            {
                return "ReportHauling".Translate(TargetThingA.Label, TargetThingA);
            }
            IntVec3 cell = job.targetB.Cell;
            if (pawn.CurJob == job && pawn.carryTracker.CarriedThing != null)
            {
                thing = pawn.carryTracker.CarriedThing;
            }
            if (thing == null)
            {
                return "ReportHaulingUnknown".Translate();
            }
            string text = null;
            SlotGroup slotGroup = cell.GetSlotGroup(base.Map);
            if (slotGroup != null)
            {
                text = slotGroup.parent.SlotYielderLabel();
            }
            if (text != null)
            {
                return "ReportHaulingTo".Translate(thing.Label, text.Named("DESTINATION"), thing.Named("THING"));
            }
            return "ReportHauling".Translate(thing.Label, thing);
        }

        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {

            Pawn pawn = base.pawn;
            LocalTargetInfo target = base.job.GetTarget(TargetIndex.B);
            Job job = base.job;
            bool errorOnFailed2 = errorOnFailed;
            bool result = false;

            if (!target.Cell.IsValidStorageFor(pawn.Map, pawn.carryTracker.CarriedThing))
            {
                StoragePriority currentPriority = StoreUtility.CurrentStoragePriorityOf(TargetThingA);
                if (!StoreUtility.TryFindBestBetterStorageFor(TargetThingA, pawn, pawn.Map, currentPriority, pawn.Faction, out IntVec3 foundCell, out IHaulDestination haulDestination))
                {
                    return false;
                }

            }

            if (pawn.Reserve(target, job, 1, -1, null, errorOnFailed2))
            {
                pawn = base.pawn;
                target = base.job.GetTarget(TargetIndex.A);
                job = base.job;
                errorOnFailed2 = errorOnFailed;
                result = pawn.Reserve(target, job, 1, -1, null, errorOnFailed2);
            }
            Log.Message("Make Reservation result: " + result);
            return result;
        }

        public override void Notify_Starting()
        {
            base.Notify_Starting();
            AddFinishAction(() => Log.Message("Job finished"));
            // NOTE remove log
            Log.Message("Notify Starting");
            if (TargetThingA is Apparel apparel && pawn.apparel.Contains(apparel))
            {
                // time needed to unequip
                duration = (int)(apparel.GetStatValue(StatDefOf.EquipDelay) * 60f);
            }
            else if (TargetThingA is ThingWithComps equipment)
            {
                if (pawn.equipment.Contains(equipment))
                {
                    // vanilla time to drop any equipment
                    duration = 30;
                }
            }
        }

        protected override IEnumerable<Toil> MakeNewToils()
        {
            this.FailOnDestroyedOrNull(TargetIndex.A);
            this.FailOnBurningImmobile(TargetIndex.B);

            Toil carryToCell, timeToUnequip, placeThing;
            timeToUnequip = placeThing = null;

            // Use CarriedThing to validate state
            // There is a Harmony patch to trick the validation to think
            // the pawn has Carryied Thing on it.
            // Check Pawn_CarryTracker_CarriedThing_RPGI_Patch.cs
            carryToCell = Toils_Haul.CarryHauledThingToCell(TargetIndex.B);

            // When set true, it allows toils to be execute consecutively in one tick
            // Check JobDirver.DriverTick() for more information
            carryToCell.atomicWithPrevious = true;

            timeToUnequip = Toils_General.Wait(duration).WithProgressBarToilDelay(TargetIndex.A);
            timeToUnequip.JumpIf(() => duration == 0, placeThing);
            timeToUnequip.atomicWithPrevious = true;

            placeThing = Toils_Haul.PlaceHauledThingInCell(TargetIndex.B, carryToCell, storageMode: true);
            placeThing.AddPreInitAction(delegate ()
            {
                // Move things to carry tracker
                // Where things are actually transfered to CarryTracker
                TargetThingA.holdingOwner.TryTransferToContainer(TargetThingA, pawn.carryTracker.innerContainer);
            });
            placeThing.atomicWithPrevious = true;

            yield return carryToCell;
            yield return timeToUnequip;
            yield return placeThing;
        }
    }
}
