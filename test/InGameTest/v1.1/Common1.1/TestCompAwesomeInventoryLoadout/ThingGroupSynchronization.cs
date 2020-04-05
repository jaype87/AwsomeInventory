using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AwesomeInventory.Loadout;
using RimWorld;

namespace AwesomeInventory.UnitTest
{
    public class ThingGroupSynchronization : TestCompAwesomeInventoryLoadout
    {
        AwesomeInventoryLoadout _loadout;
        ThingGroupSelector _groupSelector;

        public override void Setup()
        {
            _pawn.apparel.WornApparel.Add(_pairs[3].MakeThingWithoutID() as Apparel);
            _loadout = new AwesomeInventoryLoadout(_pawn);
            _pawn.SetLoadout(_loadout);
        }

        public override void Run(out bool result)
        {
            result = true
                  && AssertUtility.Expect(_comp.InventoryMargins.Count, 1, "ThingGroupSelector count");

            _groupSelector = this.BuildGroupSelector(_apparelThingDef, QualityCategory.Normal, 1);
            _loadout.Add(_groupSelector);
            result &=
                AssertUtility.Expect(_comp.InventoryMargins.Count, 2, "ThingGroupSelector count");

            _groupSelector = this.BuildGroupSelector(_apparelThingDef, QualityCategory.Normal, 1);
            _loadout.Add(_groupSelector);
            result &=
                AssertUtility.Expect(_comp.InventoryMargins.Count, 3, "ThingGroupSelector count");

            _loadout.RemoveAt(0);
            result &=
                AssertUtility.Expect(_comp.InventoryMargins.Count, 2, "ThingGroupSelector count");

            _loadout.RemoveAt(0);
            result &=
                AssertUtility.Expect(_comp.InventoryMargins.Count, 1, "ThingGroupSelector count");
        }
    }
}
