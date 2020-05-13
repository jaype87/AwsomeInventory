// <copyright file="OverviewTab.cs" company="Zizhen Li">
// Copyright (c) 2019 - 2020 Zizhen Li. All rights reserved.
// Licensed under the LGPL-3.0-only license. See LICENSE.md file in the project root for full license information.
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace AwesomeInventory.UI
{
    /// <summary>
    /// Base class for tabs in inventory overview dialog.
    /// </summary>
    public abstract class OverviewTab
    {
        /// <summary>
        /// Gets the label for tab.
        /// </summary>
        public abstract string Label { get; }

        /// <summary>
        /// Draw context in tab.
        /// </summary>
        /// <param name="rect"> Rect for drawing. </param>
        public abstract void DoTabContent(Rect rect);

        /// <summary>
        /// Initialize states for tab.
        /// </summary>
        public abstract void Init();
    }
}
