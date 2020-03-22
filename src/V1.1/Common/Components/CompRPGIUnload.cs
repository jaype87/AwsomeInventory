// <copyright file="CompRPGIUnload.cs" company="Zizhen Li">
// Copyright (c) 2019 - 2020 Zizhen Li. All rights reserved.
// Licensed under the LGPL-3.0-only license. See LICENSE.md file in the project root for full license information.
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;

namespace AwesomeInventory.Loadout
{
    /// <summary>
    /// Component attached to thing that is marked to unload.
    /// </summary>
    public class CompRPGIUnload : ThingComp
    {
        public CompProperties_RPGUnload Props
        {
            get
            {
                return (CompProperties_RPGUnload)props;
            }
        }

        public bool Unload
        {
            get
            {
                return Props.Unload;
            }

            set
            {
                Props.Unload = value;
            }
        }

        public CompRPGIUnload()
        {

        }

        public CompRPGIUnload(bool unload)
        {
            Initialize(new CompProperties_RPGUnload());
            Unload = unload;
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look(ref Props.Unload, "Unload");
        }
    }

    public class CompProperties_RPGUnload : CompProperties
    {
        public bool Unload;

        public CompProperties_RPGUnload()
        {
            compClass = typeof(CompRPGIUnload);
        }
    }
}
