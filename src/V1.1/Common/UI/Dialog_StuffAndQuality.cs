// <copyright file="Dialog_StuffAndQuality.cs" company="Zizhen Li">
// Copyright (c) 2019 - 2020 Zizhen Li. All rights reserved.
// Licensed under the LGPL-3.0-only license. See LICENSE.md file in the project root for full license information.
// </copyright>

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text.RegularExpressions;
using AwesomeInventory.Loadout;
using AwesomeInventory.Utilities;
using RimWorld;
using RPG_Inventory_Remake_Common;
using RPGIResource;
using UnityEngine;
using Verse;

namespace AwesomeInventory.UI
{
    using TSPWQuality = ThingStuffPairWithQuality;
    public class Dialog_StuffAndQuality : Window
    {
        private Thing _thing;
        private AILoadout _loadout;
        private TSPWQuality _pair;
        private static ThingDef _stuffPreview;
        private static QualityRange _qualityRange;
        private static FloatRange _hitpointRange;

        private static Vector2 _initialSize;
        private static Vector2 _initialSizeForWeapon;
        private static float _scrollViewHeight;
        private static Vector2 _scrollPosition = Vector2.zero;

        private static readonly Regex _regex = new Regex(@"\d*", RegexOptions.Compiled);

        public static readonly List<StatDef> ArmorStats;
        public static readonly List<StatDef> BaseWeaponStats;
        public static readonly List<StatDef> MeleeWeaponStats;
        public static readonly List<string> RangedWeaponStats;

        public const float RangeLabelHeight = 19f;
        public const float SliderHeight = 28f;
        public const float SliderTab = 20f;

        static Dialog_StuffAndQuality()
        {
            float width = GenUI.GetWidthCached(UIText.TenCharsString.Times(9));
            _initialSize = new Vector2(width, Verse.UI.screenHeight / 3f + GenUI.ListSpacing);
            _initialSizeForWeapon = new Vector2(_initialSize.x * 1.1f, _initialSize.y);
            ArmorStats = new List<StatDef>()
            {
                DefDatabase<StatDef>.GetNamed(StatDefOf.MaxHitPoints.defName),
                DefDatabase<StatDef>.GetNamed(StatDefOf.ArmorRating_Sharp.defName),
                DefDatabase<StatDef>.GetNamed(StatDefOf.ArmorRating_Blunt.defName),
                DefDatabase<StatDef>.GetNamed(StatDefOf.ArmorRating_Heat.defName),
                DefDatabase<StatDef>.GetNamed(StatDefOf.Insulation_Cold.defName),
                DefDatabase<StatDef>.GetNamed(StatDefOf.Insulation_Heat.defName),
                DefDatabase<StatDef>.GetNamed(StatDefOf.Mass.defName)
            };
            BaseWeaponStats = new List<StatDef>()
            {
                DefDatabase<StatDef>.GetNamed(StatDefOf.MaxHitPoints.defName),
                DefDatabase<StatDef>.GetNamed(StatDefOf.MeleeWeapon_AverageDPS.defName),
                DefDatabase<StatDef>.GetNamed(UtilityConstant.MeleeWeapon_AverageArmorPenetration.defName),
                DefDatabase<StatDef>.GetNamed(StatDefOf.Mass.defName)
            };
            MeleeWeaponStats = new List<StatDef>()
            {
                DefDatabase<StatDef>.GetNamed(StatDefOf.MeleeWeapon_CooldownMultiplier.defName)
            };
            RangedWeaponStats = new List<string>()
            {
                StringConstant.ArmorPenetration.Translate(),
                StringConstant.Damage.Translate()
            };
        }

        public Dialog_StuffAndQuality(Thing thing, AILoadout loadout)
        {
            _loadout = loadout ?? throw new ArgumentNullException(nameof(loadout));
            _thing = thing;
            _pair = thing.MakeThingStuffPairWithQuality();
            _stuffPreview = _pair.stuff;
            closeOnClickedOutside = true;
            absorbInputAroundWindow = true;

            ThingFilter filter = loadout[_pair];
            QualityRange qualityRange = filter.AllowedQualityLevels;
            _thing.TryGetQuality(out qualityRange.min);
            filter.AllowedQualityLevels = qualityRange;

            _qualityRange = filter.AllowedQualityLevels;
            _hitpointRange = filter.AllowedHitPointsPercents;
        }


        public override Vector2 InitialSize
        {
            get
            {
                if (_thing.def.IsWeapon)
                {
                    return _initialSizeForWeapon;
                }
                return _initialSize;
            }
        }

        public override void DoWindowContents(Rect inRect)
        {
            // <Layout>
            // Title
            // Dropdown list | Stats
            //               | Hitpoint slider
            //               | Quality slider
            //               | Close Button

            Rect canvas = new Rect(inRect);
            float rollingY = 0;

            //Draw Header
            //Rect titleRec = UtilityDraw.DrawTitle(canvas.position, UIText.ChooseMaterialAndQuality.Translate(), ref rollingY);
            Rect titleRec = DrawUtility.DrawTitle(
                canvas.position, string.Concat(UIText.Customize.Translate(), " ", _thing.def.LabelCap)
                , ref rollingY);
            GUI.color = Color.grey;
            Text.Anchor = TextAnchor.LowerLeft;
            Text.WordWrap = false;
            Widgets.Label(new Rect(titleRec.xMax + GenUI.Gap, titleRec.y, canvas.width - titleRec.width, titleRec.height), string.Concat("(", UIText.MouseOverNumbersForDetails.Translate(), ")"));
            Text.Anchor = TextAnchor.UpperLeft;
            Text.WordWrap = true;
            GUI.color = Color.white;

            // Draw dropdown list
            canvas.yMin = rollingY + GenUI.GapSmall;
            DrawStuffDropdownList(canvas.LeftPart(0.25f), ref rollingY);

            // Draw stats
            Rect rect = canvas.RightPart(0.75f);
            float rollingY2 = rect.y;
            DrawStats(rect, _thing, ref rollingY2);
            rollingY2 += GenUI.GapWide;

            // Draw hitpoint slider
            ThingFilter filter = _loadout[_pair];
            int dragID = Rand.Int;
            Rect hitpointRect = new Rect(0, rollingY2, rect.width * 0.6f, SliderHeight);
            hitpointRect = hitpointRect.CenteredOnXIn(rect);

            FloatRange hitpointRange = filter.AllowedHitPointsPercents;
            Widgets.FloatRange(hitpointRect, dragID, ref hitpointRange, 0f, 1f, "HitPoints", ToStringStyle.PercentZero);
            if (hitpointRange != _hitpointRange)
            {
                _loadout.UpdateItem(_thing, hitpointRange);
                //filter.AllowedHitPointsPercents = hitpointRange;
                //_thing.HitPoints = Mathf.RoundToInt(hitpointRange.TrueMin * _thing.MaxHitPoints);
                _hitpointRange = hitpointRange;
            }

            // Draw quality slider
            Rect qualityRect = new Rect(hitpointRect)
            {
                y = hitpointRect.yMax + GenUI.GapSmall
            };

            QualityRange qualityRange = filter.AllowedQualityLevels;
            Widgets.QualityRange(qualityRect, ++dragID, ref qualityRange);
            if (_qualityRange != qualityRange)
            {
                filter.AllowedQualityLevels = _qualityRange = qualityRange;
                _loadout.UpdateItem(_thing, qualityRange.min);
                _pair.quality = qualityRange.min;
                _thing = _loadout[_pair].Thing;
            }

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
            Rect viewRect = new Rect(0, 0, outRect.width - GenUI.Gap, _scrollViewHeight);
            Text.Font = GameFont.Small;
            Widgets.BeginScrollView(outRect, ref _scrollPosition, viewRect);

            Rect row = new Rect(viewRect.x, viewRect.y, viewRect.width, GenUI.ListSpacing);
            List<ThingDef> stuffs = GenStuff.AllowedStuffsFor(_thing.def).ToList();
            if (!stuffs.Any())
            {
                Rect rect = outRect.TopPart(0.6f);
                Text.Font = GameFont.Medium;
                Widgets.NoneLabelCenteredVertically(rect, UIText.NoMaterial.Translate());
                Text.Font = GameFont.Small;
            }
            stuffs.OrderBy(t => t.defName);
            for (int i = 0; i < stuffs.Count; ++i)
            {
                Texture2D texture2D = i % 2 == 0 ? TexUI.TextBGBlack : TexUI.GrayTextBG;
                GUI.DrawTexture(row, texture2D);

                ThingDef stuff = stuffs[i];
                DrawUtility.DrawLineButton
                    (row
                    , stuff.LabelAsStuff.CapitalizeFirst()
                    , _thing
                    , (thing) =>
                    {
                        _loadout.UpdateItem(thing, stuff);
                        _pair.stuff = stuff;
                        _thing = _loadout[_pair].Thing;
                    });

                if (stuff == _pair.stuff)
                {
                    Widgets.DrawHighlightSelected(row);
                }
                if (Mouse.IsOver(row))
                {
                    _stuffPreview = stuff;
                    Widgets.DrawHighlight(row);
                }
                row.y = row.yMax;
            }

            if (Event.current.type == EventType.Layout)
            {
                _scrollViewHeight = row.yMax;
                rollingY = row.yMax;
            }
            Widgets.EndScrollView();
        }

        private void DrawStats(Rect rect, Thing thing, ref float rollingY)
        {
            List<TSPWQuality> pairList = new List<TSPWQuality>();
            WidgetRow row = new WidgetRow(rect.x, rect.y, UIDirection.RightThenDown, rect.width);
            float lineHeaderWidth = rect.width * 1.8f / 5;
            float numColumnWidth = rect.width * 1.5f / 5;

            pairList.Add(_pair);
            if (thing.def.stuffCategories != null)
            {
                // If one ever wants to add more preview columns, start here.
                pairList.Add(new TSPWQuality(_pair.thing, _stuffPreview, _pair.Quality));
            }

            // Draw header
            row.Label(string.Empty, lineHeaderWidth);
            if (thing.def.MadeFromStuff)
            {
                Text.Anchor = TextAnchor.MiddleCenter;
                Text.WordWrap = false;
                foreach (TSPWQuality pair in pairList)
                {
                    if (row.ButtonIcon(TexResource.Info))
                    {
                        Thing temp = _thing.DeepCopySimple(false);
                        temp.SetStuffDirect(pair.stuff);
                        temp.HitPoints = temp.MaxHitPoints;
                        Find.WindowStack.Add(new Dialog_InfoCard(temp));
                    }
                    row.Label(pair.stuff.LabelAsStuff.CapitalizeFirst(), numColumnWidth - WidgetRow.IconSize - WidgetRow.DefaultGap);
                }
                Text.Anchor = TextAnchor.UpperLeft;
                Text.WordWrap = true;
            }
            else
            {
                row.Label(string.Empty, numColumnWidth * 2);
            }

            // Draw stats
            if (thing.def.IsApparel)
            {
                DrawStatRows(ArmorStats, pairList, row, lineHeaderWidth, numColumnWidth);
            }
            else if (thing.def.IsMeleeWeapon)
            {
                DrawStatRows(BaseWeaponStats, pairList, row, lineHeaderWidth, numColumnWidth);
                DrawStatRows(MeleeWeaponStats, pairList, row, lineHeaderWidth, numColumnWidth);
            }
            else if (thing.def.IsRangedWeapon)
            {
                DrawStatRows(BaseWeaponStats, pairList, row, lineHeaderWidth, numColumnWidth);
                DrawRangedStatRows(pairList, row, lineHeaderWidth, numColumnWidth);
            }
            Text.Anchor = TextAnchor.UpperLeft;

            rollingY = row.FinalY + WidgetRow.IconSize;
        }

        [SuppressMessage("Performance", "CA1806:Do not ignore method results", Justification = "<Pending>")]
        private void DrawRangedStatRows(List<TSPWQuality> pairList, WidgetRow row, float lineHeaderWidth, float numColumnWidth)
        {
            List<List<StatDrawInfo>> listHolder = new List<List<StatDrawInfo>>();
            List<List<StatDrawInfo>> transposedLists = new List<List<StatDrawInfo>>();

            // listHolder[0]  listHolder[1]
            // | stuff1 |     | stuff2 |
            // | Damage |     | Damage |
            // | AP     |     | AP     |

            //                       stuff1   stuff2
            // transposedLists[0]    Damage   Damage
            // transposedLists[1]    AP       AP

            List<StatDef> rangedStats = RangedWeaponStats.Select(r => ToStatDef(r)).ToList();

            foreach (TSPWQuality pair in pairList)
            {
                List<StatDrawInfo> infoList = new List<StatDrawInfo>();

                _thing.SetStuffDirect(pair.stuff);
                infoList.AddRange(_thing.def.SpecialDisplayStats(StatRequest.For(_thing))
                                .Where(r => rangedStats.Any(s => s.LabelCap == r.LabelCap))
                                .Select(r => (StatDrawInfo)r)
                                .ToList());
                listHolder.Add(infoList);
            }
            _thing.SetStuffDirect(_pair.stuff);


            // Transpose lists
            for (int i = 0; i < rangedStats.Count; i++)
            {
                List<StatDrawInfo> newList = new List<StatDrawInfo>();
                foreach (List<StatDrawInfo> list in listHolder)
                {
                    newList.Add(list.Find(s => s.LabelCap == rangedStats[i].LabelCap));
                }
                List<StatDrawInfo> orderedList = newList.OrderByDescending(t => t.Value).ToList();
                if (orderedList.Count > 1 && orderedList[0].Value != orderedList[1].Value)
                {
                    orderedList[0].Color = Color.green;
                }
                transposedLists.Add(newList);
            }
#if DEBUG
            Log.Message("listHolder count: " + listHolder.Count);
            foreach (var entry in _thing.def.SpecialDisplayStats(StatRequest.For(_thing)))
            {
                Log.Message("Stats: " + entry.LabelCap);
            }
            Log.Message("rangedStats Count: " + rangedStats.Count);
            Log.Message("transposedLists Count: " + transposedLists.Count);
            if (rangedStats.Count != transposedLists.Count)
            {
                return;
            }
#endif
            DrawStatRows(rangedStats, null, row, lineHeaderWidth, numColumnWidth, transposedLists, true);
        }

        private void DrawStatRows(List<StatDef> stats, List<TSPWQuality> pairs, WidgetRow row
                                 , float lineHeaderWidth, float numColumnWidth, List<List<StatDrawInfo>> listHolder = null, bool specialStats = false)
        {
            for (int i = 0; i < stats.Count; i++)
            {
                List<StatDrawInfo> statInfoList = new List<StatDrawInfo>();
                if (!specialStats)
                {
                    statInfoList = new List<StatDrawInfo>();

                    foreach (TSPWQuality pair in pairs)
                    {
                        StatDrawInfo drawInfo = new StatDrawInfo();
                        if (Event.current.type == EventType.Repaint)
                        {
                            if (pair.stuff != RPGI_StuffDefOf.RPGIGenericResource)
                            {
                                _thing.SetStuffDirect(pair.stuff);
                                drawInfo.StatRequest = StatRequest.For(_thing);
                                drawInfo.Value = stats[i].Worker.GetValue(drawInfo.StatRequest);
                                drawInfo.Tip = stats[i].Worker.GetExplanationFull(drawInfo.StatRequest, stats[i].toStringNumberSense, drawInfo.Value);
                            }
                        }
                        statInfoList.Add(drawInfo);
                    }
                    _thing.SetStuffDirect(_pair.stuff);
                    List<StatDrawInfo> orderedList = statInfoList.OrderByDescending(t => t.Value).ToList();
                    if (orderedList.Count > 1 && orderedList[0].Value != orderedList[1].Value)
                    {
                        orderedList[0].Color = Color.green;
                    }
                }
                else
                {
                    statInfoList = listHolder[i];
                }

                Text.Anchor = TextAnchor.MiddleCenter;
                row.Label(stats[i].LabelCap, lineHeaderWidth);

                foreach (StatDrawInfo info in statInfoList)
                {
                    GUI.color = info.Color;
                    info.DrawRect = row.Label(specialStats ? info.ValueString : info.Value != -1
                                        ? stats[i].Worker.ValueToString(info.Value, true, stats[i].toStringNumberSense)
                                        : "-"
                                        , numColumnWidth);
                    Widgets.DrawHighlightIfMouseover(info.DrawRect);
                    Text.Anchor = TextAnchor.MiddleLeft;
                    TooltipHandler.TipRegion(info.DrawRect, info.Tip);
                    Text.Anchor = TextAnchor.MiddleCenter;
                }
                Text.Anchor = TextAnchor.UpperLeft;
                GUI.color = Color.white;
            }
        }

        #endregion private methods

        private class StatDrawInfo
        {
            public Color Color = Color.white;
            public float Value = -1;
            public string ValueString;
            public StatRequest StatRequest = default;
            public string Tip = string.Empty;
            public Rect DrawRect;
            public string LabelCap;

            /// <summary>
            /// Adapter for transforming type StatDrawEntry to StatDrawInfo
            /// </summary>
            /// <param name="entry"></param>
            public static explicit operator StatDrawInfo(StatDrawEntry entry)
            {
                StatDrawInfo result = new StatDrawInfo
                {
                    ValueString = entry.ValueString,
                    LabelCap = entry.LabelCap
                };
                _ = float.TryParse(_regex.Match(entry.ValueString).Value, out result.Value);
                result.Tip = entry.GetExplanationText(StatRequest.ForEmpty());
                return result;
            }
        }

        /// <summary>
        /// Only used to pass the for loop in DrawStatRows
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        private static StatDef ToStatDef(string str)
        {
            return new StatDef() { label = str };
        }
    }
}
