﻿// <copyright file="UnitTest_Root.cs" company="Zizhen Li">
// Copyright (c) 2019 - 2020 Zizhen Li. All rights reserved.
// Licensed under the LGPL-3.0-only license. See LICENSE.md file in the project root for full license information.
// </copyright>

#if UnitTest
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace RPG_Inventory_Remake_Common.UnitTest
{
    public class UnitTest_Root : RPGIUnitTest
    {
        public override void Setup()
        {
            Tests.Add(new TestRPGILoadout());
            Tests.Add(new TestcompRPGILoadout());
        }
    }
}
#endif