// <copyright file="JobDriver_AwesomeInventory_Unload.cs" company="Zizhen Li">
// Copyright (c) 2019 - 2020 Zizhen Li. All rights reserved.
// Licensed under the LGPL-3.0-only license. See LICENSE.md file in the project root for full license information.
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AwesomeInventory.UI;
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
        private int _duration = 0;
        private bool _cellStorage = false;
        private bool _container = false;

        /// <summary>
        /// Get a report string for displaying.
        /// </summary>
        /// <returns> A report string. </returns>
        public override string GetReport()
        {
            if (this.TargetB == LocalTargetInfo.Invalid)
            {
                return UIText.ReportHauling.Translate(TargetThingA.Label, TargetThingA);
            }

            Thing thing = pawn.carryTracker?.CarriedThing;

            if (thing == null)
            {
                return UIText.ReportHaulingUnknown.Translate();
            }

            if (_cellStorage)
            {
                SlotGroup slotGroup = this.TargetB.Cell.GetSlotGroup(Map);
                if (slotGroup != null)
                {
                    string text = slotGroup.parent?.SlotYielderLabel();
                    if (text != null)
                    {
                        return UIText.ReportHaulingTo.Translate(thing.Label, text.Named("DESTINATION"), thing.Named("THING"));
                    }
                }
            }
            else if (_container)
            {
                return ((job.GetTarget(TargetIndex.B).Thing is Building_Grave) ? UIText.ReportHaulingToGrave : UIText.ReportHaulingTo).Translate(thing.Label, job.targetB.Thing.LabelShort.Named("DESTINATION"), thing.Named("THING"));
            }

            return UIText.ReportHauling.Translate(thing.Label, thing);
        }

        /// <summary>
        /// Make reservation for job targets before doing the job.
        /// </summary>
        /// <param name="errorOnFailed"> If true, log result as error if failed to make a reservation. </param>
        /// <returns> Returns true if a reservation is made. </returns>
        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            StoragePriority currentPriority = StoreUtility.CurrentStoragePriorityOf(TargetThingA);
            if (!StoreUtility.TryFindBestBetterStorageFor(TargetThingA, pawn, pawn.Map, currentPriority, pawn.Faction, out IntVec3 foundCell, out IHaulDestination haulDestination))
            {
                JobFailReason.Is(UIText.NoEmptyPlaceLower);
                Messages.Message(
                    UIText.NoEmptyPlaceLower.TranslateSimple()
                    , new TargetInfo(this.pawn.PositionHeld, this.pawn.MapHeld), MessageTypeDefOf.NeutralEvent);
                return false;
            }

            this.Init(foundCell, haulDestination);

            if (pawn.Reserve(TargetB, this.job, 1, -1, null, errorOnFailed))
            {
                return pawn.Reserve(TargetA, this.job, 1, -1, null, errorOnFailed);
            }

            return false;
        }

        /// <summary>
        /// Notification that is given to this driver.
        /// </summary>
        public override void Notify_Starting()
        {
            base.Notify_Starting();

            if (this.TargetThingA is Apparel apparel && pawn.apparel.Contains(apparel))
            {
                // time needed to unequip
                _duration = (int)(apparel.GetStatValue(StatDefOf.EquipDelay) * 60f);
            }
            else if (this.TargetThingA is ThingWithComps equipment)
            {
                if (pawn.equipment.Contains(equipment))
                {
                    // vanilla time to drop any equipment
                    _duration = 30;
                }
            }

            if (_container)
            {
                Thing container = job.targetB.Thing;
                if (container is Building)
                {
                    _duration = Math.Max(container.def.building.haulToContainerDuration, _duration);
                }
            }
        }

        /// <summary>
        /// Give instructions on how to do job.
        /// </summary>
        /// <returns> A list of instructions. </returns>
        protected override IEnumerable<Toil> MakeNewToils()
        {
            this.FailOnDestroyedOrNull(TargetIndex.A);
            this.FailOnBurningImmobile(TargetIndex.B);

            Toil carryToDestination, timeToWait, placeThing;
            carryToDestination = timeToWait = placeThing = null;

            if (_cellStorage)
            {
                // This toil uses CarriedThing to validate state
                // There is a Harmony patch to trick the validation to think
                // the pawn has Carryied Thing on it.
                // Check Pawn_CarryTracker_CarriedThing_AwesomeInventory_Patch.cs
                carryToDestination = Toils_Haul.CarryHauledThingToCell(TargetIndex.B);

                // PlaceHaulThingInCell will recursively call itself until it runs out of cell on map.
                placeThing = Toils_Haul.PlaceHauledThingInCell(TargetIndex.B, carryToDestination, true, true);
            }

            if (_container)
            {
                carryToDestination = Toils_Haul.CarryHauledThingToContainer();
                placeThing = Toils_Haul.DepositHauledThingInContainer(TargetIndex.B, TargetIndex.C);
            }

            // When set true, it allows toils to be execute consecutively in one tick
            // Check JobDirver.DriverTick() for more information
            carryToDestination.atomicWithPrevious = true;

            timeToWait = Toils_General.Wait(_duration).WithProgressBarToilDelay(TargetIndex.A);
            timeToWait.JumpIf(() => _duration == 0, placeThing);
            timeToWait.atomicWithPrevious = true;

            placeThing.AddPreInitAction(() =>
            {
                // Move things to carry tracker
                // Where things are actually transfered to CarryTracker
                TargetThingA.holdingOwner.TryTransferToContainer(TargetThingA, pawn.carryTracker.innerContainer);
            });
            placeThing.AddFinishAction(() =>
            {
                if (!(this.job is UnloadApparelJob))
                    TargetThingA.SetForbidden(true, false);

                if (_container)
                {
                    if (TargetThingA.holdingOwner.Owner == pawn && TargetThingA.stackCount > 0)
                    {
                        StoragePriority currentPriority = StoreUtility.CurrentStoragePriorityOf(TargetThingA);
                        if (StoreUtility.TryFindBestBetterStorageFor(TargetThingA, pawn, pawn.Map, currentPriority, pawn.Faction, out IntVec3 foundCell, out IHaulDestination haulDestination))
                        {
                            this.Init(foundCell, haulDestination);
                            this.JumpToToil(carryToDestination);
                        }
                    }
                }
            });

            placeThing.atomicWithPrevious = true;

            yield return carryToDestination;
            yield return timeToWait;
            yield return placeThing;

            Toil statusCheck = Toils_General.DoAtomic(
                () =>
                {
                    if (TargetThingA.holdingOwner.Owner.ParentHolder == pawn && TargetThingA.stackCount > 0)
                    {
                        JobFailReason.Is(UIText.NoEmptyPlaceLower);
                        Messages.Message(
                            UIText.NoEmptyPlaceLower.TranslateSimple()
                            , new TargetInfo(this.pawn.PositionHeld, this.pawn.MapHeld), MessageTypeDefOf.NeutralEvent);
                    }

                    if (SimpleSidearmUtility.IsActive)
                    {
                        SimpleSidearmUtility.RemoveWeaponFromMemory(this.pawn, TargetThingA);
                    }
                });

            yield return statusCheck;
        }

        /// <summary>
        /// Initialize states based on the returned result from <see cref="StoreUtility.TryFindBestBetterStorageFor"/>.
        /// </summary>
        /// <param name="foundCell"> Found cell for storage. </param>
        /// <param name="haulDestination"> Destination object for hauling. </param>
        protected virtual void Init(IntVec3 foundCell, IHaulDestination haulDestination)
        {
            if (haulDestination is ISlotGroupParent)
            {
                this.job.targetB = foundCell;
                this.job.haulMode = HaulMode.ToCellStorage;
                _cellStorage = true;
            }

            if (haulDestination is Thing thing && thing.TryGetInnerInteractableThingOwner() != null)
            {
                this.job.targetB = thing;
                this.job.haulMode = HaulMode.ToContainer;
                _container = true;
            }
        }
    }
}
