using System;
using Verse;
using RimWorld;

namespace RPG_Inventory_Remake
{
	[DefOf]
    public static class CorgiBodyPartGroupDefOf
    {
        // Comments below are formatted with "Better Comments" in Visual Studio Marketplace

        //! BodyPargGroupDefs below are also declared in C# RimWorld.BodyPartGroupDefOf
        public static BodyPartGroupDef UpperHead;
        public static BodyPartGroupDef FullHead;
        public static BodyPartGroupDef Torso;
        public static BodyPartGroupDef LeftHand;
        public static BodyPartGroupDef RightHand;
        public static BodyPartGroupDef Legs;
        public static BodyPartGroupDef Eyes;

        //! Following BodyPartGroupDefs are defined in Core
        //! Path:RimWorld\Mods\Core\Defs\Bodies\BodyPartGroups.xml
        //! The reason why they are not in the BodyPartGroupDefOf Class is anyone's guess
        public static BodyPartGroupDef Teeth;
        public static BodyPartGroupDef Mouth;
        public static BodyPartGroupDef Neck;
        public static BodyPartGroupDef Arms;
        public static BodyPartGroupDef Shoulders;
        public static BodyPartGroupDef Hands;
        public static BodyPartGroupDef Waist;
        public static BodyPartGroupDef Feet;

        //These are added for CE
        public static BodyPartGroupDef LeftArm;
        public static BodyPartGroupDef RightArm;
        public static BodyPartGroupDef LeftShoulder;
        public static BodyPartGroupDef RightShoulder;

        //These are added for Jewelry
        //Two defs file are added, they are in Defs\Jewelry_compat
        public static BodyPartGroupDef Ears;
        public static ApparelLayerDef Accessories;

        //Placeholder
        public static BodyPartGroupDef Arse;

        static CorgiBodyPartGroupDefOf()
        {
            DefOfHelper.EnsureInitializedInCtor(typeof(CorgiBodyPartGroupDefOf));
        }
    }
}    