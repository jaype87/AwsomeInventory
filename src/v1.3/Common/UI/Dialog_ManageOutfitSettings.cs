// <copyright file="Dialog_ManageOutfitSettings.cs" company="Zizhen Li">
// Copyright (c) 2019 - 2020 Zizhen Li. All rights reserved.
// Licensed under the LGPL-3.0-only license. See LICENSE.md file in the project root for full license information.
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AwesomeInventory.Loadout;
using RimWorld;
using UnityEngine;
using Verse;

namespace AwesomeInventory.UI
{
    /// <summary>
    /// A dialog window for managing global settings for <see cref="Loadout.AwesomeInventoryLoadout"/>.
    /// </summary>
    public class Dialog_ManageOutfitSettings : Window
    {
        private static ThingFilter _apparelGlobalFilter;
        private ThingFilter _filter;
        private Vector2 _scrollPosition = Vector2.zero;
        private ThingFilterUI.UIState _uiState;

        static Dialog_ManageOutfitSettings()
        {
            _apparelGlobalFilter = new ThingFilter();
            _apparelGlobalFilter.SetAllow(ThingCategoryDefOf.Apparel, true);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Dialog_ManageOutfitSettings"/> class.
        /// </summary>
        /// <param name="filter"> Global filter for loadout. </param>
        public Dialog_ManageOutfitSettings(ThingFilter filter)
        {
            _uiState                = new ThingFilterUI.UIState();
            _filter                 = filter;
            doCloseX                = true;
            absorbInputAroundWindow = true;
            closeOnClickedOutside   = true;
        }

        /// <summary>
        /// Gets initial size for the window.
        /// </summary>
        public override Vector2 InitialSize => new Vector2(350, 700);

        /// <summary>
        /// Draw window contents.
        /// </summary>
        /// <param name="inRect"> Rect for drawing. </param>
        public override void DoWindowContents(Rect inRect)
        {
            Rect labelRect = inRect.ReplaceHeight(GenUI.ListSpacing * 2);
            Widgets.TextArea(labelRect, UIText.GlobalOutfitSettingWarning.TranslateSimple(), true);

            Rect importRect = labelRect.ReplaceY(labelRect.yMax).ReplaceHeight(GenUI.ListSpacing);
            if (Widgets.ButtonText(importRect, UIText.ImportLoadout.TranslateSimple()))
            {
                FloatMenuUtility.MakeMenu(
                    LoadoutManager.Loadouts.Where(t => t.GetType() == typeof(AwesomeInventoryLoadout))
                    , (loadout) => loadout.label
                    , (loadout) => () => { _filter.CopyAllowancesFrom(loadout.filter); });
            }

            ThingFilterUI.DoThingFilterConfigWindow(
                inRect.ReplaceyMin(importRect.yMax + GenUI.GapSmall)
                , _uiState
                , _filter
                , _apparelGlobalFilter
                , 16
                , null
                , new[] { SpecialThingFilterDefOf.AllowNonDeadmansApparel });
        }
    }
}
