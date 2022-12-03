// <copyright file="StatChoiceModel.cs" company="Zizhen Li">
// Copyright (c) 2019 - 2020 Zizhen Li. All rights reserved.
// Licensed under the LGPL-3.0-only license. See LICENSE.md file in the project root for full license information.
// </copyright>

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using RimWorldUtility;
using UnityEngine;
using Verse;

namespace AwesomeInventory.UI
{
    public class StatChoiceModel : IEnumerable<StatEntryModel>, ICollectionNotify<StatDef>
    {
        private Vector2 _size;

        private string _searchString = string.Empty;

        /// <summary>
        /// Initializes a new instance of the <see cref="StatChoiceModel"/> class.
        /// </summary>
        public StatChoiceModel()
        {
            this.StatChoices.AddRange(AllStatEntries);
            this.StatChoices = this.StatChoices.CustomSort();
        }

        public string SearchString
        {
            get => _searchString;
            set
            {
                if (_searchString == (value ?? string.Empty))
                    return;

                this.UpdateStatEntry(_searchString, value);
                _searchString = value;
                this.Changed = true;
            }
        }

        public Vector2 Size
        {
            get => _size;
            set
            {
                if (_size == value)
                    return;

                _size = value;
                this.Changed = true;
            }
        }

        public List<StatEntryModel> StatChoices { get; private set; } = new List<StatEntryModel>();

        public HashSet<StatEntryModel> AllStatEntries { get; }
            = StatPanelManager.StatDefs
                .Select(def => new StatEntryModel(def, StatPanelManager.SelectedDefs.Data.Contains(def)))
                .OrderBy(model => model.StatDef.LabelCap.ToString())
                .ToHashSet();

        public string Title { get; } = UIText.AddRemoveStat.TranslateSimple();

        public bool Changed { get; set; }

        #region Implementation of IEnumerable

        /// <inheritdoc />
        public IEnumerator<StatEntryModel> GetEnumerator()
        {
            return this.StatChoices.GetEnumerator();
        }

        /// <inheritdoc />
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion

        #region Implementation of ICollectionNotify<in StatDef>

        /// <inheritdoc />
        public void Add(StatDef data)
        {
            StatEntryModel find = new StatEntryModel(data, false);
            UpdateSelectState(this.AllStatEntries, find, true);
            this.StatChoices = this.StatChoices.CustomSort();

            this.Changed = true;
        }

        /// <inheritdoc />
        public void Remove(StatDef data)
        {
            StatEntryModel find = new StatEntryModel(data, true);
            UpdateSelectState(this.AllStatEntries, find, false);
            this.StatChoices = this.StatChoices.CustomSort();

            this.Changed = true;
        }

        #endregion

        private static void UpdateSelectState(HashSet<StatEntryModel> source, StatEntryModel model, bool newState)
        {
            if (source.TryGetValue(model, out StatEntryModel value))
            {
                source.Remove(value);
                value.Selected = newState;
                source.Add(value);
            }
        }

        private void UpdateStatEntry(string old, string @new)
        {
            string upper = @new.ToUpperInvariant();
            if (string.IsNullOrEmpty(_searchString))
            {
                this.StatChoices.Clear();
                this.StatChoices.AddRange(this.AllStatEntries);
            }
            else if (old.Length > @new.Length)
            {
                this.StatChoices.Clear();
                this.StatChoices
                    .AddRange(
                        this.AllStatEntries.Where(
                            entry => entry.StatDef.LabelCap
                                .ToString()
                                .ToUpperInvariant()
                                .Contains(upper)));
            }
            else
            {
                this.StatChoices.RemoveAll(
                    c => !c.StatDef.LabelCap.ToString().ToUpperInvariant().Contains(upper));
            }

            this.StatChoices = this.StatChoices.CustomSort();
        }
    }
}
