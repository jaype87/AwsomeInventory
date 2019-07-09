using System;
using Verse;
using RimWorld;

namespace RPG_Inventory_for_CE
{
	[DefOf]
    public static class Sandy_Gear_DefOf
    {	
    	public static BodyPartGroupDef Teeth;
    	public static BodyPartGroupDef Mouth;
    	public static BodyPartGroupDef Neck;
    	public static BodyPartGroupDef Shoulders;
    	public static BodyPartGroupDef Arms;
    	public static BodyPartGroupDef Hands;
    	public static BodyPartGroupDef Waist;
    	public static BodyPartGroupDef Feet;

        //This was added for CE
        public static BodyPartGroupDef LeftArm;

        //This was added for Jewelry
        //Two defs file was added, they are in Defs\Jewelry_compat
        public static BodyPartGroupDef Ears;
        public static ApparelLayerDef Accessories;
    }
}
