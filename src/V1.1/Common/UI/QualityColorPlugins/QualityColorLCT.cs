// <copyright file="QualityColorLCT.cs" company="Zizhen Li">
// Copyright (c) 2019 - 2020 Zizhen Li. All rights reserved.
// Licensed under the LGPL-3.0-only license. See LICENSE.md file in the project root for full license information.
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace AwesomeInventory.UI
{
    /// <inheritdoc/>
    [StaticConstructorOnStartup]
    public class QualityColorLCT : QualityColor
    {
        static QualityColorLCT()
        {
            Register(new QualityColorLCT());
        }

        /// <inheritdoc/>
        public override Color Awful => AwesomeInventoryTex.LCT_PantoneRed;

        /// <inheritdoc/>
        public override Color Poor => AwesomeInventoryTex.LCT_AtomicTangerine;

        /// <inheritdoc/>
        public override Color Normal => AwesomeInventoryTex.LCT_CanaryYellow;

        /// <inheritdoc/>
        public override Color Good => Color.white;

        /// <inheritdoc/>
        public override Color Excellent => AwesomeInventoryTex.LCT_Olivine;

        /// <inheritdoc/>
        public override Color Masterwork => ColorLibrary.Cyan;

        /// <inheritdoc/>
        public override Color Legendary => AwesomeInventoryTex.LCT_MediumPurple;

        /// <inheritdoc/>
        public override Color Generic => ColorLibrary.LightGreen;

        /// <inheritdoc/>
        public override int ID { get; } = AwesomeInventoryServiceProvider.GetNextAvailablePluginID();

        /// <inheritdoc/>
        public override string DisplayName => "Lighter Colorful Traits";
    }
}
