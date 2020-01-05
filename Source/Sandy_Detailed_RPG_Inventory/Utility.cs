using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using Verse.Sound;
using Verse.AI;
using RimWorld;
using RimWorld.Planet;
using UnityEngine;
//using CombatExtended;

namespace RPG_Inventory_Remake
{
    public static class Utility
    {
        #region Fields  

        private const float _barHeight = 20f;
        private const float _margin = 15f;
        private const float _thingIconSize = 28f;
        private const float _thingLeftX = 36f;
        private const float _thingRowHeight = 28f;
        private const float _topPadding = 20f;
        public const float StandardLineHeight = 22f;
        private static readonly Color _highlightColor = new Color(0.5f, 0.5f, 0.5f, 1f);
        private static readonly Color _thingLabelColor = new Color(0.9f, 0.9f, 0.9f, 1f);
        private static Vector2 _scrollPosition = Vector2.zero;
        public static readonly Vector3 PawnTextureCameraOffset = new Vector3(0f, 0f, 0f);

        private static float _scrollViewHeight;

        private static List<Thing> workingInvList = new List<Thing>();

        #endregion Fields

        //public enum ListOrders
        //{
        //    UpperHead = 200, FullHead = 199, Eyes = 198, Teeth = 197, Mouth = 196,
        //    Neck = 180,
        //    Torso = 100, Arms = 90, LeftArm = 89, RightArm = 88,
        //    Shoulders = 85, LeftShoulder = 84, RightShoulder = 83,
        //    Hands = 80, LeftHand = 79, RightHand = 78,
        //    Waist = 50,
        //    Legs = 10,
        //    Feet = 9
        //}


        public static bool ShouldShowOverallArmor(Pawn p)
        {
            return p.RaceProps.Humanlike || ShouldShowApparel(p) || p.GetStatValue(StatDefOf.ArmorRating_Sharp, true) > 0f || p.GetStatValue(StatDefOf.ArmorRating_Blunt, true) > 0f || p.GetStatValue(StatDefOf.ArmorRating_Heat, true) > 0f;
        }

        public static bool ShouldShowInventory(Pawn p)
        {
            return p.RaceProps.Humanlike || p.inventory.innerContainer.Any;
        }

        public static bool ShouldShowApparel(Pawn p)
        {
            return p.apparel != null && (p.RaceProps.Humanlike || p.apparel.WornApparel.Any<Apparel>());
        }

        public static bool ShouldShowEquipment(Pawn p)
        {
            return p.equipment != null;
        }

        public static void TryDrawComfyTemperatureRange(Pawn selPawn, ref float curY, float width)
        {
            if (selPawn.Dead)
            {
                return;
            }
            Rect rect = new Rect(0f, curY, width, 22f);
            float statValue = selPawn.GetStatValue(StatDefOf.ComfyTemperatureMin, true);
            float statValue2 = selPawn.GetStatValue(StatDefOf.ComfyTemperatureMax, true);
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

        /// <summary>
        ///     Draw overall armor in a format as "Sharp   100 Rha", wihout the double quotes
        /// </summary>
        /// <param name="selPawn"></param>
        /// <param name="curY">current Y position</param>
        /// <param name="width"></param>
        /// <param name="stat"></param>
        /// <param name="label"></param>
        /// <param name="unit"></param>
        public static void TryDrawOverallArmor(Pawn selPawn, ref float curY, float width, StatDef stat, string label, string unit)
        {
            Rect rect = new Rect(0f, curY, width, StandardLineHeight);
            string toolTipText = "";
            float armorRating = CalculateArmorByParts(selPawn, stat, ref toolTipText, unit);
            if (toolTipText.NullOrEmpty())
            {
                TooltipHandler.TipRegion(rect, toolTipText);
            }
            DrawArmorRatingWithText(rect, label, armorRating, unit);
            curY += StandardLineHeight;
        }

        /// <summary>
        ///     Draw overall armor for the jealous tab
        /// </summary>
        /// <param name="selPawn"></param>
        /// <param name="rect"></param>
        /// <param name="stat"></param>
        /// <param name="label"></param>
        /// <param name="image"></param>
        /// <param name="unit"></param>
        public static void TryDrawOverallArmorCE(Pawn selPawn, Rect rect, StatDef stat, string label, Texture image, string unit)
        {
            string text = "";
            float armorRating = CalculateArmorByParts(selPawn, stat, ref text, unit);
            Rect rect1 = new Rect(rect.x, rect.y, 24f, 27f);
            GUI.DrawTexture(rect1, image);
            TooltipHandler.TipRegion(rect1, label);
            Rect rect2 = new Rect(rect.x + 60f, rect.y + 3f, 104f, 24f);
            if (text.NullOrEmpty())
            {
                TooltipHandler.TipRegion(rect2, text);
            }
            Widgets.Label(rect2, armorRating.ToStringPercent());
        }

        private static string FormatArmorValue(float value, string unit)
        {
            var asPercent = unit.Equals("%");
            if (asPercent)
            {
                value *= 100f;
            }
            return value.ToStringByStyle(asPercent ? ToStringStyle.FloatMaxOne : ToStringStyle.FloatMaxTwo) + unit;
        }

        // RimWorld.ITab_Pawn_Gear
        private static void InterfaceIngest(Thing t, Pawn selPawn)
        {
            Job job = new Job(JobDefOf.Ingest, t);
            job.count = Mathf.Min(t.stackCount, t.def.ingestible.maxNumToIngestAtOnce);
            job.count = Mathf.Min(job.count, FoodUtility.WillIngestStackCountOf(selPawn, t.def, t.GetStatValue(StatDefOf.Nutrition, true)));
            selPawn.jobs.TryTakeOrderedJob(job, JobTag.Misc);
        }

        public static void InterfaceDrop(Thing t, Pawn pawn)
        {
            ThingWithComps thingWithComps = t as ThingWithComps;
            Apparel apparel = t as Apparel;
            if (apparel != null && pawn.apparel != null && pawn.apparel.WornApparel.Contains(apparel))
            {
                pawn.jobs.TryTakeOrderedJob(new Job(JobDefOf.RemoveApparel, apparel));
            }
            else if (thingWithComps != null && pawn.equipment != null && pawn.equipment.AllEquipmentListForReading.Contains(thingWithComps))
            {
                pawn.jobs.TryTakeOrderedJob(new Job(JobDefOf.DropEquipment, thingWithComps));
            }
            else if (!t.def.destroyOnDrop)
            {
                pawn.inventory.innerContainer.TryDrop(t, pawn.Position, pawn.Map, ThingPlaceMode.Near, out Thing _);
            }
        }

        //public static void InterfaceDropCE(Thing t, Pawn selPawn)
        //{
        //    if (selPawn.HoldTrackerIsHeld(t))
        //        selPawn.HoldTrackerForget(t);
        //    ThingWithComps thingWithComps = t as ThingWithComps;
        //    Apparel apparel = t as Apparel;
        //    if (apparel != null && selPawn.apparel != null && selPawn.apparel.WornApparel.Contains(apparel))
        //    {
        //        selPawn.jobs.TryTakeOrderedJob(new Job(JobDefOf.RemoveApparel, apparel));
        //    }
        //    else if (thingWithComps != null && selPawn.equipment != null && selPawn.equipment.AllEquipmentListForReading.Contains(thingWithComps))
        //    {
        //        selPawn.jobs.TryTakeOrderedJob(new Job(JobDefOf.DropEquipment, thingWithComps));
        //    }
        //    else if (!t.def.destroyOnDrop)
        //    {
        //        Thing thing;
        //        Thing dropThing = t;
        //        if (t.def.Minifiable)
        //            dropThing = selPawn.inventory.innerContainer.FirstOrDefault(x => x.GetInnerIfMinified().ThingID == t.ThingID);
        //        selPawn.inventory.innerContainer.TryDrop(dropThing, selPawn.Position, selPawn.Map, ThingPlaceMode.Near, out thing, null);
        //    }
        //}

        //private static void InterfaceDropHaul(Thing t, Pawn selPawn)
        //{
        //    if (selPawn.HoldTrackerIsHeld(t))
        //        selPawn.HoldTrackerForget(t);
        //    ThingWithComps thingWithComps = t as ThingWithComps;
        //    Apparel apparel = t as Apparel;
        //    if (apparel != null && selPawn.apparel != null && selPawn.apparel.WornApparel.Contains(apparel))
        //    {
        //        selPawn.jobs.TryTakeOrderedJob(new Job(JobDefOf.RemoveApparel, apparel) { haulDroppedApparel = true });
        //    }
        //    else if (thingWithComps != null && selPawn.equipment != null && selPawn.equipment.AllEquipmentListForReading.Contains(thingWithComps))
        //    {
        //        selPawn.jobs.TryTakeOrderedJob(new Job(JobDefOf.DropEquipment, thingWithComps));
        //    }
        //    else if (!t.def.destroyOnDrop)
        //    {
        //        Thing thing;
        //        selPawn.inventory.innerContainer.TryDrop(t, selPawn.Position, selPawn.Map, ThingPlaceMode.Near, out thing);
        //    }
        //}

        // Serve straight up from source, no idea why it is made private
        public static void DrawThingRow(RPG_Pawn selPawn, ref float y, float width, Thing thing, bool inventory = false)
        {
            Rect rect = new Rect(0f, y, width, 28f);
            Widgets.InfoCardButton(rect.width - 24f, y, thing);
            rect.width -= 24f;
            if (selPawn.CanControl && (inventory || selPawn.CanControlColonist || (selPawn.Pawn.Spawned && !selPawn.Pawn.Map.IsPlayerHome)))
            {
                Rect rect2 = new Rect(rect.width - 24f, y, 24f, 24f);
                TooltipHandler.TipRegion(rect2, "DropThing".Translate());
                if (Widgets.ButtonImage(rect2, TexButton.Drop))
                {
                    SoundDefOf.Tick_High.PlayOneShotOnCamera();
                    InterfaceDrop(thing, selPawn.Pawn);
                }
                rect.width -= 24f;
            }
            if (selPawn.CanControlColonist)
            {
                if ((thing.def.IsNutritionGivingIngestible || thing.def.IsNonMedicalDrug) && thing.IngestibleNow && selPawn.Pawn.WillEat(thing))
                {
                    Rect rect3 = new Rect(rect.width - 24f, y, 24f, 24f);
                    TooltipHandler.TipRegion(rect3, "ConsumeThing".Translate(thing.LabelNoCount, thing));
                    if (Widgets.ButtonImage(rect3, TexButton.Ingest))
                    {
                        SoundDefOf.Tick_High.PlayOneShotOnCamera();
                        InterfaceIngest(thing, selPawn.Pawn);
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
                GUI.color = ITab_Pawn_Gear.HighlightColor;
                GUI.DrawTexture(rect, TexUI.HighlightTex);
            }
            if (thing.def.DrawMatSingle != null && thing.def.DrawMatSingle.mainTexture != null)
            {
                Widgets.ThingIcon(new Rect(4f, y, 28f, 28f), thing);
            }
            Text.Anchor = TextAnchor.MiddleLeft;
            GUI.color = ITab_Pawn_Gear.ThingLabelColor;
            Rect rect5 = new Rect(36f, y, rect.width - 36f, rect.height);
            string text = thing.LabelCap;
            Apparel apparel = thing as Apparel;
            if (apparel != null && selPawn.Pawn.outfits != null && selPawn.Pawn.outfits.forcedHandler.IsForced(apparel))
            {
                text = text + ", " + "ApparelForcedLower".Translate();
            }
            Text.WordWrap = false;
            Widgets.Label(rect5, text.Truncate(rect5.width));
            Text.WordWrap = true;
            string text2 = thing.DescriptionDetailed;
            if (thing.def.useHitPoints)
            {
                string text3 = text2;
                text2 = text3 + "\n" + thing.HitPoints + " / " + thing.MaxHitPoints;
            }
            TooltipHandler.TipRegion(rect, text2);
            y += 28f;
        }

        //public static void DrawThingRowCE(RPG_Pawn selPawn, ref float y, float width, Thing thing, bool showDropButtonIfPrisoner = false)
        //{

        //    Rect rect = new Rect(0f, y, width, _thingRowHeight);
        //    Widgets.InfoCardButton(rect.width - 24f, y, thing);
        //    rect.width -= 24f;
        //    if (selPawn.CanControl ||
        //       (selPawn.Pawn.Faction == Faction.OfPlayer && selPawn.Pawn.RaceProps.packAnimal) ||
        //       (showDropButtonIfPrisoner && selPawn.Pawn.IsPrisonerOfColony))
        //    {
        //        Rect dropRect = new Rect(rect.width - 24f, y, 24f, 24f);
        //        TooltipHandler.TipRegion(dropRect, "DropThing".Translate());
        //        if (Widgets.ButtonImage(dropRect, TexButton.Drop))
        //        {
        //            SoundDefOf.Tick_High.PlayOneShotOnCamera();
        //            InterfaceDrop(thing, selPawn.Pawn);
        //        }
        //        rect.width -= 24f;
        //    }
        //    if (selPawn.CanControlColonist)
        //    {
        //        if ((thing.def.IsNutritionGivingIngestible || thing.def.IsNonMedicalDrug) && thing.IngestibleNow && selPawn.Pawn.WillEat(thing, null))
        //        {
        //            Rect rect3 = new Rect(rect.width - 24f, y, 24f, 24f);
        //            TooltipHandler.TipRegion(rect3, "ConsumeThing".Translate(thing.LabelNoCount, thing));
        //            if (Widgets.ButtonImage(rect3, TexButton.Ingest))
        //            {
        //                SoundDefOf.Tick_High.PlayOneShotOnCamera(null);
        //                InterfaceIngest(thing, selPawn.Pawn);
        //            }
        //        }
        //        rect.width -= 24f;
        //    }
        //    Rect rect4 = rect;
        //    rect4.xMin = rect4.xMax - 60f;
        //    CaravanThingsTabUtility.DrawMass(thing, rect4);
        //    rect.width -= 60f;
        //    if (Mouse.IsOver(rect))
        //    {
        //        GUI.color = _highlightColor;
        //        GUI.DrawTexture(rect, TexUI.HighlightTex);
        //    }

        //    if (thing.def.DrawMatSingle != null && thing.def.DrawMatSingle.mainTexture != null)
        //    {
        //        Widgets.ThingIcon(new Rect(4f, y, _thingIconSize, _thingIconSize), thing, 1f);
        //    }
        //    Text.Anchor = TextAnchor.MiddleLeft;
        //    GUI.color = ITab_Pawn_Gear.ThingLabelColor;
        //    Rect thingLabelRect = new Rect(_thingLeftX, y, rect.width - _thingLeftX, _thingRowHeight);
        //    string thingLabel = thing.LabelCap;
        //    if ((thing is Apparel && selPawn.Pawn.outfits != null && selPawn.Pawn.outfits.forcedHandler.IsForced((Apparel)thing))
        //        || (selPawn.Pawn.inventory != null && selPawn.Pawn.HoldTrackerIsHeld(thing)))
        //    {
        //        thingLabel = thingLabel + ", " + "ApparelForcedLower".Translate();
        //    }

        //    Text.WordWrap = false;
        //    Widgets.Label(thingLabelRect, thingLabel.Truncate(thingLabelRect.width, null));
        //    Text.WordWrap = true;
        //    string text2 = string.Concat(new object[]
        //    {
        //        thing.LabelCap,
        //        "\n",
        //        thing.DescriptionDetailed,
        //        "\n",
        //        thing.GetWeightAndBulkTip()
        //    });
        //    if (thing.def.useHitPoints)
        //    {
        //        string text3 = text2;
        //        text2 = string.Concat(new object[]
        //        {
        //            text3,
        //            "\n",
        //            "HitPointsBasic".Translate().CapitalizeFirst(),
        //            ": ",
        //            thing.HitPoints,
        //            " / ",
        //            thing.MaxHitPoints
        //        });
        //    }
        //    TooltipHandler.TipRegion(thingLabelRect, text2);
        //    y += 28f;

        //    // RMB menu
        //    if (Widgets.ButtonInvisible(thingLabelRect) && Event.current.button == 1)
        //    {
        //        List<FloatMenuOption> floatOptionList = new List<FloatMenuOption>();
        //        floatOptionList.Add(new FloatMenuOption("ThingInfo".Translate(), delegate
        //        {
        //            Find.WindowStack.Add(new Dialog_InfoCard(thing));
        //        }, MenuOptionPriority.Default, null, null));
        //        if (selPawn.CanControl)
        //        {
        //            // Equip option
        //            ThingWithComps eq = thing as ThingWithComps;
        //            if (eq != null && eq.TryGetComp<CompEquippable>() != null)
        //            {
        //                CompInventory compInventory = selPawn.Pawn.TryGetComp<CompInventory>();
        //                if (compInventory != null)
        //                {
        //                    FloatMenuOption equipOption;
        //                    string eqLabel = GenLabel.ThingLabel(eq.def, eq.Stuff, 1);
        //                    if (selPawn.Pawn.equipment.AllEquipmentListForReading.Contains(eq) && selPawn.Pawn.inventory != null)
        //                    {
        //                        equipOption = new FloatMenuOption("CE_PutAway".Translate(eqLabel),
        //                            new Action(delegate
        //                            {
        //                                selPawn.Pawn.equipment.TryTransferEquipmentToContainer(selPawn.Pawn.equipment.Primary, selPawn.Pawn.inventory.innerContainer);
        //                            }));
        //                    }
        //                    else if (!selPawn.Pawn.health.capacities.CapableOf(PawnCapacityDefOf.Manipulation))
        //                    {
        //                        equipOption = new FloatMenuOption("CannotEquip".Translate(eqLabel), null);
        //                    }
        //                    else
        //                    {
        //                        string equipOptionLabel = "Equip".Translate(eqLabel);
        //                        if (eq.def.IsRangedWeapon && selPawn.Pawn.story != null && selPawn.Pawn.story.traits.HasTrait(TraitDefOf.Brawler))
        //                        {
        //                            equipOptionLabel = equipOptionLabel + " " + "EquipWarningBrawler".Translate();
        //                        }
        //                        equipOption = new FloatMenuOption(
        //                            equipOptionLabel,
        //                            (selPawn.Pawn.story != null && selPawn.Pawn.story.WorkTagIsDisabled(WorkTags.Violent))
        //                            ? null
        //                            : new Action(delegate
        //                            {
        //                                compInventory.TrySwitchToWeapon(eq);
        //                            }));
        //                    }
        //                    floatOptionList.Add(equipOption);
        //                }
        //            }
        //            // Drop option
        //            Action dropApparel = delegate
        //            {
        //                SoundDefOf.Tick_High.PlayOneShotOnCamera();
        //                InterfaceDrop(thing, selPawn.Pawn);
        //            };
        //            Action dropApparelHaul = delegate
        //            {
        //                SoundDefOf.Tick_High.PlayOneShotOnCamera();
        //                InterfaceDropHaul(thing, selPawn.Pawn);
        //            };
        //            if (selPawn.CanControl && thing.IngestibleNow && selPawn.Pawn.RaceProps.CanEverEat(thing))
        //            {
        //                Action eatFood = delegate
        //                {
        //                    SoundDefOf.Tick_High.PlayOneShotOnCamera();
        //                    InterfaceIngest(thing, selPawn.Pawn);
        //                };
        //                string label = thing.def.ingestible.ingestCommandString.NullOrEmpty() ? "ConsumeThing".Translate(thing.LabelShort, thing) : string.Format(thing.def.ingestible.ingestCommandString, thing.LabelShort);
        //                floatOptionList.Add(new FloatMenuOption(label, eatFood));
        //            }
        //            floatOptionList.Add(new FloatMenuOption("DropThing".Translate(), dropApparel));
        //            floatOptionList.Add(new FloatMenuOption("CE_DropThingHaul".Translate(), dropApparelHaul));
        //            if (selPawn.CanControl && selPawn.Pawn.HoldTrackerIsHeld(thing))
        //            {
        //                Action forgetHoldTracker = delegate
        //                {
        //                    SoundDefOf.Tick_High.PlayOneShotOnCamera();
        //                    selPawn.Pawn.HoldTrackerForget(thing);
        //                };
        //                floatOptionList.Add(new FloatMenuOption("CE_HoldTrackerForget".Translate(), forgetHoldTracker));
        //            }
        //        }
        //        FloatMenu window = new FloatMenu(floatOptionList, thing.LabelCap, false);
        //        Find.WindowStack.Add(window);
        //    }
        //    // end menu
        //}

        public static void DrawThingRowWithImage(RPG_Pawn selPawn, Rect rect, ThingWithComps thing, bool inventory = false)
        {
            QualityCategory c;
            if (thing.TryGetQuality(out c))
            {
                switch (c)
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
            float num2 = rect5.height * ((float)thing.HitPoints / (float)thing.MaxHitPoints);
            rect5.yMin = rect5.yMax - num2;
            rect5.height = num2;
            // draw background indicator for hitpoints
            GUI.DrawTexture(rect5, SolidColorMaterials.NewSolidColorTexture(new Color(0.4f, 0.47f, 0.53f, 0.44f)));
            if ((float)thing.HitPoints <= ((float)thing.MaxHitPoints / 2))
            {
                GUI.DrawTexture(rect5, SolidColorMaterials.NewSolidColorTexture(new Color(1f, 0.5f, 0.31f, 0.44f)));
            }
            if (thing.def.DrawMatSingle != null && thing.def.DrawMatSingle.mainTexture != null)
            {
                Rect rect1 = new Rect(rect.x + 4f, rect.y + 4f, rect.width - 8f, rect.height - 8f);
                Widgets.ThingIcon(rect1, thing, 1f);
            }
            if (Mouse.IsOver(rect))
            {
                GUI.color = _highlightColor;
                GUI.DrawTexture(rect, TexUI.HighlightTex);
                Widgets.InfoCardButton(rect.x, rect.y, thing);
                if (selPawn.CanControl && (inventory || selPawn.CanControlColonist || (selPawn.Pawn.Spawned && !selPawn.Pawn.Map.IsPlayerHome)))
                {
                    Rect rect2 = new Rect(rect.xMax - 24f, rect.y, 24f, 24f);
                    TooltipHandler.TipRegion(rect2, "DropThing".Translate());
                    if (Widgets.ButtonImage(rect2, ContentFinder<Texture2D>.Get("UI/Buttons/Drop", true)))
                    {
                        SoundDefOf.Tick_High.PlayOneShotOnCamera(null);
                        InterfaceDrop(thing, selPawn.Pawn);
                    }
                }
            }
            Apparel apparel = thing as Apparel;
            if (apparel != null && selPawn.Pawn.outfits != null && apparel.WornByCorpse)
            {
                Rect rect3 = new Rect(rect.xMax - 20f, rect.yMax - 20f, 20f, 20f);
                GUI.DrawTexture(rect3, ContentFinder<Texture2D>.Get("UI/Icons/Sandy_Tainted_Icon", true));
                TooltipHandler.TipRegion(rect3, "WasWornByCorpse".Translate());
            }
            if (apparel != null && selPawn.Pawn.outfits != null && selPawn.Pawn.outfits.forcedHandler.IsForced(apparel))
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
            if (thing is Apparel app)
            {
                float sharp = app.GetStatValue(StatDefOf.ArmorRating_Sharp);
                float blunt = app.GetStatValue(StatDefOf.ArmorRating_Blunt);
                float heat = app.GetStatValue(StatDefOf.ArmorRating_Heat);
                if (sharp > 0.005)
                {
                    text3 = string.Concat(text3, "\n", "ArmorSharp".Translate(), ":", sharp.ToStringPercent());
                }
                if (blunt > 0.005)
                {
                    text3 = string.Concat(text3, "\n", "ArmorBlunt".Translate(), ":", blunt.ToStringPercent());
                }
                if (heat > 0.005)
                {
                    text3 = string.Concat(text3, "\n", "ArmorHeat".Translate(), ":", heat.ToStringPercent());
                }
            }
            TooltipHandler.TipRegion(rect, text3);
        }

        public static void TryDrawComfyTemperatureRange(Pawn pawn, Rect rect)
        {
            if (pawn.Dead)
            {
                return;
            }
            Rect rect1 = new Rect(rect.x, rect.y + 26f, 24f, 24f);
            GUI.DrawTexture(rect1, ContentFinder<Texture2D>.Get("UI/Icons/minumun_temperature", true));
            TooltipHandler.TipRegion(rect1, "ComfyTemperatureRange".Translate());
            float statValue = pawn.GetStatValue(StatDefOf.ComfyTemperatureMin, true);
            Rect rect2 = new Rect(rect.x + 30f, rect.y + 28f, 104f, 24f);
            Widgets.Label(rect2, string.Concat(new string[]
            {
                " ",
                statValue.ToStringTemperature("F0")
            }));

            rect1 = new Rect(rect.x, rect.y + 52f, 24f, 24f);
            GUI.DrawTexture(rect1, ContentFinder<Texture2D>.Get("UI/Icons/max_temperature", true));
            TooltipHandler.TipRegion(rect1, "ComfyTemperatureRange".Translate());
            float statValue2 = pawn.GetStatValue(StatDefOf.ComfyTemperatureMax, true);
            rect2 = new Rect(rect.x + 30f, rect.y + 56f, 104f, 24f);
            Widgets.Label(rect2, string.Concat(new string[]
            {
                " ",
                statValue2.ToStringTemperature("F0")
            }));

        }

        /// <summary>
        /// Not a very effective way to get armor number, but one with most compatibility. Only if modders add
        /// the "ListOrder" tag and correct value to their new "BodyPartGroup" element in xml.
        /// </summary>
        /// <param name="pawn"></param>
        /// <param name="stat">Sharp, blunt or heat, maybe electrical?</param>
        /// <param name="text">Text for tooltip</param>
        /// <param name="unit">unit type for the corresponding stat</param>
        /// <returns></returns>
        public static float CalculateArmorByParts(Pawn pawn, StatDef stat, ref string text, string unit)
        {
            text = "";
            float effectiveArmor = 0;
            // Max amor rating is at 200% in vanilla, divide it by 2 is for scaling manipulation purpose
            // the final value will be multiplied by 2 to restore the original scaling.
            float natureArmor = Mathf.Clamp01(pawn.GetStatValue(stat, true) / 2f);
            List<BodyPartRecord> allParts = pawn.RaceProps.body.AllParts;
            List<Apparel> apparelList = (pawn.apparel == null) ? null : pawn.apparel.WornApparel;
            if (apparelList != null)
            {
                for (int i = 0; i < allParts.Count; i++)
                {
                    float effectivePen = 1 - natureArmor;
                    for (int j = 0; j < apparelList.Count; j++)
                    {
                        if (apparelList[j].def.apparel.CoversBodyPart(allParts[i]))
                        {
                            effectivePen *= 1 - Mathf.Clamp01(apparelList[j].GetStatValue(stat, true) / 2f);
                        }
                    }
                    float eArmorForPart = allParts[i].coverageAbs * (1 - effectivePen);
                    if (eArmorForPart > 0.005)
                    {
                        // UNDONE: Armor values for part are off
                        text += allParts[i].LabelCap + ": ";
                        text += FormatArmorValue(eArmorForPart * 2, unit) + "\n";
                    }
                    effectiveArmor += eArmorForPart;
                }
            }
            effectiveArmor = Mathf.Clamp(effectiveArmor * 2, 0, 2);

            return effectiveArmor;
        }

        public static void DrawArmorRatingWithText(Rect pos, string label, float armorRating, string unit)
        {
            // Draw Label
            Widgets.Label(pos, label.Truncate(200f, null));
            // Draw armor value
            pos.x += 200;
            Widgets.Label(pos, FormatArmorValue(armorRating, unit));
        }

        public static void TryDrawMassInfo(Pawn pawn, ref float curY, float width)
        {
            if (pawn.Dead || !ShouldShowInventory(pawn))
            {
                return;
            }
            Rect rect = new Rect(0f, curY, width, 22f);
            float num = MassUtility.GearAndInventoryMass(pawn);
            float num2 = MassUtility.Capacity(pawn, null);
            Widgets.Label(rect, "MassCarried".Translate(num.ToString("0.##"), num2.ToString("0.##")));
            curY += 22f;
        }

        public static void TryDrawMassInfoWithImage(Pawn pawn, Vector2 rect)
        {
            if (pawn.Dead || !ShouldShowInventory(pawn))
            {
                return;
            }
            Rect rect1 = new Rect(rect.x, rect.y, 24f, 24f);
            GUI.DrawTexture(rect1, ContentFinder<Texture2D>.Get("UI/Icons/Sandy_MassCarried_Icon", true));
            TooltipHandler.TipRegion(rect1, "SandyMassCarried".Translate());
            float num = MassUtility.GearAndInventoryMass(pawn);
            float num2 = MassUtility.Capacity(pawn, null);
            Rect rect2 = new Rect(rect.x + 30f, rect.y + 2f, 104f, 24f);
            Widgets.Label(rect2, "SandyMassValue".Translate(num.ToString("0.##"), num2.ToString("0.##")));
        }

        public static void TryDrawComfyTemperatureRangeWithImage(Pawn pawn, Vector2 rect)
        {
            if (pawn.Dead)
            {
                return;
            }
            Rect rect1 = new Rect(rect.x, rect.y + 26f, 24f, 24f);
            GUI.DrawTexture(rect1, ContentFinder<Texture2D>.Get("UI/Icons/min_temperature", true));
            TooltipHandler.TipRegion(rect1, "ComfyTemperatureRange".Translate());
            float statValue = pawn.GetStatValue(StatDefOf.ComfyTemperatureMin, true);
            Rect rect2 = new Rect(rect.x + 30f, rect.y + 28f, 104f, 24f);
            Widgets.Label(rect2, string.Concat(new string[]
            {
                " ",
                statValue.ToStringTemperature("F0")
            }));

            rect1 = new Rect(rect.x, rect.y + 52f, 24f, 24f);
            GUI.DrawTexture(rect1, ContentFinder<Texture2D>.Get("UI/Icons/max_temperature", true));
            TooltipHandler.TipRegion(rect1, "ComfyTemperatureRange".Translate());
            float statValue2 = pawn.GetStatValue(StatDefOf.ComfyTemperatureMax, true);
            rect2 = new Rect(rect.x + 30f, rect.y + 56f, 104f, 24f);
            Widgets.Label(rect2, string.Concat(new string[]
            {
                " ",
                statValue2.ToStringTemperature("F0")
            }));
        }

        public static void TryDrawOverallArmorWithImage(Pawn pawn, Rect rect, StatDef stat, string label, Texture image)
        {
            string text = "";
            float num = CalculateArmorByParts(pawn, stat, ref text, "%");
            // draw thumbmail
            Rect rect1 = new Rect(rect.x, rect.y, 24f, 27f);
            GUI.DrawTexture(rect1, image);
            TooltipHandler.TipRegion(rect1, label);
            // draw values
            Rect rect2 = new Rect(rect.x + 60f, rect.y + 3f, 104f, 24f);
            Widgets.Label(rect2, num.ToStringPercent());
            TooltipHandler.TipRegion(rect2, text);
        }

        public static void DrawColonist(Rect rect, Pawn pawn)
        {
            Vector2 pos = new Vector2(rect.width, rect.height);
            GUI.DrawTexture(rect, PortraitsCache.Get(pawn, pos, PawnTextureCameraOffset, 1.18f));
        }

        public static SmartRect GetWorkingRect(this SmartRect curr, BodyPartGroupDef bodyPartGroupDef,
                                               float x_leftCurPosition, float x_rightCurPosition)
        {
            if (curr.BodyPartGroup != bodyPartGroupDef)
            {
                var temp = curr.List.Find(s => s.BodyPartGroup == bodyPartGroupDef);
                if (temp == default)
                {
                    temp = curr.CreateSibling(bodyPartGroupDef, x_leftCurPosition, x_rightCurPosition);
                    curr.NextSibling = temp;
                    temp.PreviousSibling = curr;
                }
                return temp;
            }
            return curr;
        }
    }
}
