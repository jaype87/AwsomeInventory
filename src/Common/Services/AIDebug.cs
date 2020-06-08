// <copyright file="AIDebug.cs" company="Zizhen Li">
// Copyright (c) 2019 - 2020 Zizhen Li. All rights reserved.
// Licensed under the LGPL-3.0-only license. See LICENSE.md file in the project root for full license information.
// </copyright>

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace AwesomeInventory
{
    /// <summary>
    /// Provides debug funtionality for AwesomeInventory.
    /// </summary>
    public static class AIDebug
    {
        /// <summary>
        /// Header for message output.
        /// </summary>
        public static readonly string Header = $"[AI v{Assembly.GetExecutingAssembly().GetName().Version}]: ";

        static AIDebug()
        {
            if (AwesomeInventoryServiceProvider.TryGetImplementation(out ILogger logger))
            {
                Init(logger);
            }
        }

        /// <summary>
        /// Gets or sets logger used for debug.
        /// </summary>
        public static ILogger Logger { get; set; }

        /// <summary>
        /// Gets or sets a timer class.
        /// </summary>
        public static Timer Timer { get; set; }

        /// <summary>
        /// Initialize <see cref="AIDebug"/>.
        /// </summary>
        /// <param name="logger"> Logger used in <see cref="AIDebug"/> to record messages. </param>
        public static void Init(ILogger logger)
        {
            Logger = logger;
            Timer = new Timer(logger);
        }
    }
}