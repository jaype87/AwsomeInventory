// <copyright file="RemoveAll.cs" company="Zizhen Li">
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
    public class RemoveAll : Test_RemoveItem
    {
        public override void Setup()
        {
            loadoutInstance.Clear();
        }

        public override void Run(out bool result)
        {
            result = true;

            result &=
                    // Check count
                    AssertUtility.Expect(loadoutInstance.Count, 0, string.Format(StringResource.ObjectCount, nameof(loadoutInstance)))
                    &&
                    AssertUtility.Expect(loadoutInstance.CachedList.Count, 0, string.Format(StringResource.ObjectCount, nameof(loadoutInstance.CachedList)))
                    ;
        }

        public override void Cleanup()
        {
            foreach(Thing thing in things)
            {
                loadoutInstance.Add(thing);
            }
        }
    }
}
#endif