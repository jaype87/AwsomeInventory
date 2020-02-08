#if UnitTest
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using RimWorld;
using UnityEngine;
using UnityEngine.Assertions;

#if RPG_Inventory_Remake
using RPG_Inventory_Remake.Loadout;
using RPG_Inventory_Remake;
#endif

namespace RPG_Inventory_Remake_Common.UnitTest
{
    public class Test_AddItem : RPGIUnitTest
    {
        public static RPGILoadout loadoutInstance = new RPGILoadout();
        public static List<Thing> things = new List<Thing>();

        static Test_AddItem()
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
        }

        public override void Setup()
        {
            // Add tests
            Tests.Add(new AddOneItem());
            Tests.Add(new AddUnmergeableItemTwice());
            Tests.Add(new AddTwoThingsDifferentInStuff());
            Tests.Add(new AddTwoThingsDifferentInQuality());
            Tests.Add(new AddMergeableItemTwice());
        }
    }
}
#endif