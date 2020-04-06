// <copyright file="QualityColorCCMB.cs" company="Zizhen Li">
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
    /// <summary>
    /// Color Coded Mood Bar theme for quality color.
    /// </summary>
    [StaticConstructorOnStartup]
    public class QualityColorCCMB : QualityColor
    {
        static QualityColorCCMB()
        {
            Register(new QualityColorCCMB());
        }

        /// <inheritdoc/>
        public override Color Awful => AwesomeInventoryTex.CCMB_Red;

        /// <inheritdoc/>
        public override Color Poor => AwesomeInventoryTex.CCMB_Orange;

        /// <inheritdoc/>
        public override Color Normal => AwesomeInventoryTex.CCMB_Yellow;

        /// <inheritdoc/>
        public override Color Good => AwesomeInventoryTex.CCMB_NeutralColor;

        /// <inheritdoc/>
        public override Color Excellent => AwesomeInventoryTex.CCMB_Cyan;

        /// <inheritdoc/>
        public override Color Masterwork => AwesomeInventoryTex.CCMB_HappyColor;

        /// <inheritdoc/>
        public override Color Legendary => ColorLibrary.BrightPurple;

        /// <inheritdoc/>
        public override Color Generic => Color.white;

        /// <inheritdoc/>
        public override int ID { get; } = AwesomeInventoryServiceProvider.GetNextAvailablePluginID();

        /// <inheritdoc/>
        public override string DisplayName => "Color Coded Mood Bar";
    }
}
