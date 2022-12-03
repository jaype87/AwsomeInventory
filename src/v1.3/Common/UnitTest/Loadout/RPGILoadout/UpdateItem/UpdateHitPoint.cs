// <copyright file="UpdateHitPoint.cs" company="Zizhen Li">
// Copyright (c) 2019 - 2020 Zizhen Li. All rights reserved.
// Licensed under the LGPL-3.0-only license. See LICENSE.md file in the project root for full license information.
// </copyright>

#if UnitTest
using System.Collections.Generic;
using System.Linq;
using AwesomeInventory.Loadout;
using RimWorld;
using Verse;

namespace RPG_Inventory_Remake_Common.UnitTest
{
    public class UpdateHitPoint : Test_UpdateItem
    {
        private Thing _innerThing;
        private ThingFilterAll _filter;
        private List<Thing> _thingsNotUpdated;
        private List<FloatRange> _ranges;
        private List<FloatRange> _rangeOfOthers;

        public override void Setup()
        {
            _innerThing = loadoutInstance[things[1]].Thing;
            _thingsNotUpdated = things.Where(t => t != things[1]).ToList();
            _filter = loadoutInstance[_innerThing];

            _ranges = new List<FloatRange>()
            {
                new FloatRange(0,0),
                new FloatRange(100, 100),
                new FloatRange(1,99)
            };

            _rangeOfOthers = new List<FloatRange>();
            foreach (Thing thing in _thingsNotUpdated)
            {
                _rangeOfOthers.Add(loadoutInstance[thing].AllowedHitPointsPercents);
            }
        }

        public override void Run(out bool result)
        {
            result = true;
            foreach (FloatRange range in _ranges)
            {
                loadoutInstance.UpdateItem(_innerThing, range);
                result &=
                    AssertUtility.Expect(_filter.AllowedHitPointsPercents.TrueMin, range.TrueMin, nameof(_filter.AllowedHitPointsPercents))
                    &&
                    AssertUtility.Expect(_filter.AllowedHitPointsPercents.TrueMax, range.TrueMax, nameof(_filter.AllowedHitPointsPercents))
                    ;
                for (int i = 0; i < _thingsNotUpdated.Count; i++)
                {
                    result &=
                        AssertUtility.Expect(loadoutInstance[_thingsNotUpdated[i]].AllowedHitPointsPercents.TrueMin, _rangeOfOthers[i].TrueMin, nameof(_thingsNotUpdated))
                        &&
                        AssertUtility.Expect(loadoutInstance[_thingsNotUpdated[i]].AllowedHitPointsPercents.TrueMax, _rangeOfOthers[i].TrueMax, nameof(_thingsNotUpdated))
                        ;
                }
            }
        }

        public override void Cleanup()
        {
            loadoutInstance.Remove(_innerThing);
            loadoutInstance.Add(things[1]);
        }
    }
}
#endif