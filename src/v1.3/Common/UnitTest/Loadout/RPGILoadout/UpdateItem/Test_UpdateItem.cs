// <copyright file="Test_UpdateItem.cs" company="Zizhen Li">
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
    public class Test_UpdateItem : RPGIUnitTest
    {
        public static AILoadout loadoutInstance = new AILoadout();
        public static List<Thing> things = new List<Thing>();

        static Test_UpdateItem()
        {
            ThingDef thingDef = ThingDef.Named("MeleeWeapon_Knife");
            ThingStuffPairWithQuality pair1 = new ThingStuffPairWithQuality
            {
                thing = thingDef,
                stuff = ThingDefOf.Plasteel,
                quality = QualityCategory.Legendary
            };

            ThingStuffPairWithQuality pair2 = new ThingStuffPairWithQuality
            {
                thing = thingDef,
                stuff = ThingDefOf.Steel,
                quality = QualityCategory.Good
            };

            ThingStuffPairWithQuality pair3 = new ThingStuffPairWithQuality
            {
                thing = thingDef,
                stuff = ThingDefOf.Steel,
                quality = QualityCategory.Legendary
            };

            Thing finemeal = ThingMaker.MakeThing(ThingDefOf.MealFine);
            finemeal.stackCount = 11;

            things.Add(pair1.MakeThing());
            things.Add(pair2.MakeThing());
            things.Add(pair3.MakeThing());
            things.Add(finemeal);

            foreach (Thing thing in things)
            {
                loadoutInstance.Add(thing, false);
            }
        }

        public override void Setup()
        {
            Tests.Add(new UpdateQualityNoMerge());
            Tests.Add(new UpdateStuffNoMerge());
            Tests.Add(new UpdateQualityAndMerged());
            Tests.Add(new UpdateStuffAndMerged());
            Tests.Add(new UpdateHitPoint());
        }
    }
}
#endif