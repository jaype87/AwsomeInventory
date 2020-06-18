// <copyright file="SearchableList.cs" company="Zizhen Li">
// Copyright (c) 2019 - 2020 Zizhen Li. All rights reserved.
// Licensed under the LGPL-3.0-only license. See LICENSE.md file in the project root for full license information.
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace AwesomeInventory.UI
{
    /// <summary>
    /// A scroll list with a search field.
    /// </summary>
    public static class SearchableList
    {
        /// <summary>
        /// Draw scrollable list with search field on top.
        /// </summary>
        /// <typeparam name="T"> Generic type. </typeparam>
        /// <param name="outRect"> Rect for drawing the list. </param>
        /// <param name="rowHeight"> The height of each row. </param>
        /// <param name="drawRow"> How to draw a row in the list. </param>
        /// <param name="scrollPos"> Current scroll position of this list. </param>
        /// <param name="listLength"> Length in pixel of the list. </param>
        /// <param name="listItems"> Items on the list. </param>
        /// <param name="searchText"> Text for search. </param>
        public static void Draw<T>(Rect outRect, float rowHeight, Action<Rect, T, int> drawRow, ref Vector2 scrollPos, float listLength, IEnumerable<T> listItems, ref string searchText)
            where T : Thing
        {
            searchText = Widgets.TextField(outRect.ReplaceHeight(GenUI.ListSpacing), searchText);
            string formattedText = searchText.ToUpperInvariant();
            float listHeight = outRect.height - GenUI.ListSpacing;

            List<T> searchResult = listItems.Where(
                t =>
                {
                    string label = string.Empty;
                    if (t is Corpse corpse)
                        label = corpse.Bugged ? corpse.ThingID : corpse.LabelCapNoCount;
                    else
                        label = t.LabelNoCount;

                    return label.ToUpperInvariant().Contains(formattedText);
                }).ToList();
            float modifiedListLength = searchResult.Count * rowHeight;

            Widgets.BeginScrollView(outRect.ReplaceyMin(GenUI.ListSpacing + outRect.yMin), ref scrollPos, new Rect(0, 0, outRect.width, modifiedListLength));
            DrawUtility.GetIndexRangeFromScrollPosition((searchResult.Count - Mathf.FloorToInt(scrollPos.y / GenUI.ListSpacing)) * GenUI.ListSpacing, scrollPos.y, out int from, out int to, GenUI.ListSpacing);
            for (int i = from; i < to; i++)
            {
                Rect rowRect = new Rect(0, i * rowHeight, outRect.width, rowHeight);
                GUI.DrawTexture(rowRect, i % 2 == 0 ? TexUI.GrayTextBG : TexUI.TextBGBlack);
                drawRow?.Invoke(rowRect, searchResult[i], i);
            }

            Widgets.EndScrollView();
        }
    }
}
