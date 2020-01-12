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
        // TODO set a static constructor to queue up unload jobs after game restarts

        private static bool _isJealous = true;
        private static bool _isGreedy = false;
        private static bool _isAscetic = false;

        private RPG_Pawn _selPawn;

        private static List<Thing> workingInvList = new List<Thing>();
        public static readonly Vector3 PawnTextureCameraOffset = new Vector3(0f, 0f, 0f);

        //some variables CE used
        #region CE_Field
        public static bool IsCE = false;
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
            Rect rectJealous = new Rect(20f, 0f, 160f, 30f);
            if (Widgets.RadioButtonLabeled(rectJealous, "Sandy_Jealous".Translate(), _isJealous))
            {
                _isGreedy = _isAscetic = false;
                _isJealous = true;
            }
            // Draw checkbox option for Greedy
            Rect rectGreedy = new Rect(220f, 0f, 120f, 30f);
            if (Widgets.RadioButtonLabeled(rectGreedy, "Sandy_Greedy".Translate(), _isGreedy))
            {
                _isJealous = _isAscetic = false;
                _isGreedy = true;
            }
            // Draw checkbox option for Ascetic
            Rect rectAscetic = new Rect(380f, 0f, 80f, 30f);
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

            if (!IsCE)
            {
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
            else
            {
                if (_isJealous)
                {
                    FillTabOperations.DrawJealousCE(_selPawn, this.size);
                }
                else if (_isGreedy)
                {
                    FillTabOperations.DrawGreedyCE(_selPawn, this.size);
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
}
