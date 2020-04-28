// <copyright file="Dialog_Settings.cs" company="Zizhen Li">
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
    /// Draw settings for Awesome Inventory.
    /// </summary>
    public class Dialog_Settings : Window
    {
        private Mod _awesomeInventory = LoadedModManager.ModHandles
                .First(mod => mod.SettingsCategory() == UIText.AwesomeInventoryDisplayName.TranslateSimple());

        /// <summary>
        /// Initializes a new instance of the <see cref="Dialog_Settings"/> class.
        /// </summary>
        public Dialog_Settings()
        {
            doCloseX = true;
            forcePause = true;
            absorbInputAroundWindow = true;
            closeOnClickedOutside = true;
        }

        /// <summary>
        /// Gets initial size for dialog.
        /// </summary>
        public override Vector2 InitialSize => new Vector2(900, 370);

        /// <summary>
        /// Draw mod settings.
        /// </summary>
        /// <param name="inRect"> Rect for drawing. </param>
        public override void DoWindowContents(Rect inRect)
        {
            _awesomeInventory.DoSettingsWindowContents(inRect);
        }

        /// <summary>
        /// Invoked before the dialog is closed.
        /// </summary>
        public override void PreClose()
        {
            _awesomeInventory.WriteSettings();
        }
    }
}
