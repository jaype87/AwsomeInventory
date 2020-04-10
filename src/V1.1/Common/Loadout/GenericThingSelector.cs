// <copyright file="GenericThingSelector.cs" company="Zizhen Li">
// Copyright (c) 2019 - 2020 Zizhen Li. All rights reserved.
// Licensed under the LGPL-3.0-only license. See LICENSE.md file in the project root for full license information.
// </copyright>

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AwesomeInventory.UI;
using RimWorld;
using Verse;

namespace AwesomeInventory.Loadout
{
    /// <summary>
    /// Selector for generic things, e.g., generic meals and generic medicines.
    /// </summary>
    public class GenericThingSelector : ThingSelector
    {
        private AIGenericDef _genericDef;

        /// <summary>
        /// Initializes a new instance of the <see cref="GenericThingSelector"/> class.
        /// Reserved for xml serialization, should not be called anywhere else.
        /// </summary>
        public GenericThingSelector()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GenericThingSelector"/> class.
        /// </summary>
        /// <param name="genericDef"> The generic def used for this selector. </param>
        public GenericThingSelector(AIGenericDef genericDef)
        {
            ValidateArg.NotNull(genericDef, nameof(genericDef));

            _genericDef = genericDef;
            _genericDef.ThingCategoryDefs.ToList().ForEach(t => _thingFilter.SetAllow(t, true, _genericDef.ExcepDefs));
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GenericThingSelector"/> class.
        /// </summary>
        /// <param name="other"> Copy <paramref name="other"/> to this selector. </param>
        public GenericThingSelector(GenericThingSelector other)
        {
            ValidateArg.NotNull(other, nameof(other));

            _genericDef = other._genericDef;
            _genericDef.thingCategories.ForEach(t => _thingFilter.SetAllow(t, true, _genericDef.ExcepDefs));
        }

        /// <inheritdoc/>
        public override string LabelCapNoCount => _genericDef.label.Colorize(QualityColor.Instance.Generic);

        /// <inheritdoc/>
        public override float Weight => _thingFilter.AllowedThingDefs.Average(def => def.statBases.GetStatValueFromList(StatDefOf.Mass, 0));

        /// <inheritdoc/>
        public override bool Allows(Thing thing)
        {
            ValidateArg.NotNull(thing, nameof(thing));

            return _thingFilter.Allows(thing.GetInnerIfMinified());
        }

        /// <summary>
        /// Save state.
        /// </summary>
        public override void ExposeData()
        {
            base.ExposeData();
            string defName = _genericDef?.defName ?? string.Empty;
            Scribe_Values.Look(ref defName, nameof(defName));

            if (Scribe.mode == LoadSaveMode.LoadingVars)
            {
                _genericDef = DefDatabase<AIGenericDef>.GetNamed(defName);
            }
        }
    }
}
