// <copyright file="UIText.cs" company="Zizhen Li">
// Copyright (c) Zizhen Li. All rights reserved.
// Licensed under the GPL-3.0-only license. See LICENSE.md file in the project root for full license information.
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AwesomeInventory.UI
{
    /// <summary>
    /// Text displayed on UI. Call Translate() on members whenever they are referenced, e.g., UIText.Equip.Translate(); .
    /// </summary>
    public static class UIText
    {
#pragma warning disable SA1600 // Elements should be documented
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

        public const string Apparel = "Apparel";

        public const string ApparelForcedLower = "ApparelForcedLower";

        public const string ArmorBlunt = "ArmorBlunt";

        public const string ArmorHeat = "ArmorHeat";

        public const string ArmorSharp = "ArmorSharp";

        public const string CannotEquip = "CannotEquip";

        public const string ConsumeThing = "ConsumeThing";

        public const string Delete = "Delete";

        public const string DropThing = "DropThing";

        public const string Equip = "Equip";

        public const string Equipment = "Equipment";

        public const string EquipWarningBrawler = "EquipWarningBrawler";

        public const string ForcedApparel = "ForcedApparel";

        public const string HitPointsBasic = "HitPointsBasic";

        public const string Incapable = "Incapable";

        public const string Inventory = "Inventory";

        public const string IsIncapableOfViolenceLower = "IsIncapableOfViolenceLower";

        public const string Mass = "Mass";

        public const string MassCarried = "MassCarried";

        public const string NoEmptyPlaceLower = "NoEmptyPlaceLower";

        public const string OK = "OK";

        public const string OverallArmor = "OverallArmor";

        public const string AIEquip = "AwesomeInventory_Equip";

        public const string AIForceWear = "AwesomeInventory_ForceWear";

        public const string AIWear = "AwesomeInventory_Wear";

        public const string AsceticTab = "AwesomeInventory_Ascetic";

        public const string Belt = "AwesomeInventory_Belt";

        public const string ChooseMaterialAndQuality = "AwesomeInventory_ChooseMaterialAndQuality";

        public const string ComfyTemperatureRange = "ComfyTemperatureRange";

        public const string CopyLoadout = "AwesomeInventory_CopyLoadout";

        public const string CountFieldTip = "AwesomeInventory_CountFieldTip";

        public const string CurrentTemperature = "AwesomeInventory_CurrentTemperature";

        public const string Customize = "AwesomeInventory_Customize";

        public const string DeleteLoadout = "AwesomeInventory_DeleteLoadout";

        public const string DragToReorder = "AwesomeInventory_DragToReorder";

        public const string ExtraApparels = "AwesomeInventory_ExtraApparels";

        public const string FailToDrop = "AwesomeInventory_FailToDrop";

        public const string GreedyTab = "AwesomeInventory_GreedyTab";

        public const string Head = "AwesomeInventory_Head";

        public const string JealousTab = "AwesomeInventory_JealousTab";

        public const string AIMassCarried = "AwesomeInventory_MassCarried";

        public const string MouseOverNumbersForDetails = "AwesomeInventory_MouseOverNumbersForDetails";

        public const string NewLoadout = "AwesomeInventory_NewLoadout";

        public const string NoLoadout = "AwesomeInventory_NoLoadout";

        public const string NoLoadoutSelected = "AwesomeInventory_NoLoadoutSelected";

        public const string NoMaterial = "AwesomeInventory_NoMaterial";

        public const string OpenLoadout = "AwesomeInventory_OpenLoadout";

        public const string Pant = "AwesomeInventory_Pants";

        public const string PrimaryWeapon = "AwesomeInventory_PrimaryWeapon";

        public const string PutAway = "AwesomeInventory_PutAway";

        public const string SelectLoadout = "AwesomeInventory_SelectLoadout";

        public const string SourceAllTip = "AwesomeInventory_SourceAllTip";

        public const string SourceFilterTip = "AwesomeInventory_SourceFilterTip";

        public const string SourceGenericTip = "AwesomeInventory_SourceGenericTip";

        public const string SourceMeleeTip = "AwesomeInventory_SourceMeleeTip";

        public const string SourceMinifiedTip = "AwesomeInventory_SourceMinifiedTip";

        public const string SourceRangedTip = "AwesomeInventory_SourceRangedTip";

        public const string TorsoMiddleLayer = "AwesomeInventory_TorsoMiddle";

        public const string TorsoOnSkinLayer = "AwesomeInventory_TorsoOnSkin";

        public const string TorsoShellLayer = "AwesomeInventory_TorsoShell";

        public const string Weight = "AwesomeInventory_Weight";

        public const string OutfitAnything = "OutfitAnything";

        public const string SourceApparelTip = "SourceApparelTip";

        public const string TenCharsString = "AAAAAAAAAA";

        public const string Traits = "Traits";

        public const string UnloadNow = "AwesomeInventory_UnloadNow";

        public const string VanillaWear = "AwesomeInventory_VanillaWear";

#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
#pragma warning restore SA1600 // Elements should be documented
    }
}
