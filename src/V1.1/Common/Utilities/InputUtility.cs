// <copyright file="InputUtility.cs" company="Zizhen Li">
// Copyright (c) 2019 - 2020 Zizhen Li. All rights reserved.
// Licensed under the LGPL-3.0-only license. See LICENSE.md file in the project root for full license information.
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using UnityEngine;
using Verse;

namespace AwesomeInventory
{
    /// <summary>
    /// Utility support for input from players.
    /// </summary>
    public static class InputUtility
    {
        /// <summary>
        /// Gets a value indicating whether the control key is pressed.
        /// </summary>
        public static bool IsControl => Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl);

        /// <summary>
        /// Gets a value indicating whether the left mouse button is pressed.
        /// </summary>
        public static bool IsLeftMouseClick => Input.GetMouseButtonDown(0);

        /// <summary>
        /// Gets a value indicating whether the left mouse button is released.
        /// </summary>
        public static bool IsLeftMouseUp => Input.GetMouseButtonUp(0);

        /// <summary>
        /// Check whether user double-clicks on a rect.
        /// </summary>
        /// <param name="rect"> Rect which is double-clicked. </param>
        /// <returns> Returns true if a rect is double-clicked. </returns>
        public static bool DoubleClick(Rect rect)
        {
            if (Mouse.IsOver(rect))
            {
                if (Event.current.type == EventType.Repaint && Input.GetMouseButtonUp(0))
                {
                    Log.Warning("Mouse up");
                    DateTime now = DateTime.Now;
                    if (now - DoubleClickState.FirstClick > DoubleClickState.Interval)
                    {
                        DoubleClickState.FirstClick = now;
                        return false;
                    }
                    else
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        private static class DoubleClickState
        {
            public static DateTime FirstClick = DateTime.MinValue;

            public static readonly TimeSpan Interval = new TimeSpan(0, 0, 0, 0, 200);
        }
    }
}
