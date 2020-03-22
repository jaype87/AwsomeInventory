// <copyright file="RPGI_StuffDefOf.cs" company="Zizhen Li">
// Copyright (c) 2019 - 2020 Zizhen Li. All rights reserved.
// Licensed under the LGPL-3.0-only license. See LICENSE.md file in the project root for full license information.
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;

namespace AwesomeInventory.Loadout
{
    [DefOf]
    public static class RPGI_StuffDefOf
    {
        public static ThingDef RPGIGenericResource;

        static RPGI_StuffDefOf()
        {
            DefOfHelper.EnsureInitializedInCtor(typeof(RPGI_StuffDefOf));
        }
    }
}
