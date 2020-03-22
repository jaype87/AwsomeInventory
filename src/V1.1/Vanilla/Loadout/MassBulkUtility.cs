// <copyright file="MassBulkUtility.cs" company="Zizhen Li">
// Copyright (c) 2019 - 2020 Zizhen Li. All rights reserved.
// Licensed under the LGPL-3.0-only license. See LICENSE.md file in the project root for full license information.
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;
using UnityEngine;

namespace RPG_Inventory_Remake.Loadout
{
    public class MassBulkUtility
    {
        public const float WeightCapacityPerBodySize = 35f;
        public const float BulkCapacityPerBodySize = 20f;

        public static float BaseCarryWeight(Pawn p)
        {
            return p.BodySize * WeightCapacityPerBodySize;
        }

        public static float BaseCarryBulk(Pawn p)
        {
            return p.BodySize * BulkCapacityPerBodySize;
        }

        public static float MoveSpeedFactor(float weight, float weightCapacity)
        {
            float t = weight / weightCapacity;
            if (float.IsNaN(t)) t = 1f;
            return Mathf.Lerp(1f, 0.75f, t);
        }

        public static float WorkSpeedFactor(float bulk, float bulkCapacity)
        {
            return Mathf.Lerp(1f, 0.75f, bulk / bulkCapacity);
        }

        public static float EncumberPenalty(float weight, float weightCapacity)
        {
            if (weight > weightCapacity)
            {
                float weightPercent = weight / weightCapacity;
                if (float.IsPositiveInfinity(weightPercent))
                {
                    return 1f;
                }
                return weightPercent - 1;
            }
            else
                return 0f;
        }

    }
}
