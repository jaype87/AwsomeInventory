// <copyright file="JobDriver_AwesomeInventory_Undress.cs" company="Zizhen Li">
// Copyright (c) 2019 - 2020 Zizhen Li. All rights reserved.
// Licensed under the LGPL-3.0-only license. See LICENSE.md file in the project root for full license information.
// </copyright>

using System.Collections.Generic;
using RimWorld;
using Verse.AI;

namespace AwesomeInventory.Jobs
{
    /// <summary>
    /// When apparel is being removed, instead of dropping it on the map, it is transfered to pawn's inventory.
    /// </summary>
    public class JobDriver_AwesomeInventory_Undress : JobDriver
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
        /// When notified job is about to start, calculate unequip delay for underlying apparel.
        /// </summary>
        public override void Notify_Starting()
        {
            base.Notify_Starting();
            _apparel = TargetThingA as Apparel;
            _duration = (int)(_apparel.GetStatValue(StatDefOf.EquipDelay) * 60f);
        }

        /// <summary>
        /// Make instruction on what to do.
        /// </summary>
        /// <returns> A list of instruction. </returns>
        protected override IEnumerable<Toil> MakeNewToils()
        {
            this.FailOnDestroyedOrNull(TargetIndex.A);

            yield return Toils_General.Wait(_duration).WithProgressBarToilDelay(TargetIndex.A);
            yield return new Toil()
            {
                initAction = () =>
                {
                    pawn.apparel.Remove(_apparel);
                    pawn.inventory.innerContainer.TryAdd(_apparel);
                },
                atomicWithPrevious = true,
            };
        }
    }
}
