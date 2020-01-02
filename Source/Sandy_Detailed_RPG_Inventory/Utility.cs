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
using CombatExtended;

namespace RPG_Inventory_for_CE
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
        private const float _standardLineHeight = 22f;
        private static readonly Color _highlightColor = new Color(0.5f, 0.5f, 0.5f, 1f);
        private static readonly Color _thingLabelColor = new Color(0.9f, 0.9f, 0.9f, 1f);
        private static Vector2 _scrollPosition = Vector2.zero;

        private static float _scrollViewHeight;

        private static List<Thing> workingInvList = new List<Thing>();

        #endregion Fields


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

        public static void TryDrawOverallArmor(Pawn selPawn, ref float curY, float width, StatDef stat, string label, string unit)
        {
            if (selPawn.RaceProps.body != BodyDefOf.Human)
            {
                return;
            }
            float num = 0f;
            List<Apparel> wornApparel = selPawn.apparel.WornApparel;
            for (int i = 0; i < wornApparel.Count; i++)
            {
                num += wornApparel[i].GetStatValue(stat, true) * wornApparel[i].def.apparel.HumanBodyCoverage;
            }
            if (num > 0.005f)
            {
                Rect rect = new Rect(0f, curY, width, _standardLineHeight);
                BodyPartRecord bpr = new BodyPartRecord();
                List<BodyPartRecord> bpList = selPawn.RaceProps.body.AllParts;
                string text = "";
                for (int i = 0; i < bpList.Count; i++)
                {
                    float armorValue = 0f;
                    BodyPartRecord part = bpList[i];
                    if (part.depth == BodyPartDepth.Outside && (part.coverage >= 0.1 || (part.def == BodyPartDefOf.Eye || part.def == BodyPartDefOf.Neck)))
                    {
                        text += part.LabelCap + ": ";
                        for (int j = wornApparel.Count - 1; j >= 0; j--)
                        {
                            Apparel apparel = wornApparel[j];
                            if (apparel.def.apparel.CoversBodyPart(part))
                            {
                                armorValue += apparel.GetStatValue(stat, true);
                            }
                        }
                        text += FormatArmorValue(armorValue, unit) + "\n";
                    }
                }
                TooltipHandler.TipRegion(rect, text);

                Widgets.Label(rect, label.Truncate(200f, null));
                rect.xMin += 200;
                Widgets.Label(rect, FormatArmorValue(num, unit));
                curY += _standardLineHeight;
            }
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

        private static void InterfaceDrop(Thing t, Pawn selPawn)
        {
            if (selPawn.HoldTrackerIsHeld(t))
                selPawn.HoldTrackerForget(t);
            ThingWithComps thingWithComps = t as ThingWithComps;
            Apparel apparel = t as Apparel;
            if (apparel != null && selPawn.apparel != null && selPawn.apparel.WornApparel.Contains(apparel))
            {
                selPawn.jobs.TryTakeOrderedJob(new Job(JobDefOf.RemoveApparel, apparel));
            }
            else if (thingWithComps != null && selPawn.equipment != null && selPawn.equipment.AllEquipmentListForReading.Contains(thingWithComps))
            {
                selPawn.jobs.TryTakeOrderedJob(new Job(JobDefOf.DropEquipment, thingWithComps));
            }
            else if (!t.def.destroyOnDrop)
            {
                Thing thing;
                Thing dropThing = t;
                if (t.def.Minifiable)
                    dropThing = selPawn.inventory.innerContainer.FirstOrDefault(x => x.GetInnerIfMinified().ThingID == t.ThingID);
                selPawn.inventory.innerContainer.TryDrop(dropThing, selPawn.Position, selPawn.Map, ThingPlaceMode.Near, out thing, null);
            }
        }

        private static void InterfaceDropHaul(Thing t, Pawn selPawn)
        {
            if (selPawn.HoldTrackerIsHeld(t))
                selPawn.HoldTrackerForget(t);
            ThingWithComps thingWithComps = t as ThingWithComps;
            Apparel apparel = t as Apparel;
            if (apparel != null && selPawn.apparel != null && selPawn.apparel.WornApparel.Contains(apparel))
            {
                selPawn.jobs.TryTakeOrderedJob(new Job(JobDefOf.RemoveApparel, apparel) { haulDroppedApparel = true });
            }
            else if (thingWithComps != null && selPawn.equipment != null && selPawn.equipment.AllEquipmentListForReading.Contains(thingWithComps))
            {
                selPawn.jobs.TryTakeOrderedJob(new Job(JobDefOf.DropEquipment, thingWithComps));
            }
            else if (!t.def.destroyOnDrop)
            {
                Thing thing;
                selPawn.inventory.innerContainer.TryDrop(t, selPawn.Position, selPawn.Map, ThingPlaceMode.Near, out thing);
            }
        }


        public static void DrawThingRow(RPG_Pawn selPawn, ref float y, float width, Thing thing, bool showDropButtonIfPrisoner = false)
        {

            Rect rect = new Rect(0f, y, width, _thingRowHeight);
            Widgets.InfoCardButton(rect.width - 24f, y, thing);
            rect.width -= 24f;
            if (selPawn.CanControl ||
               (selPawn.Pawn.Faction == Faction.OfPlayer && selPawn.Pawn.RaceProps.packAnimal) ||
               (showDropButtonIfPrisoner && selPawn.Pawn.IsPrisonerOfColony))
            {
                Rect dropRect = new Rect(rect.width - 24f, y, 24f, 24f);
                TooltipHandler.TipRegion(dropRect, "DropThing".Translate());
                if (Widgets.ButtonImage(dropRect, TexButton.Drop))
                {
                    SoundDefOf.Tick_High.PlayOneShotOnCamera();
                    InterfaceDrop(thing, selPawn.Pawn);
                }
                rect.width -= 24f;
            }
            if (selPawn.CanControlColonist)
            {
                if ((thing.def.IsNutritionGivingIngestible || thing.def.IsNonMedicalDrug) && thing.IngestibleNow && selPawn.Pawn.WillEat(thing, null))
                {
                    Rect rect3 = new Rect(rect.width - 24f, y, 24f, 24f);
                    TooltipHandler.TipRegion(rect3, "ConsumeThing".Translate(thing.LabelNoCount, thing));
                    if (Widgets.ButtonImage(rect3, TexButton.Ingest))
                    {
                        SoundDefOf.Tick_High.PlayOneShotOnCamera(null);
                        InterfaceIngest(thing,selPawn.Pawn);
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
                GUI.color = _highlightColor;
                GUI.DrawTexture(rect, TexUI.HighlightTex);
            }

            if (thing.def.DrawMatSingle != null && thing.def.DrawMatSingle.mainTexture != null)
            {
                Widgets.ThingIcon(new Rect(4f, y, _thingIconSize, _thingIconSize), thing, 1f);
            }
            Text.Anchor = TextAnchor.MiddleLeft;
            GUI.color = ITab_Pawn_Gear.ThingLabelColor;
            Rect thingLabelRect = new Rect(_thingLeftX, y, rect.width - _thingLeftX, _thingRowHeight);
            string thingLabel = thing.LabelCap;
            if ((thing is Apparel && selPawn.Pawn.outfits != null && selPawn.Pawn.outfits.forcedHandler.IsForced((Apparel)thing))
                || (selPawn.Pawn.inventory != null && selPawn.Pawn.HoldTrackerIsHeld(thing)))
            {
                thingLabel = thingLabel + ", " + "ApparelForcedLower".Translate();
            }

            Text.WordWrap = false;
            Widgets.Label(thingLabelRect, thingLabel.Truncate(thingLabelRect.width, null));
            Text.WordWrap = true;
            string text2 = string.Concat(new object[]
            {
                thing.LabelCap,
                "\n",
                thing.DescriptionDetailed,
                "\n",
                thing.GetWeightAndBulkTip()
            });
            if (thing.def.useHitPoints)
            {
                string text3 = text2;
                text2 = string.Concat(new object[]
                {
                    text3,
                    "\n",
                    "HitPointsBasic".Translate().CapitalizeFirst(),
                    ": ",
                    thing.HitPoints,
                    " / ",
                    thing.MaxHitPoints
                });
            }
            TooltipHandler.TipRegion(thingLabelRect, text2);
            y += 28f;

            // RMB menu
            if (Widgets.ButtonInvisible(thingLabelRect) && Event.current.button == 1)
            {
                List<FloatMenuOption> floatOptionList = new List<FloatMenuOption>();
                floatOptionList.Add(new FloatMenuOption("ThingInfo".Translate(), delegate
                {
                    Find.WindowStack.Add(new Dialog_InfoCard(thing));
                }, MenuOptionPriority.Default, null, null));
                if (selPawn.CanControl)
                {
                    // Equip option
                    ThingWithComps eq = thing as ThingWithComps;
                    if (eq != null && eq.TryGetComp<CompEquippable>() != null)
                    {
                        CompInventory compInventory = selPawn.Pawn.TryGetComp<CompInventory>();
                        if (compInventory != null)
                        {
                            FloatMenuOption equipOption;
                            string eqLabel = GenLabel.ThingLabel(eq.def, eq.Stuff, 1);
                            if (selPawn.Pawn.equipment.AllEquipmentListForReading.Contains(eq) && selPawn.Pawn.inventory != null)
                            {
                                equipOption = new FloatMenuOption("CE_PutAway".Translate(eqLabel),
                                    new Action(delegate
                                    {
                                        selPawn.Pawn.equipment.TryTransferEquipmentToContainer(selPawn.Pawn.equipment.Primary, selPawn.Pawn.inventory.innerContainer);
                                    }));
                            }
                            else if (!selPawn.Pawn.health.capacities.CapableOf(PawnCapacityDefOf.Manipulation))
                            {
                                equipOption = new FloatMenuOption("CannotEquip".Translate(eqLabel), null);
                            }
                            else
                            {
                                string equipOptionLabel = "Equip".Translate(eqLabel);
                                if (eq.def.IsRangedWeapon && selPawn.Pawn.story != null && selPawn.Pawn.story.traits.HasTrait(TraitDefOf.Brawler))
                                {
                                    equipOptionLabel = equipOptionLabel + " " + "EquipWarningBrawler".Translate();
                                }
                                equipOption = new FloatMenuOption(
                                    equipOptionLabel,
                                    (selPawn.Pawn.story != null && selPawn.Pawn.story.WorkTagIsDisabled(WorkTags.Violent))
                                    ? null
                                    : new Action(delegate
                                    {
                                        compInventory.TrySwitchToWeapon(eq);
                                    }));
                            }
                            floatOptionList.Add(equipOption);
                        }
                    }
                    // Drop option
                    Action dropApparel = delegate
                    {
                        SoundDefOf.Tick_High.PlayOneShotOnCamera();
                        InterfaceDrop(thing, selPawn.Pawn);
                    };
                    Action dropApparelHaul = delegate
                    {
                        SoundDefOf.Tick_High.PlayOneShotOnCamera();
                        InterfaceDropHaul(thing, selPawn.Pawn);
                    };
                    if (selPawn.CanControl && thing.IngestibleNow && selPawn.Pawn.RaceProps.CanEverEat(thing))
                    {
                        Action eatFood = delegate
                        {
                            SoundDefOf.Tick_High.PlayOneShotOnCamera();
                            InterfaceIngest(thing, selPawn.Pawn);
                        };
                        string label = thing.def.ingestible.ingestCommandString.NullOrEmpty() ? "ConsumeThing".Translate(thing.LabelShort, thing) : string.Format(thing.def.ingestible.ingestCommandString, thing.LabelShort);
                        floatOptionList.Add(new FloatMenuOption(label, eatFood));
                    }
                    floatOptionList.Add(new FloatMenuOption("DropThing".Translate(), dropApparel));
                    floatOptionList.Add(new FloatMenuOption("CE_DropThingHaul".Translate(), dropApparelHaul));
                    if (selPawn.CanControl && selPawn.Pawn.HoldTrackerIsHeld(thing))
                    {
                        Action forgetHoldTracker = delegate
                        {
                            SoundDefOf.Tick_High.PlayOneShotOnCamera();
                            selPawn.Pawn.HoldTrackerForget(thing);
                        };
                        floatOptionList.Add(new FloatMenuOption("CE_HoldTrackerForget".Translate(), forgetHoldTracker));
                    }
                }
                FloatMenu window = new FloatMenu(floatOptionList, thing.LabelCap, false);
                Find.WindowStack.Add(window);
            }
            // end menu
        }

    }
}
