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
    public class ItemDistribution : TestCompAwesomeInventoryLoadout
    {
        AwesomeInventoryLoadout _loadout = new AwesomeInventoryLoadout();
        ThingGroupSelector _strict;
        ThingGroupSelector _loose;

        public override void Setup()
        {
            _strict = this.BuildGroupSelector(_weaponThingDef, QualityCategory.Masterwork, 2);
            _loose = this.BuildGroupSelector(_weaponThingDef, QualityCategory.Awful, 2);

            _loadout.Add(_strict);
            _loadout.Add(_loose);

            _comp.UpdateForNewLoadout(_loadout);
        }

        public override void Run(out bool result)
        {
            result = true;

            // Adding lengdary quality items to two selectors with different quality filter.
            // 1
            _pawn.inventory.innerContainer.TryAddOrTransfer(_pairs[0].MakeThingWithoutID());
            result &=
                AssertUtility.AreEqual(_comp.InventoryMargins[_strict], -1, "Margin for strict ThingGroupSelector", "Expected margin")
                &&
                AssertUtility.AreEqual(_comp.InventoryMargins[_loose], -2, "Margin for loose ThingGroupSelector", "Expected margin");

            // 2
            _pawn.inventory.innerContainer.TryAddOrTransfer(_pairs[0].MakeThingWithoutID());
            result &=
                AssertUtility.AreEqual(_comp.InventoryMargins[_strict], 0, "Margin for strict ThingGroupSelector", "Expected margin")
                &&
                AssertUtility.AreEqual(_comp.InventoryMargins[_loose], -2, "Margin for loose ThingGroupSelector", "Expected margin");

            // 3
            _pawn.inventory.innerContainer.TryAddOrTransfer(_pairs[0].MakeThingWithoutID());
            result &=
                AssertUtility.AreEqual(_comp.InventoryMargins[_strict], 0, "Margin for strict ThingGroupSelector", "Expected margin")
                &&
                AssertUtility.AreEqual(_comp.InventoryMargins[_loose], -1, "Margin for loose ThingGroupSelector", "Expected margin");

            // 4
            _pawn.inventory.innerContainer.TryAddOrTransfer(_pairs[0].MakeThingWithoutID());
            result &=
                AssertUtility.AreEqual(_comp.InventoryMargins[_strict], 0, "Margin for strict ThingGroupSelector", "Expected margin")
                &&
                AssertUtility.AreEqual(_comp.InventoryMargins[_loose], 0, "Margin for loose ThingGroupSelector", "Expected margin");

            // 5
            _pawn.inventory.innerContainer.TryAddOrTransfer(_pairs[0].MakeThingWithoutID());
            result &=
                AssertUtility.AreEqual(_comp.InventoryMargins[_strict], 1, "Margin for strict ThingGroupSelector", "Expected margin")
                &&
                AssertUtility.AreEqual(_comp.InventoryMargins[_loose], 0, "Margin for loose ThingGroupSelector", "Expected margin");

            // 6
            _pawn.inventory.innerContainer.TryAddOrTransfer(_pairs[0].MakeThingWithoutID());
            result &=
                AssertUtility.AreEqual(_comp.InventoryMargins[_strict], 2, "Margin for strict ThingGroupSelector", "Expected margin")
                &&
                AssertUtility.AreEqual(_comp.InventoryMargins[_loose], 0, "Margin for loose ThingGroupSelector", "Expected margin");

            // Remove all added items.
            // 1
            _pawn.inventory.innerContainer.RemoveAt(0);
            result &=
                AssertUtility.AreEqual(_comp.InventoryMargins[_strict], 1, "Margin for strict ThingGroupSelector", "Expected margin")
                &&
                AssertUtility.AreEqual(_comp.InventoryMargins[_loose], 0, "Margin for loose ThingGroupSelector", "Expected margin");

            // 2
            _pawn.inventory.innerContainer.RemoveAt(0);
            result &=
                AssertUtility.AreEqual(_comp.InventoryMargins[_strict], 0, "Margin for strict ThingGroupSelector", "Expected margin")
                &&
                AssertUtility.AreEqual(_comp.InventoryMargins[_loose], 0, "Margin for loose ThingGroupSelector", "Expected margin");

            // 3
            _pawn.inventory.innerContainer.RemoveAt(0);
            result &=
                AssertUtility.AreEqual(_comp.InventoryMargins[_strict], -1, "Margin for strict ThingGroupSelector", "Expected margin")
                &&
                AssertUtility.AreEqual(_comp.InventoryMargins[_loose], 0, "Margin for loose ThingGroupSelector", "Expected margin");

            // 4
            _pawn.inventory.innerContainer.RemoveAt(0);
            result &=
                AssertUtility.AreEqual(_comp.InventoryMargins[_strict], -2, "Margin for strict ThingGroupSelector", "Expected margin")
                &&
                AssertUtility.AreEqual(_comp.InventoryMargins[_loose], 0, "Margin for loose ThingGroupSelector", "Expected margin");

            // 5
            _pawn.inventory.innerContainer.RemoveAt(0);
            result &=
                AssertUtility.AreEqual(_comp.InventoryMargins[_strict], -2, "Margin for strict ThingGroupSelector", "Expected margin")
                &&
                AssertUtility.AreEqual(_comp.InventoryMargins[_loose], -1, "Margin for loose ThingGroupSelector", "Expected margin");

            // 6
            _pawn.inventory.innerContainer.RemoveAt(0);
            result &=
                AssertUtility.AreEqual(_comp.InventoryMargins[_strict], -2, "Margin for strict ThingGroupSelector", "Expected margin")
                &&
                AssertUtility.AreEqual(_comp.InventoryMargins[_loose], -2, "Margin for loose ThingGroupSelector", "Expected margin");

            // Add three lengendary and three good items
            // 1
            _pawn.inventory.innerContainer.TryAddOrTransfer(_pairs[0].MakeThingWithoutID());
            result &=
                AssertUtility.AreEqual(_comp.InventoryMargins[_strict], -1, "Margin for strict ThingGroupSelector", "Expected margin")
                &&
                AssertUtility.AreEqual(_comp.InventoryMargins[_loose], -2, "Margin for loose ThingGroupSelector", "Expected margin");

            // 2
            _pawn.inventory.innerContainer.TryAddOrTransfer(_pairs[0].MakeThingWithoutID());
            result &=
                AssertUtility.AreEqual(_comp.InventoryMargins[_strict], 0, "Margin for strict ThingGroupSelector", "Expected margin")
                &&
                AssertUtility.AreEqual(_comp.InventoryMargins[_loose], -2, "Margin for loose ThingGroupSelector", "Expected margin");

            // 3
            _pawn.inventory.innerContainer.TryAddOrTransfer(_pairs[0].MakeThingWithoutID());
            result &=
                AssertUtility.AreEqual(_comp.InventoryMargins[_strict], 0, "Margin for strict ThingGroupSelector", "Expected margin")
                &&
                AssertUtility.AreEqual(_comp.InventoryMargins[_loose], -1, "Margin for loose ThingGroupSelector", "Expected margin");

            // 4
            _pawn.inventory.innerContainer.TryAddOrTransfer(_pairs[1].MakeThingWithoutID());
            result &=
                AssertUtility.AreEqual(_comp.InventoryMargins[_strict], 0, "Margin for strict ThingGroupSelector", "Expected margin")
                &&
                AssertUtility.AreEqual(_comp.InventoryMargins[_loose], 0, "Margin for loose ThingGroupSelector", "Expected margin");

            // 5
            _pawn.inventory.innerContainer.TryAddOrTransfer(_pairs[1].MakeThingWithoutID());
            result &=
                AssertUtility.AreEqual(_comp.InventoryMargins[_strict], 0, "Margin for strict ThingGroupSelector", "Expected margin")
                &&
                AssertUtility.AreEqual(_comp.InventoryMargins[_loose], 1, "Margin for loose ThingGroupSelector", "Expected margin");

            // 6
            _pawn.inventory.innerContainer.TryAddOrTransfer(_pairs[1].MakeThingWithoutID());
            result &=
                AssertUtility.AreEqual(_comp.InventoryMargins[_strict], 0, "Margin for strict ThingGroupSelector", "Expected margin")
                &&
                AssertUtility.AreEqual(_comp.InventoryMargins[_loose], 2, "Margin for loose ThingGroupSelector", "Expected margin");

            // Remove all added items.
            // 1
            _pawn.inventory.innerContainer.RemoveAt(0);
            result &=
                AssertUtility.AreEqual(_comp.InventoryMargins[_strict], -1, "Margin for strict ThingGroupSelector", "Expected margin")
                &&
                AssertUtility.AreEqual(_comp.InventoryMargins[_loose], 2, "Margin for loose ThingGroupSelector", "Expected margin");

            // 2
            _pawn.inventory.innerContainer.RemoveAt(2);
            result &=
                AssertUtility.AreEqual(_comp.InventoryMargins[_strict], -1, "Margin for strict ThingGroupSelector", "Expected margin")
                &&
                AssertUtility.AreEqual(_comp.InventoryMargins[_loose], 1, "Margin for loose ThingGroupSelector", "Expected margin");

            // 3
            _pawn.inventory.innerContainer.RemoveAt(0);
            result &=
                AssertUtility.AreEqual(_comp.InventoryMargins[_strict], -2, "Margin for strict ThingGroupSelector", "Expected margin")
                &&
                AssertUtility.AreEqual(_comp.InventoryMargins[_loose], 1, "Margin for loose ThingGroupSelector", "Expected margin");

            // 4
            _pawn.inventory.innerContainer.RemoveAt(0);
            result &=
                AssertUtility.AreEqual(_comp.InventoryMargins[_strict], -2, "Margin for strict ThingGroupSelector", "Expected margin")
                &&
                AssertUtility.AreEqual(_comp.InventoryMargins[_loose], 0, "Margin for loose ThingGroupSelector", "Expected margin");

            // 5
            _pawn.inventory.innerContainer.RemoveAt(0);
            result &=
                AssertUtility.AreEqual(_comp.InventoryMargins[_strict], -2, "Margin for strict ThingGroupSelector", "Expected margin")
                &&
                AssertUtility.AreEqual(_comp.InventoryMargins[_loose], -1, "Margin for loose ThingGroupSelector", "Expected margin");

            // 6
            _pawn.inventory.innerContainer.RemoveAt(0);
            result &=
                AssertUtility.AreEqual(_comp.InventoryMargins[_strict], -2, "Margin for strict ThingGroupSelector", "Expected margin")
                &&
                AssertUtility.AreEqual(_comp.InventoryMargins[_loose], -2, "Margin for loose ThingGroupSelector", "Expected margin");
        }
    }
}