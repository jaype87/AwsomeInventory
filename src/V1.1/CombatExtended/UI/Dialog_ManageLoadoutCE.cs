// <copyright file="Dialog_ManageLoadoutCE.cs" company="Zizhen Li">
// Copyright (c) 2019 - 2020 Zizhen Li. All rights reserved.
// Licensed under the LGPL-3.0-only license. See LICENSE.md file in the project root for full license information.
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AwesomeInventory.Loadout;
using CombatExtended;
using RimWorld;
using UnityEngine;
using Verse;

namespace AwesomeInventory.UI
{
    /// <inheritdoc/>
    public class Dialog_ManageLoadoutCE : Dialog_ManageLoadouts
    {
        private bool _drawWeight = true;

        /// <summary>
        /// Initializes a new instance of the <see cref="Dialog_ManageLoadoutCE"/> class.
        /// </summary>
        /// <param name="loadout"> Loadout to display. </param>
        /// <param name="pawn"> Pawn who wears the <paramref name="loadout"/>. </param>
        public Dialog_ManageLoadoutCE(AwesomeInventoryLoadout loadout, Pawn pawn)
            : base(loadout, pawn)
        {
        }

        /// <summary>
        /// Returns a rect for drawing weight bar and bulk bar.
        /// </summary>
        /// <param name="canvas"> Canvas at which bottom two bars will be drawn. </param>
        /// <returns> A rect where two bars are drawn. </returns>
        protected override Rect GetWeightRect(Rect canvas)
        {
            canvas.Set(canvas.x, canvas.yMax - DrawUtility.CurrentPadding - GenUI.ListSpacing * 2, canvas.width, GenUI.ListSpacing * 2);
            return canvas;
        }

        /// <summary>
        /// Draw weight bar and bulk bar.
        /// </summary>
        /// <param name="canvas"> Rect for drawing weight bar and bulk bar. </param>
        protected override void DrawWeightBar(Rect canvas)
        {
            base.DrawWeightBar(canvas.ReplaceHeight(GenUI.SmallIconSize));

            float currentBulk = _currentLoadout.GroupBy(
                groupSelector => groupSelector.AllowedThing
                , groupSelector => groupSelector.AllowedStackCount)
                .Sum(
                    group => group.Key.IsApparel
                                     ? BulkUtility.BulkFor(group.Key, null, true)
                                       + BulkUtility.BulkFor(group.Key, null, false) * (group.Sum() - 1)
                                     : BulkUtility.BulkFor(group.Key, null, false) * group.Sum());
            float capaticyBulk = MassBulkUtility.BaseCarryBulk(_pawn)
                                 +
                                 _currentLoadout.Select(selector => selector.AllowedThing).Distinct().Sum(
                                     thingDef => thingDef.equippedStatOffsets.GetStatOffsetFromList(CE_StatDefOf.CarryBulk));

            float fillPercent = Mathf.Clamp01(currentBulk / capaticyBulk);

            canvas.Set(canvas.x, canvas.y + GenUI.ListSpacing + GenUI.GapTiny, canvas.width, GenUI.SmallIconSize);
            GenBar.BarWithOverlay(
                canvas
                , fillPercent
                , SolidColorMaterials.NewSolidColorTexture(Color.Lerp(AwesomeInventoryTex.RWPrimaryColor, AwesomeInventoryTex.Valvet, fillPercent))
                , UIText.Bulk.TranslateSimple()
                , currentBulk.ToString("0.#") + "/" + capaticyBulk.ToString("0.#")
                , string.Empty);
        }

        /// <inheritdoc/>
        protected override void DrawItemRow(Rect row, int index, IList<ThingGroupSelector> groupSelectors, int reorderableGroup, bool drawShadow = false)
        {
            ValidateArg.NotNull(row, nameof(row));
            ValidateArg.NotNull(groupSelectors, nameof(groupSelectors));

            /* Label (fill) | Weight | Gear Icon | Count Field | Delete Icon */

            WidgetRow widgetRow = new WidgetRow(row.width, row.y, UIDirection.LeftThenDown, row.width);
            ThingGroupSelector groupSelector = groupSelectors[index];

            // Draw delete icon.
            this.DrawDeleteIconInThingRow(widgetRow, groupSelectors, groupSelector);

            Text.Anchor = TextAnchor.MiddleLeft;

            // Draw count field.
            if (WhiteBlacklistView.IsWishlist)
            {
                this.DrawCountFieldInThingRow(
                new Rect(widgetRow.FinalX - WidgetRow.IconSize * 2 - WidgetRow.DefaultGap, widgetRow.FinalY, WidgetRow.IconSize * 2, GenUI.ListSpacing),
                groupSelector);
                widgetRow.GapButtonIcon();
                widgetRow.GapButtonIcon();
            }

            // Draw threshold.
            if (widgetRow.ButtonIcon(TexResource.Threshold, UIText.StockMode.TranslateSimple()))
            {
                Find.WindowStack.Add(new Dialog_RestockTrigger(groupSelector));
            }

            // Draw gear icon.
            this.DrawGearIconInThingRow(widgetRow, groupSelector);

            // Draw ammo if thing is ammo user.
            this.DrawAmmoSelection(widgetRow, groupSelector.AllowedThing);

            Text.WordWrap = false;

            // Draw weight or bulk.
            if (_drawWeight)
                widgetRow.Label(groupSelector.Weight.ToStringMass());
            else
                widgetRow.Label(groupSelector.AllowedThing.GetStatValueAbstract(CE_StatDefOf.Bulk).ToString() + " b");

            // Draw label.
            Rect labelRect = widgetRow.Label(
                drawShadow
                    ? groupSelector.LabelCapNoCount.StripTags().Colorize(Theme.MilkySlicky.ForeGround)
                    : groupSelector.LabelCapNoCount
                , widgetRow.FinalX);

            Text.WordWrap = true;
            Text.Anchor = TextAnchor.UpperLeft;

            if (!drawShadow)
            {
                ReorderableWidget.Reorderable(reorderableGroup, labelRect);

                // Tooltips && Highlights
                Widgets.DrawHighlightIfMouseover(row);
                if (Event.current.type == EventType.MouseDown)
                {
                    TooltipHandler.ClearTooltipsFrom(labelRect);
                }
                else
                {
                    TooltipHandler.TipRegion(labelRect, UIText.DragToReorder.Translate());
                }
            }
        }

        /// <summary>
        /// Draw ammo selection for <paramref name="thingDef"/> if it is an ammo user.
        /// </summary>
        /// <param name="widgetRow"> Drawing helper. </param>
        /// <param name="thingDef"> Ammo user. </param>
        protected virtual void DrawAmmoSelection(WidgetRow widgetRow, ThingDef thingDef)
        {
            ValidateArg.NotNull(widgetRow, nameof(widgetRow));
            ValidateArg.NotNull(thingDef, nameof(thingDef));

            if (thingDef.HasComp(typeof(CompAmmoUser)) && widgetRow.ButtonIcon(ImageResource.IconAmmo))
            {
                CompProperties_AmmoUser ammoUser = thingDef.GetCompProperties<CompProperties_AmmoUser>();

                GenericAmmo genericAmmo = this.CreateGenericAmmoDef(thingDef);

                List<FloatMenuOption> options = new List<FloatMenuOption>();
                foreach (ThingDef ammoDef in ammoUser.ammoSet?.ammoTypes.Select(al => al.ammo as ThingDef).Concat(genericAmmo as ThingDef))
                {
                    options.Add(
                        new FloatMenuOption(
                            ammoDef.LabelCap
                            , () =>
                            {
                                ThingGroupSelector groupSelector = new ThingGroupSelector(ammoDef);
                                ThingSelector thingSelector;
                                thingSelector = AwesomeInventoryServiceProvider.MakeInstanceOf<SingleThingSelector>(ammoDef, null);

                                groupSelector.SetStackCount(ammoUser.magazineSize);
                                groupSelector.Add(thingSelector);

                                if (WhiteBlacklistView.IsWishlist)
                                    _currentLoadout.Add(groupSelector);
                                else
                                    _currentLoadout.AddToBlacklist(groupSelector);

                                Find.WindowStack.Add(new Dialog_AddAmmoPerMagSize(groupSelector, thingDef, ammoUser.magazineSize));
                            }));
                }

                Find.WindowStack.Add(new FloatMenu(options));
            }
        }

        /// <inheritdoc/>
        protected override void DrawWishBlackListOptions(Rect canvas)
        {
            Rect leftRect = canvas.ReplaceWidth(canvas.width - UIText.TenCharsString.GetWidthCached());
            base.DrawWishBlackListOptions(leftRect);

            Rect righRect = new Rect(leftRect.xMax, leftRect.y, UIText.TenCharsString.GetWidthCached(), canvas.height);
            if (_drawWeight && Widgets.ButtonText(righRect, UIText.Weight.TranslateSimple()))
            {
                _drawWeight = false;
            }
            else if (!_drawWeight && Widgets.ButtonText(righRect, UIText.Bulk.TranslateSimple()))
            {
                _drawWeight = true;
            }
        }

        private GenericAmmo CreateGenericAmmoDef(ThingDef gun)
        {
            return new GenericAmmo(
                CEStrings.GenericAmmoPrefix + gun.defName
                , string.Format(
                    System.Globalization.CultureInfo.InvariantCulture
                    , CEStrings.AmmoDescription
                    , gun.LabelCap)
                , string.Format(
                    System.Globalization.CultureInfo.InvariantCulture
                    , CEStrings.AmmoLabel.TranslateSimple()
                    , gun.LabelCap)
                , typeof(AmmoThing)
                , gun.GetCompProperties<CompProperties_AmmoUser>().ammoSet.ammoTypes.Select(l => l.ammo));
        }
    }
}
