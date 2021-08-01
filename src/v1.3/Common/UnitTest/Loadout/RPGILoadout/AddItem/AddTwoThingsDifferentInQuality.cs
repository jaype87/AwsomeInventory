// <copyright file="AddTwoThingsDifferentInQuality.cs" company="Zizhen Li">
// Copyright (c) 2019 - 2020 Zizhen Li. All rights reserved.
// Licensed under the LGPL-3.0-only license. See LICENSE.md file in the project root for full license information.
// </copyright>

#if UnitTest
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;

namespace RPG_Inventory_Remake_Common.UnitTest
{
    public class AddTwoThingsDifferentInQuality : Test_AddItem
    {
        public override void Setup()
        {
            loadoutInstance.Add(things[1], false);
            loadoutInstance.Add(things[2], false);
        }

        public override void Run(out bool result)
        {
            result = true;
            // Test if items are seperated
            result &=
                AssertUtility.Expect(loadoutInstance.Count(), 2, string.Format(StringResource.ObjectCount, nameof(loadoutInstance)));
            // Test if CachedList is in sync
            result &=
                AssertUtility.Expect(loadoutInstance.CachedList.Count, 2, string.Format(StringResource.ObjectCount, nameof(loadoutInstance.CachedList)));
            // Test if the instance of the item is shared between source and loadout
            result &=
                AssertUtility.AreEqual(loadoutInstance.CachedList[0], loadoutInstance[loadoutInstance.CachedList[0]].Thing, nameof(loadoutInstance), nameof(loadoutInstance.CachedList));
            result &=
                AssertUtility.AreEqual(loadoutInstance.CachedList[1], loadoutInstance[loadoutInstance.CachedList[1]].Thing, nameof(loadoutInstance), nameof(loadoutInstance.CachedList));
            // Test if it stores the correct thing
            result &=
                AssertUtility.Expect(loadoutInstance.Contains(things[1]), true, string.Format(StringResource.ThingHas, nameof(loadoutInstance), things[1].LabelNoCount));
            result &=
                AssertUtility.Expect(loadoutInstance.Contains(things[2]), true, string.Format(StringResource.ThingHas, nameof(loadoutInstance), things[2].LabelNoCount));
            // Test whether the stackcount is correct
            result &=
                AssertUtility.Expect(loadoutInstance.CachedList[0].stackCount, 1, string.Format(StringResource.ObjectCount, things[1].LabelCapNoCount));
            result &=
                AssertUtility.Expect(loadoutInstance.CachedList[1].stackCount, 1, string.Format(StringResource.ObjectCount, things[2].LabelCapNoCount));
        }

        public override void Cleanup()
        {
            loadoutInstance.Clear();
        }
    }
}
#endif