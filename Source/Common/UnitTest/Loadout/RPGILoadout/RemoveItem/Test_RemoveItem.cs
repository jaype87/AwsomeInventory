#if UnitTest
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using RimWorld;

#if RPG_Inventory_Remake
using RPG_Inventory_Remake.Loadout;
using RPG_Inventory_Remake;
#endif

namespace RPG_Inventory_Remake_Common.UnitTest
{
    public class Test_RemoveItem : RPGIUnitTest
    {
        public static RPGILoadout loadoutInstance = new RPGILoadout();
        public static List<Thing> things = new List<Thing>();

        static Test_RemoveItem()
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

            for (int i = 0; i < 2; i++)
            {
                foreach (Thing thing in things)
                {
                    loadoutInstance.AddItem(thing, false);
                }
            }
        }

        public override void Setup()
        {
            // Add tests
            Tests.Add(new RemoveOneItem());
        }
    }
}
#endif