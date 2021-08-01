// <copyright file="StatPanelModel.cs" company="Zizhen Li">
// Copyright (c) 2019 - 2020 Zizhen Li. All rights reserved.
// Licensed under the LGPL-3.0-only license. See LICENSE.md file in the project root for full license information.
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using RimWorldUtility;
using UnityEngine;
using Verse;

namespace AwesomeInventory.UI
{
    /// <summary>
    /// A model which contains data for drawing stat panel.
    /// </summary>
    public class StatPanelModel
    {
        private Pawn _pawn;

        private Vector2 _size;

        private bool _used;

        public StatPanelModel(Pawn pawn, string title, Vector2 size)
        {
            this.Pawn = pawn;
            this.StatCacheKeys = new NotifiedStatCacheKey(pawn, StatPanelManager.GetStatCacheKeysFor(pawn).ToHashSet(), this);
            this.Title = title;
            this.Size = size;
        }

        /// <summary>
        /// Gets a list of <see cref="StatCacheKeys"/>.
        /// </summary>
        public NotifiedStatCacheKey StatCacheKeys { get; private set; }

        /// <summary>
        /// Gets or sets the selected pawn and update <see cref="StatCacheKeys"/> accordingly.
        /// </summary>
        public Pawn Pawn
        {
            get => _pawn;
            set
            {
                if (_pawn == value)
                    return;

                _pawn = value;
                if (this.StatCacheKeys != null)
                    this.StatCacheKeys.Pawn = value;

                this.Changed = true;
                _used = false;
            }
        }

        /// <summary>
        /// Gets or sets the title for Stat Panel.
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// Gets or sets the size of Stat Panel.
        /// </summary>
        public Vector2 Size
        {
            get => _size;
            set
            {
                if (_size == value)
                    return;

                _size = value;

                this.Changed = true;
                _used = false;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether data in this model has been changed.
        /// </summary>
        public bool Changed { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether data in this model has been consumed.
        /// </summary>
        public bool Used
        {
            get => _used;
            set
            {
                _used = value;
                if (value)
                    this.Changed = false;
            }
        }

        public class NotifiedStatCacheKey : ICollectionNotify<StatDef>
        {
            private Pawn _pawn;

            private StatPanelModel _model;

            public NotifiedStatCacheKey(Pawn pawn, HashSet<StatCacheKey> keys, StatPanelModel model)
            {
                this.Pawn = pawn;
                this.Keys = keys;
                _model = model;
            }

            public Pawn Pawn
            {
                get => _pawn;

                set
                {
                    if (_pawn == value)
                        return;

                    _pawn = value;

                    if (_pawn == null)
                    {
                        this.Keys.Clear();
                    }

                    this.Keys = StatPanelManager.GetStatCacheKeysFor(value).ToHashSet();
                }
            }

            public HashSet<StatCacheKey> Keys { get; private set; }

            public void Add(StatDef data)
            {
                this.Keys.Add(new StatCacheKey(this.Pawn, data));
                _model.Changed = true;
            }

            public void Remove(StatDef data)
            {
                this.Keys.Remove(new StatCacheKey(this.Pawn, data));
                _model.Changed = true;
            }
        }
    }
}
