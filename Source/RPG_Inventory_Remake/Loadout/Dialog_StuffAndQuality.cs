using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;
using RimWorld;
using RPG_Inventory_Remake_Common;
using RPG_Inventory_Remake.Resources;

namespace RPG_Inventory_Remake.Loadout
{
    public class Dialog_StuffAndQuality : Window
    {
        private ThingDef _thingDef;
        private static float _scrollViewHeight;
        private static Vector2 _scrollPosition = Vector2.zero;


        public Dialog_StuffAndQuality(ThingDef thingDef)
        {
            _thingDef = thingDef;
        }


        public override Vector2 InitialSize => new Vector2(450, 300);
        public override void DoWindowContents(Rect inRect)
        {

            // <Layout>
            // Title
            // Dropdown list | Quality slider
            //               | OK Button

            Rect canvas = new Rect(inRect);
            GUI.DrawTexture(canvas, Texture2D.blackTexture);
            float rollingY = 0;

            // Draw title
            UtilityDraw.DrawTitle(canvas.position, UIText.ChooseMaterialAndQuality.Translate(), ref rollingY);

            // Draw dropdown list
            canvas.y = rollingY + GenUI.GapSmall;
            DrawStuffDropdownList(canvas, ref rollingY);
        }

        public override void PreClose()
        {
            _scrollPosition = Vector2.zero;
        }

        #region Private Methods

        /// <summary>
        /// Draw list of stuffs that can be used to make thing
        /// </summary>
        /// <param name="canvas"></param>
        /// <param name="rollingY">return next available Y</param>
        private void DrawStuffDropdownList(Rect canvas, ref float rollingY)
        {
            Rect outRect = canvas.LeftPart(0.4f);
            Rect viewRect = new Rect(0, 0, outRect.width, _scrollViewHeight);
            Text.Font = GameFont.Small;
            Widgets.BeginScrollView(outRect, ref _scrollPosition, viewRect);

            Rect row = new Rect(viewRect.x, viewRect.y, viewRect.width, Text.LineHeight);
            List<ThingDef> stuffs = GenStuff.AllowedStuffsFor(_thingDef).ToList();
            if (!stuffs.Any())
            {
                Widgets.Label(row, UIText.NoMaterial.Translate());
            }
            for (int i = 0; i < stuffs.Count; ++i)
            {
                if (i % 2 == 0)
                {
                    GUI.DrawTexture(row, TexUI.TextBGBlack);
                }
                else
                {
                    GUI.DrawTexture(row, TexUI.GrayTextBG);
                }
                Widgets.Label(row, stuffs[i].LabelAsStuff.CapitalizeFirst());
                row.y = row.yMax;
            }

            if (Event.current.type == EventType.layout)
            {
                _scrollViewHeight = row.yMax;
                rollingY = row.yMax;
            }
            Widgets.EndScrollView();
        }


        #endregion

    }
}
