// <copyright file="ContainerState.cs" company="Zizhen Li">
// Copyright (c) 2019 - 2020 Zizhen Li. All rights reserved.
// Licensed under the LGPL-3.0-only license. See LICENSE.md file in the project root for full license information.
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AwesomeInventory.UI
{
    /// <summary>
    /// Keep track of states of an UI container.
    /// </summary>
    public class ContainerState
    {
        /// <summary>
        /// Whether the container is minimized.
        /// </summary>
        public bool Minimized;

        /// <summary>
        /// Whether the container is in the process of minimizing.
        /// </summary>
        public bool Minimizing;
    }
}
