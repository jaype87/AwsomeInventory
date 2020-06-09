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
    [RegisterType(typeof(GenericThingSelector), typeof(GenericThingSelector))]
    public class GenericThingSelector : ThingSelector
    {
        private AIGenericDef _genericDef;
        private string _defName;

        /// <summary>
        /// Initializes a new instance of the <see cref="GenericThingSelector"/> class.
        /// Reserved for xml serialization, should not be called anywhere else.
        /// </summary>
        [Obsolete(ErrorText.NoDirectCall, true)]
        public GenericThingSelector()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GenericThingSelector"/> class.
        /// </summary>
        /// <param name="genericDef"> The generic def used for this selector. </param>
        [Obsolete(ErrorText.NoDirectCall, true)]
        public GenericThingSelector(AIGenericDef genericDef)
        {
            ValidateArg.NotNull(genericDef, nameof(genericDef));

            _genericDef = genericDef;
            foreach (ThingDef def in _genericDef.AvailableDefs)
                _thingFilter.SetAllow(def, true);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GenericThingSelector"/> class.
        /// </summary>
        /// <param name="other"> Copy <paramref name="other"/> to this selector. </param>
        [Obsolete(ErrorText.NoDirectCall, true)]
        public GenericThingSelector(GenericThingSelector other)
        {
            ValidateArg.NotNull(other, nameof(other));

            _genericDef = other._genericDef;
            foreach (ThingDef def in _genericDef.AvailableDefs)
                _thingFilter.SetAllow(def, true);
        }

        /// <summary>
        /// Gets the generic def this selector is about.
        /// </summary>
        public AIGenericDef GenericDef => _genericDef;

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
            if (Scribe.mode == LoadSaveMode.Saving || Scribe.mode == LoadSaveMode.LoadingVars)
            {
                _defName = _genericDef?.defName ?? string.Empty;
                Scribe_Values.Look(ref _defName, nameof(_defName));
            }

            if (Scribe.mode == LoadSaveMode.ResolvingCrossRefs)
            {
                _genericDef = DefDatabase<AIGenericDef>.GetNamed(_defName);
            }
        }
    }
}
