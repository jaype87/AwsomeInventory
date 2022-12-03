﻿// <copyright file="WindowOnMouseDrag.cs" company="Zizhen Li">
// Copyright (c) 2019 - 2020 Zizhen Li. All rights reserved.
// Licensed under the LGPL-3.0-only license. See LICENSE.md file in the project root for full license information.
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;
using RimWorld;

namespace RPG_Inventory_Remake_Common
{
    public class WindowOnMouseDrag : Window
    {
        private Rect mouseRect;

        public Action DoWindowAction;

        public int WindowID
        {
            get => ID;
        }

        public WindowOnMouseDrag(Rect rect)
        {
            doCloseButton = false;
            doCloseX = false;
            soundAppear = null;
            soundClose = null;
            closeOnClickedOutside = true;
            closeOnAccept = false;
            closeOnCancel = false;
            focusWhenOpened = true;
            preventCameraMotion = false;
            doWindowBackground = false;
            layer = WindowLayer.Super;
            mouseRect = rect;
        }



        protected override float Margin => 0f;

        public override void PostOpen()
        {
            base.PostOpen();
            windowRect = mouseRect;
        }

        public override void WindowOnGUI()
        {
            base.WindowOnGUI();
        }

        public override void DoWindowContents(Rect inRect)
        {
            DoWindowAction();
        }
    }
}
