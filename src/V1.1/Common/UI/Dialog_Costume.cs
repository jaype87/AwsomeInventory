// <copyright file="Dialog_Costume.cs" company="Zizhen Li">
// Copyright (c) 2019 - 2020 Zizhen Li. All rights reserved.
// Licensed under the LGPL-3.0-only license. See LICENSE.md file in the project root for full license information.
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AwesomeInventory.Loadout;
using RimWorld;
using UnityEngine;
using Verse;

namespace AwesomeInventory.UI
{
    /// <summary>
    /// Dialog window for setting up getup.
    /// </summary>
    public class Dialog_Costume : Window
    {
        private AwesomeInventoryLoadout _loadout;
        private AwesomeInventoryCostume _costume;
        private List<ThingGroupSelector> _apparelInLoadout;
        private Pawn _pawn;
        private Vector2 _loadoutScrollPos;
        private Vector2 _getupdetailScrollPos;

        /// <summary>
        /// Initializes a new instance of the <see cref="Dialog_Costume"/> class.
        /// </summary>
        /// <param name="loadout"> loadout that getups are dervied from. </param>
        /// <param name="pawn"> The pawn who <paramref name="loadout"/> is assigned to. </param>
        public Dialog_Costume(AwesomeInventoryLoadout loadout, Pawn pawn)
        {
            ValidateArg.NotNull(loadout, nameof(loadout));
            ValidateArg.NotNull(pawn, nameof(pawn));

            if (loadout is AwesomeInventoryCostume costume)
            {
                _loadout = costume.Base;
                _costume = costume;
            }
            else
            {
                _loadout = loadout;
            }

            _apparelInLoadout = _loadout.ThingGroupSelectors
                .Where(t => t.AllowedThing.IsApparel)
                .OrderBy(t => t.LabelCapNoCount)
                .ToList();

            _pawn = pawn;

            doCloseX = true;
            absorbInputAroundWindow = true;
            closeOnClickedOutside = true;
            resizeable = true;
            draggable = true;
        }

        /// <summary>
        /// Gets initial size for the window.
        /// </summary>
        public override Vector2 InitialSize => new Vector2(AwesomeInventoryMod.Settings.CostumeWindowWidth, AwesomeInventoryMod.Settings.CostumeWindowHeight);

        /// <summary>
        /// Invoked once before the window is closed.
        /// </summary>
        public override void PreClose()
        {
            base.PreClose();
            if (_costume != null && _pawn.outfits.CurrentOutfit != _costume)
            {
                _pawn.outfits.CurrentOutfit = _costume;
                if (BetterPawnControlUtility.IsActive)
                    BetterPawnControlUtility.SaveState(new List<Pawn> { _pawn });
            }

            AwesomeInventoryMod.Settings.CostumeWindowHeight = windowRect.height;
            AwesomeInventoryMod.Settings.CostumeWindowWidth = windowRect.width;
            AwesomeInventoryMod.Settings.Write();
        }

        /// <summary>
        /// Draw content for getup.
        /// </summary>
        /// <param name="inRect"> Rect for drawing. </param>
        public override void DoWindowContents(Rect inRect)
        {
            Rect innerRect = inRect.ContractedBy(DrawUtility.CurrentPadding);
            Rect headerRect = innerRect.ReplaceHeight(GenUI.ListSpacing);
            Rect firstColumn = innerRect.ReplaceyMin(GenUI.ListSpacing + GenUI.GapSmall).ReplaceWidth(innerRect.width / 2 - GenUI.ScrollBarWidth);
            Rect secondColumn = firstColumn.ReplaceX(firstColumn.xMax + GenUI.ScrollBarWidth);

            this.DrawHeader(headerRect);
            this.DrawItemsInLoadout(firstColumn);
            this.DrawItemsInCostume(secondColumn);
            Text.WordWrap = true;
        }

        private static void DrawNoCostumeWindow()
        {
            Find.WindowStack.Add(
                new Dialog_InstantMessage(
                    UIText.NoCostumeSelected.TranslateSimple()
                    , new Vector2(300, 200))
                {
                });
        }

        private void DrawHeader(Rect canvas)
        {
            WidgetRow widgetRow = new WidgetRow(canvas.x, canvas.y, UIDirection.RightThenDown);
            if (widgetRow.ButtonText(UIText.CreateCostume.TranslateSimple()))
            {
                AwesomeInventoryCostume costume = new AwesomeInventoryCostume(_loadout);

                _costume = costume;
                _loadout.Costumes.Add(costume);
                LoadoutManager.AddLoadout(costume);
            }

            if (widgetRow.ButtonText(UIText.CopyCostume.TranslateSimple()))
            {
                if (_costume != null)
                {
                    AwesomeInventoryCostume costume = new AwesomeInventoryCostume(_costume);

                    _costume = costume;
                    _loadout.Costumes.Add(costume);
                    LoadoutManager.AddLoadout(costume);
                }
                else
                {
                    DrawNoCostumeWindow();
                }
            }

            if (widgetRow.ButtonText(UIText.DeleteCostume.TranslateSimple()))
            {
                List<FloatMenuOption> options = new List<FloatMenuOption>();
                foreach (AwesomeInventoryCostume costume in _loadout.Costumes)
                {
                    options.Add(
                        new FloatMenuOption(
                            costume.label
                            , () =>
                            {
                                if (LoadoutManager.TryRemoveLoadout(costume))
                                {
                                    _loadout.Costumes.Remove(costume);
                                    _costume = null;
                                    _pawn.outfits.CurrentOutfit = _loadout;
                                }
                            }));
                }

                if (!options.Any())
                {
                    options.Add(new FloatMenuOption(UIText.NoCostumeAvailable.TranslateSimple(), null));
                }

                Find.WindowStack.Add(new FloatMenu(options));
            }

            if (widgetRow.ButtonText(UIText.SelectCostume.TranslateSimple()))
            {
                List<FloatMenuOption> options = new List<FloatMenuOption>();
                foreach (AwesomeInventoryCostume costume in _loadout.Costumes)
                {
                    options.Add(
                        new FloatMenuOption(
                            costume.label
                            , () =>
                            {
                                _costume = costume;
                            }));
                }

                if (!options.Any())
                {
                    options.Add(new FloatMenuOption(UIText.NoCostumeAvailable.TranslateSimple(), null));
                }

                Find.WindowStack.Add(new FloatMenu(options));
            }

            Rect nameRect = new Rect(widgetRow.FinalX, widgetRow.FinalY, canvas.width - widgetRow.FinalX, GenUI.ListSpacing);
            string name;
            if (_costume == null)
            {
                name = UIText.NoCostumeAvailable.TranslateSimple();
                GUI.color = Color.grey;
            }
            else
            {
                name = _costume.label;
            }

            name = Widgets.TextField(nameRect, name);
            if (_costume != null)
                _costume.label = name;

            GUI.color = Color.white;
        }

        private void DrawItemsInLoadout(Rect canvas)
        {
            Widgets.NoneLabelCenteredVertically(canvas.ReplaceHeight(GenUI.ListSpacing), UIText.ApparelsInLoadout.TranslateSimple());

            Rect outRect = canvas.ReplaceyMin(canvas.y + GenUI.ListSpacing);
            Rect viewRect = new Rect(outRect);
            viewRect.height = _loadout.Count * GenUI.ListSpacing;

            GUI.DrawTexture(outRect, TexResource.DarkBackground);
            Widgets.BeginScrollView(outRect, ref _loadoutScrollPos, viewRect);

            bool alternate = true;
            Rect labelRect = new Rect(viewRect).ReplaceHeight(GenUI.ListSpacing).ReplaceX(viewRect.x + GenUI.GapTiny);
            foreach (ThingGroupSelector selector in _apparelInLoadout)
            {
                Text.WordWrap = false;
                GUI.DrawTexture(labelRect, (alternate ^= true) ? TexUI.TextBGBlack : TexUI.GrayTextBG);
                DrawUtility.DrawLabelButton(
                    labelRect
                    , selector.LabelCapNoCount
                    , () =>
                    {
                        Rect msgRect = new Rect(Vector2.zero, Text.CalcSize(UIText.NoCostumeSelected.TranslateSimple()));
                        if (_costume == null)
                        {
                            Find.WindowStack.Add(
                                new Dialog_InstantMessage(
                                    UIText.NoCostumeSelected.TranslateSimple(), new Vector2(300, 200))
                                {
                                    windowRect = msgRect,
                                });
                        }

                        _costume?.AddItemToCostume(selector);
                    });

                labelRect = labelRect.ReplaceY(labelRect.yMax);
            }

            Widgets.EndScrollView();
        }

        private void DrawItemsInCostume(Rect canvas)
        {
            Widgets.NoneLabelCenteredVertically(canvas.ReplaceHeight(GenUI.ListSpacing), UIText.ItemsInCostume.TranslateSimple());

            Rect outRect = canvas.ReplaceyMin(canvas.y + GenUI.ListSpacing);
            Rect viewRect = new Rect(outRect);
            viewRect.height = _loadout.Count * GenUI.ListSpacing;

            GUI.DrawTexture(outRect, TexResource.DarkBackground);
            Widgets.BeginScrollView(outRect, ref _getupdetailScrollPos, viewRect);

            if (_costume != null)
            {
                bool alternate = true;
                Rect labelRect = new Rect(viewRect.x + GenUI.GapTiny, viewRect.y, viewRect.width - GenUI.ListSpacing - GenUI.ScrollBarWidth, GenUI.ListSpacing);
                _costume.CostumeItems.SortBy(c => c.LabelCapNoCount);
                for (int i = 0; i < _costume.CostumeItems.Count; i++)
                {
                    ThingGroupSelector selector = _costume.CostumeItems[i];
                    Text.WordWrap = false;
                    GUI.DrawTexture(labelRect, (alternate ^= true) ? TexUI.TextBGBlack : TexUI.GrayTextBG);
                    Widgets.Label(labelRect, selector.LabelCapNoCount);

                    if (Widgets.ButtonImage(new Rect(labelRect.xMax, labelRect.y, GenUI.SmallIconSize, GenUI.SmallIconSize), TexResource.CloseXSmall))
                    {
                        _costume.RemoveItemFromCostume(selector);
                    }

                    labelRect = labelRect.ReplaceY(labelRect.yMax);
                }
            }

            Widgets.EndScrollView();
        }
    }
}
