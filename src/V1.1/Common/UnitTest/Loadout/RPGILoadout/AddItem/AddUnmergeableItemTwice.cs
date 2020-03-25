// <copyright file="AddUnmergeableItemTwice.cs" company="Zizhen Li">
// Copyright (c) 2019 - 2020 Zizhen Li. All rights reserved.
// Licensed under the LGPL-3.0-only license. See LICENSE.md file in the project root for full license information.
// </copyright>

#if UnitTest
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AwesomeInventory.Loadout;
using RimWorld;
using Verse;

namespace RPG_Inventory_Remake_Common.UnitTest
{
    public class AddUnmergeableItemTwice : Test_AddItem
    {
        private Thing _toAdd;
        public override void Setup()
        {
            _toAdd = things[0].DeepCopy();
            loadoutInstance.Add(_toAdd, true);
            loadoutInstance.Add(_toAdd, true);
        }

        public override void Run(out bool result)
        {
            result = true;
            result &=
                // Test if items are merged. Even it has a stack limit of 1, it doesn't matter in loadout
                AssertUtility.Expect(loadoutInstance.Count(), 1, string.Format(StringResource.ObjectCount, nameof(loadoutInstance)))
                // Test if CachedList is in sync
                &&
                AssertUtility.Expect(loadoutInstance.CachedList.Count, 1, string.Format(StringResource.ObjectCount, nameof(loadoutInstance.CachedList)))
                // Test if the instance of the item is shared between source and loadout
                &&
                AssertUtility.AreEqual(_toAdd, loadoutInstance[things[0]].Thing, nameof(_toAdd), nameof(loadoutInstance))
                &&
                AssertUtility.AreEqual(_toAdd, loadoutInstance.CachedList[0], nameof(_toAdd), nameof(loadoutInstance.CachedList))
                // Test whether the stackcount is correct
                &&
                AssertUtility.Expect(loadoutInstance.CachedList[0].stackCount, 2, string.Format(StringResource.ObjectCount, _toAdd.LabelCapNoCount));
        }

        public override void Cleanup()
        {
            loadoutInstance.Clear();
        }
    }
}
#endif