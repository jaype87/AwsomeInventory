// <copyright file="Logger.cs" company="Zizhen Li">
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

namespace AwesomeInventory
{
    /// <summary>
    /// Implementation of <see cref="ILogger"/>.
    /// </summary>
    [StaticConstructorOnStartup]
    public class Logger : ILogger
    {
        static Logger()
        {
            AwesomeInventoryServiceProvider.AddService(typeof(ILogger), new Logger());
        }

        /// <inheritdoc/>
        public void Message(string message)
        {
#if DEBUG
            Log.Message(message, true);
#endif
        }

        /// <inheritdoc/>
        public void Warning(string warning)
        {
#if DEBUG
            Log.Warning(warning, true);
#endif
        }
    }
}
