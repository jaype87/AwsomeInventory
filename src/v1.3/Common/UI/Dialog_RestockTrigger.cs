// <copyright file="Dialog_RestockTrigger.cs" company="Zizhen Li">
// Copyright (c) 2019 - 2020 Zizhen Li. All rights reserved.
// Licensed under the LGPL-3.0-only license. See LICENSE.md file in the project root for full license information.
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AwesomeInventory.Loadout;
using RimWorld;
using UnityEngine;
using Verse;

namespace AwesomeInventory.UI
{
    /// <summary>
    /// Used for players to set trigger when AI should restock items in loadout.
    /// </summary>
    public class Dialog_RestockTrigger : Window
    {
        private bool _checked;
        private ThingGroupSelector _groupSelector;

        /// <summary>
        /// Initializes a new instance of the <see cref="Dialog_RestockTrigger"/> class.
        /// </summary>
        /// <param name="groupSelector"> Set restock trigger for this selector. </param>
        public Dialog_RestockTrigger(ThingGroupSelector groupSelector)
        {
            ValidateArg.NotNull(groupSelector, nameof(groupSelector));

            _groupSelector = groupSelector;
            _checked = _groupSelector.UseBottomThreshold;

            this.doCloseX = true;
            this.absorbInputAroundWindow = true;
            this.closeOnClickedOutside = true;
            this.draggable = true;
        }

        /// <summary>
        /// Gets initial size of the window.
        /// </summary>
        public override Vector2 InitialSize => new Vector2(450, 135);

        /// <summary>
        /// Draw content in window.
        /// </summary>
        /// <param name="inRect"> Rect for drawing. </param>
        public override void DoWindowContents(Rect inRect)
        {
            Text.Anchor = TextAnchor.MiddleLeft;

            _checked = !_checked;
            Rect centeredRect = new Rect(0, 0, 0, GenUI.ListSpacing * 3).CenteredOnYIn(inRect);
            Rect firstRow = new Rect(0, centeredRect.y, inRect.width - DrawUtility.CurrentPadding, GenUI.ListSpacing);
            Widgets.Checkbox(new Rect(0, 0, GenUI.SmallIconSize, GenUI.SmallIconSize).CenteredOnYIn(firstRow).position, ref _checked);
            Widgets.Label(firstRow.ReplacexMin(GenUI.ListSpacing), UIText.RestockAlways.TranslateSimple());
            if (_checked)
                _groupSelector.SetBottomThreshold(false, 0);

            _checked = !_checked;
            Rect secondRow = firstRow.ReplaceY(firstRow.yMax);
            Widgets.Checkbox(new Rect(0, 0, GenUI.SmallIconSize, GenUI.SmallIconSize).CenteredOnYIn(secondRow).position, ref _checked);
            Rect labelRect = new Rect(GenUI.ListSpacing, secondRow.y, UIText.RestockBottomThreshold.TranslateSimple().GetWidthCached(), GenUI.ListSpacing);
            Widgets.Label(labelRect, UIText.RestockBottomThreshold.TranslateSimple());
            if (_checked)
            {
                int num = _groupSelector.BottomThresoldCount;
                string buffer = num.ToString();
                Widgets.TextFieldNumeric(
                    new Rect(labelRect.xMax + GenUI.GapTiny, 0, GenUI.SmallIconSize * 2, GenUI.ListSpacing)
                        .CenteredOnYIn(secondRow)
                    , ref num
                    , ref buffer
                    , 0
                    , _groupSelector.AllowedStackCount);
                _groupSelector.SetBottomThreshold(true, num);

                Rect thirdRow = secondRow.ReplaceY(secondRow.yMax);
                GUI.color = Color.grey;
                Widgets.Label(new Rect(0, thirdRow.y, UIText.CantExceedStackCount.TranslateSimple().GetWidthCached(), thirdRow.height).CenteredOnXIn(thirdRow), UIText.CantExceedStackCount.TranslateSimple());
                GUI.color = Color.white;
            }
            else
            {
                GUI.color = Color.grey;
                Widgets.Label(new Rect(labelRect.xMax + GenUI.GapTiny, labelRect.y, secondRow.width - labelRect.xMax, GenUI.ListSpacing), "0");
                GUI.color = Color.white;
            }

            Text.Anchor = TextAnchor.UpperLeft;
        }
    }
}
