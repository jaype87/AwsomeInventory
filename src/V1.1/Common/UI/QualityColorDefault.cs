// <copyright file="QualityColorDefault.cs" company="Zizhen Li">
// Copyright (c) 2019 - 2020 Zizhen Li. All rights reserved.
// Licensed under the LGPL-3.0-only license. See LICENSE.md file in the project root for full license information.
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace AwesomeInventory.UI
{
    /// <inheritdoc/>
    [StaticConstructorOnStartup]
    public class QualityColorDefault : QualityColor
    {
        static QualityColorDefault()
        {
            Register(new QualityColorDefault());
        }

        /// <inheritdoc/>
        public override Color Awful => AwesomeInventoryTex.Valvet;

        /// <inheritdoc/>
        public override Color Poor => AwesomeInventoryTex.LightGrey;

        /// <inheritdoc/>
        public override Color Normal => Color.white;

        /// <inheritdoc/>
        public override Color Good => ColorLibrary.LightGreen;

        /// <inheritdoc/>
        public override Color Excellent => ColorLibrary.Cyan;

        /// <inheritdoc/>
        public override Color Masterwork => ColorLibrary.Sand;

        /// <inheritdoc/>
        public override Color Legendary => ColorLibrary.Orange;

        /// <inheritdoc/>
        public override Color Generic => AwesomeInventoryTex.Lavendar;

        /// <inheritdoc/>
        public override int ID => 0;

        /// <inheritdoc/>
        public override string DisplayName => UIText.AIDefault.TranslateSimple();
    }
}
