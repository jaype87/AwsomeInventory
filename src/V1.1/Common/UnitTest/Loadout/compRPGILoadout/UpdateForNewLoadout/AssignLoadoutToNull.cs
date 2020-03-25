// <copyright file="AssignLoadoutToNull.cs" company="Zizhen Li">
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
    public class AssignLoadoutToNull : Test_UpdateForNewLoadout
    {
        public override void Run(out bool result)
        {
            CompAwesomeInventoryLoadout compRPGI = _pawn.GetComp<CompAwesomeInventoryLoadout>();
            result = true;

            foreach (AILoadout loadout in _loadouts)
            {
                _pawn.SetLoadout(loadout);
                result &= AssertUtility.AreEqual(loadout, compRPGI.Loadout, nameof(loadout), nameof(compRPGI.Loadout));
                if (loadout != null)
                {
                    result &= AssertUtility.AreEqual(
                        compRPGI.InventoryTracker.Count
                        , loadout.Count
                        , string.Format(StringResource.ObjectCount, nameof(compRPGI.InventoryTracker))
                        , string.Format(StringResource.ObjectCount, nameof(loadout)));
                    foreach (Thing thing in loadout)
                    {
                        result &= AssertUtility.Contains(
                            compRPGI.InventoryTracker.Keys
                            , thing
                            , nameof(compRPGI.InventoryTracker)
                            , thing.LabelCapNoCount);

                        result &= AssertUtility.Expect(
                            compRPGI.InventoryTracker[thing]
                            , thing.stackCount * -1
                            , string.Format(StringResource.ObjectCount, nameof(compRPGI.InventoryTracker)));
                    }
                }

                _pawn.outfits.CurrentOutfit = null;
                compRPGI.Loadout = null;
                compRPGI.InventoryTracker = null;
            }

            return;
        }
    }
}
#endif