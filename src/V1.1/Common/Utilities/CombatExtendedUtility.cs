// <copyright file="CombatExtendedUtility.cs" company="Zizhen Li">
// Copyright (c) 2019 - 2020 Zizhen Li. All rights reserved.
// Licensed under the LGPL-3.0-only license. See LICENSE.md file in the project root for full license information.
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace AwesomeInventory
{
    /// <summary>
    /// Utility class for supporting Combat Extended.
    /// </summary>
    public static class CombatExtendedUtility
    {
        private const string _packageID = "CETeam.CombatExtended";

        private static string _assemblyName = "CombatExtended";

        static CombatExtendedUtility()
        {
            IsActive = LoadedModManager.RunningModsListForReading.Any(m => m.PackageId == _packageID);
            if (IsActive)
            {
                Assembly assembly = AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(a => a.GetName().Name == _assemblyName);
                if (assembly != null)
                {
                }
            }
        }

        /// <summary>
        /// Gets a value indicating whether the mod to support is present in the current save.
        /// </summary>
        public static bool IsActive { get; private set; }
    }
}
