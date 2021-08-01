// <copyright file="StatPanelTitleRect.cs" company="Zizhen Li">
// Copyright (c) 2019 - 2020 Zizhen Li. All rights reserved.
// Licensed under the LGPL-3.0-only license. See LICENSE.md file in the project root for full license information.
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using RimWorldUtility.UI;
using UnityEngine;
using Verse;

namespace AwesomeInventory.UI
{
    public class StatPanelTitleRect : RectBase<StatPanelModel>
    {
        #region Overrides of TextRect

        /// <inheritdoc />
        public override Rect Draw(StatPanelModel model, Vector2 position)
        {
            ValidateArg.NotNull(model, nameof(model));

            GameFont oldFont = Verse.Text.Font;
            Color oldColor = GUI.color;

            Verse.Text.Font = GameFont.Medium;
            GUI.color = Color.grey;

            Rect rect = new Rect(position + this.Offset, this.Size);

            Widgets.Label(rect, model.Title);
            Widgets.DrawLineHorizontal(rect.x, rect.yMax, rect.width);

            Verse.Text.Font = oldFont;
            GUI.color = oldColor;

            return rect;
        }

        /// <inheritdoc />
        public override Vector2 Build(StatPanelModel model)
        {
            ValidateArg.NotNull(model, nameof(model));

            return this.Size = new Vector2(model.Size.x, GenUI.ListSpacing);
        }

        /// <inheritdoc />
        public override void Update(StatPanelModel model)
        {
            this.Build(model);
        }

        #endregion
    }
}
