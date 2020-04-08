// <copyright file="AwesomeInventorySetting.cs" company="Zizhen Li">
// Copyright (c) 2019 - 2020 Zizhen Li. All rights reserved.
// Licensed under the LGPL-3.0-only license. See LICENSE.md file in the project root for full license information.
// </copyright>

using Verse;

namespace AwesomeInventory
{
#pragma warning disable SA1401 // Fields should be private
#pragma warning disable CA1051 // Do not declare visible instance fields

    /// <summary>
    /// User setting for AwesomeInventory.
    /// </summary>
    public class AwesomeInventorySetting : ModSettings
    {
        /// <summary>
        /// Use loadout if true.
        /// </summary>
        public bool UseLoadout = true;

        /// <summary>
        /// Use toggle gizmo.
        /// </summary>
        public bool UseToggleGizmo = true;

        /// <summary>
        /// Allow AwesomeInventory to choose appropriate weapons for pawns.
        /// </summary>
        public bool AutoEquipWeapon = true;

        /// <summary>
        /// Plugin ID for the <see cref="AwesomeInventory.UI.QualityColor"/> class.
        /// </summary>
        public int QualityColorPluginID;

        /// <summary>
        /// Save state.
        /// </summary>
        public override void ExposeData()
        {
            Scribe_Values.Look(ref UseLoadout, nameof(UseLoadout));
            Scribe_Values.Look(ref AutoEquipWeapon, nameof(AutoEquipWeapon));
            Scribe_Values.Look(ref QualityColorPluginID, nameof(QualityColorPluginID));
            Scribe_Values.Look(ref UseToggleGizmo, nameof(UseToggleGizmo));
        }
    }

#pragma warning restore CA1051 // Do not declare visible instance fields
#pragma warning restore SA1401 // Fields should be private
}
