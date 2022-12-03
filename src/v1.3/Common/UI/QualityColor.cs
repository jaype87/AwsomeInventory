// <copyright file="QualityColor.cs" company="Zizhen Li">
// Copyright (c) 2019 - 2020 Zizhen Li. All rights reserved.
// Licensed under the LGPL-3.0-only license. See LICENSE.md file in the project root for full license information.
// </copyright>

using System.ComponentModel.Design;
using System.Linq;
using UnityEngine;
using Verse;

namespace AwesomeInventory.UI
{
    /// <summary>
    /// Decides display color for each quality level.
    /// </summary>
    public abstract class QualityColor : Plugin
    {
        private static QualityColor _instance;

        private Texture2D _awfulTex;
        private Texture2D _poorTex;
        private Texture2D _normalTex;
        private Texture2D _goodTex;
        private Texture2D _excellentTex;
        private Texture2D _masterworkTex;
        private Texture2D _legendaryTex;
        private Texture2D _genericTex;

        /// <summary>
        /// Gets an implementation of <see cref="QualityColor"/>.
        /// </summary>
        public static QualityColor Instance
        {
            get
            {
                if (_instance == null && !AwesomeInventoryServiceProvider.Plugins.Values.OfType<QualityColor>().EnumerableNullOrEmpty())
                {
                    _instance = AwesomeInventoryServiceProvider.GetPlugin<QualityColor>(AwesomeInventoryMod.Settings.QualityColorPluginID);
                }

                return _instance;
            }
        }

        /// <summary>
        /// Gets a color that decorates item of awful quality.
        /// </summary>
        public abstract Color Awful { get; }

        /// <summary>
        /// Gets a color that decorates item of poor quality.
        /// </summary>
        public abstract Color Poor { get; }

        /// <summary>
        /// Gets a color that decorates item of normal quality.
        /// </summary>
        public abstract Color Normal { get; }

        /// <summary>
        /// Gets a color that decorates item of good quality.
        /// </summary>
        public abstract Color Good { get; }

        /// <summary>
        /// Gets a color that decorates item of excellent quality.
        /// </summary>
        public abstract Color Excellent { get; }

        /// <summary>
        /// Gets a color that decorates item of masterwork quality.
        /// </summary>
        public abstract Color Masterwork { get; }

        /// <summary>
        /// Gets a color that decorates item of legendary quality.
        /// </summary>
        public abstract Color Legendary { get; }

        /// <summary>
        /// Gets a color that decorates generic items or items with multiple stuff source selectted.
        /// </summary>
        public abstract Color Generic { get; }

        /// <summary>
        /// Gets <see cref="Texture2D"/> for aweful quality.
        /// </summary>
        public virtual Texture2D AwfulTex { get => _awfulTex ?? (_awfulTex = SolidColorMaterials.NewSolidColorTexture(this.Awful)); }

        /// <summary>
        /// Gets <see cref="Texture2D"/> for poor quality.
        /// </summary>
        public virtual Texture2D PoorTex { get => _poorTex ?? (_poorTex = SolidColorMaterials.NewSolidColorTexture(this.Poor)); }

        /// <summary>
        /// Gets <see cref="Texture2D"/> for normal quality.
        /// </summary>
        public virtual Texture2D NormalTex { get => _normalTex ?? (_normalTex = SolidColorMaterials.NewSolidColorTexture(this.Normal)); }

        /// <summary>
        /// Gets <see cref="Texture2D"/> for good quality.
        /// </summary>
        public virtual Texture2D GoodTex { get => _goodTex ?? (_goodTex = SolidColorMaterials.NewSolidColorTexture(this.Good)); }

        /// <summary>
        /// Gets <see cref="Texture2D"/> for excellent quality.
        /// </summary>
        public virtual Texture2D ExcellentTex { get => _excellentTex ?? (_excellentTex = SolidColorMaterials.NewSolidColorTexture(this.Excellent)); }

        /// <summary>
        /// Gets <see cref="Texture2D"/> for masterwork quality.
        /// </summary>
        public virtual Texture2D MasterworkTex { get => _masterworkTex ?? (_masterworkTex = SolidColorMaterials.NewSolidColorTexture(this.Masterwork)); }

        /// <summary>
        /// Gets <see cref="Texture2D"/> for legendary quality.
        /// </summary>
        public virtual Texture2D LegendaryTex { get => _legendaryTex ?? (_legendaryTex = SolidColorMaterials.NewSolidColorTexture(this.Legendary)); }

        /// <summary>
        /// Gets <see cref="Texture2D"/> for legendary quality.
        /// </summary>
        public virtual Texture2D GenericTex { get => _genericTex ?? (_genericTex = SolidColorMaterials.NewSolidColorTexture(this.Generic)); }

        /// <summary>
        /// Change color theme to plugin of <paramref name="id"/>.
        /// </summary>
        /// <param name="id"> ID of the plugin. </param>
        public static void ChangeTheme(int id)
        {
            _instance = AwesomeInventoryServiceProvider.GetPlugin<QualityColor>(id);
        }

        /// <summary>
        /// Register <paramref name="qualityColor"/> to <see cref="AwesomeInventoryServiceProvider"/>.
        /// </summary>
        /// <param name="qualityColor"> Service to register. </param>
        protected static void Register(QualityColor qualityColor)
        {
            AwesomeInventoryServiceProvider.AddPlugIn(qualityColor);
        }
    }
}
