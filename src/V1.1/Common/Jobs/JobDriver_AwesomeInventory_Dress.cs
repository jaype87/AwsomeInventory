// <copyright file="JobDriver_AwesomeInventory_Dress.cs" company="Zizhen Li">
// Copyright (c) 2019 - 2020 Zizhen Li. All rights reserved.
// Licensed under the LGPL-3.0-only license. See LICENSE.md file in the project root for full license information.
// </copyright>

using System.Collections.Generic;
using AwesomeInventory.UI;
using RimWorld;
using Verse;
using Verse.AI;

namespace AwesomeInventory.Jobs
{
    /// <summary>
    /// Equip apparel and put the replaced into inventory instead of dropping it on the map.
    /// </summary>
    public class JobDriver_AwesomeInventory_Dress : JobDriver
    {
        private int _duration;
        private Apparel _apparel;

        /// <summary>
        /// Make reservation on apparel to undress.
        /// </summary>
        /// <param name="errorOnFailed"> If true, logs error when fails. </param>
        /// <returns> Returns true, if all reservations are made. </returns>
        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            return this.pawn.Reserve(this.TargetThingA, this.job, errorOnFailed: errorOnFailed);
        }

        /// <summary>
        /// Returns a report string for displaying on pawn's HUD.
        /// </summary>
        /// <returns> A report string. </returns>
        public override string GetReport()
        {
            if ((this.job as DressJob)?.ForceWear ?? false)
                return UIText.AIForceWear.Translate(TargetThingA.LabelShort);
            else
                return UIText.AIWear.Translate(TargetThingA.LabelShort);
        }

        /// <summary>
        /// When notified job is about to start, calculate unequip delay for underlying apparel.
        /// </summary>
        public override void Notify_Starting()
        {
            base.Notify_Starting();
            _apparel = TargetThingA as Apparel;
            _duration = (int)(_apparel.GetStatValue(StatDefOf.EquipDelay) * 60f);

            List<Apparel> wornApparel = pawn.apparel.WornApparel;
            for (int num = wornApparel.Count - 1; num >= 0; num--)
            {
                if (!ApparelUtility.CanWearTogether(_apparel.def, wornApparel[num].def, pawn.RaceProps.body))
                {
                    _duration += (int)(wornApparel[num].GetStatValue(StatDefOf.EquipDelay) * 60f);
                }
            }
        }

        /// <summary>
        /// Make instruction on what to do.
        /// </summary>
        /// <returns> A list of instruction. </returns>
        protected override IEnumerable<Toil> MakeNewToils()
        {
            this.FailOnDestroyedOrNull(TargetIndex.A);

            if (TargetThingA.PositionHeld != pawn.Position)
            {
                this.FailOnBurningImmobile(TargetIndex.A);
                yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.ClosestTouch).FailOnDespawnedNullOrForbidden(TargetIndex.A);
            }

            yield return Toils_General.Wait(_duration).WithProgressBarToilDelay(TargetIndex.A);
            yield return new Toil()
            {
                initAction = () =>
                {
                    // Safe to call Remove even if the apparel is not in inventory.
                    pawn.inventory.innerContainer.Remove(_apparel);
                    Utility.Wear(pawn, _apparel, false);
                    if ((this.job as DressJob)?.ForceWear ?? false)
                    {
                        pawn.outfits.forcedHandler.SetForced(_apparel, true);
                    }
                },
                atomicWithPrevious = true,
            };
        }
    }
}
