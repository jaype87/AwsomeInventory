// <copyright file="IReset.cs" company="Zizhen Li">
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
    /// Classes that can be reset to initial states.
    /// </summary>
    public interface IReset
    {
        /// <summary>
        /// Reset to initial states.
        /// </summary>
        void Reset();
    }
}
