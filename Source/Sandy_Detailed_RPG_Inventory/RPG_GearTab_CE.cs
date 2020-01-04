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

        private float scrollViewHeight;

        private const float TopPadding = 20f;


        private const float ThingIconSize = 28f;

        private const float ThingRowHeight = 28f;

        private const float ThingLeftX = 36f;

        private RPG_Pawn _selPawn;

        private static List<Thing> workingInvList = new List<Thing>();

        public static readonly Vector3 PawnTextureCameraOffset = new Vector3(0f, 0f, 0f);

        //some variables CE used
        #region CE_Field

        #endregion CE_Field

        private bool _isGreedy = false;
        private bool _isJealous = false;
        private bool _isAscetic = false;

        public RPG_GearTab_CE()
        {
            size = new Vector2(550f, 500f);
            labelKey = "TabGear";
            tutorTag = "Gear";
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
            //_isGreedy = true;
            _isJealous = true;
            _selPawn = _selPawn ?? new RPG_Pawn(SelPawn, SelThing);
            
            Text.Font = GameFont.Small;

            // Draw checkbox option for Greddy
            Rect rectGreedy = new Rect(20f, 0f, 100f, 30f);
            Widgets.CheckboxLabeled(rectGreedy, "Sandy_Greedy".Translate(), ref _isGreedy, false, null, null, false);
            // Draw checkbox option for Jealous
            Rect rectJealous = new Rect(120f, 0f, 100f, 30f);
            Widgets.CheckboxLabeled(rectJealous, "Sandy_Jealous".Translate(), ref _isJealous, false, null, null, false);
            // Draw checkbox option for Ascetic
            Rect rectAscetic = new Rect(220f, 0f, 100f, 30f);
            Widgets.CheckboxLabeled(rectAscetic, "Sandy_Ascetic".Translate(), ref _isAscetic, false, null, null, false);

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
                //FillTabOperations.DrawGreedy(_selPawn, this.size);
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
