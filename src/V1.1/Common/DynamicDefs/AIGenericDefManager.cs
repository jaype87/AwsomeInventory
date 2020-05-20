// <copyright file="AIGenericDefManager.cs" company="Zizhen Li">
// Copyright (c) 2019 - 2020 Zizhen Li. All rights reserved.
// Licensed under the LGPL-3.0-only license. See LICENSE.md file in the project root for full license information.
// </copyright>

using Verse;

namespace AwesomeInventory.Loadout
{
    /// <summary>
    /// LoadoutGenericDef handles Generic LoadoutSlots.
    /// </summary>
    [StaticConstructorOnStartup]
    public class AIGenericDefManager : ThingDef
    {
        /// <remark>
        /// This constructor gets run on startup of RimWorld and generates the various LoadoutGenericDef instance objects akin to having been loaded from xml.
        /// </remark>
        static AIGenericDefManager()
        {
            DefDatabase<AIGenericDef>.Add(AIGenericMeal.Instance);
            DefDatabase<AIGenericDef>.Add(AIGenericRawFood.Instance);
            DefDatabase<AIGenericDef>.Add(AIGenericDrugs.Instance);
            DefDatabase<AIGenericDef>.Add(AIGenericMedicine.Instance);
            DefDatabase<AIGenericDef>.Add(AIGenericMeat.Instance);
        }
    }
}
