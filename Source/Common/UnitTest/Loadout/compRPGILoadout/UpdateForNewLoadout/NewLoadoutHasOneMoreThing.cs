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
    public class NewLoadoutHasOneMoreThing : Test_UpdateForNewLoadout
    {
        private compRPGILoadout _compRPGI;
        public override void Run(out bool result)
        {
            _compRPGI = _pawn.GetComp<compRPGILoadout>();
            result = true;

            foreach (RPGILoadout loadout in _loadouts)
            {
                _pawn.SetLoadout(loadout);
                result &= AssertUtility.AreEqual(loadout, _compRPGI.Loadout, nameof(loadout), nameof(_compRPGI.Loadout));
                if (loadout != null)
                {
                    result &= AssertUtility.AreEqual(
                        _compRPGI.InventoryTracker.Count
                        , loadout.Count
                        , string.Format(StringResource.ObjectCount, nameof(_compRPGI.InventoryTracker))
                        , string.Format(StringResource.ObjectCount, nameof(loadout)));
                    foreach (Thing thing in loadout)
                    {
                        result &= AssertUtility.Contains(
                            _compRPGI.InventoryTracker.Keys
                            , thing
                            , nameof(_compRPGI.InventoryTracker)
                            , thing.LabelCapNoCount);

                        result &= AssertUtility.Expect(
                            _compRPGI.InventoryTracker[thing]
                            , thing.stackCount * -1
                            , string.Format(StringResource.ObjectCount, nameof(_compRPGI.InventoryTracker)));
                    }
                }
            }
            return;
        }

        public override void Cleanup()
        {
            _pawn.outfits.CurrentOutfit = null;
            _compRPGI.Loadout = null;
            _compRPGI.InventoryTracker = null;
        }
    }
}
#endif