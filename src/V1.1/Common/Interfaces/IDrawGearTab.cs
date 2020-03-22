// <copyright file="IDrawGearTab.cs" company="Zizhen Li">
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
    /// Draw contents for <see cref="ITab_Pawn_Gear"/>.
    /// </summary>
    public interface IDrawGearTab
    {
        /// <summary>
        /// Draw jealous tab.
        /// </summary>
        /// <param name="pawn"> Pawn that owns the tab. </param>
        /// <param name="canvas"> Position on screen to draw on. </param>
        /// <param name="apparelChanged"> Indicates whether apparels on pawn have changed. </param>
        void DrawJealous(Pawn pawn, Rect canvas, bool apparelChanged);

        /// <summary>
        /// Draw greedy tab.
        /// </summary>
        /// <param name="pawn"> Pawn that owns the tab. </param>
        /// <param name="canvas"> Position on screen to draw on. </param>
        /// <param name="apparelChanged"> Indicates whether apparels on pawn have changed. </param>
        void DrawGreedy(Pawn pawn, Rect canvas, bool apparelChanged);

        /// <summary>
        /// Draw ascetic tab.
        /// </summary>
        void DrawAscetic();

        /// <summary>
        /// Reset the scroll position of gear tab and clear cache.
        /// </summary>
        void Reset();
    }
}
