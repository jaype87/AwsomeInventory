// <copyright file="IDrawGearTab.cs" company="Zizhen Li">
// Copyright (c) Zizhen Li. All rights reserved.
// Licensed under the GPL-3.0-only license. See LICENSE.md file in the project root for full license information.
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using UnityEngine;
using Verse;

namespace AwesomeInventory.Common
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
        void DrawJealous(PawnModal pawn, Rect canvas);

        /// <summary>
        /// Draw greedy tab.
        /// </summary>
        /// <param name="pawn"> Pawn that owns the tab. </param>
        /// <param name="canvas"> Position on screen to draw on. </param>
        void DrawGreedy(PawnModal pawn, Rect canvas);
    }
}
