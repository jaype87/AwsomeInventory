// <copyright file="Dialog_InstantMessage.cs" company="Zizhen Li">
// Copyright (c) 2019 - 2020 Zizhen Li. All rights reserved.
// Licensed under the LGPL-3.0-only license. See LICENSE.md file in the project root for full license information.
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;

namespace AwesomeInventory.UI
{
    /// <summary>
    /// Assign default values to some window properties that are commonly used in this mod.
    /// </summary>
    public class Dialog_InstantMessage : Dialog_MessageBox
    {
        private Vector2 _initialSize;

        /// <summary>
        /// Initializes a new instance of the <see cref="Dialog_InstantMessage"/> class.
        /// </summary>
        /// <param name="text"> Text to display in the pop-up window.</param>
        /// <param name="initialSize"> Size of the dialog window. </param>
        /// <param name="buttonAText"> Text on button A. </param>
        /// <param name="buttonAAction"> Action to take when button A is pressed. </param>
        /// <param name="buttonBText"> Text on button B. Default is "Cancel". </param>
        /// <param name="buttonBAction"> Action to take when button B is pressed. </param>
        /// <param name="title"> Title of the window. </param>
        /// <param name="buttonADestructive"> Gives a red bg color if true. </param>
        /// <param name="acceptAction"> Not in use. </param>
        /// <param name="cancelAction"> Not currently in use. </param>
        public Dialog_InstantMessage(string text, Vector2 initialSize, string buttonAText = null, Action buttonAAction = null, string buttonBText = null, Action buttonBAction = null, string title = null, bool buttonADestructive = false, Action acceptAction = null, Action cancelAction = null)
            : base(text, buttonAText, buttonAAction, buttonBText, buttonBAction, title, buttonADestructive, acceptAction, cancelAction)
        {
            _initialSize = initialSize;
            closeOnClickedOutside = true;
            layer = WindowLayer.SubSuper;
        }

        /// <inheritdoc/>
        public override Vector2 InitialSize => _initialSize;
    }
}
