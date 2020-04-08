// <copyright file="ToggleGearTab.cs" company="Zizhen Li">
// Copyright (c) 2019 - 2020 Zizhen Li. All rights reserved.
// Licensed under the LGPL-3.0-only license. See LICENSE.md file in the project root for full license information.
// </copyright>

using System;
using System.Linq;
using RimWorld;
using Verse;
using Verse.Sound;

namespace AwesomeInventory.UI
{
    /// <summary>
    /// Toggle Gear Tab.
    /// </summary>
    /// <remarks> Gear_Helmet.png Designed By nickfz from https://pngtree.com/Pngtree.com .</remarks>
    public class ToggleGearTab : Command_Action
    {
        private Type _tabType;

        /// <summary>
        /// Initializes a new instance of the <see cref="ToggleGearTab"/> class.
        /// </summary>
        /// <param name="tabType"> Tab to open when toggle. </param>
        public ToggleGearTab(Type tabType)
        {
            hotKey = KeyBindingDefOf.Misc12;
            action = ToggleTab;
            icon = TexResource.GearHelmet;
            _tabType = tabType;
        }

        /// <summary>
        /// Gets tooltip when hover on the Gizmo.
        /// </summary>
        public override string Desc => string.Concat(UIText.ToggleGearTab.TranslateSimple()
                                                    + "\n"
                                                    + "Hotkey binded to Misc 12, recommand assign it to key X");

        private void ToggleTab()
        {
            MainTabWindow_Inspect mainTabWindow_Inspect = (MainTabWindow_Inspect)Find.MainTabsRoot.OpenTab.TabWindow;

            if (_tabType == mainTabWindow_Inspect.OpenTabType)
            {
                mainTabWindow_Inspect.OpenTabType = null;
                SoundDefOf.TabClose.PlayOneShotOnCamera();
            }
            else
            {
                InspectTabBase inspectTabBase = mainTabWindow_Inspect.CurTabs.Where((InspectTabBase t) => _tabType.IsAssignableFrom(t.GetType())).FirstOrDefault();
                inspectTabBase?.OnOpen();
                mainTabWindow_Inspect.OpenTabType = _tabType;
                SoundDefOf.TabOpen.PlayOneShotOnCamera();
            }
        }
    }
}