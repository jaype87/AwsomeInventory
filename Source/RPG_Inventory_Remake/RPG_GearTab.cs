using RimWorld;
using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;
using RPG_Inventory_Remake_Common;
using RPGIResource;

namespace RPG_Inventory_Remake
{
    public class RPG_GearTab : ITab_Pawn_Gear
    {
        // TODO set a static constructor to queue up unload jobs after game restarts

        private static bool _isJealous = true;
        private static bool _isGreedy = false;
        private static bool _isAscetic = false;

        private RPG_Pawn _selPawn;

        public static readonly Vector3 PawnTextureCameraOffset = new Vector3(0f, 0f, 0f);

        public RPG_GearTab()
        {
            size = new Vector2(550f, 500f);
            labelKey = "TabGear";
            tutorTag = "Gear";
        }

        // It is called right before the tab is drawn
        protected override void UpdateSize()
        {

        }

        public override void OnOpen()
        {
            base.OnOpen();
            FillTabOperations.ScrollPosition = Vector2.zero;

        }

        public override bool IsVisible
        {
            get
            {
                if (SelPawn != _selPawn?.Pawn)
                {
                    FillTabOperations.ScrollPosition = Vector2.zero;
                }
                _selPawn = new RPG_Pawn(SelPawn, SelThing);
                return Utility.ShouldShowInventory(_selPawn.Pawn) ||
                       Utility.ShouldShowApparel(_selPawn.Pawn) ||
                       Utility.ShouldShowEquipment(_selPawn.Pawn);
            }
        }

        protected override void FillTab()
        {
            _selPawn = _selPawn ?? new RPG_Pawn(SelPawn, SelThing);
            Text.Font = GameFont.Small;
            GUI.color = Color.white;


            // Draw checkbox option for Jealous
            string translatedText = UIText.JealousTab.Translate();
            Rect headerRect = GetHeaderRect(GenUI.Gap, translatedText);
            if (Widgets.RadioButtonLabeled(headerRect, translatedText, _isJealous))
            {
                _isGreedy = _isAscetic = false;
                _isJealous = true;
            }
            // Draw checkbox option for Greedy
            translatedText = UIText.GreedyTab.Translate();
            headerRect = GetHeaderRect(headerRect.xMax + GenUI.GapWide, translatedText);
            if (Widgets.RadioButtonLabeled(headerRect, translatedText, _isGreedy))
            {
                _isJealous = _isAscetic = false;
                _isGreedy = true;
            }
            // Draw checkbox option for Ascetic
            translatedText = UIText.AsceticTab.Translate();
            headerRect = GetHeaderRect(headerRect.xMax + GenUI.GapWide, translatedText);
            if (Widgets.RadioButtonLabeled(headerRect, translatedText, _isAscetic))
            {
                _isJealous = _isGreedy = false;
                _isAscetic = true;
            }

            if (_isJealous)
            {
                FillTabOperations.DrawJealous(_selPawn, new Rect(0, headerRect.yMax, size.x, size.y - headerRect.yMax));
            }
            else if (_isGreedy)
            {
                FillTabOperations.DrawGreedy(_selPawn, new Rect(0, headerRect.yMax, size.x, size.y - headerRect.yMax));
            }
            else if (_isAscetic)
            {
            }
            else
            {
                throw new InvalidOperationException("No Display Option is chosen.");
            }
        }

        private Rect GetHeaderRect(float x, string translatedText)
        {
            float width = GenUI.GetWidthCached(translatedText) + Widgets.RadioButtonSize + GenUI.GapSmall;
            return new Rect(x, GenUI.GapSmall, width, GenUI.ListSpacing);
        }
    }
}
