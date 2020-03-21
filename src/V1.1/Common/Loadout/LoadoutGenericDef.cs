// <copyright file="LoadoutGenericDef.cs" company="Zizhen Li">
// Copyright (c) Zizhen Li. All rights reserved.
// Licensed under the GPL-3.0-only license. See LICENSE.md file in the project root for full license information.
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;

// TODO Map generic type to ThingRequestGroup so to get rid of querying ListerThing.AllThings
namespace AwesomeInventory.Loadout
{
    /// <summary>
    /// LoadoutGenericDef handles Generic LoadoutSlots.
    /// </summary>
    [StaticConstructorOnStartup]
    public class LoadoutGenericDef : ThingDef, IEquatable<LoadoutGenericDef>
    {
        #region Fields
        public int defaultCount = 1;
        public Predicate<ThingDef> Validator = td => true;
        public ThingRequestGroup thingRequestGroup = ThingRequestGroup.HaulableEver;
        public Predicate<ThingDef> Includes;
        public bool isBasic = false;

        // The following group are intentionally left unsaved so that if the game state changes between saves the new value is calculated.
        private float _mass;
        private bool _cachedVars = false;
        private static List<LoadoutGenericDef> _genericDefs = new List<LoadoutGenericDef>();

        #endregion

        #region Constructors

        /// <remark>
        /// This constructor gets run on startup of RimWorld and generates the various LoadoutGenericDef instance objects akin to having been loaded from xml.
        /// </remark>
        static LoadoutGenericDef()
        {
            IEnumerable<ThingDef> everything = DefDatabase<ThingDef>.AllDefs;

            // need to generate a list as that's how new defs are taken by DefDatabase.
            LoadoutGenericDef generic = new LoadoutGenericDef
            {
                defName = "RPGIGenericMeal",
                thingClass = typeof(ThingWithComps),
                description = "Generic Loadout for perishable meals.  Intended for compatibility with pawns automatically picking up a meal for themself.",
                label = "Corgi_Generic_Meal".Translate(),
                Validator = (ThingDef thingDef) => thingDef.IsNutritionGivingIngestible && thingDef.ingestible.preferability >= FoodPreferability.MealAwful && thingDef.GetCompProperties<CompProperties_Rottable>()?.daysToRotStart <= 5 && !thingDef.IsDrug,
                thingRequestGroup = ThingRequestGroup.FoodSourceNotPlantOrTree,
                isBasic = true
            };
            generic.Includes = (ThingDef thingDef) => generic.thingRequestGroup.Includes(thingDef) && generic.Validator(thingDef);

            _genericDefs.Add(generic);

            float targetNutrition = 0.85f;
            generic = new LoadoutGenericDef
            {
                defName = "RPGIGenericRawFood",
                thingClass = typeof(ThingWithComps),
                description = "Generic Loadout for Raw Food.  Intended for compatibility with pawns automatically picking up raw food to train animals.",
                label = "Corgi_Generic_RawFood".Translate(),

                // Exclude drugs and corpses.  Also exclude any food worse than RawBad as in testing the pawns would not even pick it up for training.
                Validator = (ThingDef thingDef) => thingDef.IsNutritionGivingIngestible && !thingDef.IsCorpse && thingDef.ingestible.HumanEdible && !thingDef.HasComp(typeof(CompHatcher)) && (int)thingDef.ingestible.preferability < 6,
                defaultCount = Convert.ToInt32(Math.Floor(targetNutrition / everything.Where(td => generic.Validator(td)).Average(td => td.ingestible.CachedNutrition)))
            };
            generic.Includes = (ThingDef thingDef) => generic.Validator(thingDef);
            _genericDefs.Add(generic);

            generic = new LoadoutGenericDef
            {
                defName = "RPGIGenericDrugs",
                thingClass = typeof(ThingWithComps),
                defaultCount = 3,
                description = "Generic Loadout for Drugs.  Intended for compatibility with pawns automatically picking up drugs in compliance with drug policies.",
                label = "Corgi_Generic_Drugs".Translate(),
                thingRequestGroup = ThingRequestGroup.Drug,
                isBasic = true
            };
            generic.Includes = (ThingDef thingDef) => generic.thingRequestGroup.Includes(thingDef);
            _genericDefs.Add(generic);

            generic = new LoadoutGenericDef
            {
                defName = "RPGIGenericMedicine",
                thingClass = typeof(Medicine),
                defaultCount = 5,
                description = "Generic Loadout for Medicine.  Intended for pawns which will handle triage activities.",
                label = "Corgi_Generic_Medicine".Translate(),
                thingRequestGroup = ThingRequestGroup.Medicine
            };
            generic.Includes = (ThingDef thingDef) => generic.thingRequestGroup.Includes(thingDef);
            _genericDefs.Add(generic);

            // finally we add all the defs generated to the DefDatabase.
            DefDatabase<LoadoutGenericDef>.Add(_genericDefs);
        }

        #endregion Constructors

        #region Properties

        /// <summary>
        /// Property gets the calculated mass of this def.  This is determined at runtime based on stored Lambda rather than a static value.
        /// </summary>
        public float mass
        {
            get
            {
                if (!_cachedVars)
                    UpdateVars();
                return _mass;
            }
        }

        /// <summary>
        /// Generic defs that are added to DefDatabase
        /// </summary>
        public static List<LoadoutGenericDef> GenericDefs
        {
            get => _genericDefs;
        }

        public static List<ThingDef> GenericDefsToThingDefs
        {
            get
            {
                return _genericDefs.Select(def => (ThingDef)def).ToList();
            }
        }
        #endregion Properties

        #region Methods

        /// <summary>
        /// Handles updating this def's stored mass values.
        /// </summary>
        /// <remarks>Can be a bit expensive but only done once per def the first time such values are requested.</remarks>
        private void UpdateVars()
        {
            IEnumerable<ThingDef> matches;
            matches = DefDatabase<ThingDef>.AllDefs.Where(td => Validator(td) && thingRequestGroup.Includes(td));
            _mass = matches.Max(t => t.GetStatValueAbstract(StatDefOf.Mass));
            _cachedVars = true;
        }

        private IEnumerable<ThingDef> PossibleRawFood(TechLevel techLevel)
        {
            return ThingSetMakerUtility.allGeneratableItems.Where((ThingDef x) => x.IsNutritionGivingIngestible && !x.IsCorpse && x.ingestible.HumanEdible && !x.HasComp(typeof(CompHatcher)) && (int)x.techLevel <= (int)techLevel && (int)x.ingestible.preferability < 6);
        }

        public bool Equals(LoadoutGenericDef other)
        {
            if (other == null)
            {
                return false;
            }

            return defName == other.defName;
        }

        public override bool Equals(object obj)
        {
            if (!(obj is LoadoutGenericDef genericDef))
            {
                return false;
            }

            return Equals(genericDef);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public static bool operator ==(LoadoutGenericDef a, LoadoutGenericDef b)
        {
            if (ReferenceEquals(a, b))
            {
                return true;
            }
            if (a == null || b == null)
            {
                return false;
            }
            return a.defName == b.defName;
        }

        public static bool operator !=(LoadoutGenericDef a, LoadoutGenericDef b)
        {
            return !(a == b);
        }

        #endregion Methods
    }
}
