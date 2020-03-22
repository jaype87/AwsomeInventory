// <copyright file="ReflectionUtility.cs" company="Zizhen Li">
// Copyright (c) 2019 - 2020 Zizhen Li. All rights reserved.
// Licensed under the LGPL-3.0-only license. See LICENSE.md file in the project root for full license information.
// </copyright>

using System.Reflection;
using Verse;

namespace AwesomeInventory
{
    /// <summary>
    /// Some hacks using reflection.
    /// </summary>
    public static class ReflectionUtility
    {
        /// <summary>
        /// Gets maxWidth field in <see cref="WidgetRow"/>.
        /// </summary>
        public static FieldInfo MaxWidthField { get; }
            = typeof(WidgetRow).GetField("maxWidth", BindingFlags.NonPublic | BindingFlags.Instance);

        /// <summary>
        /// Gets startX field in <see cref="WidgetRow"/>.
        /// </summary>
        public static FieldInfo StartXField { get; }
            = typeof(WidgetRow).GetField("startX", BindingFlags.NonPublic | BindingFlags.Instance);

        /// <summary>
        /// Available width left in <see cref="WidgetRow"/>.
        /// </summary>
        /// <param name="row"> Instance of <see cref="WidgetRow"/>. </param>
        /// <returns> Available width left in <paramref name="row"/>. </returns>
        public static float AvailableWidth(this WidgetRow row)
        {
            return (float)MaxWidthField.GetValue(row) - (row.FinalX - (float)StartXField.GetValue(row));
        }
    }
}
