// <copyright file="AddOneItem.cs" company="Zizhen Li">
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

#if RPG_Inventory_Remake
using RPG_Inventory_Remake.Loadout;
using RPG_Inventory_Remake;
#endif

namespace RPG_Inventory_Remake_Common.UnitTest
{
    public class AddOneItem : Test_AddItem
    {
        public override void Setup()
        {
            loadoutInstance.Add(things[0], false);
        }

        public override void Run(out bool result)
        {
            if (loadoutInstance.Count == 1)
            {
                Thing thing = loadoutInstance.CachedList[0];
                if (thing == loadoutInstance[things[0]].Thing)
                {
                    result = true;
                    return;
                }

                Log.Error(string.Format(StringResource.ObjectsAreNotEqual, thing.GetType(), loadoutInstance[thing].Thing.GetType()));
                result = false;
            }
            else
            {
                Log.Error(string.Format(
                    StringResource.ExpectedString
                    , string.Format(StringResource.ObjectCount, nameof(loadoutInstance))
                    , 1
                    , loadoutInstance.Count));
                result = false;
            }
        }

        public override void Cleanup()
        {
            loadoutInstance.Clear();
        }
    }
}
#endif