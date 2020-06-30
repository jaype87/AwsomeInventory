// <copyright file="StatPanelRect.cs" company="Zizhen Li">
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
    public class StatPanelRect : RectBase<StatPanelModel>
    {
        private static Vector2 _listScrollPos = Vector2.zero;

        private readonly StatPanelTitleRect _title = new StatPanelTitleRect();

        private readonly StatListRect _statList = new StatListRect();

        private readonly StatButtonRect _statButton = new StatButtonRect();

        #region Overrides of RectBase<StatPanelModel>

        /// <inheritdoc />
        public override Rect Draw(StatPanelModel model, Vector2 position)
        {
            ValidateArg.NotNull(model, nameof(model));

            _title.Draw(model, position);

            Rect outRect = new Rect(_statList.Offset + position, new Vector2(this.Size.x, _statList.Size.y + GenUI.ScrollBarWidth));
            Rect viewRect = new Rect(_statList.Offset + position, _statList.Size);

            Widgets.ScrollHorizontal(outRect, ref _listScrollPos, viewRect);
            Widgets.BeginScrollView(outRect, ref _listScrollPos, viewRect);

            _statList.Draw(model.StatCacheKeys.Keys, position);
            Widgets.EndScrollView();

            _statButton.Draw(model, position);

            return new Rect(position, this.Size);
        }

        /// <inheritdoc />
        public override Vector2 Build(StatPanelModel model)
        {
            ValidateArg.NotNull(model, nameof(model));

            Vector2 size1 = _title.Build(model);
            Vector2 size3 = _statButton.Build(model);
            Vector2 size2 = _statList.Build(model.StatCacheKeys.Keys, new Vector2(0, model.Size.y - size1.y - size3.y - GenUI.ListSpacing));

            _title.Offset = Vector2.zero;
            _statList.Offset = new Vector2(0, size1.y + GenUI.GapTiny);
            _statButton.Offset = new Vector2(0, _statList.Offset.y + size2.y + GenUI.ListSpacing);

            this.Size = model.Size;

            return this.Size;
        }

        /// <inheritdoc />
        public override void Update(StatPanelModel model)
        {
            ValidateArg.NotNull(model, nameof(model));

            if (!model.Changed)
                return;

            _title.Update(model);
            _statList.Update(model.StatCacheKeys.Keys);
            _statButton.Update(model);
            this.Build(model);
            model.Used = true;
        }

        #endregion
    }
}
