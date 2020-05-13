// <copyright file="Dialog_InventoryOverview.cs" company="Zizhen Li">
// Copyright (c) 2019 - 2020 Zizhen Li. All rights reserved.
// Licensed under the LGPL-3.0-only license. See LICENSE.md file in the project root for full license information.
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace AwesomeInventory.UI
{
    /// <summary>
    /// A dialog window of overview on inventory.
    /// </summary>
    public class Dialog_InventoryOverview : Window
    {
        /// <summary>
        /// Gets the initial size for the dialog window.
        /// </summary>
        public override Vector2 InitialSize { get; } = new Vector2(GenUI.GetWidthCached(UIText.TenCharsString.Times(11)), Verse.UI.screenHeight / 2f);

        /// <summary>
        /// Draw contents in <paramref name="inRect"/>.
        /// </summary>
        /// <param name="inRect"> Rect for drawing. </param>
        public override void DoWindowContents(Rect inRect)
        {
        }

        public void DoTabButton(Rect rect)
        {
            WidgetRow widgetRow = new WidgetRow(rect.x, rect.y, UIDirection.RightThenDown);
        }

        public void DoTabs(Rect rect)
        {

        }
    }
}
