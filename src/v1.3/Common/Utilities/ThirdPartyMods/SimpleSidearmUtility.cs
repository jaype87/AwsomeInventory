// <copyright file="SimpleSidearmUtility.cs" company="Zizhen Li">
// Copyright (c) 2019 - 2020 Zizhen Li. All rights reserved.
// Licensed under the LGPL-3.0-only license. See LICENSE.md file in the project root for full license information.
// </copyright>

using System;
using System.CodeDom;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace AwesomeInventory
{
    /// <summary>
    /// Utility support for Simple Sidearm.
    /// </summary>
    public static class SimpleSidearmUtility
    {
        private const string _assemblyName = "SimpleSidearms";

        [SuppressMessage("Globalization", "CA1308:Normalize strings to uppercase", Justification = "Lower case is required.")]
        private static string _packageID = "PeteTimesSix.SimpleSidearms".ToLowerInvariant();

        private static Type _compSidearmMemory;
        private static PropertyInfo _pawnMemory;
        private static MethodInfo _toThingDefStuffDefPair;
        private static MethodInfo _forgetSidearmMemory;

        [SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "Handled by Log.Error")]
        static SimpleSidearmUtility()
        {
            IsActive = LoadedModManager.RunningModsListForReading.Any(m => m.PackageId == _packageID);
            if (IsActive)
            {
                Assembly assembly = AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(a => a.GetName().Name == _assemblyName);
                if (assembly != null)
                {
                    try
                    {
                        _compSidearmMemory = assembly.GetType("SimpleSidearms.rimworld.CompSidearmMemory");
                        _pawnMemory = _compSidearmMemory.GetProperty("RememberedWeapons", BindingFlags.Public | BindingFlags.Instance);
                        _toThingDefStuffDefPair = assembly.GetType("SimpleSidearms.Extensions").GetMethod("toThingDefStuffDefPair", BindingFlags.Static | BindingFlags.Public);
                        _forgetSidearmMemory = _compSidearmMemory.GetMethod("ForgetSidearmMemory", BindingFlags.Public | BindingFlags.Instance);
                    }
                    catch (Exception e)
                    {
                        Log.Error(e.ToString());
                    }
                }
            }
        }

        /// <summary>
        /// Gets a value indicating whether simple sidearm is actived in this save.
        /// </summary>
        public static bool IsActive { get; private set; }

        /// <summary>
        /// Check if <paramref name="thing"/> is in Simple Sidearms's memory.
        /// </summary>
        /// <param name="pawn"> Pawn who has <paramref name="thing"/>. </param>
        /// <param name="thing"> Thing to check. </param>
        /// <returns> True, if <paramref name="thing"/> is in SS memory. </returns>
        public static bool InMemory(Pawn pawn, Thing thing)
        {
            ValidateArg.NotNull(pawn, nameof(pawn));

            ThingComp comp = pawn.AllComps.FirstOrDefault(t => t.GetType() == _compSidearmMemory);
            if (comp != null)
            {
                IList memory = (IList)_pawnMemory.GetValue(comp);
                return memory.Contains(_toThingDefStuffDefPair.Invoke(null, new[] { thing }));
            }

            return false;
        }

        /// <summary>
        /// Remove weaspon from sidearm memory.
        /// </summary>
        /// <param name="pawn"> Pawn who carries <paramref name="thing"/>. </param>
        /// <param name="thing"> Weapon to remove. </param>
        public static void RemoveWeaponFromMemory(Pawn pawn, Thing thing)
        {
            ValidateArg.NotNull(pawn, nameof(pawn));

            ThingComp comp = pawn.AllComps.FirstOrDefault(t => t.GetType() == _compSidearmMemory);
            if (comp != null)
            {
                _forgetSidearmMemory.Invoke(comp, new[] { _toThingDefStuffDefPair.Invoke(null, new[] { thing }) });
            }
        }
    }
}
