// <copyright file="StatButtonRect.cs" company="Zizhen Li">
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
    /// <summary>
    /// The "Add Stat" button on stat panel.
    /// </summary>
    public class StatButtonRect : RectBase<StatPanelModel>
    {
        private static readonly string _label = UIText.AddRemoveStat.TranslateSimple();

        #region Overrides of RectBase<StatPanelModel>

        /// <inheritdoc />
        public override Rect Draw(StatPanelModel model, Vector2 position)
        {
            Rect rect = new Rect(position + this.Offset, this.Size);
            if (Widgets.ButtonText(rect, _label))
            {
                Find.WindowStack.Add(new Dialog_StatChoice());
            }

            return rect;
        }

        /// <inheritdoc />
        public override Vector2 Build(StatPanelModel model)
        {
            return this.Size = Text.CalcSize(_label) + new Vector2(GenUI.Gap, GenUI.GapTiny);
        }

        /// <inheritdoc />
        public override void Update(StatPanelModel model)
        {
        }

        #endregion
    }
}
