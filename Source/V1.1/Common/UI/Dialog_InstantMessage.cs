// <copyright file="Dialog_InstantMessage.cs" company="Zizhen Li">
// Copyright (c) Zizhen Li. All rights reserved.
// Licensed under the GPL-3.0-only license. See LICENSE.md file in the project root for full license information.
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;

namespace RPG_Inventory_Remake_Common
{
    public class Dialog_InstantMessage : Dialog_MessageBox
    {
        private Vector2 _initialSize;

        public Dialog_InstantMessage(string text, Vector2 initialSize, string buttonAText = null, Action buttonAAction = null, string buttonBText = null, Action buttonBAction = null, string title = null, bool buttonADestructive = false, Action acceptAction = null, Action cancelAction = null)
            : base(text, buttonAText, buttonAAction, buttonBText, buttonBAction, title, buttonADestructive, acceptAction, cancelAction)
        {
            _initialSize = initialSize;
            closeOnClickedOutside = true;
            layer = WindowLayer.SubSuper;
        }

        public override void PreOpen()
        {
            base.PreOpen();
        }

        public override void DoWindowContents(Rect inRect)
        {
            base.DoWindowContents(inRect);
        }

        public override Vector2 InitialSize => _initialSize;
    }
}
