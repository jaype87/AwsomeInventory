// <copyright file="UpdateStuffAndMerge.cs" company="Zizhen Li">
// Copyright (c) 2019 - 2020 Zizhen Li. All rights reserved.
// Licensed under the LGPL-3.0-only license. See LICENSE.md file in the project root for full license information.
// </copyright>

#if UnitTest
using System.Collections.Generic;
using System.Linq;
using Verse;
using RimWorld;

namespace RPG_Inventory_Remake_Common.UnitTest
{
    public class UpdateStuffAndMerged : Test_UpdateItem
    {
        private int _count;
        private Thing _innerThing;
        private List<Thing> _thingsNotUpdated;

        public override void Setup()
        {
            _innerThing = loadoutInstance[things[0]].Thing;
            _count = loadoutInstance.Count;
            _thingsNotUpdated = things.Where(t => t != things[0]).ToList();
        }

        public override void Run(out bool result)
        {
            loadoutInstance.UpdateItem(_innerThing, ThingDefOf.Steel);

            result =
                // Check count
                AssertUtility.Expect(loadoutInstance.Count, _count - 1, string.Format(StringResource.ObjectCount, nameof(loadoutInstance)))
                && // Check if _innerThing is re-added
                AssertUtility.Contains(loadoutInstance, _innerThing, nameof(loadoutInstance), _innerThing.LabelCapNoCount)
                && // Cehck stackcount
                AssertUtility.Expect(loadoutInstance[_innerThing].Thing.stackCount, 2, _innerThing.LabelCapNoCount)
                && // Check stuff is set correctly
                AssertUtility.Expect(_innerThing.Stuff, ThingDefOf.Steel, nameof(_innerThing.Stuff))
                ;

            // Check integrity of the rest
            foreach (Thing thing in _thingsNotUpdated)
            {
                result &= AssertUtility.Contains(loadoutInstance, thing, nameof(loadoutInstance), thing.LabelCapNoCount);
            }
        }

        public override void Cleanup()
        {
            loadoutInstance.Remove(_innerThing);
            loadoutInstance.Add(things[0]);
            loadoutInstance.Add(things[2]);
        }
    }
}
#endif