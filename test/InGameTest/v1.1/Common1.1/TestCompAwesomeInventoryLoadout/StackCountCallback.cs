
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AwesomeInventory.Loadout;
using RimWorld;

namespace AwesomeInventory.UnitTest
{
    public class StackCountCallback : TestCompAwesomeInventoryLoadout
    {
        AwesomeInventoryLoadout _loadout = new AwesomeInventoryLoadout();
        ThingGroupSelector _controlGroup;
        ThingGroupSelector _experimentGroup;

        public override void Setup()
        {
            _controlGroup = this.BuildGroupSelector(_weaponThingDef, QualityCategory.Masterwork, 2);
            _experimentGroup = this.BuildGroupSelector(_weaponThingDef, QualityCategory.Masterwork, 2);

            _loadout.Add(_controlGroup);
            _loadout.Add(_experimentGroup);


            _comp.UpdateForNewLoadout(_loadout);
        }

        public override void Run(out bool result)
        {
            result = true;

            _experimentGroup.SetStackCount(3);
            result &=
                AssertUtility.AreEqual(_comp.InventoryMargins[_experimentGroup], -3, "Margin for Experiment Group", "Expected margin")
                &&
                AssertUtility.AreEqual(_comp.InventoryMargins[_controlGroup], -2, "Margin for Controlled Group", "Expected margin");

            _experimentGroup.SetStackCount(1);
            result &=
                AssertUtility.AreEqual(_comp.InventoryMargins[_experimentGroup], -1, "Margin for Experiment Group", "Expected margin")
                &&
                AssertUtility.AreEqual(_comp.InventoryMargins[_controlGroup], -2, "Margin for Controlled Group", "Expected margin");

            _experimentGroup.SetStackCount(0);
            result &=
                AssertUtility.AreEqual(_comp.InventoryMargins.TryGetValue(_experimentGroup, out _), false, "Experiment Group is included", "Expected presence")
                &&
                AssertUtility.AreEqual(_comp.InventoryMargins[_controlGroup], -2, "Margin for Controlled Group", "Expected margin");
        }
    }
}
