// <copyright file="AddMergeableItemTwice.cs" company="Zizhen Li">
// Copyright (c) 2019 - 2020 Zizhen Li. All rights reserved.
// Licensed under the LGPL-3.0-only license. See LICENSE.md file in the project root for full license information.
// </copyright>

#if UnitTest
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using UnityEngine;
using Verse;

namespace RPG_Inventory_Remake_Common.UnitTest
{
    public class AddMergeableItemTwice : Test_AddItem
    {
        public override void Setup()
        {
            loadoutInstance.Add(things[3], false);
            loadoutInstance.Add(things[3], false);
        }

        public override void Run(out bool result)
        {
            result = true;

            // Test if items are merged. Even it has a stack limit of 1, it doesn't matter in loadout
            result &=
                AssertUtility.Expect(loadoutInstance.Count, 1, string.Format(StringResource.ObjectCount, nameof(loadoutInstance)));

            // Test if CachedList is in sync
            result &=
                AssertUtility.Expect(loadoutInstance.CachedList.Count, 1, string.Format(StringResource.ObjectCount, nameof(loadoutInstance.CachedList)));

            // Test if the instance of the item is shared between the dictionary and the cachedList
            result &=
                AssertUtility.AreEqual(loadoutInstance.CachedList[0], loadoutInstance[loadoutInstance.CachedList[0]].Thing, nameof(loadoutInstance.CachedList), nameof(loadoutInstance));

            // Test if it stores the correct thing
            result &=
                AssertUtility.Expect(loadoutInstance.Contains(things[3]), true, string.Format(StringResource.ThingHas, nameof(loadoutInstance), things[3].LabelNoCount));

            // Test whether the stackcount is correct
            result &=
                AssertUtility.Expect(loadoutInstance.CachedList[0].stackCount, things[3].stackCount * 2, string.Format(StringResource.ObjectCount, things[3].LabelCapNoCount));
        }

        public override void Cleanup()
        {
            loadoutInstance.Clear();
        }
    }
}
#endif