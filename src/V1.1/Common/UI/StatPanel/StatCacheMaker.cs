// <copyright file="StatCacheMaker.cs" company="Zizhen Li">
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
using Verse;

namespace AwesomeInventory.UI
{
    /// <summary>
    /// Maker for cache of stat.
    /// </summary>
    public class StatCacheMaker : CacheMaker<CacheableTick<float>, StatCacheKey>
    {
        private const int _interval = 50;

        /// <inheritdoc />
        public override CacheableTick<float> Make(StatCacheKey key)
        {
            return new CacheableTick<float>(
                        key.Pawn.GetStatValue(key.StatDef)
                        , () => Find.TickManager.TicksGame
                        , _interval
                        , () => key.Pawn.GetStatValue(key.StatDef)
                        , Find.TickManager.TicksGame);
        }
    }
}
