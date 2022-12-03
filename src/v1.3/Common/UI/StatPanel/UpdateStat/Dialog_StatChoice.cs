// <copyright file="Dialog_StatChoice.cs" company="Zizhen Li">
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
    public class Dialog_StatChoice : Window
    {
        private readonly StatChoiceRect _statChoiceRect = new StatChoiceRect();

        private readonly StatChoiceModel _model = new StatChoiceModel();

        /// <summary>
        /// Initializes a new instance of the <see cref="Dialog_StatChoice"/> class.
        /// </summary>
        public Dialog_StatChoice()
        {
            this.forcePause = true;
            this.draggable = true;
            this.resizeable = true;
            this.closeOnClickedOutside = true;
            this.doCloseX = true;
            this.closeOnCancel = false;

            StatPanelManager.SelectedDefs.Register(_model);
        }

        #region Overrides of Window

        /// <inheritdoc />
        public override void PreOpen()
        {
            base.PreOpen();
            _statChoiceRect.Build(_model);
        }

        /// <inheritdoc />
        public override void DoWindowContents(Rect inRect)
        {
            _statChoiceRect.Draw(_model, inRect.position);
        }

        /// <inheritdoc />
        public override void WindowUpdate()
        {
            base.WindowUpdate();

            _model.Size = this.windowRect.ContractedBy(this.Margin).size.Rounded();
            _statChoiceRect.Update(_model);
        }

        /// <inheritdoc />
        public override void PreClose()
        {
            base.PreClose();
            StatPanelManager.SelectedDefs.Unregister(_model);
        }

        #endregion
    }
}
