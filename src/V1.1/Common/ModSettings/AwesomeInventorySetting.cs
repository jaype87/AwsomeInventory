// <copyright file="AwesomeInventorySetting.cs" company="Zizhen Li">
// Copyright (c) Zizhen Li. All rights reserved.
// Licensed under the GPL-3.0-only license. See LICENSE.md file in the project root for full license information.
// </copyright>

using Verse;

namespace AwesomeInventory
{
#pragma warning disable SA1401 // Fields should be private
#pragma warning disable CA1051 // Do not declare visible instance fields

    /// <summary>
    /// User setting for AwesomeInventory.
    /// </summary>
    public class AwesomeInventorySetting : Verse.ModSettings
    {
        /// <summary>
        /// Use loadout if true.
        /// </summary>
        public bool UseLoadout;

        /// <summary>
        /// Save state.
        /// </summary>
        public override void ExposeData()
        {
            Scribe_Values.Look(ref UseLoadout, "UseLoadout");
        }
    }

#pragma warning restore CA1051 // Do not declare visible instance fields
#pragma warning restore SA1401 // Fields should be private
}
