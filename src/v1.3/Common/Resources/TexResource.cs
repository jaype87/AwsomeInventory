// <copyright file="TexResource.cs" company="Zizhen Li">
// Copyright (c) 2019 - 2020 Zizhen Li. All rights reserved.
// Licensed under the LGPL-3.0-only license. See LICENSE.md file in the project root for full license information.
// </copyright>

using UnityEngine;
using Verse;

namespace AwesomeInventory.UI
{
    /// <summary>
    /// Texture resources.
    /// </summary>
    [StaticConstructorOnStartup]
    public static class TexResource
    {
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable SA1600 // Elements should be documented

        public static readonly Texture2D ButtonBGAtlas = ContentFinder<Texture2D>.Get("UI/Widgets/ButtonBG");

        public static readonly Texture2D ButtonBGAtlasMouseover = ContentFinder<Texture2D>.Get("UI/Widgets/ButtonBGMouseover");

        public static readonly Texture2D ButtonBGAtlasClick = ContentFinder<Texture2D>.Get("UI/Widgets/ButtonBGClick");

        public static readonly Texture2D Checklist = ContentFinder<Texture2D>.Get("UI/Icons/CheckList");

        public static readonly Texture2D CloseXBig = ContentFinder<Texture2D>.Get("UI/Widgets/CloseX", true);

        public static readonly Texture2D CloseXSmall = ContentFinder<Texture2D>.Get("UI/Widgets/CloseXSmall", true);

        public static readonly Texture2D NextBig = ContentFinder<Texture2D>.Get("UI/Widgets/NextArrow", true);

        public static readonly Texture2D DeleteX = ContentFinder<Texture2D>.Get("UI/Buttons/Delete", true);

        public static readonly Texture2D ReorderUp = ContentFinder<Texture2D>.Get("UI/Buttons/ReorderUp", true);

        public static readonly Texture2D ReorderDown = ContentFinder<Texture2D>.Get("UI/Buttons/ReorderDown", true);

        public static readonly Texture2D Plus = ContentFinder<Texture2D>.Get("UI/Buttons/Plus", true);

        public static readonly Texture2D Minus = ContentFinder<Texture2D>.Get("UI/Buttons/Minus", true);

        public static readonly Texture2D Suspend = ContentFinder<Texture2D>.Get("UI/Buttons/Suspend", true);

        public static readonly Texture2D SelectOverlappingNext = ContentFinder<Texture2D>.Get("UI/Buttons/SelectNextOverlapping", true);

        public static readonly Texture2D Info = ContentFinder<Texture2D>.Get("UI/Buttons/InfoButton", true);

        public static readonly Texture2D Rename = ContentFinder<Texture2D>.Get("UI/Buttons/Rename", true);

        public static readonly Texture2D OpenStatsReport = ContentFinder<Texture2D>.Get("UI/Buttons/OpenStatsReport", true);

        public static readonly Texture2D Copy = ContentFinder<Texture2D>.Get("UI/Buttons/Copy", true);

        public static readonly Texture2D Paste = ContentFinder<Texture2D>.Get("UI/Buttons/Paste", true);

        public static readonly Texture2D Drop = ContentFinder<Texture2D>.Get("UI/Buttons/Drop", true);

        public static readonly Texture2D Ingest = ContentFinder<Texture2D>.Get("UI/Buttons/Ingest", true);

        public static readonly Texture2D DragHash = ContentFinder<Texture2D>.Get("UI/Buttons/DragHash", true);

        public static readonly Texture2D ToggleLog = ContentFinder<Texture2D>.Get("UI/Buttons/DevRoot/ToggleLog", true);

        public static readonly Texture2D OpenDebugActionsMenu = ContentFinder<Texture2D>.Get("UI/Buttons/DevRoot/OpenDebugActionsMenu", true);

        public static readonly Texture2D OpenInspector = ContentFinder<Texture2D>.Get("UI/Buttons/DevRoot/OpenInspector", true);

        public static readonly Texture2D OpenInspectSettings = ContentFinder<Texture2D>.Get("UI/Buttons/DevRoot/OpenInspectSettings", true);

        public static readonly Texture2D ToggleGodMode = ContentFinder<Texture2D>.Get("UI/Buttons/DevRoot/ToggleGodMode", true);

        public static readonly Texture2D TogglePauseOnError = ContentFinder<Texture2D>.Get("UI/Buttons/DevRoot/TogglePauseOnError", true);

        public static readonly Texture2D Add = ContentFinder<Texture2D>.Get("UI/Buttons/Dev/Add", true);

        public static readonly Texture2D NewItem = ContentFinder<Texture2D>.Get("UI/Buttons/Dev/NewItem", true);

        public static readonly Texture2D Reveal = ContentFinder<Texture2D>.Get("UI/Buttons/Dev/Reveal", true);

        public static readonly Texture2D Collapse = ContentFinder<Texture2D>.Get("UI/Buttons/Dev/Collapse", true);

        public static readonly Texture2D Empty = ContentFinder<Texture2D>.Get("UI/Buttons/Dev/Empty", true);

        public static readonly Texture2D Save = ContentFinder<Texture2D>.Get("UI/Buttons/Dev/Save", true);

        public static readonly Texture2D NewFile = ContentFinder<Texture2D>.Get("UI/Buttons/Dev/NewFile", true);

        public static readonly Texture2D RenameDev = ContentFinder<Texture2D>.Get("UI/Buttons/Dev/Rename", true);

        public static readonly Texture2D Reload = ContentFinder<Texture2D>.Get("UI/Buttons/Dev/Reload", true);

        public static readonly Texture2D Play = ContentFinder<Texture2D>.Get("UI/Buttons/Dev/Play", true);

        public static readonly Texture2D Stop = ContentFinder<Texture2D>.Get("UI/Buttons/Dev/Stop", true);

        public static readonly Texture2D RangeMatch = ContentFinder<Texture2D>.Get("UI/Buttons/Dev/RangeMatch", true);

        public static readonly Texture2D InspectModeToggle = ContentFinder<Texture2D>.Get("UI/Buttons/Dev/InspectModeToggle", true);

        public static readonly Texture2D CenterOnPointsTex = ContentFinder<Texture2D>.Get("UI/Buttons/Dev/CenterOnPoints", true);

        public static readonly Texture2D CurveResetTex = ContentFinder<Texture2D>.Get("UI/Buttons/Dev/CurveReset", true);

        public static readonly Texture2D QuickZoomHor1Tex = ContentFinder<Texture2D>.Get("UI/Buttons/Dev/QuickZoomHor1", true);

        public static readonly Texture2D QuickZoomHor100Tex = ContentFinder<Texture2D>.Get("UI/Buttons/Dev/QuickZoomHor100", true);

        public static readonly Texture2D QuickZoomHor20kTex = ContentFinder<Texture2D>.Get("UI/Buttons/Dev/QuickZoomHor20k", true);

        public static readonly Texture2D QuickZoomVer1Tex = ContentFinder<Texture2D>.Get("UI/Buttons/Dev/QuickZoomVer1", true);

        public static readonly Texture2D QuickZoomVer100Tex = ContentFinder<Texture2D>.Get("UI/Buttons/Dev/QuickZoomVer100", true);

        public static readonly Texture2D QuickZoomVer20kTex = ContentFinder<Texture2D>.Get("UI/Buttons/Dev/QuickZoomVer20k", true);

        public static readonly Texture2D IconBlog = ContentFinder<Texture2D>.Get("UI/HeroArt/WebIcons/Blog", true);

        public static readonly Texture2D IconForums = ContentFinder<Texture2D>.Get("UI/HeroArt/WebIcons/Forums", true);

        public static readonly Texture2D IconTwitter = ContentFinder<Texture2D>.Get("UI/HeroArt/WebIcons/Twitter", true);

        public static readonly Texture2D IconBook = ContentFinder<Texture2D>.Get("UI/HeroArt/WebIcons/Book", true);

        public static readonly Texture2D IconSoundtrack = ContentFinder<Texture2D>.Get("UI/HeroArt/WebIcons/Soundtrack", true);

        public static readonly Texture2D ShowLearningHelper = ContentFinder<Texture2D>.Get("UI/Buttons/ShowLearningHelper", true);

        public static readonly Texture2D ShowZones = ContentFinder<Texture2D>.Get("UI/Buttons/ShowZones", true);

        public static readonly Texture2D ShowBeauty = ContentFinder<Texture2D>.Get("UI/Buttons/ShowBeauty", true);

        public static readonly Texture2D ShowColonistBar = ContentFinder<Texture2D>.Get("UI/Buttons/ShowColonistBar", true);

        public static readonly Texture2D ShowRoofOverlay = ContentFinder<Texture2D>.Get("UI/Buttons/ShowRoofOverlay", true);

        public static readonly Texture2D AutoHomeArea = ContentFinder<Texture2D>.Get("UI/Buttons/AutoHomeArea", true);

        public static readonly Texture2D CategorizedResourceReadout = ContentFinder<Texture2D>.Get("UI/Buttons/ResourceReadoutCategorized", true);

        public static readonly Texture2D LockNorthUp = ContentFinder<Texture2D>.Get("UI/Buttons/LockNorthUp", true);

        public static readonly Texture2D Gear = ContentFinder<Texture2D>.Get("UI/Icons/Gear", true);

        public static readonly Texture2D RWButtonBG = ContentFinder<Texture2D>.Get("UI/Widgets/ButtonBG");

        // Custom icons
        public static readonly Texture2D Apparel = ContentFinder<Texture2D>.Get("UI/Icons/Apparel", true);

        public static readonly Texture2D ArrowBottom = ContentFinder<Texture2D>.Get("UI/Icons/RPGI_Loadout/arrowBottom");

        public static readonly Texture2D ArrowDown = ContentFinder<Texture2D>.Get("UI/Icons/RPGI_Loadout/arrowDown");

        public static readonly Texture2D ArrowTop = ContentFinder<Texture2D>.Get("UI/Icons/RPGI_Loadout/arrowTop");

        public static readonly Texture2D ArrowUp = ContentFinder<Texture2D>.Get("UI/Icons/RPGI_Loadout/arrowUp");

        public static readonly Texture2D Costume = ContentFinder<Texture2D>.Get("UI/Icons/Costume");

        public static readonly Texture2D DarkBackground = SolidColorMaterials.NewSolidColorTexture(0f, 0f, 0f, .2f);

        public static readonly Texture2D DoubleDownArrow = ContentFinder<Texture2D>.Get("UI/Icons/DoubleDownArrow");

        public static readonly Texture2D GearHelmet = ContentFinder<Texture2D>.Get("UI/Icons/Gear_Helmet_Colored");

        public static readonly Texture2D GenericTransform = ContentFinder<Texture2D>.Get("UI/Icons/GenericTransform");

        public static readonly Texture2D Getup = ContentFinder<Texture2D>.Get("UI/Icons/Getup");

        public static readonly Texture2D ChangeClothActive = ContentFinder<Texture2D>.Get("UI/Icons/ChangeClothActive");

        public static readonly Texture2D ChangeClothInactive = ContentFinder<Texture2D>.Get("UI/Icons/ChangeClothInactive");

        public static readonly Texture2D ChangeClothInterrupted = ContentFinder<Texture2D>.Get("UI/Icons/ChangeClothInterrupted");

        public static readonly Texture2D IconEdit = ContentFinder<Texture2D>.Get("UI/Icons/RPGI_Loadout/edit");

        public static readonly Texture2D IconClear = ContentFinder<Texture2D>.Get("UI/Icons/RPGI_Loadout/clear");

        public static readonly Texture2D IconAmmo = ContentFinder<Texture2D>.Get("UI/Icons/RPGI_Loadout/ammo");

        public static readonly Texture2D IconRanged = ContentFinder<Texture2D>.Get("UI/Icons/RPGI_Loadout/ranged");

        public static readonly Texture2D IconMelee = ContentFinder<Texture2D>.Get("UI/Icons/RPGI_Loadout/melee");

        public static readonly Texture2D IconMinified = ContentFinder<Texture2D>.Get("UI/Icons/RPGI_Loadout/minified");

        public static readonly Texture2D IconGeneric = ContentFinder<Texture2D>.Get("UI/Icons/RPGI_Loadout/generic");

        public static readonly Texture2D IconAll = ContentFinder<Texture2D>.Get("UI/Icons/RPGI_Loadout/all");

        public static readonly Texture2D IconAmmoAdd = ContentFinder<Texture2D>.Get("UI/Icons/RPGI_Loadout/ammoAdd");

        public static readonly Texture2D IconSearch = ContentFinder<Texture2D>.Get("UI/Icons/RPGI_Loadout/search");

        public static readonly Texture2D IconMove = ContentFinder<Texture2D>.Get("UI/Icons/RPGI_Loadout/move");

        public static readonly Texture2D IconPickupDrop = ContentFinder<Texture2D>.Get("UI/Icons/RPGI_Loadout/loadoutPickupDrop");

        public static readonly Texture2D IconDropExcess = ContentFinder<Texture2D>.Get("UI/Icons/RPGI_Loadout/loadoutDropExcess");

        public static readonly Texture2D IconTainted = ContentFinder<Texture2D>.Get("UI/Icons/Sandy_Tainted_Icon");

        public static readonly Texture2D IconForced = ContentFinder<Texture2D>.Get("UI/Icons/Sandy_Forced_Icon");

        public static readonly Texture2D IconLock = ContentFinder<Texture2D>.Get("UI/Icons/LockedLock");

        public static readonly Texture2D ImportLoadout = ContentFinder<Texture2D>.Get("UI/Icons/ImportLoadout");

        public static readonly Texture2D Mass = ContentFinder<Texture2D>.Get("UI/Icons/Sandy_MassCarried_Icon");

        public static readonly Texture2D MaxTemperature = ContentFinder<Texture2D>.Get("UI/Icons/Fire_Mini");

        public static readonly Texture2D MinTemperature = ContentFinder<Texture2D>.Get("UI/Icons/min_temperature");

        public static readonly Texture2D NoDrug = ContentFinder<Texture2D>.Get("UI/Icons/NoDrug");

        public static readonly Texture2D ArmorBlunt = ContentFinder<Texture2D>.Get("UI/Icons/Sandy_ArmorBlunt_Icon");

        public static readonly Texture2D ArmorSharp = ContentFinder<Texture2D>.Get("UI/Icons/Sandy_ArmorSharp_Icon");

        public static readonly Texture2D ArmorHeat = ContentFinder<Texture2D>.Get("UI/Icons/Sandy_ArmorHeat_Icon");

        public static readonly Texture2D Resize = ContentFinder<Texture2D>.Get("UI/Icons/Resize");

        public static readonly Texture2D SortLetterA = ContentFinder<Texture2D>.Get("UI/Icons/SortLetterA");

        public static readonly Texture2D ThermometerHot = ContentFinder<Texture2D>.Get("UI/Icons/Hot_Thermometer");

        public static readonly Texture2D ThermometerCold = ContentFinder<Texture2D>.Get("UI/Icons/Cold_Thermometer");

        public static readonly Texture2D ThermometerGen = ContentFinder<Texture2D>.Get("UI/Icons/Gen_Thermometer");

        public static readonly Texture2D Threshold = ContentFinder<Texture2D>.Get("UI/Icons/ThresholdMini");

        public static readonly Texture2D TriangleLeft = ContentFinder<Texture2D>.Get("UI/Icons/LeftTriangle");

        public static readonly Texture2D TriangleRight = ContentFinder<Texture2D>.Get("UI/Icons/RightTriangle");

#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
#pragma warning restore SA1600 // Elements should be documented
    }
}
