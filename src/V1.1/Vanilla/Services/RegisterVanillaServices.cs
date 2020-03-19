// <copyright file="RegisterVanillaServices.cs" company="Zizhen Li">
// Copyright (c) Zizhen Li. All rights reserved.
// Licensed under the GPL-3.0-only license. See LICENSE.md file in the project root for full license information.
// </copyright>

using AwesomeInventory.UI;
using AwesomeInventory.Utilities;
using Verse;

namespace AwesomeInventory
{
    /// <summary>
    /// Register services for the vanilla mod.
    /// </summary>
    [StaticConstructorOnStartup]
    public static class RegisterVanillaServices
    {
        static RegisterVanillaServices()
        {
            RegisterServices.RegisterIInventoryHelper(new InventoryHelper());
            RegisterServices.RegisterIDrawHelper(new DrawHelper());
        }
    }
}
