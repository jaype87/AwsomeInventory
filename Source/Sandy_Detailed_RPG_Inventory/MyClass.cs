using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using Verse;
using Verse.AI;
using Verse.Sound;
using CombatExtended;

namespace RPG_Inventory_for_CE
{
	public class RPG_GearTab_for_CE : ITab_Pawn_Gear
	{
		private Vector2 scrollPosition = Vector2.zero;

		private float scrollViewHeight;

		private const float TopPadding = 20f;


		private const float ThingIconSize = 28f;

		private const float ThingRowHeight = 28f;

		private const float ThingLeftX = 36f;

		private const float StandardLineHeight = 22f;

		private static List<Thing> workingInvList = new List<Thing>();
		
		public static readonly Vector3 PawnTextureCameraOffset = new Vector3(0f, 0f, 0f);

        //some variables CE used
        #region CE_Field
        private const float _barHeight = 20f;
        private const float _margin = 15f;
        #endregion CE_Field

        private bool viewlist = false;

		public RPG_GearTab_for_CE()
		{
			this.size = new Vector2(550f, 500f);
			this.labelKey = "TabGear";
			this.tutorTag = "Gear";
		}

		public override bool IsVisible
		{
			get
			{
				Pawn selPawnForGear = this.SelPawnForGear;
				return this.ShouldShowInventory(selPawnForGear) || this.ShouldShowApparel(selPawnForGear) || this.ShouldShowEquipment(selPawnForGear);
			}
		}
		
		/*private bool colonist
		{
			get
			{
				Pawn selPawnForGear = this.SelPawnForGear;
				return !this.SelPawnForGear.RaceProps.IsMechanoid && !this.SelPawnForGear.RaceProps.Animal;
			}
		}*/

		private bool CanControl
		{
			get
			{
				Pawn selPawnForGear = this.SelPawnForGear;
				return !selPawnForGear.Downed && !selPawnForGear.InMentalState && (selPawnForGear.Faction == Faction.OfPlayer || selPawnForGear.IsPrisonerOfColony) && (!selPawnForGear.IsPrisonerOfColony || !selPawnForGear.Spawned || selPawnForGear.Map.mapPawns.AnyFreeColonistSpawned) && (!selPawnForGear.IsPrisonerOfColony || (!PrisonBreakUtility.IsPrisonBreaking(selPawnForGear) && (selPawnForGear.CurJob == null || !selPawnForGear.CurJob.exitMapOnArrival)));
			}
		}

		private bool CanControlColonist
		{
			get
			{
				return this.CanControl && this.SelPawnForGear.IsColonistPlayerControlled;
			}
		}

		private Pawn SelPawnForGear
		{
			get
			{
				if (base.SelPawn != null)
				{
					return base.SelPawn;
				}
				Corpse corpse = base.SelThing as Corpse;
				if (corpse != null)
				{
					return corpse.InnerPawn;
				}
				throw new InvalidOperationException("Gear tab on non-pawn non-corpse " + base.SelThing);
			}
		}

		protected override void FillTab()
		{
			Text.Font = GameFont.Small;
			Rect rect0 = new Rect(20f, 0f, 100f, 30f);
			Widgets.CheckboxLabeled(rect0, "Sandy_ViewList".Translate(), ref viewlist, false, null, null, false);
			Rect rect = new Rect(0f, 20f, this.size.x, this.size.y - 20f);
			Rect rect2 = rect.ContractedBy(10f);
			Rect position = new Rect(rect2.x, rect2.y, rect2.width, rect2.height);
			GUI.BeginGroup(position);
			Text.Font = GameFont.Small;
			GUI.color = Color.white;
			Rect outRect = new Rect(0f, 0f, position.width, position.height - 60);
			Rect viewRect = new Rect(0f, 0f, position.width - 20f, this.scrollViewHeight);
			Widgets.BeginScrollView(outRect, ref this.scrollPosition, viewRect, true);
			float num = 0f;
			if (!viewlist)
			{
				if (this.SelPawnForGear.RaceProps.Humanlike)
				{
					Rect rectstat = new Rect(374f, 0f, 128f, 50f);
					this.TryDrawMassInfo1(rectstat);
					this.TryDrawComfyTemperatureRange1(rectstat);
				}
				else
				{
					this.TryDrawMassInfo(ref num, viewRect.width);
					this.TryDrawComfyTemperatureRange(ref num, viewRect.width);
				}
			}
			else if (viewlist)
			{
				this.TryDrawMassInfo(ref num, viewRect.width);
				this.TryDrawComfyTemperatureRange(ref num, viewRect.width);
			}
			if (this.ShouldShowOverallArmor(this.SelPawnForGear) && !viewlist && this.SelPawnForGear.RaceProps.Humanlike)
			{
				Rect rectarmor = new Rect(374f, 84f, 128f, 85f);
				TooltipHandler.TipRegion(rectarmor, "OverallArmor".Translate());
				Rect rectsharp = new Rect(rectarmor.x, rectarmor.y, rectarmor.width, 27f);
				this.TryDrawOverallArmor1(rectsharp, StatDefOf.ArmorRating_Sharp, "ArmorSharp".Translate(),
				                         ContentFinder<Texture2D>.Get("UI/Icons/Sandy_ArmorSharp_Icon", true));
				Rect rectblunt = new Rect(rectarmor.x, rectarmor.y + 30f, rectarmor.width, 27f);
				this.TryDrawOverallArmor1(rectblunt, StatDefOf.ArmorRating_Blunt, "ArmorBlunt".Translate(),
				                         ContentFinder<Texture2D>.Get("UI/Icons/Sandy_ArmorBlunt_Icon", true));
				Rect rectheat = new Rect(rectarmor.x, rectarmor.y + 60f, rectarmor.width, 27f);
				this.TryDrawOverallArmor1(rectheat, StatDefOf.ArmorRating_Heat, "ArmorHeat".Translate(),
				                         ContentFinder<Texture2D>.Get("UI/Icons/Sandy_ArmorHeat_Icon", true));
			}
			else if (this.ShouldShowOverallArmor(this.SelPawnForGear))
			{
				Widgets.ListSeparator(ref num, viewRect.width, "OverallArmor".Translate());
				this.TryDrawOverallArmor(ref num, viewRect.width, StatDefOf.ArmorRating_Sharp, "ArmorSharp".Translate());
				this.TryDrawOverallArmor(ref num, viewRect.width, StatDefOf.ArmorRating_Blunt, "ArmorBlunt".Translate());
				this.TryDrawOverallArmor(ref num, viewRect.width, StatDefOf.ArmorRating_Heat, "ArmorHeat".Translate());
			}
			if (this.IsVisible && this.SelPawnForGear.RaceProps.Humanlike && !viewlist)
			{
				//Hats
				Rect newRect1 = new Rect(150f, 10f, 64f, 64f);
				GUI.DrawTexture(newRect1, ContentFinder<Texture2D>.Get("UI/Widgets/DesButBG", true));
				Rect tipRect1 = newRect1.ContractedBy(12f);
				TooltipHandler.TipRegion(tipRect1, "Sandy_Head".Translate());
				//Vests
				Rect newRect2 = new Rect(76f, 94f, 64f, 64f);
				GUI.DrawTexture(newRect2, ContentFinder<Texture2D>.Get("UI/Widgets/DesButBG", true));
				Rect tipRect2 = newRect2.ContractedBy(12f);
				TooltipHandler.TipRegion(tipRect2, "Sandy_TorsoMiddle".Translate());
				//Shirts
				Rect newRect3 = new Rect(150f, 94f, 64f, 64f);
				GUI.DrawTexture(newRect3, ContentFinder<Texture2D>.Get("UI/Widgets/DesButBG", true));
				Rect tipRect3 = newRect3.ContractedBy(12f);
				TooltipHandler.TipRegion(tipRect3, "Sandy_TorsoOnSkin".Translate());
				//Dusters
				Rect newRect4 = new Rect(224f, 94f, 64f, 64f);
				GUI.DrawTexture(newRect4, ContentFinder<Texture2D>.Get("UI/Widgets/DesButBG", true));
				Rect tipRect4 = newRect4.ContractedBy(12f);
				TooltipHandler.TipRegion(tipRect4, "Sandy_TorsoShell".Translate());
				//Belts
				Rect newRect5 = new Rect(150f, 178f, 64f, 64f);
				GUI.DrawTexture(newRect5, ContentFinder<Texture2D>.Get("UI/Widgets/DesButBG", true));
				Rect tipRect5 = newRect5.ContractedBy(12f);
				TooltipHandler.TipRegion(tipRect5, "Sandy_Belt".Translate());
				//Pants
				Rect newRect6 = new Rect(150f, 262f, 64f, 64f);
				GUI.DrawTexture(newRect6, ContentFinder<Texture2D>.Get("UI/Widgets/DesButBG", true));
				Rect tipRect6 = newRect6.ContractedBy(12f);
				TooltipHandler.TipRegion(tipRect6, "Sandy_Pants".Translate());
				Color color = new Color(1f, 1f, 1f, 1f);
				GUI.color = color;
				Rect PawnRect = new Rect(374f, 172f, 128f, 128f);
				this.DrawColonist(PawnRect, this.SelPawnForGear);
			}
			if (this.ShouldShowEquipment(this.SelPawnForGear) && !viewlist && this.SelPawnForGear.RaceProps.Humanlike)
			{
				foreach (ThingWithComps current in this.SelPawnForGear.equipment.AllEquipmentListForReading)
				{
					Rect newRect = new Rect(402f, 338f, 72f, 72f);
					GUI.DrawTexture(newRect, ContentFinder<Texture2D>.Get("UI/Widgets/DesButBG", true));
					this.DrawThingRow1(newRect, current, false);
					if (this.SelPawnForGear.story.traits.HasTrait(TraitDefOf.Brawler) && this.SelPawnForGear.equipment.Primary != null && this.SelPawnForGear.equipment.Primary.def.IsRangedWeapon)
					{
						Rect rect6 = new Rect(newRect.x, newRect.yMax - 20f, 20f, 20f);
						GUI.DrawTexture(rect6, ContentFinder<Texture2D>.Get("UI/Icons/Sandy_Forced_Icon", true));
						TooltipHandler.TipRegion(rect6, "BrawlerHasRangedWeapon".Translate());
					}
				}
			}
			else if (this.ShouldShowEquipment(this.SelPawnForGear))
			{
				Widgets.ListSeparator(ref num, viewRect.width, "Equipment".Translate());
				foreach (ThingWithComps thing in this.SelPawnForGear.equipment.AllEquipmentListForReading)
				{
					this.DrawThingRow(ref num, viewRect.width, thing, false);
				}
			}
			if (this.ShouldShowApparel(this.SelPawnForGear) && !viewlist && this.SelPawnForGear.RaceProps.Humanlike)
			{
				foreach (Apparel current2 in this.SelPawnForGear.apparel.WornApparel)
				{
					/*switch (current2.def.apparel)
					{
						case ApparelProperties a when (a.bodyPartGroups.Contains(BodyPartGroupDefOf.UpperHead) || a.bodyPartGroups.Contains(BodyPartGroupDefOf.FullHead)
						&& a.layers.Contains(ApparelLayerDefOf.Overhead)):
						break;
					}*/
					if ((current2.def.apparel.bodyPartGroups.Contains(BodyPartGroupDefOf.UpperHead)	|| current2.def.apparel.bodyPartGroups.Contains(BodyPartGroupDefOf.FullHead))
					   && current2.def.apparel.layers.Contains(ApparelLayerDefOf.Overhead))
					{
						Rect newRect = new Rect(150f, 10f, 64f, 64f);
						this.DrawThingRow1(newRect, current2, false);
					}
					if (current2.def.apparel.bodyPartGroups.Contains(BodyPartGroupDefOf.Eyes) && !current2.def.apparel.bodyPartGroups.Contains(BodyPartGroupDefOf.UpperHead)
						&& current2.def.apparel.layers.Contains(ApparelLayerDefOf.Overhead))
					{
						Rect newRect = new Rect(224f, 10f, 64f, 64f);
						GUI.DrawTexture(newRect, ContentFinder<Texture2D>.Get("UI/Widgets/DesButBG", true));
						this.DrawThingRow1(newRect, current2, false);
					}
					if (current2.def.apparel.bodyPartGroups.Contains(Sandy_Gear_DefOf.Teeth) && !current2.def.apparel.bodyPartGroups.Contains(BodyPartGroupDefOf.UpperHead)
					    && current2.def.apparel.layers.Contains(ApparelLayerDefOf.Overhead) && !current2.def.apparel.bodyPartGroups.Contains(BodyPartGroupDefOf.Eyes))
					{
						Rect newRect = new Rect(76f, 10f, 64f, 64f);
						GUI.DrawTexture(newRect, ContentFinder<Texture2D>.Get("UI/Widgets/DesButBG", true));
						this.DrawThingRow1(newRect, current2, false);
					}
					if (current2.def.apparel.bodyPartGroups.Contains(BodyPartGroupDefOf.Torso) && !current2.def.apparel.layers.Contains(ApparelLayerDefOf.Shell)
					    && current2.def.apparel.layers.Contains(ApparelLayerDefOf.Middle))
					{
						Rect newRect = new Rect(76f, 94f, 64f, 64f);
						this.DrawThingRow1(newRect, current2, false);
					}
					if (current2.def.apparel.bodyPartGroups.Contains(BodyPartGroupDefOf.Torso) && current2.def.apparel.layers.Contains(ApparelLayerDefOf.OnSkin)
					    && !current2.def.apparel.layers.Contains(ApparelLayerDefOf.Middle) && !current2.def.apparel.layers.Contains(ApparelLayerDefOf.Shell))
					{
						Rect newRect = new Rect(150f, 94f, 64f, 64f);
						this.DrawThingRow1(newRect, current2, false);
					}
					if (current2.def.apparel.bodyPartGroups.Contains(BodyPartGroupDefOf.Torso) && current2.def.apparel.layers.Contains(ApparelLayerDefOf.Shell))
					{
						Rect newRect = new Rect(224f, 94f, 64f, 64f);
						this.DrawThingRow1(newRect, current2, false);
					}
					if (current2.def.apparel.bodyPartGroups.Contains(Sandy_Gear_DefOf.Hands) && !current2.def.apparel.bodyPartGroups.Contains(BodyPartGroupDefOf.Torso)
					    && current2.def.apparel.layers.Contains(ApparelLayerDefOf.Middle) && !current2.def.apparel.bodyPartGroups.Contains(Sandy_Gear_DefOf.Shoulders)
					    && !current2.def.apparel.layers.Contains(ApparelLayerDefOf.Shell))
					{
						Rect newRect = new Rect(10f, 76f, 56f, 56f);
						GUI.DrawTexture(newRect, ContentFinder<Texture2D>.Get("UI/Widgets/DesButBG", true));
						this.DrawThingRow1(newRect, current2, false);
					}
					if (current2.def.apparel.bodyPartGroups.Contains(Sandy_Gear_DefOf.Hands) && !current2.def.apparel.bodyPartGroups.Contains(BodyPartGroupDefOf.Torso)
					    && (current2.def.apparel.layers.Contains(ApparelLayerDefOf.Shell) || current2.def.apparel.layers.Contains(ApparelLayerDefOf.Overhead)))
					{
						Rect newRect = new Rect(10f, 10f, 56f, 56f);
						GUI.DrawTexture(newRect, ContentFinder<Texture2D>.Get("UI/Widgets/DesButBG", true));
						this.DrawThingRow1(newRect, current2, false);
					}
					if (current2.def.apparel.bodyPartGroups.Contains(Sandy_Gear_DefOf.Hands) && !current2.def.apparel.bodyPartGroups.Contains(BodyPartGroupDefOf.Torso)
					    && current2.def.apparel.layers.Contains(ApparelLayerDefOf.OnSkin) && !current2.def.apparel.bodyPartGroups.Contains(Sandy_Gear_DefOf.Shoulders)
					    && !current2.def.apparel.layers.Contains(ApparelLayerDefOf.Middle) && !current2.def.apparel.layers.Contains(ApparelLayerDefOf.Shell))
					{
						Rect newRect = new Rect(10f, 142f, 56f, 56f);
						GUI.DrawTexture(newRect, ContentFinder<Texture2D>.Get("UI/Widgets/DesButBG", true));
						this.DrawThingRow1(newRect, current2, false);
					}
					if (current2.def.apparel.bodyPartGroups.Contains(Sandy_Gear_DefOf.Shoulders) && !current2.def.apparel.layers.Contains(ApparelLayerDefOf.Shell)
					    && !current2.def.apparel.bodyPartGroups.Contains(BodyPartGroupDefOf.Torso) && !current2.def.apparel.bodyPartGroups.Contains(BodyPartGroupDefOf.LeftHand)
					    && current2.def.apparel.layers.Contains(ApparelLayerDefOf.Middle) && !current2.def.apparel.bodyPartGroups.Contains(BodyPartGroupDefOf.RightHand))
					{
						Rect newRect = new Rect(298f, 142f, 56f, 56f);
						GUI.DrawTexture(newRect, ContentFinder<Texture2D>.Get("UI/Widgets/DesButBG", true));
						this.DrawThingRow1(newRect, current2, false);
					}
					if (current2.def.apparel.bodyPartGroups.Contains(Sandy_Gear_DefOf.Shoulders) && current2.def.apparel.layers.Contains(ApparelLayerDefOf.Shell)
					    && !current2.def.apparel.bodyPartGroups.Contains(BodyPartGroupDefOf.Torso) && !current2.def.apparel.bodyPartGroups.Contains(BodyPartGroupDefOf.LeftHand)
					    && !current2.def.apparel.bodyPartGroups.Contains(BodyPartGroupDefOf.RightHand))
					{
						Rect newRect = new Rect(298f, 76f, 56f, 56f);
						GUI.DrawTexture(newRect, ContentFinder<Texture2D>.Get("UI/Widgets/DesButBG", true));
						this.DrawThingRow1(newRect, current2, false);
					}
					if (current2.def.apparel.bodyPartGroups.Contains(Sandy_Gear_DefOf.Shoulders) && !current2.def.apparel.layers.Contains(ApparelLayerDefOf.Shell)
					    && !current2.def.apparel.bodyPartGroups.Contains(BodyPartGroupDefOf.Torso) && !current2.def.apparel.bodyPartGroups.Contains(BodyPartGroupDefOf.LeftHand)
					    && current2.def.apparel.layers.Contains(ApparelLayerDefOf.OnSkin) && !current2.def.apparel.layers.Contains(ApparelLayerDefOf.Middle)
					    && !current2.def.apparel.bodyPartGroups.Contains(BodyPartGroupDefOf.RightHand))
					{
						Rect newRect = new Rect(298f, 10f, 56f, 56f);
						GUI.DrawTexture(newRect, ContentFinder<Texture2D>.Get("UI/Widgets/DesButBG", true));
						this.DrawThingRow1(newRect, current2, false);
					}

                    #region CE_equipment
                    //this part display CE equipment
                    //CE tatical vest
                    if (current2.def.apparel.layers.Contains(ApparelLayerDefOf.Belt) && current2.def.apparel.bodyPartGroups.Contains(Sandy_Gear_DefOf.Shoulders))
					{
						Rect newRect = new Rect(76f, 178f, 64f, 64f);
						this.DrawThingRow1(newRect, current2, false);
					}
                    //vanila shield belt
                    if (current2.def.apparel.layers.Contains(ApparelLayerDefOf.Belt) && current2.def.apparel.bodyPartGroups.Contains(Sandy_Gear_DefOf.Waist)) {
                        Rect newRect = new Rect(150f, 178f, 64f, 64f);
                        this.DrawThingRow1(newRect, current2, false);
                    }
                    //CE back pack
                    if (current2.def.apparel.layers.Contains(ApparelLayerDefOf.Belt) && current2.def.apparel.bodyPartGroups.Contains(BodyPartGroupDefOf.Torso)) {
                        Rect newRect = new Rect(224f, 178f, 64f, 64f);
                        this.DrawThingRow1(newRect, current2, false);
                    }
                    //CE shield
                    if (current2.def.apparel.layers.Contains(ApparelLayerDefOf.Belt) && current2.def.apparel.bodyPartGroups.Contains(Sandy_Gear_DefOf.LeftArm)) {
                        Rect newRect = new Rect(320f, 338f, 72f, 72f);
                        this.DrawThingRow1(newRect, current2, false);
                    }
                    #region CE_equipment

                    if (current2.def.apparel.bodyPartGroups.Contains(BodyPartGroupDefOf.RightHand) && !current2.def.apparel.bodyPartGroups.Contains(Sandy_Gear_DefOf.Hands)
					    && current2.def.apparel.layers.Contains(ApparelLayerDefOf.Middle) && !current2.def.apparel.bodyPartGroups.Contains(BodyPartGroupDefOf.Torso)
					    && !current2.def.apparel.layers.Contains(ApparelLayerDefOf.Shell))
					{
						Rect newRect = new Rect(10f, 288f, 56f, 56f);
						GUI.DrawTexture(newRect, ContentFinder<Texture2D>.Get("UI/Widgets/DesButBG", true));
						this.DrawThingRow1(newRect, current2, false);
					}
					if (current2.def.apparel.bodyPartGroups.Contains(BodyPartGroupDefOf.RightHand) && !current2.def.apparel.bodyPartGroups.Contains(Sandy_Gear_DefOf.Hands)
					    && current2.def.apparel.layers.Contains(ApparelLayerDefOf.Shell) && !current2.def.apparel.bodyPartGroups.Contains(BodyPartGroupDefOf.Torso))
					{
						Rect newRect = new Rect(10f, 354f, 56f, 56f);
						GUI.DrawTexture(newRect, ContentFinder<Texture2D>.Get("UI/Widgets/DesButBG", true));
						this.DrawThingRow1(newRect, current2, false);
					}
					if (current2.def.apparel.bodyPartGroups.Contains(BodyPartGroupDefOf.RightHand) && !current2.def.apparel.bodyPartGroups.Contains(Sandy_Gear_DefOf.Hands)
					    && current2.def.apparel.layers.Contains(ApparelLayerDefOf.OnSkin) && !current2.def.apparel.bodyPartGroups.Contains(BodyPartGroupDefOf.Torso)
					    && !current2.def.apparel.layers.Contains(ApparelLayerDefOf.Middle) && !current2.def.apparel.layers.Contains(ApparelLayerDefOf.Shell))
					{
						Rect newRect = new Rect(10f, 222f, 56f, 56f);
						GUI.DrawTexture(newRect, ContentFinder<Texture2D>.Get("UI/Widgets/DesButBG", true));
						this.DrawThingRow1(newRect, current2, false);
					}
					if (current2.def.apparel.bodyPartGroups.Contains(BodyPartGroupDefOf.Legs) && current2.def.apparel.layers.Contains(ApparelLayerDefOf.Middle)
					    && !current2.def.apparel.layers.Contains(ApparelLayerDefOf.Shell) && !current2.def.apparel.bodyPartGroups.Contains(BodyPartGroupDefOf.Torso))
					{
						Rect newRect = new Rect(76f, 262f, 64f, 64f);
						GUI.DrawTexture(newRect, ContentFinder<Texture2D>.Get("UI/Widgets/DesButBG", true));
						this.DrawThingRow1(newRect, current2, false);
					}
					if (current2.def.apparel.bodyPartGroups.Contains(BodyPartGroupDefOf.Legs) && !current2.def.apparel.layers.Contains(ApparelLayerDefOf.Middle)
					    && current2.def.apparel.layers.Contains(ApparelLayerDefOf.OnSkin) && !current2.def.apparel.layers.Contains(ApparelLayerDefOf.Shell)
					    && !current2.def.apparel.bodyPartGroups.Contains(BodyPartGroupDefOf.Torso))
					{
						Rect newRect = new Rect(150f, 262f, 64f, 64f);
						this.DrawThingRow1(newRect, current2, false);
					}
					if (current2.def.apparel.bodyPartGroups.Contains(BodyPartGroupDefOf.Legs) && current2.def.apparel.layers.Contains(ApparelLayerDefOf.Shell)
					    && !current2.def.apparel.bodyPartGroups.Contains(BodyPartGroupDefOf.Torso))
					{
						Rect newRect = new Rect(224f, 262f, 64f, 64f);
						GUI.DrawTexture(newRect, ContentFinder<Texture2D>.Get("UI/Widgets/DesButBG", true));
						this.DrawThingRow1(newRect, current2, false);
					}
					if (current2.def.apparel.bodyPartGroups.Contains(BodyPartGroupDefOf.LeftHand) && !current2.def.apparel.bodyPartGroups.Contains(Sandy_Gear_DefOf.Hands)
					    && current2.def.apparel.layers.Contains(ApparelLayerDefOf.Middle) && !current2.def.apparel.bodyPartGroups.Contains(BodyPartGroupDefOf.Torso)
					    && !current2.def.apparel.layers.Contains(ApparelLayerDefOf.Shell))
					{
						Rect newRect = new Rect(298f, 288f, 56f, 56f);
						GUI.DrawTexture(newRect, ContentFinder<Texture2D>.Get("UI/Widgets/DesButBG", true));
						this.DrawThingRow1(newRect, current2, false);
					}
					if (current2.def.apparel.bodyPartGroups.Contains(BodyPartGroupDefOf.LeftHand) && !current2.def.apparel.bodyPartGroups.Contains(Sandy_Gear_DefOf.Hands)
					    && current2.def.apparel.layers.Contains(ApparelLayerDefOf.Shell) && !current2.def.apparel.bodyPartGroups.Contains(BodyPartGroupDefOf.Torso))
					{
						Rect newRect = new Rect(298f, 354f, 56f, 56f);
						GUI.DrawTexture(newRect, ContentFinder<Texture2D>.Get("UI/Widgets/DesButBG", true));
						this.DrawThingRow1(newRect, current2, false);
					}
					if (current2.def.apparel.bodyPartGroups.Contains(BodyPartGroupDefOf.LeftHand) && !current2.def.apparel.bodyPartGroups.Contains(Sandy_Gear_DefOf.Hands)
					    && current2.def.apparel.layers.Contains(ApparelLayerDefOf.OnSkin) && !current2.def.apparel.bodyPartGroups.Contains(BodyPartGroupDefOf.Torso)
					    && !current2.def.apparel.layers.Contains(ApparelLayerDefOf.Middle) && !current2.def.apparel.layers.Contains(ApparelLayerDefOf.Shell))
					{
						Rect newRect = new Rect(298f, 222f, 56f, 56f);
						GUI.DrawTexture(newRect, ContentFinder<Texture2D>.Get("UI/Widgets/DesButBG", true));
						this.DrawThingRow1(newRect, current2, false);
					}
					if (current2.def.apparel.bodyPartGroups.Contains(Sandy_Gear_DefOf.Feet) && current2.def.apparel.layers.Contains(ApparelLayerDefOf.OnSkin)
					    && !current2.def.apparel.layers.Contains(ApparelLayerDefOf.Shell) && !current2.def.apparel.bodyPartGroups.Contains(BodyPartGroupDefOf.Legs)
					    && !current2.def.apparel.layers.Contains(ApparelLayerDefOf.Middle))
					{
						Rect newRect = new Rect(76f, 346f, 64f, 64f);
						GUI.DrawTexture(newRect, ContentFinder<Texture2D>.Get("UI/Widgets/DesButBG", true));
						this.DrawThingRow1(newRect, current2, false);
					}
					if (current2.def.apparel.bodyPartGroups.Contains(Sandy_Gear_DefOf.Feet) && !current2.def.apparel.layers.Contains(ApparelLayerDefOf.Shell)
					    && current2.def.apparel.layers.Contains(ApparelLayerDefOf.Middle) && !current2.def.apparel.bodyPartGroups.Contains(BodyPartGroupDefOf.Legs))
					{
						Rect newRect = new Rect(150f, 346f, 64f, 64f);
						GUI.DrawTexture(newRect, ContentFinder<Texture2D>.Get("UI/Widgets/DesButBG", true));
						this.DrawThingRow1(newRect, current2, false);
					}
					if (current2.def.apparel.bodyPartGroups.Contains(Sandy_Gear_DefOf.Feet) && !current2.def.apparel.bodyPartGroups.Contains(BodyPartGroupDefOf.Legs)
					    && (current2.def.apparel.layers.Contains(ApparelLayerDefOf.Shell) || current2.def.apparel.layers.Contains(ApparelLayerDefOf.Overhead)))
					{
						Rect newRect = new Rect(224f, 346f, 64f, 64f);
						GUI.DrawTexture(newRect, ContentFinder<Texture2D>.Get("UI/Widgets/DesButBG", true));
						this.DrawThingRow1(newRect, current2, false);
					}

                    #region Jewelry_support
                    //this part add jewelry support
                    //They currently overlape with some appearoll 2 stuff
                    if (current2.def.apparel.bodyPartGroups.Contains(Sandy_Gear_DefOf.Neck) && (current2.def.apparel.layers.Contains(Sandy_Gear_DefOf.Accessories))) {
                        Rect newRect = new Rect(298f, 76f, 56f, 56f);
                        GUI.DrawTexture(newRect, ContentFinder<Texture2D>.Get("UI/Widgets/DesButBG", true));
                        this.DrawThingRow1(newRect, current2, false);
                    }
                    if (current2.def.apparel.bodyPartGroups.Contains(Sandy_Gear_DefOf.Ears) && (current2.def.apparel.layers.Contains(Sandy_Gear_DefOf.Accessories))) {
                        Rect newRect = new Rect(298f, 10f, 56f, 56f);
                        GUI.DrawTexture(newRect, ContentFinder<Texture2D>.Get("UI/Widgets/DesButBG", true));
                        this.DrawThingRow1(newRect, current2, false);
                    }
                    if (current2.def.apparel.bodyPartGroups.Contains(BodyPartGroupDefOf.LeftHand) && (current2.def.apparel.layers.Contains(Sandy_Gear_DefOf.Accessories))) {
                        Rect newRect = new Rect(298f, 142f, 56f, 56f);
                        GUI.DrawTexture(newRect, ContentFinder<Texture2D>.Get("UI/Widgets/DesButBG", true));
                        this.DrawThingRow1(newRect, current2, false);
                    }
                    #endregion Jewelry_support
                }
            }
			else if (this.ShouldShowApparel(this.SelPawnForGear))
			{
				Widgets.ListSeparator(ref num, viewRect.width, "Apparel".Translate());
				foreach (Apparel thing2 in from ap in this.SelPawnForGear.apparel.WornApparel
				orderby ap.def.apparel.bodyPartGroups[0].listOrder descending
				select ap)
				{
					this.DrawThingRow(ref num, viewRect.width, thing2, false);
				}
			}
			if (this.ShouldShowInventory(this.SelPawnForGear))
			{
				if (this.SelPawnForGear.RaceProps.Humanlike && !viewlist)
				{
					num = 420f;
				}
				else if (!this.SelPawnForGear.RaceProps.Humanlike && !viewlist)
				{
					num += StandardLineHeight;
				}
				Widgets.ListSeparator(ref num, viewRect.width, "Inventory".Translate());
				RPG_GearTab_for_CE.workingInvList.Clear();
				RPG_GearTab_for_CE.workingInvList.AddRange(this.SelPawnForGear.inventory.innerContainer);
				for (int i = 0; i < RPG_GearTab_for_CE.workingInvList.Count; i++)
				{
					this.DrawThingRow(ref num, viewRect.width, RPG_GearTab_for_CE.workingInvList[i], true);
				}
				RPG_GearTab_for_CE.workingInvList.Clear();
			}

            if (Event.current.type == EventType.Layout)
			{
				this.scrollViewHeight = num + 30f;
			}

			Widgets.EndScrollView();

            //This line draws CE bulk and mass bar
            //It can also be called inside scroll view
            TryDrawCEloadout(position.height - 60, viewRect.width);
            GUI.EndGroup();
			GUI.color = Color.white;
			Text.Anchor = TextAnchor.UpperLeft;
		}

		private void DrawColonist(Rect rect, Pawn pawn)
		{
			Vector2 pos = new Vector2(rect.width, rect.height);
			GUI.DrawTexture(rect, PortraitsCache.Get(pawn, pos, PawnTextureCameraOffset, 1.28205f));
		}

		private void DrawThingRow1(Rect rect, Thing thing, bool inventory = false)
		{
			QualityCategory c;
			if (thing.TryGetQuality(out c))
			{
				switch(c)
				{
					case QualityCategory.Legendary:
					{
						GUI.DrawTexture(rect, ContentFinder<Texture2D>.Get("UI/Frames/RPG_Legendary", true));
						break;
					}
					case QualityCategory.Masterwork:
					{
						GUI.DrawTexture(rect, ContentFinder<Texture2D>.Get("UI/Frames/RPG_Masterwork", true));
						break;
					}
					case QualityCategory.Excellent:
					{
						GUI.DrawTexture(rect, ContentFinder<Texture2D>.Get("UI/Frames/RPG_Excellent", true));
						break;
					}
					case QualityCategory.Good:
					{
						GUI.DrawTexture(rect, ContentFinder<Texture2D>.Get("UI/Frames/RPG_Good", true));
						break;
					}
					case QualityCategory.Normal:
					{
						GUI.DrawTexture(rect, ContentFinder<Texture2D>.Get("UI/Frames/RPG_Normal", true));
						break;
					}
					case QualityCategory.Poor:
					{
						GUI.DrawTexture(rect, ContentFinder<Texture2D>.Get("UI/Frames/RPG_Poor", true));
						break;
					}
					case QualityCategory.Awful:
					{
						GUI.DrawTexture(rect, ContentFinder<Texture2D>.Get("UI/Frames/RPG_Awful", true));
						break;
					}
				}
			}
			float mass = thing.GetStatValue(StatDefOf.Mass, true) * (float)thing.stackCount;
			string smass = mass.ToString("G") + " kg";
			string text = thing.LabelCap;
			Rect rect5 = rect.ContractedBy(2f);
			float num2 = rect5.height * ((float) thing.HitPoints / (float) thing.MaxHitPoints);
			rect5.yMin = rect5.yMax - num2;
			rect5.height = num2;
			GUI.DrawTexture(rect5, SolidColorMaterials.NewSolidColorTexture(new Color(0.4f, 0.47f, 0.53f, 0.44f)));
			if ((float)thing.HitPoints <= ((float)thing.MaxHitPoints/2))
			{
				Rect tattered = rect5;
				GUI.DrawTexture(rect5, SolidColorMaterials.NewSolidColorTexture(new Color(1f, 0.5f, 0.31f, 0.44f)));
			}
			if (thing.def.DrawMatSingle != null && thing.def.DrawMatSingle.mainTexture != null)
			{
				Rect rect1 = new Rect(rect.x + 4f, rect.y + 4f, rect.width - 8f, rect.height - 8f);
				Widgets.ThingIcon(rect1, thing, 1f);
			}
			if (Mouse.IsOver(rect))
			{
				GUI.color = RPG_GearTab_for_CE.HighlightColor;
				GUI.DrawTexture(rect, TexUI.HighlightTex);
				Widgets.InfoCardButton(rect.x, rect.y, thing);
				if (this.CanControl && (inventory || this.CanControlColonist || (this.SelPawnForGear.Spawned && !this.SelPawnForGear.Map.IsPlayerHome)))
				{
					Rect rect2 = new Rect(rect.xMax - 24f, rect.y, 24f, 24f);
					TooltipHandler.TipRegion(rect2, "DropThing".Translate());
					if (Widgets.ButtonImage(rect2, ContentFinder<Texture2D>.Get("UI/Buttons/Drop", true)))
					{
						SoundDefOf.Tick_High.PlayOneShotOnCamera(null);
						this.InterfaceDrop(thing);
					}
				}
			}
			Apparel apparel = thing as Apparel;
			if (apparel != null && this.SelPawnForGear.outfits != null && apparel.WornByCorpse)
			{
				Rect rect3 = new Rect(rect.xMax - 20f, rect.yMax - 20f, 20f, 20f);
				GUI.DrawTexture(rect3, ContentFinder<Texture2D>.Get("UI/Icons/Sandy_Tainted_Icon", true));
				TooltipHandler.TipRegion(rect3, "WasWornByCorpse".Translate());
			}
			if (apparel != null && this.SelPawnForGear.outfits != null && this.SelPawnForGear.outfits.forcedHandler.IsForced(apparel))
			{
				text = text + ", " + "ApparelForcedLower".Translate();
				Rect rect4 = new Rect(rect.x, rect.yMax - 20f, 20f, 20f);
				GUI.DrawTexture(rect4, ContentFinder<Texture2D>.Get("UI/Icons/Sandy_Forced_Icon", true));
				TooltipHandler.TipRegion(rect4, "ForcedApparel".Translate());
			}
			Text.WordWrap = true;
			string text2 = thing.DescriptionDetailed;
			string text3 = text + "\n" + text2 + "\n" + smass;
			if (thing.def.useHitPoints)
			{
				string text4 = text3;
				text3 = string.Concat(new object[]
				{
					text4,
					"\n",
					thing.HitPoints,
					" / ",
					thing.MaxHitPoints
				});
			}
			TooltipHandler.TipRegion(rect, text3);
		}

        //This function was changed
        //CE has a tool tip that, when mouseover overall armor value, it will display armor by body parts
        //It doesnot call CE function, just vanila funcions
		private void TryDrawOverallArmor1(Rect rect, StatDef stat, string label, Texture image)
		{
			float num = 0f;
            List<Apparel> wornApparel = SelPawnForGear.apparel.WornApparel;
            for (int i = 0; i < wornApparel.Count; i++) {
                num += Mathf.Clamp01(wornApparel[i].GetStatValue(stat, true)) * wornApparel[i].def.apparel.HumanBodyCoverage;
            }
            num = Mathf.Clamp01(num);
            if (num > 0.005f) {
                BodyPartRecord bpr = new BodyPartRecord();
                List<BodyPartRecord> bpList = SelPawnForGear.RaceProps.body.AllParts;
                string text = "";
                for (int i = 0; i < bpList.Count; i++) {
                    float armorValue = 0f;
                    BodyPartRecord part = bpList[i];
                    if (part.depth == BodyPartDepth.Outside && (part.coverage >= 0.1 || (part.def == BodyPartDefOf.Eye || part.def == BodyPartDefOf.Neck))) {
                        text += part.LabelCap + ": ";
                        for (int j = wornApparel.Count - 1; j >= 0; j--) {
                            Apparel apparel = wornApparel[j];
                            if (apparel.def.apparel.CoversBodyPart(part)) {
                                armorValue += Mathf.Clamp01(apparel.GetStatValue(stat, true));
                            }
                        }
                        //note: CE change the armor value from percentage to a flat value
                        text += Mathf.Clamp01(armorValue).ToString("0.##") + "\n";
                    }
                }
                TooltipHandler.TipRegion(rect, text);
                Rect rect1 = new Rect(rect.x, rect.y, 24f, 27f);
                GUI.DrawTexture(rect1, image);
                TooltipHandler.TipRegion(rect1, label);
                Rect rect2 = new Rect(rect.x + 30f, rect.y + 3f, 104f, 24f);
                //note: CE change the armor value from percentage to a flat value
                Widgets.Label(rect2, Mathf.Clamp01(num).ToString("0.##"));
            }
		}

		private void TryDrawMassInfo1(Rect rect)
		{
			if (this.SelPawnForGear.Dead || !this.ShouldShowInventory(this.SelPawnForGear))
			{
				return;
			}
			Rect rect1 = new Rect(rect.x, rect.y, 24f, 24f);
			GUI.DrawTexture(rect1, ContentFinder<Texture2D>.Get("UI/Icons/Sandy_MassCarried_Icon", true));
			TooltipHandler.TipRegion(rect1, "SandyMassCarried".Translate());
			float num = MassUtility.GearAndInventoryMass(this.SelPawnForGear);
			float num2 = MassUtility.Capacity(this.SelPawnForGear, null);
			Rect rect2 = new Rect(rect.x + 30f, rect.y + 2f, 104f, 24f);
			Widgets.Label(rect2, "SandyMassValue".Translate(num.ToString("0.##"), num2.ToString("0.##")));
		}

        //This function was changed to add Celcius and Farenheight support
        //Simplly changed the temperaure display into 2 lines
        //I used some free art from the internet, you might want to change that
        private void TryDrawComfyTemperatureRange1(Rect rect)
		{
			if (this.SelPawnForGear.Dead)
			{
				return;
			}
			Rect rect1 = new Rect(rect.x, rect.y + 26f, 24f, 24f);
			GUI.DrawTexture(rect1, ContentFinder<Texture2D>.Get("UI/Icons/minumun_temperature", true));
			TooltipHandler.TipRegion(rect1, "ComfyTemperatureRange".Translate());
			float statValue = this.SelPawnForGear.GetStatValue(StatDefOf.ComfyTemperatureMin, true);
			Rect rect2 = new Rect(rect.x + 30f, rect.y + 28f, 104f, 24f);
			Widgets.Label(rect2, string.Concat(new string[]
			{
				" ",
				statValue.ToStringTemperature("F0")
			}));

            rect1 = new Rect(rect.x, rect.y + 52f, 24f, 24f);
            GUI.DrawTexture(rect1, ContentFinder<Texture2D>.Get("UI/Icons/max_temperature", true));
            TooltipHandler.TipRegion(rect1, "ComfyTemperatureRange".Translate());
            float statValue2 = this.SelPawnForGear.GetStatValue(StatDefOf.ComfyTemperatureMax, true);
            rect2 = new Rect(rect.x + 30f, rect.y + 56f, 104f, 24f);
            Widgets.Label(rect2, string.Concat(new string[]
            {
                " ",
                statValue2.ToStringTemperature("F0")
            }));

        }
		
        // This function was changed to add CE RMB menu
        // The RMB menu allows player to equip weapons and appeals from inventory
		private void DrawThingRow(ref float y, float width, Thing thing, bool inventory = false)
		{
			Rect rect = new Rect(0f, y, width, 28f);
			Widgets.InfoCardButton(rect.width - 24f, y, thing);
			rect.width -= 24f;
			if (this.CanControl && (inventory || this.CanControlColonist || (this.SelPawnForGear.Spawned && !this.SelPawnForGear.Map.IsPlayerHome)))
			{
				Rect rect2 = new Rect(rect.width - 24f, y, 24f, 24f);
				TooltipHandler.TipRegion(rect2, "DropThing".Translate());
				if (Widgets.ButtonImage(rect2, ContentFinder<Texture2D>.Get("UI/Buttons/Drop", true)))
				{
					SoundDefOf.Tick_High.PlayOneShotOnCamera(null);
					this.InterfaceDrop(thing);
				}
				rect.width -= 24f;
			}
			if (this.CanControlColonist)
			{
				if ((thing.def.IsNutritionGivingIngestible || thing.def.IsNonMedicalDrug) && thing.IngestibleNow && base.SelPawn.WillEat(thing, null))
				{
					Rect rect3 = new Rect(rect.width - 24f, y, 24f, 24f);
					TooltipHandler.TipRegion(rect3, "ConsumeThing".Translate(thing.LabelNoCount, thing));
					if (Widgets.ButtonImage(rect3, ContentFinder<Texture2D>.Get("UI/Buttons/Ingest", true)))
					{
						SoundDefOf.Tick_High.PlayOneShotOnCamera(null);
						this.InterfaceIngest(thing);
					}
				}
				rect.width -= 24f;
			}
			Rect rect4 = rect;
			rect4.xMin = rect4.xMax - 60f;
			CaravanThingsTabUtility.DrawMass(thing, rect4);
			rect.width -= 60f;
			if (Mouse.IsOver(rect))
			{
				GUI.color = RPG_GearTab_for_CE.HighlightColor;
				GUI.DrawTexture(rect, TexUI.HighlightTex);
			}
			if (thing.def.DrawMatSingle != null && thing.def.DrawMatSingle.mainTexture != null)
			{
				Widgets.ThingIcon(new Rect(4f, y, 28f, 28f), thing, 1f);
			}
			Text.Anchor = TextAnchor.MiddleLeft;
			GUI.color = RPG_GearTab_for_CE.ThingLabelColor;
			Rect rect5 = new Rect(36f, y, rect.width - 36f, rect.height);
			string text = thing.LabelCap;
			Apparel apparel = thing as Apparel;
			if (apparel != null && this.SelPawnForGear.outfits != null && this.SelPawnForGear.outfits.forcedHandler.IsForced(apparel))
			{
				text = text + ", " + "ApparelForcedLower".Translate();
			}
			Text.WordWrap = false;
			Widgets.Label(rect5, text.Truncate(rect5.width, null));
			Text.WordWrap = true;
			string text2 = thing.DescriptionDetailed;
			if (thing.def.useHitPoints)
			{
				string text3 = text2;
				text2 = string.Concat(new object[]
				{
					text3,
					"\n",
					thing.HitPoints,
					" / ",
					thing.MaxHitPoints
				});
			}
			TooltipHandler.TipRegion(rect, text2);
            Rect thingLabelRect = new Rect(ThingLeftX, y, rect.width - ThingLeftX, ThingRowHeight);
            y += 28f;

            // RMB menu
            if (Widgets.ButtonInvisible(thingLabelRect) && Event.current.button == 1) {
                List<FloatMenuOption> floatOptionList = new List<FloatMenuOption>();
                floatOptionList.Add(new FloatMenuOption("ThingInfo".Translate(), delegate {
                    Find.WindowStack.Add(new Dialog_InfoCard(thing));
                }, MenuOptionPriority.Default, null, null));
                if (CanControl) {
                    // Equip option
                    ThingWithComps eq = thing as ThingWithComps;
                    if (eq != null && eq.TryGetComp<CompEquippable>() != null) {
                        CompInventory compInventory = SelPawnForGear.TryGetComp<CompInventory>();
                        if (compInventory != null) {
                            FloatMenuOption equipOption;
                            string eqLabel = GenLabel.ThingLabel(eq.def, eq.Stuff, 1);
                            if (SelPawnForGear.equipment.AllEquipmentListForReading.Contains(eq) && SelPawnForGear.inventory != null) {
                                equipOption = new FloatMenuOption("CE_PutAway".Translate(eqLabel),
                                    new Action(delegate {
                                        SelPawnForGear.equipment.TryTransferEquipmentToContainer(SelPawnForGear.equipment.Primary, SelPawnForGear.inventory.innerContainer);
                                    }));
                            } else if (!SelPawnForGear.health.capacities.CapableOf(PawnCapacityDefOf.Manipulation)) {
                                equipOption = new FloatMenuOption("CannotEquip".Translate(eqLabel), null);
                            } else {
                                string equipOptionLabel = "Equip".Translate(eqLabel);
                                if (eq.def.IsRangedWeapon && SelPawnForGear.story != null && SelPawnForGear.story.traits.HasTrait(TraitDefOf.Brawler)) {
                                    equipOptionLabel = equipOptionLabel + " " + "EquipWarningBrawler".Translate();
                                }
                                equipOption = new FloatMenuOption(
                                    equipOptionLabel,
                                    (SelPawnForGear.story != null && SelPawnForGear.story.WorkTagIsDisabled(WorkTags.Violent))
                                    ? null
                                    : new Action(delegate {
                                        compInventory.TrySwitchToWeapon(eq);
                                    }));
                            }
                            floatOptionList.Add(equipOption);
                        }
                    }
                    // Drop option
                    Action dropApparel = delegate {
                        SoundDefOf.Tick_High.PlayOneShotOnCamera();
                        InterfaceDrop(thing);
                    };
                    if (CanControl && thing.IngestibleNow && base.SelPawn.RaceProps.CanEverEat(thing)) {
                        Action eatFood = delegate {
                            SoundDefOf.Tick_High.PlayOneShotOnCamera();
                            InterfaceIngest(thing);
                        };
                        string label = thing.def.ingestible.ingestCommandString.NullOrEmpty() ? "ConsumeThing".Translate(thing.LabelShort, thing) : string.Format(thing.def.ingestible.ingestCommandString, thing.LabelShort);
                        floatOptionList.Add(new FloatMenuOption(label, eatFood));
                    }
                    floatOptionList.Add(new FloatMenuOption("DropThing".Translate(), dropApparel));
                }
                FloatMenu window = new FloatMenu(floatOptionList, thing.LabelCap, false);
                Find.WindowStack.Add(window);
            }
            // end menu
        }

        //This function was changed
        //CE has a tool tip that, when mouseover overall armor value, it will display armor by body parts
        //It doesnot call CE function, just vanila funcions
        private void TryDrawOverallArmor(ref float curY, float width, StatDef stat, string label) {
            if (SelPawnForGear.RaceProps.body != BodyDefOf.Human) {
                return;
            }
            float num = 0f;
            List<Apparel> wornApparel = SelPawnForGear.apparel.WornApparel;
            for (int i = 0; i < wornApparel.Count; i++) {
                num += Mathf.Clamp01(wornApparel[i].GetStatValue(stat, true)) * wornApparel[i].def.apparel.HumanBodyCoverage;
            }
            num = Mathf.Clamp01(num);
            if (num > 0.005f) {
                Rect rect = new Rect(0f, curY, width, StandardLineHeight);
                BodyPartRecord bpr = new BodyPartRecord();
                List<BodyPartRecord> bpList = SelPawnForGear.RaceProps.body.AllParts;
                string text = "";
                for (int i = 0; i < bpList.Count; i++) {
                    float armorValue = 0f;
                    BodyPartRecord part = bpList[i];
                    if (part.depth == BodyPartDepth.Outside && (part.coverage >= 0.1 || (part.def == BodyPartDefOf.Eye || part.def == BodyPartDefOf.Neck))) {
                        text += part.LabelCap + ": ";
                        for (int j = wornApparel.Count - 1; j >= 0; j--) {
                            Apparel apparel = wornApparel[j];
                            if (apparel.def.apparel.CoversBodyPart(part)) {
                                armorValue += Mathf.Clamp01(apparel.GetStatValue(stat, true));
                            }
                        }
                        text += Mathf.Clamp01(armorValue).ToString("0.###") + "\n";
                    }
                }
                TooltipHandler.TipRegion(rect, text);

                Widgets.Label(rect, label.Truncate(200f, null));
                rect.xMin += 200;
                Widgets.Label(rect, Mathf.Clamp01(num).ToString("F3"));
            }
            curY += StandardLineHeight;
        }

        private void TryDrawMassInfo(ref float curY, float width)
		{
			if (this.SelPawnForGear.Dead || !this.ShouldShowInventory(this.SelPawnForGear))
			{
				return;
			}
			Rect rect = new Rect(0f, curY, width, 22f);
			float num = MassUtility.GearAndInventoryMass(this.SelPawnForGear);
			float num2 = MassUtility.Capacity(this.SelPawnForGear, null);
			Widgets.Label(rect, "MassCarried".Translate(num.ToString("0.##"), num2.ToString("0.##")));
			curY += 22f;
		}

		private void TryDrawComfyTemperatureRange(ref float curY, float width)
		{
			if (this.SelPawnForGear.Dead)
			{
				return;
			}
			Rect rect = new Rect(0f, curY, width, 22f);
			float statValue = this.SelPawnForGear.GetStatValue(StatDefOf.ComfyTemperatureMin, true);
            float statValue2 = this.SelPawnForGear.GetStatValue(StatDefOf.ComfyTemperatureMax, true);
            Widgets.Label(rect, string.Concat(new string[]
			{
				"ComfyTemperatureRange".Translate(),
				": ",
				statValue.ToStringTemperature("F0"),
                "~",
                statValue2.ToStringTemperature("F0")
            }));
			curY += 22f;
        }

		private void InterfaceDrop(Thing t)
		{
			ThingWithComps thingWithComps = t as ThingWithComps;
			Apparel apparel = t as Apparel;
			if (apparel != null && this.SelPawnForGear.apparel != null && this.SelPawnForGear.apparel.WornApparel.Contains(apparel))
			{
				this.SelPawnForGear.jobs.TryTakeOrderedJob(new Job(JobDefOf.RemoveApparel, apparel), JobTag.Misc);
			}
			else if (thingWithComps != null && this.SelPawnForGear.equipment != null && this.SelPawnForGear.equipment.AllEquipmentListForReading.Contains(thingWithComps))
			{
				this.SelPawnForGear.jobs.TryTakeOrderedJob(new Job(JobDefOf.DropEquipment, thingWithComps), JobTag.Misc);
			}
			else if (!t.def.destroyOnDrop)
			{
				Thing thing;
				this.SelPawnForGear.inventory.innerContainer.TryDrop(t, this.SelPawnForGear.Position, this.SelPawnForGear.Map, ThingPlaceMode.Near, out thing, null, null);
			}
		}

		private void InterfaceIngest(Thing t)
		{
			Job job = new Job(JobDefOf.Ingest, t);
			job.count = Mathf.Min(t.stackCount, t.def.ingestible.maxNumToIngestAtOnce);
			job.count = Mathf.Min(job.count, FoodUtility.WillIngestStackCountOf(this.SelPawnForGear, t.def, t.GetStatValue(StatDefOf.Nutrition, true)));
			this.SelPawnForGear.jobs.TryTakeOrderedJob(job, JobTag.Misc);
		}

		private bool ShouldShowInventory(Pawn p)
		{
			return p.RaceProps.Humanlike || p.inventory.innerContainer.Any;
		}

		private bool ShouldShowApparel(Pawn p)
		{
			return p.apparel != null && (p.RaceProps.Humanlike || p.apparel.WornApparel.Any<Apparel>());
		}

		private bool ShouldShowEquipment(Pawn p)
		{
			return p.equipment != null;
		}

		private bool ShouldShowOverallArmor(Pawn p)
		{
			return p.RaceProps.Humanlike || this.ShouldShowApparel(p) || p.GetStatValue(StatDefOf.ArmorRating_Sharp, true) > 0f || p.GetStatValue(StatDefOf.ArmorRating_Blunt, true) > 0f || p.GetStatValue(StatDefOf.ArmorRating_Heat, true) > 0f;
		}

        //This function draw CE bulk and mass bar
        //It can be called inside or ouside the scroll
        //This function need to call CE
        private void TryDrawCEloadout(float y, float width) {
            CompInventory comp = SelPawn.TryGetComp<CompInventory>();
            if (comp != null) {

                PlayerKnowledgeDatabase.KnowledgeDemonstrated(CE_ConceptDefOf.CE_InventoryWeightBulk, KnowledgeAmount.FrameDisplayed);
                // adjust rects if comp found
                Rect weightRect = new Rect(_margin, y + _margin / 2, width, _barHeight);
                Rect bulkRect = new Rect(_margin, weightRect.yMax + _margin / 2, width, _barHeight);

                // draw bars
                Utility_Loadouts.DrawBar(bulkRect, comp.currentBulk, comp.capacityBulk, "CE_Bulk".Translate(), SelPawn.GetBulkTip());
                Utility_Loadouts.DrawBar(weightRect, comp.currentWeight, comp.capacityWeight, "CE_Weight".Translate(), SelPawn.GetWeightTip());

                // draw text overlays on bars
                Text.Font = GameFont.Small;
                Text.Anchor = TextAnchor.MiddleCenter;

                string currentBulk = CE_StatDefOf.CarryBulk.ValueToString(comp.currentBulk, CE_StatDefOf.CarryBulk.toStringNumberSense);
                string capacityBulk = CE_StatDefOf.CarryBulk.ValueToString(comp.capacityBulk, CE_StatDefOf.CarryBulk.toStringNumberSense);
                Widgets.Label(bulkRect, currentBulk + "/" + capacityBulk);

                string currentWeight = comp.currentWeight.ToString("0.#");
                string capacityWeight = CE_StatDefOf.CarryWeight.ValueToString(comp.capacityWeight, CE_StatDefOf.CarryWeight.toStringNumberSense);
                Widgets.Label(weightRect, currentWeight + "/" + capacityWeight);

                Text.Anchor = TextAnchor.UpperLeft;
            }
        }
    }
}