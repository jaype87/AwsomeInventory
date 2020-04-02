// <copyright file="IntegrationTestHandler.cs" company="Zizhen Li">
// Copyright (c) 2019 - 2020 Zizhen Li. All rights reserved.
// Licensed under the LGPL-3.0-only license. See LICENSE.md file in the project root for full license information.
// </copyright>

#if UnitTest
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RPG_Inventory_Remake_Common.UnitTest
{
    public class IntegrationTestHandler
    {
        public static Dictionary<Type, RPGIIntegrationTest> Tests;

        public static void SetFlag(TestFlags flag, Type type, object value)
        {
            Tests[type].SetFlag(flag, value);
        }
    }
}
#endif