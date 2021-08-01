// <copyright file="SmartRect.cs" company="Zizhen Li">
// Copyright (c) 2019 - 2020 Zizhen Li. All rights reserved.
// Licensed under the LGPL-3.0-only license. See LICENSE.md file in the project root for full license information.
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using UnityEngine;
using Verse;

namespace AwesomeInventory.UI
{
    /// <summary>
    /// Keep track of space usage in the Gear Tab and return the next available rect for drawing
    /// </summary>
    /// <typeparam name="T"> Type of thing this smart rect holds. </typeparam>
    public class SmartRect<T>
        where T : Thing
    {
        /// <summary>
        /// Gets or sets left edge of the operating range of smart rect.
        /// </summary>
        public float XLeftEdge { get; set; }

        /// <summary>
        /// Gets or sets right edge of the operating range of smart rect.
        /// </summary>
        public float XRightEdge { get; set; }

        /// <summary>
        /// Gets or sets the left most coordinate of allocated rects.
        /// </summary>
        public float XLeftCurrentPosition { get; set; }

        /// <summary>
        /// Gets or sets the right most coordinate of allocated rects.
        /// </summary>
        public float XRightCurrentPosition { get; set; }

        /// <summary>
        /// Gets or sets defines on which level this smart rect is on.
        /// </summary>
        public Func<T, bool> Selector { get; set; }

        public float WidthGap;
        public float HeightGap;
        public Direction LastDirection;
        public SmartRectList<T> List;

        public List<IconRect<T>> DefaultRects { get; } = new List<IconRect<T>>();

        public Rect RectTemplate;

        public enum Direction : uint
        {
            Left = uint.MinValue,
            Right = uint.MaxValue
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SmartRect{T}"/> class.
        /// </summary>
        /// <param name="template"></param>
        /// <param name="selector"></param>
        /// <param name="xLeftCurPosition"></param>
        /// <param name="xRightCurPosition"></param>
        /// <param name="list"></param>
        /// <param name="xLeftEdge"></param>
        /// <param name="xRightEdge"></param>
        /// <param name="widthGap"></param>
        /// <param name="heightGap"></param>
        /// <param name="lastDirection"></param>
        public SmartRect(
            Rect template,
            Func<T, bool> selector,
            float xLeftCurPosition,
            float xRightCurPosition,
            SmartRectList<T> list = null,
            float xLeftEdge = 0,
            float xRightEdge = 0,
            float widthGap = 10,
            float heightGap = 10,
            Direction lastDirection = Direction.Right)
        {
            this.RectTemplate = template;
            this.Selector = selector;
            this.List = list;
            this.XLeftEdge = xLeftEdge;
            this.XRightEdge = xRightEdge;
            this.XLeftCurrentPosition = xLeftCurPosition;
            this.XRightCurrentPosition = xRightCurPosition;
            this.WidthGap = widthGap;
            this.HeightGap = heightGap;
            this.LastDirection = lastDirection;
        }

        public float y
        {
            get
            {
                return RectTemplate.y;
            }

            set
            {
                RectTemplate.y = value;
            }
        }

        public float width
        {
            get
            {
                return RectTemplate.width;
            }

            set
            {
                RectTemplate.width = value;
            }
        }

        public float Height
        {
            get
            {
                return RectTemplate.height;
            }

            set
            {
                RectTemplate.height = value;
            }
        }

        public float yMax
        {
            get
            {
                return RectTemplate.yMax;
            }
        }

        public void AddDefaultRect(Func<T, bool> filter, string tooltip)
        {
            this.DefaultRects.Add(
                new IconRect<T>(this.NextAvailableRect(), filter, tooltip));
        }

        /// <summary>
        /// Returns next available <see cref="Rect"/> for current level.
        /// Returns null, if no <see cref="Rect"/> is available.
        /// </summary>
        /// <returns> An empty <see cref="Rect"/> for drawing. </returns>
        public Rect NextAvailableRect()
        {
            // Return the first rect on the row
            if (XLeftCurrentPosition == XRightCurrentPosition)
            {
                XRightCurrentPosition += width;
                return new Rect(XLeftCurrentPosition, y, width, Height);
            }

            LastDirection = ~LastDirection;
            return NextAvailableRect(LastDirection);
        }

        public Rect NextAvailableRect(Direction direction)
        {
            if (direction == Direction.Left)
            {
                if (XLeftEdge < XLeftCurrentPosition - WidthGap - width)
                {
                    XLeftCurrentPosition -= WidthGap + width;
                    return new Rect(XLeftCurrentPosition, y, width, Height);
                }
            }

            if (XRightEdge > XRightCurrentPosition + WidthGap + width)
            {
                float x_temp = XRightCurrentPosition + WidthGap;
                XRightCurrentPosition += WidthGap + width;
                return new Rect(x_temp, y, width, Height);
            }

            return default;
        }

        public Rect GetRectFor(T thing)
        {
            foreach (IconRect<T> iconRect in this.DefaultRects)
            {
                if (iconRect.Filter(thing))
                {
                    this.DefaultRects.Remove(iconRect);
                    return iconRect.Rect;
                }
            }

            return this.NextAvailableRect();
        }
    }
}
