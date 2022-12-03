// <copyright file="DefManager.cs" company="Zizhen Li">
// Copyright (c) 2019 - 2020 Zizhen Li. All rights reserved.
// Licensed under the LGPL-3.0-only license. See LICENSE.md file in the project root for full license information.
// </copyright>

using System.Collections.Generic;
using System.Linq;
using AwesomeInventory.Loadout;
using RimWorld;
using Verse;

namespace AwesomeInventory
{
    /// <summary>
    /// Manages defs for the loadout window.
    /// </summary>
    public class DefManager : GameComponent
    {
        private static HashSet<ThingDef> _allSuitableDefs;

        /// <summary>
        /// Initializes a new instance of the <see cref="DefManager"/> class.
        /// </summary>
        /// <param name="game"> Current game. </param>
        public DefManager(Game game)
        {
        }

        /// <summary>
        /// Gets all Suitable Defs for displaying in loadout.
        /// </summary>
        /// <returns> A set of defs available to be selected in loadout dialog. </returns>
        public static HashSet<ThingDef> SuitableDefs => _allSuitableDefs;

        /// <summary>
        /// Get called by the game just before the game is about to start.
        /// </summary>
        public override void FinalizeInit()
        {
            List<ThingDef> allSuitableDefs;
            allSuitableDefs = DefDatabase<ThingDef>
                                .AllDefsListForReading
                                .Where(thingDef => IsSuitableThingDef(thingDef))
                                .ToList();
            _allSuitableDefs = new HashSet<ThingDef>(allSuitableDefs, ThingDefComparer.Instance);
            _allSuitableDefs.AddRange(DefDatabase<AIGenericDef>.AllDefs);
        }

        private bool IsSuitableThingDef(ThingDef td)
        {
            return (td.EverHaulable
                && !td.IsFrame
                && !td.destroyOnDrop
                && !typeof(UnfinishedThing).IsAssignableFrom(td.thingClass)
                && !typeof(MinifiedThing).IsAssignableFrom(td.thingClass))
                ||
                td.Minifiable;
        }
    }
}
