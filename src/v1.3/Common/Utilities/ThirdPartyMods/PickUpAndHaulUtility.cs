// <copyright file="PickUpAndHaulUtility.cs" company="Zizhen Li">
// Copyright (c) 2019 - 2020 Zizhen Li. All rights reserved.
// Licensed under the LGPL-3.0-only license. See LICENSE.md file in the project root for full license information.
// </copyright>

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace AwesomeInventory
{
    /// <summary>
    /// Utility support for Pickup and haul.
    /// </summary>
    public static class PickUpAndHaulUtility
    {
        [SuppressMessage("Globalization", "CA1308:Normalize strings to uppercase", Justification = "Lower case is required.")]
        private static string _packageID = "Mehni.PickUpAndHaul".ToLowerInvariant();

        static PickUpAndHaulUtility()
        {
            IsActive = LoadedModManager.RunningModsListForReading.Any(m => m.PackageId == _packageID);
        }

        /// <summary>
        /// Gets a value indicating whether pickup and Haul is actived in this save.
        /// </summary>
        public static bool IsActive { get; private set; }
    }
}
