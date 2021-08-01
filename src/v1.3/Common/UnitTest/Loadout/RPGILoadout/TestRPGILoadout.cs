// <copyright file="TestAILoadout.cs" company="Zizhen Li">
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
using RPGIResource;

#if RPG_Inventory_Remake
using RPG_Inventory_Remake.Loadout;
using RPG_Inventory_Remake;
#endif

namespace RPG_Inventory_Remake_Common.UnitTest
{
    public class TestAILoadout : RPGIUnitTest
    {
        public override void Setup()
        {
            Tests.Add(new Test_AddItem());
            Tests.Add(new Test_RemoveItem());
            Tests.Add(new Test_UpdateItem());
        }
    }
}
#endif