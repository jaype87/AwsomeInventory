// <copyright file="IconRect.cs" company="Zizhen Li">
// Copyright (c) 2019 - 2020 Zizhen Li. All rights reserved.
// Licensed under the LGPL-3.0-only license. See LICENSE.md file in the project root for full license information.
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using UnityEngine;
using Verse;

namespace AwesomeInventory.UI
{
    /// <summary>
    /// Rect for drawing thing icons on the gear tab.
    /// </summary>
    /// <typeparam name="T"> Type of thing this icon rect represents. </typeparam>
    public class IconRect<T>
        where T : Thing
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="IconRect{T}"/> class.
        /// </summary>
        /// <param name="rect"> A <see cref="UnityEngine.Rect"/> to draw on. </param>
        /// <param name="filter"> Used to determine if a thing fits in <paramref name="filter"/>. </param>
        /// <param name="tooltip"> Tooltip for <paramref name="rect"/>. </param>
        public IconRect(Rect rect, Func<T, bool> filter, string tooltip)
        {
            this.Rect = rect;
            this.Filter = filter;
            this.Tooltip = tooltip;
        }

        /// <summary>
        /// Gets or sets a filter which is used to determine if a thing fits in <see cref="IconRect.Rect"/>.
        /// </summary>
        public Func<T, bool> Filter { get; set; }

        /// <summary>
        /// Gets or sets a <see cref="UnityEngine.Rect"/> to draw on.
        /// </summary>
        public Rect Rect { get; set; }

        /// <summary>
        /// Gets or sets tooltip for this rect.
        /// </summary>
        public string Tooltip { get; set; }
    }
}
