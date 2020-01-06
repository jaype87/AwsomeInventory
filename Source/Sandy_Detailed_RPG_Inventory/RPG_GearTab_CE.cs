using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using UnityEngine;
using Verse;

namespace RPG_Inventory_Remake
{
    public class RPG_GearTab_CE : ITab_Pawn_Gear
    {
        private Vector2 scrollPosition = Vector2.zero;
        private const float TopPadding = 20f;
        private const float ThingIconSize = 28f;
        private const float ThingRowHeight = 28f;
        private const float ThingLeftX = 36f;

        private static bool _isJealous = true;
        private static bool _isGreedy = false;
        private static bool _isAscetic = false;
        private static bool _selectJealous = false;
        private static bool _selectGreedy = false;
        private static bool _selectAscetic = false;

        private RPG_Pawn _selPawn;

        private static List<Thing> workingInvList = new List<Thing>();
        public static readonly Vector3 PawnTextureCameraOffset = new Vector3(0f, 0f, 0f);

        //some variables CE used
        #region CE_Field

        #endregion CE_Field

        public RPG_GearTab_CE()
        {
            size = new Vector2(550f, 500f);
            labelKey = "TabGear";
            tutorTag = "Gear";
        }

        // It is called right before the tab is drawn
        protected override void UpdateSize()
        {
            
        }

        public override bool IsVisible
        {
            get
            {
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

            // Draw checkbox option for Jealous
            Rect rectJealous = new Rect(20f, 0f, 80f, 30f);
            if (Widgets.RadioButtonLabeled(rectJealous, "Sandy_Jealous".Translate(), _isJealous))
            {
                _isGreedy = _isAscetic = false;
                _isJealous = true;
            }
            // Draw checkbox option for Greddy
            Rect rectGreedy = new Rect(140f, 0f, 80f, 30f);
            if (Widgets.RadioButtonLabeled(rectGreedy, "Sandy_Greedy".Translate(), _isGreedy))
            {
                _isJealous = _isAscetic = false;
                _isGreedy = true;
            }
            // Draw checkbox option for Ascetic
            Rect rectAscetic = new Rect(260f, 0f, 80f, 30f);
            if (Widgets.RadioButtonLabeled(rectAscetic, "Sandy_Ascetic".Translate(), _isAscetic))
            {
                _isJealous = _isGreedy = false;
                _isAscetic = true;
            }

            //// Starting to draw gear/stats display
            //Rect position = new Rect(10, 30, this.size.x - 20, this.size.y - 20 - 20);
            //GUI.BeginGroup(position);
            //// Redundent: Text.Font = GameFont.Small;
            //GUI.color = Color.white;
            //Rect outerRect = new Rect(0f, 0f, position.width, position.height - 60);
            //Rect viewRect = new Rect(0f, 0f, position.width - 20f, this.scrollViewHeight);
            //Widgets.BeginScrollView(outerRect, ref this.scrollPosition, viewRect, true);

            if (_isJealous)
            {
                FillTabOperations.DrawJealous(_selPawn, this.size);
            }
            else if (_isGreedy)
            {
                FillTabOperations.DrawGreedy(_selPawn, this.size);
            }
            else if (_isAscetic)
            {
                FillTabOperations.DrawAscetic();
            }
            else
            {
                throw new InvalidOperationException("No Display Option is chosen.");
            }
        }
    }
}
