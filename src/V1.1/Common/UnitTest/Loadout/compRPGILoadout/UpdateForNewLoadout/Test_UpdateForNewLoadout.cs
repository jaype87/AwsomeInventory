// <copyright file="Test_UpdateForNewLoadout.cs" company="Zizhen Li">
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

#if RPG_Inventory_Remake
using RPG_Inventory_Remake.Loadout;
using RPG_Inventory_Remake;
#endif

namespace RPG_Inventory_Remake_Common.UnitTest
{
    public class Test_UpdateForNewLoadout : RPGIUnitTest
    {
        protected static readonly Pawn _pawn;
        protected static readonly AILoadout _loadout0;
        protected static readonly AILoadout _loadout1;
        protected static readonly AILoadout _loadout2;
        protected static readonly AILoadout _loadout3;
        protected static readonly AILoadout _loadout4;
        protected static readonly List<AILoadout> _loadouts;

        protected static readonly Thing _knifePlasteelLegendary;
        protected static readonly Thing _knifePlasteelGood;
        protected static readonly Thing _knifeSteelGood;
        protected static readonly Thing _shirtClothNormal;
        protected static readonly Thing _finemeal;

        static Test_UpdateForNewLoadout()
        {
            _pawn = PawnGenerator.GeneratePawn(PawnKindDefOf.Colonist);
            _pawn.outfits = new Pawn_OutfitTracker(_pawn);
            _pawn.apparel.DestroyAll();
            _pawn.equipment.DestroyAllEquipment();
            _pawn.inventory.DestroyAll();

            ThingDef knifeDef = ThingDef.Named("MeleeWeapon_Knife");
            ThingDef shirtDef = ThingDef.Named("Apparel_BasicShirt");

            _knifePlasteelLegendary
                = new ThingStuffPairWithQuality(
                    knifeDef
                    , ThingDefOf.Plasteel
                    , QualityCategory.Legendary)
                .MakeThing();

            _knifePlasteelGood
                = new ThingStuffPairWithQuality(
                    knifeDef
                    , ThingDefOf.Plasteel
                    , QualityCategory.Good)
                .MakeThing();

            _knifeSteelGood
                = new ThingStuffPairWithQuality(
                    knifeDef
                    , ThingDefOf.Steel
                    , QualityCategory.Good)
                .MakeThing();

            _shirtClothNormal
                = new ThingStuffPairWithQuality(
                    shirtDef
                    , ThingDefOf.Cloth
                    , QualityCategory.Normal)
                .MakeThing();

            _finemeal = ThingMaker.MakeThing(ThingDefOf.MealFine);
            _finemeal.stackCount = 11;

            _loadout0 = null;
            _loadout1 = new AILoadout(_pawn);
            LoadoutManager.AddLoadout(_loadout1);

            _loadout2 = new AILoadout(_pawn);
            LoadoutManager.AddLoadout(_loadout2);

            _loadout3 = new AILoadout(_pawn);
            LoadoutManager.AddLoadout(_loadout3);

            _loadout4 = new AILoadout(_pawn);
            LoadoutManager.AddLoadout(_loadout4);

            _loadout1.Add(_knifePlasteelLegendary);

            _loadout2.Add(_knifePlasteelLegendary);
            _loadout2.Add(_knifePlasteelGood);

            _loadout3.Add(_knifePlasteelLegendary);
            _loadout3.Add(_knifePlasteelGood);
            _loadout3.Add(_knifeSteelGood);

            _loadout4.Add(_knifePlasteelLegendary);
            _loadout4.Add(_knifePlasteelGood);
            _loadout4.Add(_knifeSteelGood);
            _loadout4.Add(_shirtClothNormal);
            _loadout4.Add(_finemeal);

            _loadouts = new List<AILoadout>()
            {
                _loadout0,
                _loadout1,
                _loadout2,
                _loadout3,
                _loadout4,
            };
        }

        public override void Setup()
        {
            if (GetType() == typeof(Test_UpdateForNewLoadout))
            {
                Tests.Add(new AssignLoadoutToNull());
                Tests.Add(new NewLoadoutHasOneMoreThing());
                Tests.Add(new NewLoadoutHasOneLessThing());
                Tests.Add(new DifferentInStackCount());
                Tests.Add(new UpdateLoadoutWithInventory());
            }
        }
    }
}
#endif