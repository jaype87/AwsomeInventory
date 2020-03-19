// <copyright file="IDrawHelper.cs" company="Zizhen Li">
// Copyright (c) Zizhen Li. All rights reserved.
// Licensed under the GPL-3.0-only license. See LICENSE.md file in the project root for full license information.
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace AwesomeInventory.UI
{
    /// <summary>
    /// A helper class for drawing in game.
    /// It only consists of member methods while <see cref="DrawUtility"/> only has static methods.
    /// </summary>
    public interface IDrawHelper
    {
        /// <summary>
        /// Compile a tooltip string for <paramref name="thing"/>.
        /// </summary>
        /// <param name="thing"> <paramref name="thing"/> that needs a tooltip text. </param>
        /// <param name="labelcap"> If true, capitalize the first letter of label. </param>
        /// <param name="isForced"> Indicates if <paramref name="thing"/> is forced on pawn. </param>
        /// <returns> A tooltip string. </returns>
        string TooltipTextFor(Thing thing, bool labelcap, bool isForced);
    }
}
