// <copyright file="InGameLogger.cs" company="Zizhen Li">
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
    public class InGameLogger : ILogger
    {
        static InGameLogger()
        {
            AwesomeInventoryServiceProvider.AddService(typeof(ILogger), new InGameLogger());
        }

        /// <inheritdoc/>
        public void Message(string message)
        {
            Log.Message(AIDebug.Header + message);
        }

        /// <inheritdoc/>
        public void Warning(string warning)
        {
            Log.Warning(AIDebug.Header + warning);
        }

        /// <inheritdoc/>
        public void WriteError(string errorMsg)
        {
            Log.Error(AIDebug.Header + errorMsg);
        }
    }
}
