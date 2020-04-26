// <copyright file="BetterPawnControlUtility.cs" company="Zizhen Li">
// Copyright (c) 2019 - 2020 Zizhen Li. All rights reserved.
// Licensed under the LGPL-3.0-only license. See LICENSE.md file in the project root for full license information.
// </copyright>

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace AwesomeInventory
{
    /// <summary>
    /// Utility class for supporting Better Pawn Control.
    /// </summary>
    [StaticConstructorOnStartup]
    public static class BetterPawnControlUtility
    {
        /// <summary>
        /// Package ID of the mod to support.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Globalization", "CA1308:Normalize strings to uppercase", Justification = "Lower case required.")]
        private static string _packageID = "VouLT.BetterPawnControl".ToLowerInvariant();

        private static string _assemblyName = "BetterPawnControl";

        private static MethodInfo _saveState;

        static BetterPawnControlUtility()
        {
            IsActive = LoadedModManager.RunningModsListForReading.Any(m => m.PackageId == _packageID);
            if (IsActive)
            {
                _saveState = AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault((a) => a.GetName().Name == _assemblyName)
                    ?.GetType("BetterPawnControl.AssignManager").GetMethod("SaveCurrentState", BindingFlags.NonPublic | BindingFlags.Static);

                if (_saveState == null)
                    Log.Error("Can't patch Better Pawn Control.");
            }
        }

        /// <summary>
        /// Gets a value indicating whether the mod to support is present in the current save.
        /// </summary>
        public static bool IsActive { get; private set; }

        /// <summary>
        /// Save states of <paramref name="pawns"/> in BPC.
        /// </summary>
        /// <param name="pawns"> Pawns to save state. </param>
        public static void SaveState(List<Pawn> pawns)
        {
            _saveState.Invoke(null, new[] { pawns });
        }
    }
}
