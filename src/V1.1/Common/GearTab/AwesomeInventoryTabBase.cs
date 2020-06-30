// <copyright file="AwesomeInventoryTabBase.cs" company="Zizhen Li">
// Copyright (c) 2019 - 2020 Zizhen Li. All rights reserved.
// Licensed under the LGPL-3.0-only license. See LICENSE.md file in the project root for full license information.
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using AwesomeInventory.HarmonyPatches;
using RimWorld;
using UnityEngine;
using Verse;

namespace AwesomeInventory.UI
{
    /// <summary>
    /// Replace RimWorld's default gear tab.
    /// </summary>
    public abstract class AwesomeInventoryTabBase : ITab_Pawn_Gear
    {
        /// <summary>
        /// Gets or sets a value indicating whether apparel has changed.
        /// </summary>
        protected bool _apparelChanged;

        /// <summary>
        /// Draw contents in gear tab.
        /// </summary>
        protected IDrawGearTab _drawGearTab;

        private const BindingFlags _nonPublicInstance = BindingFlags.NonPublic | BindingFlags.Instance;

        private static bool _isJealous = true;
        private static bool _isGreedy = false;
        private static bool _isAscetic = false;
        private readonly IEnumerable<CustomRace> _customRaces
            = AwesomeInventoryServiceProvider.GetPluginsOfType<CustomRace>();

        private readonly object _apparelChangedLock = new object();
        private readonly StatPanelToggle _statPanelToggle = new StatPanelToggle();
        private Pawn _selPawn;

        /// <summary>
        /// Initializes a new instance of the <see cref="AwesomeInventoryTabBase"/> class.
        /// </summary>
        public AwesomeInventoryTabBase()
        {
            this.labelKey = "TabGear";
            this.tutorTag = "Gear";
            Pawn_ApparelTracker_ApparelChanged_Patch.ApparelChangedEvent +=
                (bool changed) =>
                {
                    lock (_apparelChangedLock)
                    {
                        _apparelChanged = changed;
                    }
                };
        }

        /// <summary>
        /// Gets InterfaceDrop method from base class.
        /// </summary>
        public static MethodInfo InterfaceDrop { get; } =
            typeof(ITab_Pawn_Gear).GetMethod("InterfaceDrop", _nonPublicInstance);

        /// <summary>
        /// Gets InterfaceIngest method from base class.
        /// </summary>
        public static MethodInfo InterfaceIngest { get; } =
            typeof(ITab_Pawn_Gear).GetMethod("InterfaceIngest", _nonPublicInstance);

        /// <summary>
        /// Gets ShouldShowInventory method from base class.
        /// </summary>
        public static MethodInfo ShouldShowInventory { get; } =
            typeof(ITab_Pawn_Gear).GetMethod("ShouldShowInventory", _nonPublicInstance);

        /// <summary>
        /// Gets ShouldShowApparel method from base class.
        /// </summary>
        public static MethodInfo ShouldShowApparel { get; } =
            typeof(ITab_Pawn_Gear).GetMethod("ShouldShowApparel", _nonPublicInstance);

        /// <summary>
        /// Gets ShouldShowEquipment method from base class.
        /// </summary>
        public static MethodInfo ShouldShowEquipment { get; } =
            typeof(ITab_Pawn_Gear).GetMethod("ShouldShowEquipment", _nonPublicInstance);

        /// <summary>
        /// Gets CanControlColonist property from base class.
        /// </summary>
        public static PropertyInfo CanControlColonist { get; } =
            typeof(ITab_Pawn_Gear).GetProperty("CanControlColonist", _nonPublicInstance);

        /// <summary>
        /// Gets CanControl property from base class.
        /// </summary>
        public static PropertyInfo CanControl { get; } =
            typeof(ITab_Pawn_Gear).GetProperty("CanControl", _nonPublicInstance);

        /// <summary>
        /// Gets private property SelPawnForGear from <see cref="ITab_Pawn_Gear"/>.
        /// </summary>
        protected static PropertyInfo GetPawn { get; } =
            typeof(ITab_Pawn_Gear).GetProperty("SelPawnForGear", _nonPublicInstance);

        /// <summary>
        /// Run only once when the tab is toggle to open.
        /// Details in <see cref="InspectPaneUtility"/>.ToggleTab .
        /// </summary>
        /// <remarks>
        ///     The same instance is used when switch pawns with tab open.
        /// </remarks>
        public override void OnOpen()
        {
            base.OnOpen();
            _drawGearTab.Reset();
            _statPanelToggle.SetPosition(new Vector2(this.TabRect.xMax, this.TabRect.y));
        }

        /// <inheritdoc />
        public override void TabUpdate()
        {
            base.TabUpdate();
            _statPanelToggle.SetPosition(new Vector2(this.TabRect.xMax, this.TabRect.y));
        }

        /// <summary>
        /// Check if selected pawn is a colonist.
        /// </summary>
        /// <returns> Returns true if selected pawn is a colonist. </returns>
        public bool IsColonist()
        {
            return _selPawn.IsColonist || (_customRaces.Any() && _customRaces.All(r => r.IsColonist(_selPawn)));
        }

        /// <summary>
        /// Check if selected pawn is a colonist and player controlled.
        /// </summary>
        /// <returns> Returns true if selected pawn is a colonist and can be controlled by players. </returns>
        public bool IsColonistPlayerControlled()
        {
            return _selPawn.IsColonistPlayerControlled || (_customRaces.Any() && _customRaces.All(r => r.IsColonistPlayerControlled(_selPawn)));
        }

        /// <summary>
        /// It is called right before the tab is drawn.
        /// </summary>
        protected override void UpdateSize()
        {
            this.size = new Vector2(AwesomeInventoryMod.Settings.GearTabWidth, AwesomeInventoryMod.Settings.GearTabHeight);
        }

        /// <summary>
        /// Draw the tab.
        /// </summary>
        protected override void FillTab()
        {
            Pawn selPawn = GetPawn.GetValue((ITab_Pawn_Gear)this) as Pawn;

            // Reset scroll position if a new pawn is selected.
            if (_selPawn != selPawn)
            {
                _selPawn = selPawn;
                _drawGearTab.Reset();
                lock (_apparelChangedLock)
                {
                    _apparelChanged = true;
                }
            }

            Text.Font = GameFont.Small;
            GUI.color = Color.white;

            string translatedText;
            Rect headerRect = Rect.zero;

            // Draw checkbox option for Jealous
            if (_selPawn.RaceProps.Humanlike)
            {
                translatedText = UIText.JealousTab.TranslateSimple();
                headerRect = GetHeaderRect(GenUI.Gap, translatedText);
                if (Widgets.RadioButtonLabeled(headerRect, translatedText, _isJealous))
                {
                    this.SetJealous();
                }
            }
            else
            {
                this.SetGreedy();
            }

            // Draw checkbox option for Greedy
            translatedText = UIText.GreedyTab.TranslateSimple();
            headerRect = GetHeaderRect(headerRect.xMax + GenUI.Gap, translatedText);
            if (Widgets.RadioButtonLabeled(headerRect, translatedText, _isGreedy))
            {
                this.SetGreedy();
            }

            headerRect.Set(size.x - GenUI.ListSpacing * 2, headerRect.y + headerRect.height / 2 - GenUI.SmallIconSize / 2, GenUI.SmallIconSize, GenUI.SmallIconSize);
            if (Widgets.ButtonImage(headerRect, TexResource.Gear))
            {
                Find.WindowStack.Add(new Dialog_Settings());
            }

            if (Widgets.ButtonImage(headerRect.ReplaceX(headerRect.x - GenUI.SmallIconSize), TexResource.Checklist))
            {
                Find.WindowStack.Add(new Dialog_InventoryOverview());
            }

            /*
                // Draw checkbox option for Ascetic
                translatedText = UIText.AsceticTab.TranslateSimple();
                headerRect = GetHeaderRect(headerRect.xMax + GenUI.Gap, translatedText);
                if (Widgets.RadioButtonLabeled(headerRect, translatedText, _isAscetic))
                {
                    _isJealous = _isGreedy = false;
                    _isAscetic = true;
                }
            */

            if (Event.current.type != EventType.Layout)
            {
                lock (_apparelChangedLock)
                {
                    Rect canvas = new Rect(0, headerRect.yMax, size.x, size.y - headerRect.yMax).ContractedBy(GenUI.GapSmall);
                    GUI.BeginGroup(canvas);
                    Rect outRect = canvas.AtZero();
                    if (_isJealous)
                    {
                        _drawGearTab.DrawJealous(_selPawn, outRect, _apparelChanged);
                    }
                    else if (_isGreedy)
                    {
                        _drawGearTab.DrawGreedy(_selPawn, outRect, _apparelChanged);
                    }
                    else if (_isAscetic)
                    {
                    }
                    else
                    {
                        throw new InvalidOperationException(ErrorMessage.NoDisplayOptionChosen);
                    }

                    GUI.EndGroup();
                    _apparelChanged = false;
                }
            }

            _statPanelToggle.Draw();
        }

        private Rect GetHeaderRect(float x, string translatedText)
        {
            float width = GenUI.GetWidthCached(translatedText) + Widgets.RadioButtonSize + GenUI.GapSmall;
            return new Rect(x, GenUI.GapSmall, width, GenUI.ListSpacing);
        }

        private void SetJealous()
        {
            _isJealous = true;
            _isGreedy = _isAscetic = false;
        }

        private void SetGreedy()
        {
            _isGreedy = true;
            _isJealous = _isAscetic = false;
        }
    }
}
