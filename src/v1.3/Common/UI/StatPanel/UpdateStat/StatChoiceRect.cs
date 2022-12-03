// <copyright file="StatChoiceRect.cs" company="Zizhen Li">
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
    public class StatChoiceRect : RectBase<StatChoiceModel>
    {
        private TitleRect _titleRect = new TitleRect(GameFont.Medium);

        private TextSearchRect _textSearchRect = new TextSearchRect();

        private ListRect<IEnumerable<StatEntryModel>, StatEntryModel, StatEntryRect> _statList =
            new ListRect<IEnumerable<StatEntryModel>, StatEntryModel, StatEntryRect>(DrawDirection.Right);

        private Vector2 _scrollPos = Vector2.zero;

        #region Overrides of RectBase<StatChoiceListModel>

        /// <inheritdoc />
        public override Rect Draw(StatChoiceModel model, Vector2 position)
        {
            ValidateArg.NotNull(model, nameof(model));

            _titleRect.Draw((model.Title, model.Size.x), position);

            _textSearchRect.Draw(
                (model.SearchString, model.Size.x / 2)
                , position
                , out (string SearchString, float Width) searchModel);

            if (searchModel.SearchString != model.SearchString)
            {
                model.SearchString = searchModel.SearchString;
                this.Update(model);
            }

            Widgets.BeginScrollView(
                new Rect(_statList.Offset + position, model.Size - new Vector2(0, _statList.Offset.y))
                , ref _scrollPos
                , new Rect(_statList.Offset + position, _statList.Size));

            _statList.Draw(model, position);

            Widgets.EndScrollView();

            return new Rect(position + this.Offset, this.Size);
        }

        /// <inheritdoc />
        public override Vector2 Build(StatChoiceModel model)
        {
            ValidateArg.NotNull(model, nameof(model));

            Vector2 size1 = _titleRect.Build((model.Title, model.Size.x));
            Vector2 size2 = _textSearchRect.Build((model.SearchString, model.Size.x / 2));
            Vector2 size3 = _statList.Build(model, new Vector2(model.Size.x - GenUI.ScrollBarWidth, 0));

            _titleRect.Offset = Vector2.zero;
            _textSearchRect.Offset = new Vector2(0, size1.y + GenUI.GapTiny);
            _statList.Offset = new Vector2(0, _textSearchRect.Offset.y + size2.y + GenUI.GapSmall);

            model.Changed = false;
            return this.Size = model.Size;
        }

        /// <inheritdoc />
        public override void Update(StatChoiceModel model)
        {
            ValidateArg.NotNull(model, nameof(model));

            if (!model.Changed)
                return;

            _titleRect.Update((model.Title, model.Size.x));
            _textSearchRect.Update((model.SearchString, model.Size.x));
            _statList.Update(model.StatChoices);

            this.Build(model);
        }

        #endregion
    }
}
