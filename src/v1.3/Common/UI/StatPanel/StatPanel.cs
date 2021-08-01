// <copyright file="StatPanel.cs" company="Zizhen Li">
// Copyright (c) 2019 - 2020 Zizhen Li. All rights reserved.
// Licensed under the LGPL-3.0-only license. See LICENSE.md file in the project root for full license information.
// </copyright>

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using RimWorldUtility.Logging;
using RimWorldUtility.UI;
using UnityEngine;
using Verse;

namespace AwesomeInventory.UI
{
    /// <summary>
    /// Panel, attached to the side of Gear tab, displays stats of player's choice.
    /// </summary>
    public class StatPanel : Window
    {
        private readonly StatPanelRect _panel = new StatPanelRect();

        private static StatPanelModel _model;

        private Vector2 _position;

        public static Vector2 InitSize = new Vector2(300f, 300f);

        public new static bool IsOpen;

        public StatPanel()
        {
            this.preventCameraMotion = false;
            this.resizeable = true;
            this.closeOnCancel = false;
        }

        public void SetPosition(Vector2 position)
        {
            _position = position;
            this.windowRect.position = position;
        }

        public bool IsValid => Find.MainTabsRoot.OpenTab?.TabWindow is MainTabWindow_Inspect tabWindow
                               && typeof(AwesomeInventoryTabBase).IsAssignableFrom(tabWindow.OpenTabType);

        #region Overrides of Window

        /// <inheritdoc />
        public override Vector2 InitialSize => InitSize;

        /// <inheritdoc />
        public override void WindowOnGUI()
        {
            base.WindowOnGUI();
            InitSize = this.windowRect.size;
        }

        /// <inheritdoc />
        public override void WindowUpdate()
        {
            base.WindowUpdate();

            if (!IsValid)
            {
                this.Close();
                return;
            }

            if (Find.Selector.SingleSelectedThing is Pawn pawn)
            {
                _model.Size = this.windowRect.ContractedBy(this.Margin).size;
                _model.Pawn = pawn;
                _panel.Update(_model);
            }
        }

        /// <inheritdoc />
        [SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "Designed to catch all")]
        public override void PreOpen()
        {
            try
            {
                base.PreOpen();
                this.windowRect.position = _position;
                if (!(Find.Selector.SingleSelectedThing is Pawn pawn))
                    return;

                _model = new StatPanelModel(pawn, UIText.StatPanel.TranslateSimple(), this.windowRect.size);
                StatPanelManager.SelectedDefs.Register(_model.StatCacheKeys);

                _panel.Build(_model);
            }
            catch (Exception e)
            {
                Find.WindowStack.Add(
                    new Dialog_ErrorReporting(e.ToString(), UIText.ErrorReport.TranslateSimple(), AwesomeInventoryMod.BugReportUrl));
                IsOpen = false;
                this.Close();
            }
        }

        /// <inheritdoc />
        [SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "Designed to catch all")]
        public override void DoWindowContents(Rect inRect)
        {
            try
            {
                _panel.Draw(_model, inRect.position);
            }
            catch (Exception e)
            {
                Find.WindowStack.Add(
                    new Dialog_ErrorReporting(e.ToString(), UIText.ErrorReport.TranslateSimple(), AwesomeInventoryMod.BugReportUrl));
                IsOpen = false;
                this.Close();
            }
        }

        /// <inheritdoc />
        public override void PreClose()
        {
            base.PreClose();
            StatPanelManager.SelectedDefs.Unregister(_model.StatCacheKeys);
        }

        #endregion
    }
}