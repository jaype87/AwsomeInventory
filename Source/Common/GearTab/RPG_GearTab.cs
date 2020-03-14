// <copyright file="RPG_GearTab.cs" company="Zizhen Li">
// Copyright (c) Zizhen Li. All rights reserved.
// Licensed under the GPL-3.0-only license. See LICENSE.md file in the project root for full license information.
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using RimWorld;
using RPGIResource;
using UnityEngine;
using Verse;

namespace AwesomeInventory.Common
{
    /// <summary>
    /// Replace RimWorld's default gear tab.
    /// </summary>
    public class RPG_GearTab : ITab_Pawn_Gear
    {
        // TODO set a static constructor to queue up unload jobs after game restarts
        private static bool _isJealous = true;
        private static bool _isGreedy = false;
        private static bool _isAscetic = false;

        private IDrawGearTab _drawGearTab;
        private PawnModal _selPawn;

        /// <summary>
        /// Initializes a new instance of the <see cref="RPG_GearTab"/> class.
        /// </summary>
        public RPG_GearTab()
        {
            this.size = new Vector2(550f, 500f);
            this.labelKey = "TabGear";
            this.tutorTag = "Gear";

            // Create instance of IDrawGearTab based on loaded assembly.
            IEnumerable<Assembly> assemblies
                = AppDomain.CurrentDomain
                    .GetAssemblies()
                    .Where(assembly => assembly.GetName().Name.Contains("AwesomeInventory"));

            foreach (Assembly assembly in assemblies)
            {
                Type drawGearTab = assembly.GetTypes().First(t => typeof(IDrawGearTab).IsAssignableFrom(t));
                _drawGearTab = (IDrawGearTab)Activator.CreateInstance(drawGearTab);
            }
        }

        /// <summary>
        /// Gets a value indicating whether the tab should be displayed.
        /// </summary>
        public override bool IsVisible
        {
            get
            {
                if (this.SelPawn != _selPawn?.Pawn)
                {
                    FillTabOperations.ScrollPosition = Vector2.zero;
                }

                _selPawn = new PawnModal(SelPawn, SelThing);
                return Utility.ShouldShowInventory(_selPawn.Pawn) ||
                       Utility.ShouldShowApparel(_selPawn.Pawn) ||
                       Utility.ShouldShowEquipment(_selPawn.Pawn);
            }
        }

        public override void OnOpen()
        {
            base.OnOpen();
            FillTabOperations.ScrollPosition = Vector2.zero;
        }

        /// <summary>
        /// It is called right before the tab is drawn.
        /// </summary>
        protected override void UpdateSize()
        {
        }

        /// <summary>
        /// Draw the tab.
        /// </summary>
        protected override void FillTab()
        {
            _selPawn = _selPawn ?? new PawnModal(this.SelPawn, this.SelThing);
            Text.Font = GameFont.Small;
            GUI.color = Color.white;

            // Draw checkbox option for Jealous
            string translatedText = UIText.JealousTab.Translate();
            Rect headerRect = GetHeaderRect(GenUI.Gap, translatedText);
            if (Widgets.RadioButtonLabeled(headerRect, translatedText, _isJealous))
            {
                _isGreedy = _isAscetic = false;
                _isJealous = true;
            }

            // Draw checkbox option for Greedy
            translatedText = UIText.GreedyTab.Translate();
            headerRect = GetHeaderRect(headerRect.xMax + GenUI.GapWide, translatedText);
            if (Widgets.RadioButtonLabeled(headerRect, translatedText, _isGreedy))
            {
                _isJealous = _isAscetic = false;
                _isGreedy = true;
            }

            // Draw checkbox option for Ascetic
            translatedText = UIText.AsceticTab.Translate();
            headerRect = GetHeaderRect(headerRect.xMax + GenUI.GapWide, translatedText);
            if (Widgets.RadioButtonLabeled(headerRect, translatedText, _isAscetic))
            {
                _isJealous = _isGreedy = false;
                _isAscetic = true;
            }

            if (_isJealous)
            {
                _drawGearTab.DrawJealous(_selPawn, new Rect(0, headerRect.yMax, size.x, size.y - headerRect.yMax));
            }
            else if (_isGreedy)
            {
                _drawGearTab.DrawGreedy(_selPawn, new Rect(0, headerRect.yMax, size.x, size.y - headerRect.yMax));
            }
            else if (_isAscetic)
            {
            }
            else
            {
                throw new InvalidOperationException(Resources.ErrorMessage.NoDisplayOptionChosen);
            }
        }

        private Rect GetHeaderRect(float x, string translatedText)
        {
            float width = GenUI.GetWidthCached(translatedText) + Widgets.RadioButtonSize + GenUI.GapSmall;
            return new Rect(x, GenUI.GapSmall, width, GenUI.ListSpacing);
        }
    }
}
