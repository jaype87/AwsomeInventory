using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;
using RimWorld;
using RPGIResource;


namespace RPG_Inventory_Remake_Common
{
    public static class UtilityConstant
    {
        public static readonly Vector2 PaperDollSize = new Vector2(128f, 128f);
        public static Vector2 InstantMessageBox => new Vector2(50, 30);
        public static readonly StatDef MeleeWeapon_AverageArmorPenetration
            = DefDatabase<StatDef>.GetNamed(StringConstant.MeleeWeapon_AverageArmorPenetration);
    }
}
