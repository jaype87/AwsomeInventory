// <copyright file="StatListRect.cs" company="Zizhen Li">
// Copyright (c) 2019 - 2020 Zizhen Li. All rights reserved.
// Licensed under the LGPL-3.0-only license. See LICENSE.md file in the project root for full license information.
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using RimWorldUtility;
using RimWorldUtility.UI;
using UnityEngine;
using Verse;

namespace AwesomeInventory.UI
{
    /// <summary>
    /// A rect contains a list of stats.
    /// </summary>
    public class StatListRect : ListRect<IEnumerable<StatCacheKey>, StatCacheKey, StatRect>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="StatListRect"/> class.
        /// </summary>
        public StatListRect()
        {
        }

        /// <inheritdoc />
        public override Vector2 Build(IEnumerable<StatCacheKey> statCacheKeys)
        {
            Vector2 size = base.Build(statCacheKeys);
            var groups = this.ChildrenRects
                .GroupBy(rect => rect.Offset.x, rect => rect);

            foreach (var group in groups)
            {
                float maxWidth = group.Max(rect => rect.Size.x);

                bool blackBg = true;
                foreach (StatRect statRect in group)
                {
                    statRect.Size.x = maxWidth;
                    statRect.BgTexture = blackBg ? TexUI.TextBGBlack : TexUI.GrayTextBG;
                    blackBg ^= true;
                }
            }

            return size;
        }
    }
}