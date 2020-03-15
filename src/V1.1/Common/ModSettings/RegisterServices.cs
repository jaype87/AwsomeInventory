// <copyright file="RegisterServices.cs" company="Zizhen Li">
// Copyright (c) Zizhen Li. All rights reserved.
// Licensed under the GPL-3.0-only license. See LICENSE.md file in the project root for full license information.
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AwesomeInventory.Base;
using Verse;

namespace AwesomeInventory.Common.ModSettings
{
    /// <summary>
    /// Register services prvodied by this assembly.
    /// </summary>
    [StaticConstructorOnStartup]
    internal static class RegisterServices
    {
        static RegisterServices()
        {
            AwesomeInventoryServiceProvider.AddService(typeof(DrawGearTab), new DrawGearTab());
        }
    }
}
