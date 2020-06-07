// <copyright file="Dialog_CopyToCostume.cs" company="Zizhen Li">
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
    /// A dialog window handles copying costume to other loadouts.
    /// </summary>
    public class Dialog_CopyToCostume : Window
    {
        private static readonly float _columnWidth = UIText.TenCharsString.Times(2).GetWidthCached();

        private List<AwesomeInventoryCostume> _costumes;

        private Checklist<AwesomeInventoryCostume> _costumeList;

        private Checklist<ThingGroupSelector> _costumeItemList;

        private AwesomeInventoryCostume _selectedCostume;

        /// <inheritdoc/>
        public override Vector2 InitialSize { get; }
            = new Vector2(
                _columnWidth + Listing.ColumnSpacing + DrawUtility.WindowPadding,
                Verse.UI.screenHeight / 2 + DrawUtility.WindowPadding);

        /// <inheritdoc/>
        public override void PreOpen()
        {
            base.PreOpen();
            _costumes = LoadoutManager.Loadouts.OfType<AwesomeInventoryCostume>().ToList();
            List<ChecklistItem<AwesomeInventoryCostume>> items = new List<ChecklistItem<AwesomeInventoryCostume>>();
            foreach (AwesomeInventoryCostume costume in _costumes)
            {
                ChecklistItem<AwesomeInventoryCostume> item =
                    new ChecklistItem<AwesomeInventoryCostume>(
                        costume.label
                        , costume
                        , GenUI.ListSpacing
                        , _columnWidth
                        , false)
                    {
                        Draw = (pos, cos) =>
                        {
                            Rect labelRect = new Rect(pos, new Vector2(_columnWidth - GenUI.ListSpacing * 2, GenUI.ListSpacing));
                            Rect checkRect = new Rect(labelRect.xMax, pos.y, GenUI.ListSpacing * 2, GenUI.ListSpacing);

                            Widgets.Label(labelRect, costume.label);
                            if (Widgets.ButtonImageWithBG(checkRect, TexResource.TriangleRight, new Vector2(GenUI.SmallIconSize, GenUI.SmallIconSize)))
                            {
                                _selectedCostume = costume;
                                _costumeItemList = new Checklist<ThingGroupSelector>(
                                    costume.Select(
                                        g => new ChecklistItem<ThingGroupSelector>(g.LabelCapNoCount, g, GenUI.ListSpacing, _columnWidth, false))
                                    .ToList()
                                    , false
                                    , true
                                    , true);
                            }

                            return labelRect;
                        },
                    };
                items.Add(item);
            }

            _costumeList = new Checklist<AwesomeInventoryCostume>(items, true, true);
        }

        /// <inheritdoc/>
        public override void DoWindowContents(Rect inRect)
        {
            Rect rect = inRect.ReplaceWidth(_columnWidth);
            Rect labelRect = rect.ReplaceHeight(GenUI.ListSpacing);
            Rect listRect = rect.ReplacexMin(labelRect.yMax);

            if (_selectedCostume == null)
            {
                Widgets.Label(labelRect, "Available Costumes");
                _costumeList.Draw(listRect);
            }
            else
            {
                Widgets.Label(labelRect, "Items in costume");
                _costumeItemList.Draw(listRect);
            }

            labelRect = labelRect.ReplaceX(labelRect.xMax + Listing.ColumnSpacing);
            listRect = listRect.ReplaceX(labelRect.x);

            Widgets.Label(labelRect, "Select loadouts");
            _costumeList.Draw(listRect);
        }
    }
}
