// <copyright file="JobDriver_AwesomeInventory_MapEquip.cs" company="Zizhen Li">
// Copyright (c) 2019 - 2020 Zizhen Li. All rights reserved.
// Licensed under the LGPL-3.0-only license. See LICENSE.md file in the project root for full license information.
// </copyright>

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using RimWorld;
using Verse;
using Verse.AI;

namespace AwesomeInventory.Jobs
{
    /// <summary>
    /// Equip weapon on the map and put the previously equipped into inventory.
    /// </summary>
    [SuppressMessage("Naming", "CA1707:Identifiers should not contain underscores", Justification = "Follow naming convention")]
    public class JobDriver_AwesomeInventory_MapEquip : JobDriver
    {
        /// <summary>
        /// Make reservation before action.
        /// </summary>
        /// <param name="errorOnFailed"> If true, throw errors on failed. </param>
        /// <returns> Returns true if a reservation is made. </returns>
        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            return pawn.Reserve(job.targetA, job, 1, -1, null, errorOnFailed);
        }

        /// <summary>
        /// Make detailed instruction on how to do the job.
        /// </summary>
        /// <returns> Instructions on what to do. </returns>
        protected override IEnumerable<Toil> MakeNewToils()
        {
            this.FailOnDestroyedOrNull(TargetIndex.A);
            yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.ClosestTouch).FailOnDespawnedNullOrForbidden(TargetIndex.A);
            yield return new Toil()
            {
                initAction = () =>
                {
                    ThingWithComps thingWithComps = (ThingWithComps)job.targetA.Thing;
                    ThingWithComps thingWithComps2 = null;
                    if (thingWithComps.def.stackLimit > 1 && thingWithComps.stackCount > 1)
                    {
                        thingWithComps2 = (ThingWithComps)thingWithComps.SplitOff(1);
                    }
                    else
                    {
                        thingWithComps2 = thingWithComps;
                        thingWithComps2.DeSpawn();
                    }

                    // put away equiped weapon first
                    if (pawn.equipment.Primary != null)
                    {
                        if (!pawn.equipment.TryTransferEquipmentToContainer(pawn.equipment.Primary, pawn.inventory.innerContainer))
                        {
                            // if failed, drop the weapon
                            pawn.equipment.MakeRoomFor(thingWithComps2);
                        }
                    }

                    // There is a chance, albeit minor, even MakeRommFor can fail.
                    if (pawn.equipment.Primary == null)
                    {
                        // unregister new weapon in the inventory list and register it in equipment list.
                        pawn.equipment.GetDirectlyHeldThings().TryAddOrTransfer(thingWithComps2);
                    }
                    else
                    {
                        Messages.Message("CannotEquip".Translate(thingWithComps2.LabelShort), MessageTypeDefOf.RejectInput, false);
                    }
                },
            };
        }
    }
}
