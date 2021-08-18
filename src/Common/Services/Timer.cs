// <copyright file="Timer.cs" company="Zizhen Li">
// Copyright (c) 2019 - 2020 Zizhen Li. All rights reserved.
// Licensed under the LGPL-3.0-only license. See LICENSE.md file in the project root for full license information.
// </copyright>

using System.Diagnostics;

namespace AwesomeInventory
{
    /// <summary>
    /// A timer class to record time.
    /// </summary>
    public class Timer
    {
#if DEBUG
        private Stopwatch _stopwatch = new Stopwatch();
        private ILogger _logger;
#endif

        /// <summary>
        /// Initializes a new instance of the <see cref="Timer"/> class.
        /// </summary>
        /// <param name="logger"> Logger for writing messages. </param>
        public Timer(ILogger logger)
        {
#if DEBUG
            _logger = logger;
            _stopwatch.Start();
            _stopwatch.Reset();
#endif
        }

        /// <summary>
        /// Start timer.
        /// </summary>
        public void Start()
        {
#if DEBUG
            _stopwatch.Reset();
            _stopwatch.Start();
#endif
        }

        /// <summary>
        /// Stop timer and write stats to <see cref="ILogger"/>.
        /// </summary>
        /// <param name="header"> Header to prepand to output message. </param>
        public void Stop(string header)
        {
#if DEBUG
            _stopwatch.Stop();
            long elapsed = _stopwatch.ElapsedMilliseconds;
            _logger?.Message(header + " time elapsed: " + elapsed + "ms");
#endif
        }
    }
}
