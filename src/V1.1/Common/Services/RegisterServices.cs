// <copyright file="RegisterServices.cs" company="Zizhen Li">
// Copyright (c) Zizhen Li. All rights reserved.
// Licensed under the GPL-3.0-only license. See LICENSE.md file in the project root for full license information.
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AwesomeInventory.UI;
using AwesomeInventory.Utilities;

namespace AwesomeInventory
{
    /// <summary>
    /// Register services provided either by the vanilla or the CE assembly.
    /// It provides a template of services for vanilla and CE implementation.
    /// </summary>
    public static class RegisterServices
    {
        /// <summary>
        /// Register IInventoryHelper.
        /// </summary>
        /// <param name="inventoryHelper"> Instance of <paramref name="inventoryHelper"/> to register. </param>
        public static void RegisterIInventoryHelper(IInventoryHelper inventoryHelper)
        {
            AwesomeInventoryServiceProvider.AddService(typeof(IInventoryHelper), inventoryHelper);
        }

        /// <summary>
        /// Register IDrawHelper.
        /// </summary>
        /// <param name="drawHelper"> Instance of <paramref name="drawHelper"/> to register. </param>
        public static void RegisterIDrawHelper(IDrawHelper drawHelper)
        {
            AwesomeInventoryServiceProvider.AddService(typeof(IDrawHelper), drawHelper);
        }
    }
}
