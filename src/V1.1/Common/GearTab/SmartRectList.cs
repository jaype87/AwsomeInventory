// <copyright file="SmartRectList.cs" company="Zizhen Li">
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
    /// A list of <see cref="SmartRect{T}"/> that handles requests for creating <see cref="SmartRect{T}"/>
    /// and requests for rects to draw <typeparamref name="T"/> on.
    /// </summary>
    /// <typeparam name="T"> Type defrived from <see cref="Thing"/>. </typeparam>
    public class SmartRectList<T>
        where T : Thing
    {
        private SmartRect<T> _seed;

        /// <summary>
        /// Gets a colleciton of <see cref="SmartRect{T}"/> that it holds.
        /// </summary>
        public List<SmartRect<T>> SmartRects { get; } = new List<SmartRect<T>>();

        /// <summary>
        /// Set a seed for this list. Subsequent <see cref="SmartRect{T}"/> created with <see cref="GetWorkingSmartRect(Func{T, bool}, float, float)"/>
        /// will use <paramref name="seed"/> as a template.
        /// </summary>
        /// <param name="seed"> Template for subsequent <see cref="SmartRect{T}"/>. </param>
        public void Init(SmartRect<T> seed)
        {
            ValidateArg.NotNull(seed, nameof(seed));

            _seed = seed;
            _seed.List = this;
            this.SmartRects.Add(seed);
        }

        /// <summary>
        /// Return a new <see cref="SmartRect{T}"/> that shares the same template with the <see cref="SmartRect{T}"/> that seeds this list.
        /// </summary>
        /// <param name="selector"> Defines on which level this smart rect is on. </param>
        /// <param name="xLeftCurPosition"> The left most coordinate of allocated rects. </param>
        /// <param name="xRightCurPosition"> The right most coordinate of allocated rects. </param>
        /// <returns> A <see cref="SmartRect{T}"/>. </returns>
        public SmartRect<T> GetWorkingSmartRect(Func<T, bool> selector, float xLeftCurPosition, float xRightCurPosition)
        {
            SmartRect<T> tail = this.SmartRects.Last();
            Rect newRect = new Rect(tail.RectTemplate);
            newRect.y += tail.HeightGap + tail.Height;
            SmartRect<T> smartRect = new SmartRect<T>(
                newRect, selector, xLeftCurPosition, xRightCurPosition, this, _seed.XLeftEdge, _seed.XRightEdge);
            this.SmartRects.Add(smartRect);
            return smartRect;
        }

        /// <summary>
        /// Return an available <see cref="Rect"/> to draw for <paramref name="thing"/>.
        /// </summary>
        /// <param name="thing"> Thing to draw. </param>
        /// <returns> An empty <see cref="Rect"/>. </returns>
        public Rect GetRectFor(T thing)
        {
            foreach (SmartRect<T> smartRect in this.SmartRects)
            {
                if (smartRect.Selector != null && smartRect.Selector(thing))
                {
                    Rect newRect = smartRect.GetRectFor(thing);

                    if (newRect == default)
                        continue;
                    else
                        return newRect;
                }
            }

            return default;
        }

        /// <summary>
        /// Return a next best available <see cref="Rect"/> to draw for <paramref name="thing"/>.
        /// </summary>
        /// <param name="thing"> Thing to draw. </param>
        /// <returns> An empty <see cref="Rect"/>. </returns>
        public Rect GetNextBestRectFor(T thing)
        {
            for (int i = 0; i < this.SmartRects.Count; i++)
            {
                if (this.SmartRects[i].Selector(thing))
                {
                    int y = i - 1;
                    if (y != -1)
                    {
                        Rect emptyRect = SmartRects[y].GetRectFor(thing);
                        if (emptyRect != default)
                            return emptyRect;
                    }
                }
            }

            return default;
        }
    }
}
