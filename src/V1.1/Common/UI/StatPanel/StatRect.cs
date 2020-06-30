// <copyright file="StatRect.cs" company="Zizhen Li">
// Copyright (c) 2019 - 2020 Zizhen Li. All rights reserved.
// Licensed under the LGPL-3.0-only license. See LICENSE.md file in the project root for full license information.
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorldUtility.UI;
using UnityEngine;
using Verse;

namespace AwesomeInventory.UI
{
    public class StatRect : LabelValueRect<StatCacheKey>
    {
        protected float _defaultItemHeight = GenUI.ListSpacing;

        public Texture2D BgTexture = TexUI.TextBGBlack;

        public override Vector2 Build(StatCacheKey model)
        {
            this.GetString(model);
            return this.Size = new Vector2(Text.CalcSize(_print).x + GenUI.GapSmall, _defaultItemHeight);
        }

        public override void Update(StatCacheKey model)
        {
            this.GetString(model);
        }

        public override Rect Draw(StatCacheKey model, Vector2 position)
        {
            return DrawUtility.TextPosition(
                    TextAnchor.MiddleLeft
                    , () =>
                    {
                        Rect rect = new Rect(position + this.Offset, this.Size);
                        GUI.DrawTexture(rect, this.BgTexture);

                        Text.WordWrap = false;
                        base.Draw(model, position);
                        Text.WordWrap = true;

                        Widgets.DrawHighlightIfMouseover(rect);
                        TooltipHandler.TipRegion(rect, model.StatDef.description);
                        return rect;
                    });
        }

        private void GetString(StatCacheKey model)
        {
            _label = model.StatDef.LabelCap;
            _value = model.StatDef.ValueToString(StatPanelManager.StatCache[model]);
        }
    }
}
