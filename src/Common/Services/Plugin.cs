// <copyright file="Plugin.cs" company="Zizhen Li">
// Copyright (c) 2019 - 2020 Zizhen Li. All rights reserved.
// Licensed under the LGPL-3.0-only license. See LICENSE.md file in the project root for full license information.
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AwesomeInventory
{
    /// <summary>
    /// Define metadata for plugins.
    /// </summary>
    public abstract class Plugin
    {
        /// <summary>
        /// Gets ID for plugin.
        /// </summary>
        public abstract int ID { get; }

        /// <summary>
        /// Gets name for the plugin to display on screen.
        /// </summary>
        public abstract string DisplayName { get; }
    }
}
