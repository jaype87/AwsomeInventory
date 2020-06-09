// <copyright file="ILogger.cs" company="Zizhen Li">
// Copyright (c) 2019 - 2020 Zizhen Li. All rights reserved.
// Licensed under the LGPL-3.0-only license. See LICENSE.md file in the project root for full license information.
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

        /// <summary>
        /// Write warning to the logger.
        /// </summary>
        /// <param name="warning"> Warning to write. </param>
        void Warning(string warning);

        /// <summary>
        /// Write error to the logger.
        /// </summary>
        /// <param name="errorMsg"> Error to write. </param>
        void WriteError(string errorMsg);
    }
}