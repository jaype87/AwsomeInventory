// <copyright file="ModStarter.cs" company="Zizhen Li">
// Copyright (c) Zizhen Li. All rights reserved.
// Licensed under the GPL-3.0-only license. See LICENSE.md file in the project root for full license information.
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;

namespace AwesomeInventory.Common
{
    public class Dialog_Mod : Mod
    {
        // TODO add a getter method to update realtime value
        private static Setting settings;

        public static Setting Settings
        {
            get => settings;
        }

        // TODO Revisit ModStarter
        public Dialog_Mod(ModContentPack content) : base(content)
        {
            settings = GetSettings<Setting>();
        }

        public override void DoSettingsWindowContents(Rect inRect)
        {
            Listing_Standard listingStandard = new Listing_Standard();
            listingStandard.ColumnWidth = inRect.width / 3;
            listingStandard.Begin(inRect);
            listingStandard.CheckboxLabeled("Corgi_UseLoadout".Translate(), ref settings.UseLoadout, null);
            listingStandard.End();

            base.DoSettingsWindowContents(inRect);
        }

        public override string SettingsCategory()
        {
            return "RPG Style Inventory Remake";
        }
    }
}
