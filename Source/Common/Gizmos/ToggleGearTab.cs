using RimWorld;
using System;
using System.Linq;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace RPG_Inventory_Remake_Common
{
    public class ToggleGearTab : Command_Action
    {
        private Type _tabType;
        // Gear_Helmet.png Designed By nickfz from <a href="https://pngtree.com/">Pngtree.com</a>
        private static readonly Texture2D _icon = ContentFinder<Texture2D>.Get("UI/Icons/Gear_Helmet_Colored", true);
        public ToggleGearTab(Type tabType)
        {
            hotKey = KeyBindingDefOf.Misc12;
            action = ToggleTab;
            icon = _icon;
            _tabType = tabType;
        }

        public override string Desc => string.Concat("Corgi_ToggleGearTab".Translate()
                                                    + "\n"
                                                    + "Hotkey binded to Misc 12");

        private void ToggleTab()
        {
            Type inspectTabType = _tabType;
            MainTabWindow_Inspect mainTabWindow_Inspect = (MainTabWindow_Inspect)MainButtonDefOf.Inspect.TabWindow;

            if (inspectTabType == mainTabWindow_Inspect.OpenTabType)
            {
                mainTabWindow_Inspect.OpenTabType = null;
                SoundDefOf.TabClose.PlayOneShotOnCamera();
            }
            else
            {
                InspectTabBase inspectTabBase = mainTabWindow_Inspect.CurTabs.Where((InspectTabBase t) => inspectTabType.IsAssignableFrom(t.GetType())).FirstOrDefault();
                inspectTabBase.OnOpen();
                mainTabWindow_Inspect.OpenTabType = inspectTabType;
                SoundDefOf.TabOpen.PlayOneShotOnCamera();
            }
        }
    }
}