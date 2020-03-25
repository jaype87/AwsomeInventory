// <copyright file="RemoveAllButOne.cs" company="Zizhen Li">
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
    public class RemoveAllButOne : Test_RemoveItem
    {
        public override void Setup()
        {
        }

        public override void Run(out bool result)
        {
            result = true;
            for (int i = 0; i < things.Count; i++)
            {
                List<Thing> thingsToRemove = things.Where(t => t != things[i]).ToList();
                foreach (Thing thing in thingsToRemove)
                {
                    loadoutInstance.Remove(thing);
                }

                result &=
                        // Check count
                        AssertUtility.Expect(loadoutInstance.Count, 1, string.Format(StringResource.ObjectCount, nameof(loadoutInstance)))
                        &&
                        AssertUtility.Expect(loadoutInstance.CachedList.Count, 1, string.Format(StringResource.ObjectCount, nameof(loadoutInstance.CachedList)))
                        && // Check integrity
                        AssertUtility.Contains(loadoutInstance, things[i], nameof(loadoutInstance), things[i].LabelCapNoCount)
                        && // Check reference equality
                        AssertUtility.AreEqual(loadoutInstance.CachedList[0], loadoutInstance[things[i]].Thing, nameof(loadoutInstance.CachedList), nameof(loadoutInstance));
                        ;

                foreach (Thing thing in thingsToRemove)
                {
                    loadoutInstance.Add(thing);
                }
            }
        }

        public override void Cleanup()
        {
        }
    }
}
#endif