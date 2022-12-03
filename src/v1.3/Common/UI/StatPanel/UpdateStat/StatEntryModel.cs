// <copyright file="StatEntryModel.cs" company="Zizhen Li">
// Copyright (c) 2019 - 2020 Zizhen Li. All rights reserved.
// Licensed under the LGPL-3.0-only license. See LICENSE.md file in the project root for full license information.
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JetBrains.Annotations;
using RimWorld;
using Verse;

namespace AwesomeInventory.UI
{
    public class StatEntryModel : IEquatable<StatEntryModel>
    {
        public StatEntryModel()
        {
        }

        public StatEntryModel(StatDef statDef, bool selected)
        {
            StatDef = statDef;
            Selected = selected;
        }

        public StatEntryModel(StatEntryModel model)
            : this(model.StatDef, model.Selected)
        {
        }

        public StatDef StatDef { get; set; }

        public bool Selected { get; set; }

        public static bool operator ==(StatEntryModel left, StatEntryModel right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(StatEntryModel left, StatEntryModel right)
        {
            return !(left == right);
        }

        #region Equality members

        /// <inheritdoc />
        public bool Equals(StatEntryModel other)
        {
            if (ReferenceEquals(this, other))
                return true;

            if (other is null)
                return false;

            return Equals(StatDef, other.StatDef) && Equals(StatDef.defName, other.StatDef.defName) && Selected == other.Selected;
        }

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            return obj is StatEntryModel other && Equals(other);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            unchecked
            {
                return ((StatDef != null ? StatDef.GetHashCode() : 0) * 397) ^ Selected.GetHashCode();
            }
        }

        #endregion
    }

    public static class StatEntryModelUtility
    {
        public static List<StatEntryModel> CustomSort(this List<StatEntryModel> set)
        {
            return set.OrderByDescending(t => t.Selected)
                      .ThenBy(t => t.StatDef.LabelCap.ToString())
                      .ToList();
        }
    }
}
