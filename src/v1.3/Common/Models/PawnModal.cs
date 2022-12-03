// <copyright file="PawnModal.cs" company="Zizhen Li">
// Copyright (c) 2019 - 2020 Zizhen Li. All rights reserved.
// Licensed under the LGPL-3.0-only license. See LICENSE.md file in the project root for full license information.
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;

namespace AwesomeInventory.UI
{
    /// <summary>
    /// A pawn modal used for drawing gear tab.
    /// </summary>
    public class PawnModel
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PawnModel"/> class.
        /// </summary>
        /// <param name="selPawn"> Selected pawn. </param>
        /// <param name="selThing"> Selected corpse. </param>
        public PawnModel(Pawn selPawn, Thing selThing)
        {
            ValidateArg.NotNull(selPawn, nameof(selPawn));

            this.Pawn = SelectPawnForGear(selPawn, selThing);
            CanControl = CheckControl(selPawn);
            CanControlColonist = CheckControlColonist(selPawn);
            Bpgroups = selPawn.RaceProps.body.AllParts
                .SelectMany(r => r.groups)
                .Distinct()
                .OrderByDescending(p => p.listOrder);
        }

        /// <summary>
        /// Gets a value indicating whether a pawn can be contronlled.
        /// </summary>
        public bool CanControl { get; }

        /// <summary>
        /// Gets a value indicating whether a colonist can be controlled.
        /// </summary>
        public bool CanControlColonist { get; }

        /// <summary>
        /// Gets or sets the pawn used in this modal.
        /// </summary>
        public Pawn Pawn { get; set; }

        /// <summary>
        /// Gets a collection of <see cref="BodyPartGroupDef"/>.
        /// </summary>
        public IEnumerable<BodyPartGroupDef> Bpgroups { get; }

        private bool CheckControl(Pawn selPawn)
        {
            return !selPawn.Downed &&
                    !selPawn.InMentalState &&
                    (selPawn.Faction == Faction.OfPlayer || selPawn.IsPrisonerOfColony) &&
                    (!selPawn.IsPrisonerOfColony || !selPawn.Spawned || selPawn.Map.mapPawns.AnyFreeColonistSpawned) &&
                    (!selPawn.IsPrisonerOfColony || (!PrisonBreakUtility.IsPrisonBreaking(selPawn) &&
                    (selPawn.CurJob == null || !selPawn.CurJob.exitMapOnArrival)));
        }

        private bool CheckControlColonist(Pawn selPawn)
        {
            return this.CanControl && selPawn.IsColonistPlayerControlled;
        }

        private Pawn SelectPawnForGear(Pawn selPawn, Thing selThing)
        {
            if (selPawn != null)
            {
                return selPawn;
            }

            Corpse corpse = selThing as Corpse;
            if (corpse != null)
            {
                return corpse.InnerPawn;
            }

            throw new InvalidOperationException("Gear tab on non-pawn non-corpse " + selThing);
        }
    }
}
