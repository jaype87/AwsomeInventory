// <copyright file="StatEntryRect.cs" company="Zizhen Li">
// Copyright (c) 2019 - 2020 Zizhen Li. All rights reserved.
// Licensed under the LGPL-3.0-only license. See LICENSE.md file in the project root for full license information.
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;
using RimWorldUtility.UI;
using UnityEngine;
using Verse;

namespace AwesomeInventory.UI
{
    public class StatEntryRect : RectBase<StatEntryModel>
    {
        private static readonly Vector2 _iconSize = new Vector2(GenUI.SmallIconSize, GenUI.SmallIconSize);

        private static bool _toggleBg = false;

        private StatEntryModel _lastModel = new StatEntryModel();

        private float _entryGap = GenUI.Gap;

        private float _lineGap = GenUI.GapSmall;

        private float _padding = GenUI.GapSmall / 2;

        private Texture2D _bgTexture2D = null;

        private Vector2 _drawSize => new Vector2(this.Size.x - _entryGap, this.Size.y - _lineGap);

        #region Overrides of RectBase<StatEntryModel>

        /// <inheritdoc />
        public override Rect Draw(StatEntryModel model, Vector2 position)
        {
            ValidateArg.NotNull(model, nameof(model));

            Rect rect = new Rect(position + this.Offset, _drawSize);

            if (_bgTexture2D is Texture2D texture2D)
                GUI.DrawTexture(rect, texture2D);

            Text.WordWrap = false;
            TextAnchor oldAnchor = Text.Anchor;

            Text.Anchor = TextAnchor.MiddleLeft;

            Rect labelRect = this.CenterWithPadding(model.Selected ? rect.ReplacexMax(rect.xMax - GenUI.SmallIconSize) : rect);
            DrawUtility.DrawLabelButton(labelRect, model.StatDef.LabelCap, () => StatPanelManager.SelectedDefs.Add(model.StatDef), false, false);
            Widgets.DrawHighlightIfMouseover(rect);
            TooltipHandler.TipRegion(rect, model.StatDef.description);

            Text.WordWrap = true;
            Text.Anchor = oldAnchor;

            if (!model.Selected)
            {
                return rect;
            }

            Rect buttonRect = new Rect(new Vector2(rect.xMax - GenUI.SmallIconSize, rect.y), _iconSize)
                    .ContractedBy(GenUI.GapTiny)
                    .CenteredOnYIn(rect);

            if (Widgets.ButtonImage(buttonRect, Verse.TexButton.CloseXSmall))
            {
                StatPanelManager.SelectedDefs.Remove(model.StatDef);
            }

            return rect;
        }

        /// <inheritdoc />
        public override Vector2 Build(StatEntryModel model)
        {
            ValidateArg.NotNull(model, nameof(model));

            if (!this.NeedBuild)
                return this.Size;

            Vector2 size = new Vector2(Text.CalcSize(model.StatDef.LabelCap).x + _padding * 2, GenUI.ListSpacing);

            if (model.Selected)
                size.x += _iconSize.x;

            size.x += _entryGap;
            size.y += _lineGap;

            this.AssignBgTexture();
            this.NeedBuild = false;

            return this.Size = size;
        }

        /// <inheritdoc />
        public override void Update(StatEntryModel model)
        {
            ValidateArg.NotNull(model, nameof(model));

            if (model == _lastModel)
                return;

            if (model.StatDef == _lastModel.StatDef && model.Selected == _lastModel.Selected)
                return;

            _lastModel = new StatEntryModel(model);
            this.NeedBuild = true;
        }

        #endregion

        private Rect CenterWithPadding(Rect rect)
        {
            return new Rect(rect.x + _padding, rect.y, rect.width - _padding, rect.height);
        }

        private void AssignBgTexture()
        {
            _bgTexture2D = (_toggleBg ^= true) ? TexUI.HighlightTex : null;
        }
    }
}
