﻿// <copyright file="GameComponent_DefManager.cs" company="Zizhen Li">
// Copyright (c) 2019 - 2020 Zizhen Li. All rights reserved.
// Licensed under the LGPL-3.0-only license. See LICENSE.md file in the project root for full license information.
// </copyright>

using System.Collections.Generic;
using System.Linq;
using AwesomeInventory.Loadout;
using Verse;

namespace AwesomeInventory
{
    public class GameComponent_DefManager : GameComponent
    {
        private static HashSet<ThingDef> _allSuitableDefs;

        /// <summary>
        /// Initializes a new instance of the <see cref="GameComponent_DefManager"/> class.
        /// </summary>
        /// <param name="game"> Current game. </param>
        public GameComponent_DefManager(Game game)
        {
            AIDebug.Init(new Logger());
        }

        /// <summary>
        /// Return a copy of a cached AllSuitableDefs.
        /// </summary>
        /// <returns> A set of defs available to be selected in loadout dialog. </returns>
        public static HashSet<ThingDef> GetSuitableDefs()
        {
            return new HashSet<ThingDef>(_allSuitableDefs.AsEnumerable(), _allSuitableDefs.Comparer);
        }

        /// <summary>
        /// Get called by the game just before the game is about to start.
        /// </summary>
        public override void FinalizeInit()
        {
            List<ThingDef> allSuitableDefs;
            allSuitableDefs = DefDatabase<ThingDef>
                                .AllDefsListForReading
                                .Where(thingDef => (thingDef.EverHaulable || thingDef.Minifiable)
                                                && !thingDef.menuHidden
                                                && IsSuitableThingDef(thingDef))
                                .ToList();
            _allSuitableDefs = new HashSet<ThingDef>(allSuitableDefs, new CompareThingDef());
            _allSuitableDefs.AddRange(DefDatabase<AIGenericDef>.AllDefs);
        }

        private bool IsSuitableThingDef(ThingDef td)
        {
            return td.Minifiable
                    || (td.thingClass != typeof(Corpse)
                        && !td.IsFrame
                        && !td.destroyOnDrop);
        }

        private class CompareThingDef : EqualityComparer<ThingDef>
        {
            public override bool Equals(ThingDef x, ThingDef y)
            {
                return x.defName == y.defName;
            }

            public override int GetHashCode(ThingDef obj)
            {
                return obj.defName.GetHashCode();
            }
        }
    }
}
