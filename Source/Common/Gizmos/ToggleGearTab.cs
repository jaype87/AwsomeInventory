using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using Verse.Sound;
using RimWorld;
using UnityEngine;

namespace RPG_Inventory_Remake
{
    public class ToggleGearTab : Command_Action
    {
        public ToggleGearTab()
        {
            hotKey = KeyBindingDefOf.Misc12;
            action = ToggleTab;
        }

        public override string Desc => string.Concat("Corgi_ToggleGearTab".Translate()
                                                    + "\n"
                                                    + "Hotkey binded to Misc 12");

        private void ToggleTab()
        {
            Type inspectTabType = typeof(RPG_GearTab_CE);
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