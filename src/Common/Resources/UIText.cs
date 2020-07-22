// <copyright file="UIText.cs" company="Zizhen Li">
// Copyright (c) 2019 - 2020 Zizhen Li. All rights reserved.
// Licensed under the LGPL-3.0-only license. See LICENSE.md file in the project root for full license information.
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Policy;
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

        public const string CancelButton = "CancelButton";

        public const string CannotEquip = "CannotEquip";

        public const string ConsumeThing = "ConsumeThing";

        public const string Delete = "Delete";

        public const string DropThing = "DropThing";

        public const string Equip = "Equip";

        public const string Equipment = "Equipment";

        public const string EquipWarningBrawler = "EquipWarningBrawler";

        public const string ForcedApparel = "ForcedApparel";

        public const string HitPoints = "HitPoints";

        public const string HitPointsBasic = "HitPointsBasic";

        public const string Incapable = "Incapable";

        public const string Inventory = "Inventory";

        public const string IsIncapableOfViolenceLower = "IsIncapableOfViolenceLower";

        public const string Items = "ItemsTab";

        public const string Mass = "Mass";

        public const string MassCarried = "MassCarried";

        public const string NoEmptyPlaceLower = "NoEmptyPlaceLower";

        public const string None = "None";

        public const string OK = "OK";

        public const string OverallArmor = "OverallArmor";

        public const string ReportHauling = "ReportHauling";

        public const string ReportHaulingTo = "ReportHaulingTo";

        public const string ReportHaulingToGrave = "ReportHaulingToGrave";

        public const string ReportHaulingUnknown = "ReportHaulingUnknown";

        public const string StatWeaponName = "Stat_Weapon_Name";

        /****************** AwesomeInventory texts ********************/

        public const string AddRemoveStat = "AwesomeInventory_AddRemoveStat";

        public const string AddToAllLoadout = "AwesomeInventory_AddToAllLoadout";

        public const string AddToLoadout = "AwesomeInventory_AddToLoadout";

        public const string AIDefault = "AwesomeInventory_Default";

        public const string AIEquip = "AwesomeInventory_Equip";

        public const string AIForceWear = "AwesomeInventory_ForceWear";

        public const string AIMassCarried = "AwesomeInventory_MassCarried";

        public const string AIWear = "AwesomeInventory_Wear";

        public const string AsceticTab = "AwesomeInventory_Ascetic";

        public const string Assorted = "AwesomeInventory_Assorted";

        public const string AutoEquipWeapon = "AwesomeInventory_AutoEquipWeapon";

        public const string AutoEquipWeaponTooltip = "AwesomeInventory_AutoEquipWeaponTooltip";

        public const string AwesomeInventoryDisplayName = "AwesomeInventory_DisplayName";

        public const string Belt = "AwesomeInventory_Belt";

        public const string Blacklist = "AwesomeInventory_Blacklist";

        public const string BlacklistTooltip = "AwesomeInventory_BlacklistTooltip";

        public const string CantExceedStackCount = "AwesomeInventory_CantExceedStackCount";

        public const string CantUnloadIncapableManipulation = "AwesomeInventory_CantUnloadIncapableManipulation";

        public const string ChooseLoadut = "AwesomeInventory_ChooseLoadout";

        public const string ChooseMaterialAndQuality = "AwesomeInventory_ChooseMaterialAndQuality";

        public const string ChooseThemeColorForQuality = "AwesomeInventory_ChooseThemeColorForQuality";

        public const string ClearDrugSelection = "AwesomeInventory_ClearDrugSelection";

        public const string ComfyTemperatureRange = "ComfyTemperatureRange";

        public const string CopyCostume = "AwesomeInventory_CopyCostume";

        public const string CopyLoadout = "AwesomeInventory_CopyLoadout";

        public const string CopySuffix = "AwesomeInventory_CopySuffix";

        public const string Costume = "AwesomeInventory_Costume";

        public const string CostumeSettings = "AwesomeInventory_CostumeSettings";

        public const string CountFieldTip = "AwesomeInventory_CountFieldTip";

        public const string Consume = "AwesomeInventory_Consume";

        public const string CreateCostume = "AwesomeInventory_CreateCostume";

        public const string CurrentTemperature = "AwesomeInventory_CurrentTemperature";

        public const string CurrentHotSwapCostume = "AwesomeInventory_CurrentHotSwapCostume";

        public const string Customize = "AwesomeInventory_Customize";

        public const string DeleteCostume = "AwesomeInventory_DeleteCostume";

        public const string DeleteLoadout = "AwesomeInventory_DeleteLoadout";

        public const string DisableWindowInModSetting = "AwesomeInventory_DisableWindowInModSetting";

        public const string DontUseCostume = "AwesomeInventory_DontUseCostume";

        public const string DragToReorder = "AwesomeInventory_DragToReorder";

        public const string DropThingLocked = "DropThingLocked";

        public const string EquippedItems = "AwesomeInventory_EquippedItems";

        public const string ErrorReport = "AwesomeInventory_ErrorReport";

        public const string ExtraApparels = "AwesomeInventory_ExtraApparels";

        public const string ExplainFeatures = "AwesomeInventory_ExplainFeatures";

        public const string FailToDrop = "AwesomeInventory_FailToDrop";

        public const string ForceHotSwap = "AwesomeInventory_ForceHotSwap";

        public const string GearTabHeight = "AwesomeInventory_GearTabHeight";

        public const string GearTabWidth = "AwesomeInventory_GearTabWidht";

        public const string GlobalApparelSetting = "AwesomeInventory_GlobalApparelSetting";

        public const string GlobalOutfitSettingWarning = "AwesomeInventory_GlobalOutfitSettingWarning";

        public const string GoNuts = "AwesomeInventory_GoNuts";

        public const string GreedyTab = "AwesomeInventory_GreedyTab";

        public const string Head = "AwesomeInventory_Head";

        public const string InventoryItems = "AwesomeInventory_InventoryItems";

        public const string HotSwapInterrupted = "AwesomeInventory_HotSwapInterrupted";

        public const string TipsOnCostume = "AwesomeInventory_TipsOnCostume";

        public const string ItemsInCostume = "AwesomeInventory_ItemsInCostume";

        public const string ImportLoadout = "AwesomeInventory_ImportLoadout";

        public const string InventoryView = "AwesomeInventory_InventoryView";

        public const string ApparelsInLoadout = "AwesomeInventory_ApparelsInLoadout";

        public const string JealousTab = "AwesomeInventory_JealousTab";

        public const string Loadouts = "AwesomeInventory_Loadouts";

        public const string LoadoutName = "AwesomeInventory_LoadoutName";

        public const string LoadoutTabTip1 = "AwesomeInventory_LoadoutTabTip1";

        public const string LoadoutTabTip2 = "AwesomeInventory_LoadoutTabTip2";

        public const string LoadoutView = "AwesomeInventory_LoadoutView";

        public const string MakeEmptyLoadout = "AwesomeInventory_MakeEmptyLoadout";

        public const string MakeLoadout = "AwesomeInventory_MakeLoadout";

        public const string MakeLoadoutFrom = "AwesomeInventory_MakeLoadoutFrom";

        public const string MakeNewLoadout = "AwesomeInventory_MakeNewLoadout";

        public const string MissingItems = "AwesomeInventory_MissingItems";

        public const string MouseOverNumbersForDetails = "AwesomeInventory_MouseOverNumbersForDetails";

        public const string NewLoadout = "AwesomeInventory_NewLoadout";

        public const string NotCapableChangingApparel = "AwesomeInventory_NotCapaleChangingApparel";

        public const string NoCostumeAvailable = "AwesomeInventory_NoCostumeAvailable";

        public const string NoCostumeForHotSwap = "AwesomeInventory_NoCostumeForHotSwap";

        public const string NoCostumeSelected = "AwesomeInventory_NoCostumeSelected";

        public const string NoDrugSelected = "AwesomeInventory_NoDrugSelected";

        public const string NoDrugInInventory = "AwesomeInventory_NoDrugInInventory";

        public const string NoLoadout = "AwesomeInventory_NoLoadout";

        public const string NoLoadoutSelected = "AwesomeInventory_NoLoadoutSelected";

        public const string NoMaterial = "AwesomeInventory_NoMaterial";

        public const string OpenLoadout = "AwesomeInventory_OpenLoadout";

        public const string OpenLoadoutFromContextMenu = "AwesomeInventory_OpenLoadoutFromContextMenu";

        public const string OpenLoadoutFromContextMenuTooltip = "AwesomeInventory_OpenLoadoutFromContextMenuTooltip";

        public const string Pant = "AwesomeInventory_Pants";

        public const string PatchAllRaces = "AwesomeInventory_PatchAllRaces";

        public const string PatchAllRacesTooltip = "AwesomeInventory_PatchAllRacesTooltip";

        public const string Pickup = "AwesomeInventory_Pickup";

        public const string PrimaryWeapon = "AwesomeInventory_PrimaryWeapon";

        public const string PreviewQuality = "AwesomeInventory_PreviewQuality";

        public const string PreviewQualityTooltip = "AwesomeInventory_PreviewQuality_Tooltip";

        public const string PutAway = "AwesomeInventory_PutAway";

        public const string RemoveFromLoadout = "AwesomeInventory_RemoveFromLoadout";

        public const string RestockAlways = "AwesomeInventory_RestockAlways";

        public const string RestockBottomThreshold = "AwesomeInventory_RestockBottomThreshold";

        public const string RigthClickForMoreOptions = "AwesomeInventory_RightClickForMoreOptions";

        public const string SameCostumeForHotSwap = "AwesomeInventory_SameCostumeForHotSwap";

        public const string SelectCostume = "AwesomeInventory_SelectCostume";

        public const string SelectLoadout = "AwesomeInventory_SelectLoadout";

        public const string SelectedStuff = "AwesomeInventory_SelectorStuff";

        public const string Separate = "AwesomeInventory_Separate";

        public const string SeparateTooltip = "AwesomeInventory_Separate_Tooltip";

        public const string SortListAlphabetically = "AwesomeInventory_SortListAlphabetically";

        public const string SourceAllTip = "AwesomeInventory_SourceAllTip";

        public const string SourceFilterTip = "AwesomeInventory_SourceFilterTip";

        public const string SourceGenericTip = "AwesomeInventory_SourceGenericTip";

        public const string SourceMeleeTip = "AwesomeInventory_SourceMeleeTip";

        public const string SourceMinifiedTip = "AwesomeInventory_SourceMinifiedTip";

        public const string SourceRangedTip = "AwesomeInventory_SourceRangedTip";

        public const string StatPanel = "AwesomeInventory_StatPanel";

        public const string StockMode = "AwesomeInventory_StockMode";

        public const string StuffSource = "AwesomeInventory_StuffSource";

        public const string TipsForHotSwap = "AwesomeInventory_TipsForHotSwap";

        public const string ToggleGearTab = "AwesomeInventory_ToggleGearTab";

        public const string TorsoMiddleLayer = "AwesomeInventory_TorsoMiddle";

        public const string TorsoOnSkinLayer = "AwesomeInventory_TorsoOnSkin";

        public const string TorsoShellLayer = "AwesomeInventory_TorsoShell";

        public const string TryToDeleteLastLoadout = "AwesomeInventory_TryToDeleteLastLoadout";

        public const string Weight = "AwesomeInventory_Weight";

        public const string Wishlist = "AwesomeInventory_Wishlist";

        public const string WishlistTooltip = "AwesomeInventory_WishlistTooltip";

        public const string OutfitAnything = "OutfitAnything";

        public const string SourceApparelTip = "AwesomeInventory_SourceApparelTip";

        public const string TenCharsString = "AAAAAAAAAA";

        public const string Traits = "Traits";

        public const string EmptyLoadout = "AwesomeInventory_EmptyLoadout";

        public const string Tips = "AwesomeInventory_Tips";

        public const string UnloadNow = "AwesomeInventory_UnloadNow";

        public const string UseGearTabToggle = "AwesomeInventory_UseGearTabToggle";

        public const string UseGenericStuff = "AwesomeInventory_UseGenericStuff";

        public const string UseGearTabToggleTooltip = "AwesomeInventory_UseGearTabToggleTooltip";

        public const string UseHotSwap = "AwesomeInventory_UseHotSwap";

        public const string UseLoadout = "AwesomeInventory_UseLoadout";

        public const string UseLoadoutTooltip = "AwesomeInventory_UseLoadoutTooltip";

        public const string ShowRestartButton = "AwesomeInventory_ShowRestartButton";

        public const string ShowRestartButtonTooltip = "AwesomeInventory_ShowRestartButtonTooltip";

        public const string UseTakeDrug = "AwesomeInventory_UseTakeDrug";

        public const string UseTakeDrugTooltip = "AwesomeInventory_UseTakeDrugTooltip";

        public const string VanillaWear = "AwesomeInventory_VanillaWear";

        /***************** CE *************************/
        public const string Bulk = "CE_Bulk";

        public const string SelectedWeapon = "AwesomeInventory_SelectedWeapon";

        public const string SelectedAmmo = "AwesomeInventory_SelectedAmmo";

#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
#pragma warning restore SA1600 // Elements should be documented
    }
}
