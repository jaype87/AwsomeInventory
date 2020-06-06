// <copyright file="ChecklistItem.cs" company="Zizhen Li">
// Copyright (c) 2019 - 2020 Zizhen Li. All rights reserved.
// Licensed under the LGPL-3.0-only license. See LICENSE.md file in the project root for full license information.
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace AwesomeInventory.UI
{
    /// <summary>
    /// Basic UI unit for a checklist.
    /// </summary>
    /// <typeparam name="T">Genric type. </typeparam>
    public class ChecklistItem<T>
    {
#pragma warning disable CA1720 // Identifier contains type name
        /// <summary>
        /// Initializes a new instance of the <see cref="ChecklistItem{T}"/> class.
        /// </summary>
        /// <param name="label"> Label to show for this item. </param>
        /// <param name="object"> Object that this checklist item represents. </param>
        /// <param name="checked"> Whether this item is selected. </param>
        /// <param name="height"> Height of this item. </param>
        /// <param name="width"> Width of this item. </param>
        public ChecklistItem(string label, T @object, float height, float width, bool @checked)
        {
            Label = label;
            Object = @object;
            Checked = @checked;
            Height = height;
            Width = width;
        }

        /// <summary>
        /// Gets the label for the checklist item.
        /// </summary>
        public virtual string Label { get; }

        /// <summary>
        /// Gets underlying object for this checklist item.
        /// </summary>
        public T Object { get; }

        /// <summary>
        /// Gets or sets a value indicating whether this item is selected.
        /// </summary>
        public bool Checked { get; set; }

        /// <summary>
        /// Gets the height of item.
        /// </summary>
        public float Height { get; }

        /// <summary>
        /// Gets the width of item.
        /// </summary>
        public float Width { get; }

        /// <summary>
        /// Gets or sets the function that draws this item.
        /// </summary>
        public Func<Vector2, T, Rect> Draw { get; set; }
#pragma warning restore CA1720 // Identifier contains type name
    }
}
