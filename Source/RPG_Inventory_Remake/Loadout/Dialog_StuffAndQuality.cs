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
        private Thing _thing;
        private RPGILoadout<Thing> _loadout;
        private static float _scrollViewHeight;
        private static QualityRange _qualityRange;
        private static FloatRange _hitpointRange;
        private static Vector2 _scrollPosition = Vector2.zero;

        public const float RangeLabelHeight = 19f;
        public const float SliderHeight = 28f;
        public const float SliderTab = 20f;


        public Dialog_StuffAndQuality(Thing thing, RPGILoadout<Thing> loadout)
        {
            _loadout = loadout ?? throw new ArgumentNullException(nameof(loadout));
            _thing = thing;
            closeOnClickedOutside = true;
            absorbInputAroundWindow = true;

            ThingFilter filter = loadout[thing].Filter;
            _qualityRange = filter.AllowedQualityLevels;
            _hitpointRange = filter.AllowedHitPointsPercents;
        }


        public override Vector2 InitialSize => new Vector2(450, 300);

        public override void DoWindowContents(Rect inRect)
        {
            // <Layout>
            // Title
            // Dropdown list | Stats
            //               | Hitpoint slider
            //               | Quality slider
            //               | Close Button

            Rect canvas = new Rect(inRect);
            GUI.DrawTexture(canvas, Texture2D.blackTexture);
            float rollingY = 0;

            // Comment Group 1 
            //Draw title
            UtilityDraw.DrawTitle(canvas.position, UIText.ChooseMaterialAndQuality.Translate(), ref rollingY);

            // Group 1
            // Draw dropdown list
            canvas.y = rollingY + GenUI.GapSmall;
            DrawStuffDropdownList(canvas.LeftPart(0.4f), ref rollingY);

            // Group 1
            // Draw stats
            Rect rect = canvas.RightPart(0.6f);
            if (_thing.def.IsApparel)
            {

            }

            // Group 1
            // Draw hitpoint slider
            ThingFilter filter = _loadout[_thing].Filter;
            Rect hitpointRect = new Rect(0, rect.y, rect.width * 0.7f, SliderHeight);
            hitpointRect = hitpointRect.CenteredOnXIn(rect);

            FloatRange hitpointRange = filter.AllowedHitPointsPercents;
            Widgets.FloatRange(hitpointRect, Rand.Int, ref hitpointRange, 0f, 1f, "HitPoints", ToStringStyle.PercentZero);
            if (hitpointRange != _hitpointRange)
            {
                filter.AllowedHitPointsPercents = hitpointRange;
                _thing.HitPoints = Mathf.RoundToInt(hitpointRange.TrueMin * _thing.MaxHitPoints);
                _hitpointRange = hitpointRange;
            }

            // Group 1 2 
            // Draw quality slider
            Rect qualityRect = new Rect(hitpointRect);
            qualityRect.y = hitpointRect.yMax + GenUI.GapSmall;
            QualityRange qualityRange = filter.AllowedQualityLevels;
            _thing.TryGetQuality(out qualityRange.min);
            Widgets.QualityRange(qualityRect, Rand.Int, ref qualityRange);
            if (_qualityRange != qualityRange)
            {
                filter.AllowedQualityLevels = _qualityRange = qualityRange;
                ThingFilterPackage package = _loadout[_thing];
                // Group 2 
                // Remove old key
                _loadout.RemoveItem(_thing);
                _thing.TryGetComp<CompQuality>()?.SetQuality(filter.AllowedQualityLevels.min, default);
                // Group 2 
                // Re-hash
                _loadout.AddPackage(package);
            }

            // Group 1
            // Draw Close Button
            Rect closeButtonRect = new Rect(qualityRect.x, inRect.yMax - GenUI.ListSpacing, qualityRect.width, GenUI.ListSpacing);
            if (Widgets.ButtonText(closeButtonRect, "Close".Translate()))
            {
                Close();
            }
        }

        public override void PreClose()
        {
            _scrollPosition = Vector2.zero;
        }

        #region Private Methods

        /// <summary>
        /// Draw list of stuffs that can be used to make thing
        /// </summary>
        /// <param name="outRect"></param>
        /// <param name="rollingY">return next available Y</param>
        private void DrawStuffDropdownList(Rect outRect, ref float rollingY)
        {
            Rect viewRect = new Rect(0, 0, outRect.width, _scrollViewHeight);
            Text.Font = GameFont.Small;
            Widgets.BeginScrollView(outRect, ref _scrollPosition, viewRect);

            Rect row = new Rect(viewRect.x, viewRect.y, viewRect.width, GenUI.ListSpacing);
            List<ThingDef> stuffs = GenStuff.AllowedStuffsFor(_thing.def).ToList();
            if (!stuffs.Any())
            {
                Widgets.Label(row, UIText.NoMaterial.Translate());
            }
            for (int i = 0; i < stuffs.Count; ++i)
            {
                Texture2D texture2D = i % 2 == 0 ? TexUI.TextBGBlack : TexUI.GrayTextBG;
                GUI.DrawTexture(row, texture2D);

                ThingDef stuff = stuffs[i];
                UtilityDraw.DrawLineButton
                    (row
                    , stuff.LabelAsStuff.CapitalizeFirst()
                    , _thing
                    , (thing) =>
                    {
                        // Key has changed, has to get a new hash value
                        ThingFilterPackage package = _loadout[thing];
                        _loadout.RemoveItem(thing);
                        thing.SetStuffDirect(stuff);
                        thing.HitPoints = thing.MaxHitPoints;
                        _loadout.AddPackage(package);
                    });

                row.y = row.yMax;
            }

            if (Event.current.type == EventType.layout)
            {
                _scrollViewHeight = row.yMax;
                rollingY = row.yMax;
            }
            Widgets.EndScrollView();
        }

        private void DrawStats(Rect rect, Thing thing, float rollingY)
        {
            if (thing.def.IsApparel)
            {
            }
        }


        #endregion

    }
}
