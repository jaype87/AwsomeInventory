// <copyright file="VGPGardenUtility.cs" company="Zizhen Li">
// Copyright (c) 2019 - 2020 Zizhen Li. All rights reserved.
// Licensed under the LGPL-3.0-only license. See LICENSE.md file in the project root for full license information.
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace AwesomeInventory
{
    /// <summary>
    /// Utility support for VGP Garden Gourment.
    /// </summary>
    public static class VGPGardenUtility
    {
        private const string _sweetMealDefName = "SweetMeals";

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Globalization", "CA1308:Normalize strings to uppercase", Justification = "Lower case required.")]
        private static string _packageID = "dismarzero.VGP.VGPVegetableGarden".ToLowerInvariant();

        static VGPGardenUtility()
        {
            IsActive = LoadedModManager.RunningModsListForReading.Any(m => m.PackageId == _packageID);
            if (IsActive)
                SweetMeals = DefDatabase<ThingCategoryDef>.GetNamed(_sweetMealDefName);
        }

        /// <summary>
        /// Gets a value indicating whether the mod to support is present in the current save.
        /// </summary>
        public static bool IsActive { get; private set; }

        /// <summary>
        /// Gets the SweetMeals ThingCategoryDef defined by VGP Garden Gourmet.
        /// </summary>
        public static ThingCategoryDef SweetMeals { get; }

        /// <summary>
        /// Check if <paramref name="thingDef"/> is sweet.
        /// </summary>
        /// <param name="thingDef"> ThingDef to check. </param>
        /// <returns> Returns true, if <paramref name="thingDef"/> is sweet. </returns>
        public static bool IsSweet(this ThingDef thingDef)
        {
            return thingDef.thingCategories?.Contains(SweetMeals) ?? false;
        }
    }
}
