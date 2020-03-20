// <copyright file="ILogger.cs" company="Zizhen Li">
// Copyright (c) Zizhen Li. All rights reserved.
// Licensed under the GPL-3.0-only license. See LICENSE.md file in the project root for full license information.
// </copyright>

namespace AwesomeInventory
{
    /// <summary>
    /// A utility class for logging.
    /// </summary>
    public interface ILogger
    {
        /// <summary>
        /// Write message to the logger.
        /// </summary>
        /// <param name="message"> Message to write. </param>
        void Message(string message);
    }
}