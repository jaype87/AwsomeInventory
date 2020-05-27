// <copyright file="Dialog_InventoryOverview.cs" company="Zizhen Li">
// Copyright (c) 2019 - 2020 Zizhen Li. All rights reserved.
// Licensed under the LGPL-3.0-only license. See LICENSE.md file in the project root for full license information.
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using UnityEngine;
using Verse;

namespace AwesomeInventory.UI
{
    /// <summary>
    /// A dialog window of overview on inventory.
    /// </summary>
    public class Dialog_InventoryOverview : Window
    {
        private static readonly float _windowWidth = UIText.TenCharsString.Times(12).GetWidthCached();

        private static readonly Vector2 _initialSize = new Vector2(_windowWidth, Verse.UI.screenHeight / 2f);

        private static List<OverviewTab> _tabs = new List<OverviewTab>();

        private static ContainerState _containerState = new ContainerState();

        private static TipDisplayer _tipDisplayer = new TipDisplayer(
            new List<string>()
            {
                UIText.LoadoutTabTip1.TranslateSimple(),
                UIText.LoadoutTabTip2.TranslateSimple(),
            });

        private OverviewTab _activeTab = _tabs.First();

        private float _minWindowWidth = 70f;

        private Rect _windowRect;

        static Dialog_InventoryOverview()
        {
            _tabs.Add(new LoadoutTab(_containerState));
            _tabs.Add(new InventoryTab(_containerState));
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Dialog_InventoryOverview"/> class.
        /// </summary>
        public Dialog_InventoryOverview()
        {
            this.doCloseX = true;
            this.forcePause = true;
            this.absorbInputAroundWindow = true;
            this.closeOnClickedOutside = true;
            this.resizeable = true;
            this.draggable = true;
        }

        /// <summary>
        /// Gets the initial size for the dialog window.
        /// </summary>
        public override Vector2 InitialSize { get; } = _initialSize;

        /// <summary>
        /// Called once before the window is opened.
        /// </summary>
        public override void PreOpen()
        {
            base.PreOpen();
            _tabs.ForEach(t => t.PreOpen());
        }

        /// <summary>
        /// Draw contents in <paramref name="inRect"/>.
        /// </summary>
        /// <param name="inRect"> Rect for drawing. </param>
        public override void DoWindowContents(Rect inRect)
        {
            if (_containerState.Minimizing)
            {
                this.Minimized();
                _containerState.Minimized = true;
                _containerState.Minimizing = false;

                return;
            }
            else if (_containerState.Minimized)
            {
                Rect buttonRect = inRect;
                if (Widgets.ButtonImage(buttonRect, TexResource.Resize))
                {
                    _containerState.Minimized = false;
                    this.windowRect = _windowRect;
                    this.resizeable = true;
                    this.closeOnClickedOutside = true;
                    this.forcePause = true;
                }

                return;
            }

            WidgetRow widgetRow = new WidgetRow(inRect.x, inRect.y, UIDirection.RightThenDown);
            foreach (OverviewTab tab in _tabs)
            {
                if (widgetRow.ButtonText(tab.Label))
                {
                    _activeTab = tab;
                    _activeTab.PreSwitch();
                }
            }

            float rollingY = widgetRow.FinalY + GenUI.ListSpacing;
            Widgets.DrawLineHorizontal(inRect.x, rollingY, inRect.width);

            _activeTab.DoTabContent(inRect.ReplaceyMin(rollingY + GenUI.GapTiny).ReplaceyMax(inRect.yMax - GenUI.ListSpacing));

            widgetRow = new WidgetRow(inRect.x, inRect.yMax - GenUI.ListSpacing, UIDirection.RightThenDown);
            widgetRow.Label(UIText.Tips.Translate(_tipDisplayer.GetTip()));
        }

        /// <summary>
        /// It is invoked before this window is about to be removed from the window stack.
        /// </summary>
        public override void PreClose()
        {
            base.PreClose();
            _containerState.Minimized = false;
        }

        private void Minimized()
        {
            _windowRect = new Rect(this.windowRect);
            this.windowRect = new Rect(_windowRect.xMax - _minWindowWidth, _windowRect.y, GenUI.SmallIconSize, GenUI.SmallIconSize)
                .ExpandedBy(DrawUtility.WindowPadding);
            this.resizeable = false;
            this.doCloseX = false;
            this.forcePause = false;
        }
    }
}
