// <copyright file="DefUtility.cs" company="Zizhen Li">
// Copyright (c) 2019 - 2020 Zizhen Li. All rights reserved.
// Licensed under the LGPL-3.0-only license. See LICENSE.md file in the project root for full license information.
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse;

namespace AwesomeInventory
{
    /// <summary>
    /// Utilities for <see cref="Def"/>.
    /// </summary>
    public static class DefUtility
    {
        /// <summary>
        /// Gets a GiveShortHash from <see cref="ShortHashGiver"/>.
        /// </summary>
        public static MethodInfo GiveShortHash { get; }
            = typeof(ShortHashGiver).GetMethod("GiveShortHash", BindingFlags.NonPublic | BindingFlags.Static);
    }
}
