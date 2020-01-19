using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using RimWorld;

namespace RPG_Inventory_Remake_Common
{
    // It is one weird way to instantiate ThingComp.
    // Rimworld uses ComProerties to instantiate ThingComp
    // then attach ComProperties to ThingComp and further
    // add ThingComp to ThingWithComps. In xml, ThingComp does
    // not exist, but presents with only a name as an element
    // inside the CompProperties tag. In return, ComProperties
    // with the name can instantiate a ThingComp class.
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
