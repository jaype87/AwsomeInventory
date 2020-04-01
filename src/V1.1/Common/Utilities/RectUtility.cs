// <copyright file="RectUtility.cs" company="Zizhen Li">
// Copyright (c) 2019 - 2020 Zizhen Li. All rights reserved.
// Licensed under the LGPL-3.0-only license. See LICENSE.md file in the project root for full license information.
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace AwesomeInventory.UI
{
    /// <summary>
    /// Helper functions for rect.
    /// </summary>
    public static class RectUtility
    {
        /// <summary>
        /// Return a copy of the calling rect but with a new <paramref name="x"/> value.
        /// </summary>
        /// <param name="rect"> The rect to modify. </param>
        /// <param name="x"> New x value for <paramref name="rect"/>. </param>
        /// <returns> A <see cref="Rect"/> with the new <paramref name="x"/> value.</returns>
        public static Rect ReplaceX(this Rect rect, float x)
        {
            return new Rect(x, rect.y, rect.width, rect.height);
        }

        /// <summary>
        /// Return a copy of the calling rect but with a new <paramref name="y"/> value.
        /// </summary>
        /// <param name="rect"> The rect to modify. </param>
        /// <param name="y"> New y value for <paramref name="rect"/>. </param>
        /// <returns> A <see cref="Rect"/> with the new <paramref name="y"/> value.</returns>
        public static Rect ReplaceY(this Rect rect, float y)
        {
            return new Rect(rect.x, y, rect.width, rect.height);
        }

        /// <summary>
        /// Replace the y coordinate in <paramref name="rect"/> with <paramref name="width"/>.
        /// </summary>
        /// <param name="rect"> The rect to modify. </param>
        /// <param name="width"> New width value for <paramref name="rect"/>. </param>
        /// <returns> A <see cref="Rect"/> with the new <paramref name="width"/> value.</returns>
        public static Rect ReplaceWidth(this Rect rect, float width)
        {
            return new Rect(rect.x, rect.y, width, rect.height);
        }

        /// <summary>
        /// Replace the y coordinate in <paramref name="rect"/> with <paramref name="height"/>.
        /// </summary>
        /// <param name="rect"> The rect to modify. </param>
        /// <param name="height"> New height value for <paramref name="rect"/>. </param>
        /// <returns> A <see cref="Rect"/> with the new <paramref name="height"/> value.</returns>
        public static Rect ReplaceHeight(this Rect rect, float height)
        {
            return new Rect(rect.x, rect.y, rect.width, height);
        }

        /// <summary>
        /// Replace the xMin in <paramref name="rect"/> with <paramref name="xMin"/>.
        /// </summary>
        /// <param name="rect"> The rect to modify. </param>
        /// <param name="xMin"> New xMin value for <paramref name="rect"/>. </param>
        /// <returns> A <see cref="Rect"/> with the new <paramref name="xMin"/> value.</returns>
        public static Rect ReplacexMin(this Rect rect, float xMin)
        {
            return new Rect(rect)
            {
                xMin = xMin,
            };
        }

        /// <summary>
        /// Replace the yMin in <paramref name="rect"/> with <paramref name="yMin"/>.
        /// </summary>
        /// <param name="rect"> The rect to modify. </param>
        /// <param name="yMin"> New yMin value for <paramref name="rect"/>. </param>
        /// <returns> A <see cref="Rect"/> with the new <paramref name="yMin"/> value.</returns>
        public static Rect ReplaceyMin(this Rect rect, float yMin)
        {
            return new Rect(rect)
            {
                yMin = yMin,
            };
        }

        /// <summary>
        /// Replace the xMax in <paramref name="rect"/> with <paramref name="xMax"/>.
        /// </summary>
        /// <param name="rect"> The rect to modify. </param>
        /// <param name="xMax"> New xMax value for <paramref name="rect"/>. </param>
        /// <returns> A <see cref="Rect"/> with the new <paramref name="xMax"/> value.</returns>
        public static Rect ReplacexMax(this Rect rect, float xMax)
        {
            return new Rect(rect)
            {
                xMax = xMax,
            };
        }

        /// <summary>
        /// Replace the yMax in <paramref name="rect"/> with <paramref name="yMax"/>.
        /// </summary>
        /// <param name="rect"> The rect to modify. </param>
        /// <param name="yMax"> New yMax value for <paramref name="rect"/>. </param>
        /// <returns> A <see cref="Rect"/> with the new <paramref name="yMax"/> value.</returns>
        public static Rect ReplaceyMax(this Rect rect, float yMax)
        {
            return new Rect(rect)
            {
                yMax = yMax,
            };
        }
    }
}
