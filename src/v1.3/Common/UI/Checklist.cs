// <copyright file="Checklist.cs" company="Zizhen Li">
// Copyright (c) 2019 - 2020 Zizhen Li. All rights reserved.
// Licensed under the LGPL-3.0-only license. See LICENSE.md file in the project root for full license information.
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using UnityEngine;
using Verse;

namespace AwesomeInventory.UI
{
    /// <summary>
    /// A UI class for checklist.
    /// </summary>
    /// <typeparam name="T">Generic type.</typeparam>
    public class Checklist<T>
    {
        private readonly List<ChecklistItem<T>> _items;
        private string _searchText;
        private Vector2 _scrollPosition = Vector2.zero;

        /// <summary>
        /// Initializes a new instance of the <see cref="Checklist{T}"/> class.
        /// </summary>
        /// <param name="items"> Items in list. </param>
        /// <param name="orderByAlphanumerically"> Ordered by alphanumerically. </param>
        /// <param name="searchAble"> Whether the list provides search feature. </param>
        /// <param name="dragToReorder"> Allows drag to reorder. </param>
        /// <param name="extraReorderItemOnGUI"> Method for drawing item that is being drag-to-reorder. </param>
        public Checklist(
            List<ChecklistItem<T>> items,
            bool orderByAlphanumerically,
            bool searchAble,
            bool dragToReorder = false,
            Action<int, Vector2> extraReorderItemOnGUI = null)
        {
            ValidateArg.NotNull(items, nameof(items));

            _items = items;
            Height = items.Sum(t => t.Height);
            SearchAble = searchAble;
            OrderByAlphanumeric = orderByAlphanumerically;
            DragToReorder = dragToReorder;
        }

        /// <summary>
        /// Gets items in list.
        /// </summary>
        public IReadOnlyCollection<ChecklistItem<T>> Items => _items.AsReadOnly();

        /// <summary>
        /// Gets the height of the list.
        /// </summary>
        public float Height { get; private set; }

        /// <summary>
        /// Gets or sets a value indicating whether items in list can be searched by text.
        /// </summary>
        public bool SearchAble { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the list should be ordered by alphanumerically.
        /// </summary>
        public bool OrderByAlphanumeric { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the list allows to drag-to-reorder.
        /// </summary>
        public bool DragToReorder { get; set; }

        /// <summary>
        /// Gets or sets a method for drawing an items which is being drag-to-reorder.
        /// </summary>
        public Action<int, Vector2> ExtraReorderItemOnGUI { get; set; }

        /// <summary>
        /// Gets the width of the list.
        /// </summary>
        public float Width => this.Items.First().Width + GenUI.ScrollBarWidth;

        /// <summary>
        /// Draw items in list.
        /// </summary>
        /// <param name="rect"> Rect for drawing. </param>
        public virtual void Draw(Rect rect)
        {
            IEnumerable<ChecklistItem<T>> items = _items;

            var reorderableGroup = 0;
            if (this.DragToReorder)
            {
                reorderableGroup = ReorderableWidget.NewGroup_NewTemp(
                    this.ReorderItems
                    , ReorderableDirection.Vertical
                    , -1
                    , (index, position) => this.ExtraReorderItemOnGUI?.Invoke(index, position));
            }

            if (this.SearchAble)
            {
                var searchRect = new Rect(rect.x, rect.yMax - GenUI.ListSpacing, rect.width, GenUI.ListSpacing);
                _searchText = Widgets.TextField(searchRect, _searchText);
                var upperText = _searchText.ToUpperInvariant();
                items = _items.Where(t => t.Label.ToUpperInvariant().Contains(upperText));

                rect = rect.ReplaceHeight(rect.height - GenUI.ListSpacing);
            }

            Widgets.BeginScrollView(
                rect,
                ref _scrollPosition,
                new Rect(rect.position, new Vector2(rect.width - GenUI.ScrollBarWidth, this.Height)));

            var pos = Vector2.zero;

            foreach (var item in items)
            {
                var itemRect = item.Draw(pos, item.Object);
                pos.x += item.Height;

                if (this.DragToReorder)
                    ReorderableWidget.Reorderable(reorderableGroup, itemRect);
            }

            Widgets.EndScrollView();
        }

        /// <summary>
        /// Update the height of list and returns it.
        /// </summary>
        /// <returns> Current height of the list. </returns>
        public virtual float LatestHeight() => this.Height = Items.Any() ? this.Items.Sum(t => t.Height) : 0f;

        /// <summary>
        /// Add item to list.
        /// </summary>
        /// <param name="item"> Item to add. </param>
        public void AddItem(ChecklistItem<T> item)
        {
            _items.Add(item);
            this.RefreshList();
        }

        /// <summary>
        /// Add items to list.
        /// </summary>
        /// <param name="items"> Items to add. </param>
        public void AddItems(IEnumerable<ChecklistItem<T>> items)
        {
            _items.AddRange(items);
            this.RefreshList();
        }

        /// <summary>
        /// Remove item from list.
        /// </summary>
        /// <param name="item"> Item to remove. </param>
        public void RemoveItem(ChecklistItem<T> item)
        {
            _items.Remove(item);
            this.RefreshList();
        }

        private void RefreshList()
        {
            if (this.OrderByAlphanumeric)
                _items.OrderBy(t => t.Label);
        }

        private void ReorderItems(int from, int to)
        {
            if (from == to)
            {
                return;
            }
            else
            {
                var item = _items[from];
                _items.Insert(to, item);
                if (to < from)
                    _items.RemoveAt(++from);
                else
                    _items.RemoveAt(from);
            }
        }
    }
}
