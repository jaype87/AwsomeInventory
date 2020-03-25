// <copyright file="RemoveSameItemTwice.cs" company="Zizhen Li">
// Copyright (c) 2019 - 2020 Zizhen Li. All rights reserved.
// Licensed under the LGPL-3.0-only license. See LICENSE.md file in the project root for full license information.
// </copyright>

#if UnitTest
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using RimWorld;

namespace RPG_Inventory_Remake_Common.UnitTest
{
    public class RemoveSameItemTwice : Test_RemoveItem
    {
        private readonly int itemCount = loadoutInstance.Count;

        public override void Setup()
        {
        }

        public override void Run(out bool result)
        {
            result = true;
            for (int i = 0; i < things.Count; i++)
            {
                loadoutInstance.Remove(things[i], out Thing removed);
                loadoutInstance.Remove(things[i]);
                List<Thing> remainings = things.Where(t => t != things[i]).ToList();

                result &=
                        // Check count
                        AssertUtility.Expect(loadoutInstance.Count, itemCount - 1, string.Format(StringResource.ObjectCount, nameof(loadoutInstance)))
                        &&
                        AssertUtility.Expect(loadoutInstance.CachedList.Count, itemCount - 1, string.Format(StringResource.ObjectCount, nameof(loadoutInstance.CachedList)))
                        && // Check integrity
                        AssertUtility.Expect(loadoutInstance.Contains(things[i]), false, string.Format(StringResource.ThingHas, nameof(loadoutInstance), things[i].LabelNoCount))
                        &&
                        AssertUtility.Expect(loadoutInstance.CachedList.Contains(removed), false, string.Format(StringResource.ThingHas, nameof(loadoutInstance.CachedList), things[i].LabelNoCount))
                        ;

                for (int j = 0; j < remainings.Count; j++)
                {
                    result &=
                        // Check integrity
                        AssertUtility.Expect(loadoutInstance.Contains(remainings[j]), true, string.Format(StringResource.ThingHas, nameof(loadoutInstance), remainings[j].LabelNoCount))
                        &&
                        AssertUtility.Expect(loadoutInstance.CachedList.Contains(loadoutInstance[remainings[j]].Thing), true, string.Format(StringResource.ThingHas, nameof(loadoutInstance.CachedList), remainings[j].LabelNoCount))
                        && // Check stackcount
                        AssertUtility.Expect(loadoutInstance[remainings[j]].Thing.stackCount, remainings[j].stackCount, string.Format(StringResource.ExpectedString, remainings[j].LabelNoCount, 2, loadoutInstance[remainings[j]].Thing.stackCount))
                        ;
                }
                loadoutInstance.Add(things[i], false);
            }
        }

        public override void Cleanup()
        {
        }
    }
}
#endif