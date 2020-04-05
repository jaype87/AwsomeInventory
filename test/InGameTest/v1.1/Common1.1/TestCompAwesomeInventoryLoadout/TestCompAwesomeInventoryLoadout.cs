using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AwesomeInventory.Loadout;
using RimWorld;
using Verse;

namespace AwesomeInventory.UnitTest
{
    public class TestCompAwesomeInventoryLoadout : AwesomeInventoryUnitTest
    {
        protected Pawn _pawn;
        protected CompAwesomeInventoryLoadout _comp;
        protected List<ThingStuffPairWithQuality> _pairs = new List<ThingStuffPairWithQuality>();
        protected List<Thing> _things = new List<Thing>();
        protected ThingDef _weaponThingDef = ThingDef.Named("MeleeWeapon_Knife");
        protected ThingDef _apparelThingDef = ThingDef.Named("Apparel_CollarShirt");

        public TestCompAwesomeInventoryLoadout()
        {
            _pawn = PawnGenerator.GeneratePawn(PawnKindDefOf.Colonist);
            _pawn.outfits = new Pawn_OutfitTracker(_pawn);
            _pawn.apparel.DestroyAll();
            _pawn.equipment.DestroyAllEquipment();
            _pawn.inventory.DestroyAll();

            _comp = _pawn.TryGetComp<CompAwesomeInventoryLoadout>();

            ThingStuffPairWithQuality pair1 = new ThingStuffPairWithQuality
            {
                thing = _weaponThingDef,
                stuff = ThingDefOf.Plasteel,
                quality = QualityCategory.Legendary
            };

            ThingStuffPairWithQuality pair2 = new ThingStuffPairWithQuality
            {
                thing = _weaponThingDef,
                stuff = ThingDefOf.Steel,
                quality = QualityCategory.Good
            };

            ThingStuffPairWithQuality pair3 = new ThingStuffPairWithQuality
            {
                thing = _weaponThingDef,
                stuff = ThingDefOf.Steel,
                quality = QualityCategory.Legendary
            };

            ThingStuffPairWithQuality pair4 = new ThingStuffPairWithQuality
            {
                thing = _apparelThingDef,
                stuff = ThingDefOf.Cloth,
                quality = QualityCategory.Normal,
            };

            ThingStuffPairWithQuality pair5 = new ThingStuffPairWithQuality
            {
                thing = _apparelThingDef,
                stuff = ThingDefOf.Leather_Plain,
                quality = QualityCategory.Legendary,
            };

            Thing finemeal = ThingMaker.MakeThing(ThingDefOf.MealFine);
            finemeal.stackCount = 11;

            _pairs.Add(pair1);
            _pairs.Add(pair2);
            _pairs.Add(pair3);
            _pairs.Add(pair4);
            _pairs.Add(pair5);

            _things.Add(pair1.MakeThing());
            _things.Add(pair2.MakeThing());
            _things.Add(pair3.MakeThing());
            _things.Add(finemeal);
        }

        public override void Setup()
        {
            Tests.Add(new ItemDistribution());
            Tests.Add(new StackCountCallback());
            Tests.Add(new ThingGroupSynchronization());
        }

        public override void Cleanup()
        {
            _pawn.inventory.DestroyAll();
            _pawn.equipment.DestroyAllEquipment();
            _pawn.apparel.DestroyAll();
            _comp.InventoryMargins?.Clear();
        }

        protected ThingGroupSelector BuildGroupSelector(ThingDef thingDef, QualityCategory qualityCategory, int stackCount)
        {
            ThingGroupSelector groupSelector = new ThingGroupSelector(thingDef);
            SingleThingSelector singleThingSelector = new SingleThingSelector(thingDef);
            singleThingSelector.SetQualityRange(new QualityRange(qualityCategory, QualityCategory.Legendary));
            groupSelector.Add(singleThingSelector);
            groupSelector.SetStackCount(stackCount);
            return groupSelector;
        }
    }
}
