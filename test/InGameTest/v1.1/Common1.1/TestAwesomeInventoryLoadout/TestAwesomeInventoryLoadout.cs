using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse;

namespace AwesomeInventory.UnitTest
{
    public class TestAwesomeInventoryLoadout : AwesomeInventoryUnitTest
    {
        protected Pawn _pawn;
        protected List<Thing> _things = new List<Thing>();
        protected ThingDef _thingDef = ThingDef.Named("MeleeWeapon_Knife");

        public TestAwesomeInventoryLoadout()
        {
            ThingStuffPairWithQuality pair1 = new ThingStuffPairWithQuality
            {
                thing = _thingDef,
                stuff = ThingDefOf.Plasteel,
                quality = QualityCategory.Legendary
            };

            ThingStuffPairWithQuality pair2 = new ThingStuffPairWithQuality
            {
                thing = _thingDef,
                stuff = ThingDefOf.Steel,
                quality = QualityCategory.Good
            };

            ThingStuffPairWithQuality pair3 = new ThingStuffPairWithQuality
            {
                thing = _thingDef,
                stuff = ThingDefOf.Steel,
                quality = QualityCategory.Legendary
            };

            Thing finemeal = ThingMaker.MakeThing(ThingDefOf.MealFine);
            finemeal.stackCount = 11;

            _things.Add(pair1.MakeThing());
            _things.Add(pair2.MakeThing());
            _things.Add(pair3.MakeThing());
            _things.Add(finemeal);
        }

        public override void Setup()
        {
            Tests.Add(new CallbackCoupling());
        }
    }
}
