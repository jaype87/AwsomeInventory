// <copyright file="TipDisplayer.cs" company="Zizhen Li">
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
    /// Periodically display different tips.
    /// </summary>
    public class TipDisplayer
    {
        private TimeSpan _updateInterval = new TimeSpan(0, 0, 5);

        private DateTime _lastDisplayTime;

        private int _tipIndex = 0;

        /// <summary>
        /// Initializes a new instance of the <see cref="TipDisplayer"/> class.
        /// </summary>
        /// <param name="updateInterval"> The time interval between displaying different tips. </param>
        /// <param name="tips"> Tip pools. </param>
        public TipDisplayer(TimeSpan updateInterval, List<string> tips)
            : this(tips)
        {
            _updateInterval = updateInterval;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TipDisplayer"/> class.
        /// </summary>
        /// <param name="tips"> Tip pools. </param>
        public TipDisplayer(List<string> tips)
        {
            this.Tips = tips;
        }

        /// <summary>
        /// Gets a list of tips.
        /// </summary>
        public List<string> Tips { get; }

        /// <summary>
        /// Display tips.
        /// </summary>
        /// <param name="rect"> Rect in which tips are displayed. </param>
        public void Display(Rect rect)
        {
            Widgets.Label(rect, UIText.Tips.Translate(this.GetTip()));
        }

        /// <summary>
        /// Get tip from tip pool.
        /// </summary>
        /// <returns> Tip in string. </returns>
        public string GetTip()
        {
            string tip = this.Tips[_tipIndex];
            if (_lastDisplayTime == default)
            {
                _lastDisplayTime = DateTime.Now;
            }
            else
            {
                DateTime now = DateTime.Now;
                if (now - _lastDisplayTime > _updateInterval)
                {
                    _lastDisplayTime = now;
                    _tipIndex = ++_tipIndex % this.Tips.Count;
                }
            }

            return tip;
        }
    }
}
