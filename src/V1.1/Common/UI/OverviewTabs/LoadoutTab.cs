// <copyright file="LoadoutTab.cs" company="Zizhen Li">
// Copyright (c) 2019 - 2020 Zizhen Li. All rights reserved.
// Licensed under the LGPL-3.0-only license. See LICENSE.md file in the project root for full license information.
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AwesomeInventory.Loadout;
using UnityEngine;
using Verse;

namespace AwesomeInventory.UI
{
    /// <summary>
    /// A tab for summary on loadouts.
    /// </summary>
    public class LoadoutTab : OverviewTab
    {
        private static DrawHelper _drawHelper;

        private float _pawnNameWidth = UIText.TenCharsString.Times(2).GetWidthCached();
        private List<Pawn> _colonist = new List<Pawn>();
        private List<PawnRowViewModel> _pawnRowScrollPos = new List<PawnRowViewModel>();
        private Vector2 _tabScrollPos = Vector2.zero;
        private float _tabHeight = 0;

        static LoadoutTab()
        {
            if (!AwesomeInventoryServiceProvider.TryGetImplementation(out _drawHelper))
                Log.Error("No implementation for DrawHelper");
        }

        /// <inheritdoc/>
        public override string Label { get; } = UIText.Loadouts.TranslateSimple();

        /// <inheritdoc/>
        public override void DoTabContent(Rect rect)
        {
            Widgets.BeginScrollView(rect, ref _tabScrollPos, rect.ReplaceHeight(_tabHeight));
            Rect rowRect = rect.ReplaceHeight(GenUI.ListSpacing * 3);
            for (int i = 0; i < _colonist.Count; i++)
            {
                this.DrawTabRow(rowRect, _colonist[i], _pawnRowScrollPos[i]);
                rowRect = rowRect.ReplaceY(rowRect.yMax);
            }

            Widgets.EndScrollView();
        }

        /// <inheritdoc/>
        public override void Init()
        {
            _colonist = Find.Maps.SelectMany(map => map.mapPawns.FreeColonists).ToList();
            _pawnRowScrollPos = _colonist.Select(c => new PawnRowViewModel()).ToList();
            _tabScrollPos = Vector2.zero;
            _tabHeight = _colonist.Count * GenUI.ListSpacing * 3;
        }

        private void DrawTabRow(Rect rect, Pawn pawn, PawnRowViewModel viewModel)
        {
            Rect nameRect = rect.ReplaceWidth(_pawnNameWidth).ReplaceHeight(GenUI.ListSpacing);
            Widgets.Label(nameRect, pawn.NameFullColored);
            Widgets.Label(nameRect.ReplaceY(nameRect.yMax), pawn.LabelNoCountColored);

            float width = (rect.width - _pawnNameWidth) / 2;
            AwesomeInventoryLoadout loadout = pawn.GetLoadout();
            if (loadout != null)
            {
                Rect loadoutNameRect = new Rect(nameRect.xMax, rect.y, width, GenUI.ListSpacing);
                Widgets.Label(loadoutNameRect, loadout?.label ?? string.Empty);

                Rect loadoutItemRect = loadoutNameRect.ReplaceY(loadoutNameRect.yMax).ReplaceHeight(GenUI.ListSpacing + GenUI.ScrollBarWidth);
                Rect viewRect = loadoutItemRect.ReplaceHeight(GenUI.ListSpacing).ReplaceWidth(viewModel.LoadoutViewWidth);
                Widgets.ScrollHorizontal(loadoutItemRect, ref viewModel.LoadoutScrollPos, viewRect);
                Widgets.BeginScrollView(loadoutItemRect, ref viewModel.LoadoutScrollPos, viewRect);
                Rect iconRect = loadoutItemRect.ReplaceWidth(GenUI.SmallIconSize).ReplaceHeight(GenUI.ListSpacing);
                foreach (Thing thing in loadout.Select(s => s.SingleThingSelectors.FirstOrDefault()?.ThingSample))
                {
                    Widgets.ThingIcon(iconRect, thing);
                    Widgets.DrawHighlightIfMouseover(iconRect);
                    TooltipHandler.TipRegion(iconRect, _drawHelper.TooltipTextFor(thing, false));
                    iconRect = iconRect.ReplaceX(iconRect.xMax);
                }

                viewModel.LoadoutViewWidth = iconRect.x;

                Widgets.EndScrollView();
            }
        }

        private class PawnRowViewModel
        {
            public Vector2 LoadoutScrollPos = Vector2.zero;

            public float LoadoutViewWidth = 0;

            public Vector2 MissingScrollPos = Vector2.zero;

            public float MissingViewWidth = 0;
        }
    }
}
