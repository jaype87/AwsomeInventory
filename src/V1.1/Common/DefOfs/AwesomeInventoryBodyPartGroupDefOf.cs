// <copyright file="AwesomeInventoryBodyPartGroupDefOf.cs" company="Zizhen Li">
// Copyright (c) 2019 - 2020 Zizhen Li. All rights reserved.
// Licensed under the LGPL-3.0-only license. See LICENSE.md file in the project root for full license information.
// </copyright>

using System;
using RimWorld;
using Verse;

namespace AwesomeInventory
{
    /// <summary>
    /// Body part group needed to draw the RPG style inventory tab.
    /// </summary>
    [DefOf]
    public static class AwesomeInventoryBodyPartGroupDefOf
    {
#pragma warning disable CA2211 // Non-constant fields should not be visible
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable SA1600 // Elements should be documented
        // Comments below are formatted with "Better Comments" in Visual Studio Marketplace

        // UpperHead = 200, FullHead = 199, Eyes = 198, Teeth = 197, Mouth = 196,
        // Neck = 180,
        // Torso = 100, Arms = 90, LeftArm = 89, RightArm = 88,
        // Shoulders = 85, LeftShoulder = 84, RightShoulder = 83,
        // Hands = 80, LeftHand = 79, RightHand = 78,
        // Waist = 50,
        // Legs = 10,
        // Feet = 9

        // ! BodyPargGroupDefs below are also declared in RimWorld.BodyPartGroupDefOf
        public static BodyPartGroupDef UpperHead;
        public static BodyPartGroupDef FullHead;
        public static BodyPartGroupDef Torso;
        public static BodyPartGroupDef LeftHand;
        public static BodyPartGroupDef RightHand;
        public static BodyPartGroupDef Legs;
        public static BodyPartGroupDef Eyes;

        // ! Following BodyPartGroupDefs are defined in Core
        // ! Path:RimWorld\Mods\Core\Defs\Bodies\BodyPartGroups.xml
        // ! The reason why they are not in the BodyPartGroupDefOf Class is anyone's guess
        public static BodyPartGroupDef Teeth;
        public static BodyPartGroupDef Mouth;
        public static BodyPartGroupDef Neck;
        public static BodyPartGroupDef Arms;
        public static BodyPartGroupDef Shoulders;
        public static BodyPartGroupDef Hands;
        public static BodyPartGroupDef Waist;
        public static BodyPartGroupDef Feet;

        // These are added for CE
        public static BodyPartGroupDef LeftArm;
        public static BodyPartGroupDef RightArm;
        public static BodyPartGroupDef LeftShoulder;
        public static BodyPartGroupDef RightShoulder;

        // These are added for Jewelry
        // Two defs file are added, they are in Defs\Jewelry_compat
        public static BodyPartGroupDef Ears;
        public static ApparelLayerDef Accessories;

        // Placeholder
        public static BodyPartGroupDef None;
#pragma warning restore SA1600 // Elements should be documented
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
#pragma warning restore CA2211 // Non-constant fields should not be visible

        static AwesomeInventoryBodyPartGroupDefOf()
        {
            DefOfHelper.EnsureInitializedInCtor(typeof(AwesomeInventoryBodyPartGroupDefOf));
        }
    }
}