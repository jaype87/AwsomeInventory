using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using RPG_Inventory_Remake_Common;
using RPGIResource;
using System.Diagnostics.CodeAnalysis;

namespace RPG_Inventory_Remake.Loadout
{
    public enum SourceSelection
    {
        Ranged,
        Melee,
        Apparel,
        Minified,
        Generic,
        All // All things, won't include generics, can include minified/able now.
    }

    public class Dialog_ManageLoadouts : Window
    {
        #region Fields
        #region Static Fields

        private static Texture2D
            _arrowBottom = ContentFinder<Texture2D>.Get("UI/Icons/RPGI_Loadout/arrowBottom"),
            _arrowDown = ContentFinder<Texture2D>.Get("UI/Icons/RPGI_Loadout/arrowDown"),
            _arrowTop = ContentFinder<Texture2D>.Get("UI/Icons/RPGI_Loadout/arrowTop"),
            _arrowUp = ContentFinder<Texture2D>.Get("UI/Icons/RPGI_Loadout/arrowUp"),
            _darkBackground = SolidColorMaterials.NewSolidColorTexture(0f, 0f, 0f, .2f),
            _iconEdit = ContentFinder<Texture2D>.Get("UI/Icons/RPGI_Loadout/edit"),
            _iconClear = ContentFinder<Texture2D>.Get("UI/Icons/RPGI_Loadout/clear"),
            _iconAmmo = ContentFinder<Texture2D>.Get("UI/Icons/RPGI_Loadout/ammo"),
            _iconRanged = ContentFinder<Texture2D>.Get("UI/Icons/RPGI_Loadout/ranged"),
            _iconMelee = ContentFinder<Texture2D>.Get("UI/Icons/RPGI_Loadout/melee"),
            _iconMinified = ContentFinder<Texture2D>.Get("UI/Icons/RPGI_Loadout/minified"),
            _iconGeneric = ContentFinder<Texture2D>.Get("UI/Icons/RPGI_Loadout/generic"),
            _iconAll = ContentFinder<Texture2D>.Get("UI/Icons/RPGI_Loadout/all"),
            _iconAmmoAdd = ContentFinder<Texture2D>.Get("UI/Icons/RPGI_Loadout/ammoAdd"),
            _iconSearch = ContentFinder<Texture2D>.Get("UI/Icons/RPGI_Loadout/search"),
            _iconMove = ContentFinder<Texture2D>.Get("UI/Icons/RPGI_Loadout/move"),
            _iconPickupDrop = ContentFinder<Texture2D>.Get("UI/Icons/RPGI_Loadout/loadoutPickupDrop"),
            _iconDropExcess = ContentFinder<Texture2D>.Get("UI/Icons/RPGI_Loadout/loadoutDropExcess");

        //private static List<Thing> _currentLoadout.CachedList;
        private static Pawn _pawn;
        /// <summary>
        /// Controls the window size and position
        /// </summary>
        private static Vector2 _initialSize;
        private static List<SelectableItem> _selectableItems;
        private readonly static HashSet<ThingDef> _allSuitableDefs;

        #endregion Statis Fields

        private Vector2 _availableScrollPosition = Vector2.zero;
        private readonly int _loadoutNameMaxLength = 50;
        private const float _barHeight = 24f;
        private Vector2 _countFieldSize = new Vector2(40f, 24f);
        private RPGILoadout _currentLoadout;
        private string _filter = "";
        private const float _iconSize = 16f;
        private const float _margin = 6f;
        private const float _topAreaHeight = 30f;
        private float _scrollViewHeight;
        private Vector2 _slotScrollPosition = Vector2.zero;
        private List<SelectableItem> _source;
        private List<LoadoutGenericDef> _sourceGeneric;
        private SourceSelection _sourceType = SourceSelection.Ranged;

        #endregion Fields

        #region Constructors

        static Dialog_ManageLoadouts()
        {
            float width = GenUI.GetWidthCached(UIText.TenCharsString.Times(10));

            _initialSize = new Vector2(width, UI.screenHeight / 2f);
            _allSuitableDefs = GameComponent_DefManager.GetSuitableDefs();
        }

        [SuppressMessage("Design", "CA1062:Validate arguments of public methods", Justification = "<Pending>")]
        public Dialog_ManageLoadouts(RPGILoadout loadout, Pawn pawn)
        {
            ThrowHelper.ArgumentNullException(loadout, pawn);

            _currentLoadout = loadout;
            _pawn = pawn;

            doCloseX = true;
            forcePause = true;
            absorbInputAroundWindow = false;
            closeOnClickedOutside = true;
            closeOnAccept = false;
        }

        #endregion Constructors

        #region Properties
        public override Vector2 InitialSize => _initialSize;

        #endregion Properties

        #region Methods

        public override void DoWindowContents(Rect canvas)
        {
            GUI.BeginGroup(canvas);
            Text.Font = GameFont.Small;

            // SET UP RECTS
            // top buttons
            Rect selectRect = new Rect(0f, 0f, canvas.width * .2f, _topAreaHeight);
            Rect newRect = new Rect(selectRect.xMax + _margin, 0f, canvas.width * .2f, _topAreaHeight);
            Rect copyRect = new Rect(newRect.xMax + _margin, 0f, canvas.width * .2f, _topAreaHeight);
            Rect deleteRect = new Rect(copyRect.xMax + _margin, 0f, canvas.width * .2f, _topAreaHeight);

            // main areas
            Rect nameRect = new Rect(
                0f,
                _topAreaHeight + _margin * 2,
                (canvas.width - _margin) / 2f,
                24f);

            Rect slotListRect = new Rect(
                0f,
                nameRect.yMax + _margin,
                (canvas.width * 5 / 9 - _margin),
                canvas.height - _topAreaHeight - nameRect.height - _barHeight * 2 - _margin * 5);

            Rect weightBarRect = new Rect(slotListRect.xMin, slotListRect.yMax + _margin, slotListRect.width, _barHeight);

            Rect sourceButtonRect = new Rect(
                slotListRect.xMax + _margin,
                _topAreaHeight + _margin * 2,
                (canvas.width * 4 / 9 - _margin),
                24f);

            Rect selectionRect = new Rect(
                slotListRect.xMax + _margin,
                sourceButtonRect.yMax + _margin,
                (canvas.width * 4 / 9 - _margin),
                canvas.height - 24f - _topAreaHeight - _margin * 3);

            List<RPGILoadout> loadouts = LoadoutManager.Loadouts;
            // DRAW CONTENTS
            // buttons
            // select loadout
            if (Widgets.ButtonText(selectRect, UIText.SelectLoadout.Translate()))
            {
                List<FloatMenuOption> options = new List<FloatMenuOption>();
                if (loadouts.Count == 0)
                    options.Add(new FloatMenuOption(UIText.NoLoadouts.Translate(), null));
                else
                {
                    for (int j = 0; j < loadouts.Count; j++)
                    {
                        int i = j;
                        options.Add(new FloatMenuOption(loadouts[i].Label,
                                                        delegate
                                                        {
                                                            _currentLoadout = loadouts[i];
                                                            _slotScrollPosition = Vector2.zero;
                                                        }));
                    }
                }
                Find.WindowStack.Add(new FloatMenu(options));
            }

            // create loadout
            if (Widgets.ButtonText(newRect, UIText.NewLoadout.Translate()))
            {
                RPGILoadout loadout = new RPGILoadout()
                {
                    Label = LoadoutManager.GetIncrementalLabel(_currentLoadout.Label)
                };
                LoadoutManager.AddLoadout(loadout);
                _currentLoadout = loadout;
                //_currentLoadout.CachedList = _currentLoadout.ToList();
            }

            // copy loadout
            if (_currentLoadout != null && Widgets.ButtonText(copyRect, UIText.CopyLoadout.Translate()))
            {
                _currentLoadout = new RPGILoadout(_currentLoadout);
                LoadoutManager.AddLoadout(_currentLoadout);
                //_currentLoadout.CachedList = _currentLoadout.ToList();
            }

            // delete loadout
            if (Widgets.ButtonText(deleteRect, UIText.DeleteLoadout.Translate()))
            {
                List<FloatMenuOption> options = new List<FloatMenuOption>();

                for (int j = 0; j < loadouts.Count; j++)
                {
                    int i = j;
                    options.Add(new FloatMenuOption(loadouts[i].Label,
                        delegate
                        {
                            if (loadouts.Count > 1)
                            {
                                if (_currentLoadout == loadouts[i])
                                {
                                    LoadoutManager.RemoveLoadout(loadouts[i]);
                                    _currentLoadout = loadouts.First();
                                    //_currentLoadout.CachedList = _currentLoadout.ToList();
                                    return;
                                }
                                LoadoutManager.RemoveLoadout(loadouts[i]);
                            }
                            else
                            {
                                Rect msgRect = new Rect(Vector2.zero, Text.CalcSize(ErrorMessage.TryToDeleteLastLoadout.Translate()));
                                msgRect = msgRect.ExpandedBy(50);
                                Find.WindowStack.Add(
                                    new Dialog_InstantMessage
                                        (ErrorMessage.TryToDeleteLastLoadout.Translate(), msgRect.size, UIText.OK.Translate())
                                    {
                                        windowRect = msgRect
                                    });
                            }
                        }));
                }
                Find.WindowStack.Add(new FloatMenu(options));
            }

            // draw notification if no loadout selected
            if (_currentLoadout == null)
            {
                Text.Anchor = TextAnchor.MiddleCenter;
                GUI.color = Color.grey;
                Widgets.Label(canvas, UIText.NoLoadoutSelected.Translate());
                GUI.color = Color.white;
                Text.Anchor = TextAnchor.UpperLeft;

                // and stop further drawing
                return;
            }

            DrawLoadoutTextField(nameRect);

            DrawCategoryIcon(sourceButtonRect);

            DrawItemsInCategory(selectionRect);

            DrawItemsInLoadout(slotListRect);

            // bars
            if (_currentLoadout != null)
            {
                UtilityLoadouts.DrawBar(weightBarRect, _currentLoadout.Weight, MassUtility.Capacity(_pawn), UIText.Weight.Translate(), null);
                // draw text overlays on bars
                Text.Font = GameFont.Small;
                Text.Anchor = TextAnchor.MiddleCenter;
                Widgets.Label(weightBarRect, _currentLoadout.Weight.ToString("0.#") + "/" + MassUtility.Capacity(_pawn).ToStringMass());
                GUI.color = Color.white;
                Text.Anchor = TextAnchor.UpperLeft;
            }
            // done!
            GUI.EndGroup();
        }

        public override void PreOpen()
        {
            base.PreOpen();
            HashSet<ThingDef> visibleDefs = GameComponent_DefManager.GetSuitableDefs();
            visibleDefs.IntersectWith(Find.CurrentMap.listerThings.ThingsInGroup(ThingRequestGroup.HaulableEverOrMinifiable).Select(t => t.def));
            _selectableItems = new List<SelectableItem>();
            foreach (ThingDef td in _allSuitableDefs)
            {
                SelectableItem selectableItem = new SelectableItem() { thingDef = td };
                if (td is LoadoutGenericDef genericDef)
                {
                    selectableItem.isGreyedOut = visibleDefs.Any(def => genericDef.lambda(def));
                }
                else
                {
                    selectableItem.isGreyedOut = !visibleDefs.Contains(td);
                }
                _selectableItems.Add(selectableItem);
            }
            _selectableItems = _selectableItems.OrderBy(td => td.thingDef.label).ToList();
            SetSource(SourceSelection.Ranged);
        }

        public override void PostOpen()
        {
            base.PostOpen();
            windowRect.x += 200;
        }

        public void DrawCategoryIcon(Rect canvas)
        {
            WidgetRow row = new WidgetRow(canvas.x, canvas.y);
            DrawSourceIcon(SourceSelection.Ranged, _iconRanged, ref row, UIText.SourceRangedTip.Translate());
            DrawSourceIcon(SourceSelection.Melee, _iconMelee, ref row, UIText.SourceMeleeTip.Translate());
            DrawSourceIcon(SourceSelection.Apparel, TexButton.Apparel, ref row, UIText.SourceApparelTip.Translate());
            DrawSourceIcon(SourceSelection.Minified, _iconMinified, ref row, UIText.SourceMinifiedTip.Translate());
            DrawSourceIcon(SourceSelection.Generic, _iconGeneric, ref row, UIText.SourceGenericTip.Translate());
            DrawSourceIcon(SourceSelection.All, _iconAll, ref row, UIText.SourceAllTip.Translate());

            float nameFieldLen = GenUI.GetWidthCached(UIText.TenCharsString);
            float incrementX = canvas.xMax - row.FinalX - nameFieldLen - WidgetRow.IconSize - WidgetRow.ButtonExtraSpace;
            row.Gap(incrementX);
            row.Icon(_iconSearch, UIText.SourceFilterTip.Translate());

            Rect filterRect = new Rect(row.FinalX, canvas.y, nameFieldLen, canvas.height);
            DrawFilterField(filterRect);
            TooltipHandler.TipRegion(filterRect, UIText.SourceFilterTip.Translate());

            // reset color
            GUI.color = Color.white;
        }

        public void FilterSource(string filter)
        {
            // reset source
            SetSource(_sourceType, true);

            // filter
            _source = _source.Where(td => td.thingDef.label.ToUpperInvariant().Contains(_filter.ToUpperInvariant())).ToList();
        }

        public void SetSource(SourceSelection source, bool preserveFilter = false)
        {
            _sourceGeneric = LoadoutGenericDef.GenericDefs;
            if (!preserveFilter)
                _filter = "";

            switch (source)
            {
                case SourceSelection.Ranged:
                    _source = _selectableItems.Where(row => row.thingDef.IsRangedWeapon).ToList();
                    _sourceType = SourceSelection.Ranged;
                    break;

                case SourceSelection.Melee:
                    _source = _selectableItems.Where(row => row.thingDef.IsMeleeWeapon).ToList();
                    _sourceType = SourceSelection.Melee;
                    break;

                case SourceSelection.Apparel:
                    _source = _selectableItems.Where(row => row.thingDef.IsApparel).ToList();
                    _sourceType = SourceSelection.Apparel;
                    break;

                case SourceSelection.Minified:
                    _source = _selectableItems.Where(row => row.thingDef.Minifiable).ToList();
                    _sourceType = SourceSelection.Minified;
                    break;

                case SourceSelection.Generic:
                    _sourceType = SourceSelection.Generic;
                    _source = _selectableItems.Where(row => row.thingDef is LoadoutGenericDef).ToList();
                    break;

                case SourceSelection.All:
                default:
                    _source = _selectableItems;
                    _sourceType = SourceSelection.All;
                    break;
            }
        }

        private void DrawCountField(Rect canvas, Thing thing)
        {
            if (thing == null)
                return;

            int countInt = thing.stackCount;
            string buffer = countInt.ToString();
            Widgets.TextFieldNumeric<int>(canvas, ref countInt, ref buffer);
            TooltipHandler.TipRegion(canvas, UIText.CountFieldTip.Translate(thing.stackCount));
            if (countInt != thing.stackCount)
            {
                _currentLoadout.SetDirtyAll();
            }
            thing.stackCount = countInt;
        }

        private void DrawFilterField(Rect canvas)
        {
            string filter = GUI.TextField(canvas, _filter);
            if (filter != _filter)
            {
                _filter = filter;
                FilterSource(_filter);
            }
        }

        private void DrawLoadoutTextField(Rect canvas)
        {
            Widgets.TextField(canvas, _currentLoadout.Label, _loadoutNameMaxLength, Outfit.ValidNameRegex);
        }

        private void DrawItem(Rect row, Thing thing, int reorderableGroup, bool drawShadow = false)
        {
            // set up rects
            // label (fill) | gear icon | count | delete (iconSize)

            // Set up rects
            Rect labelRect = new Rect(row);
            labelRect.xMax = row.xMax - row.height - _countFieldSize.x - _iconSize - GenUI.GapSmall;
            if (!drawShadow)
            {
                ReorderableWidget.Reorderable(reorderableGroup, labelRect);
            }

            Rect gearIconRect = new Rect(labelRect.xMax, row.y, row.height, row.height);

            Rect countRect = new Rect(
                gearIconRect.xMax,
                row.yMin + (row.height - _countFieldSize.y) / 2f,
                _countFieldSize.x,
                _countFieldSize.y);

            Rect deleteRect = new Rect(countRect.xMax + GenUI.GapSmall, row.yMin + (row.height - _iconSize) / 2f, _iconSize, _iconSize);

            // label
            Text.Anchor = TextAnchor.MiddleLeft;
            Text.WordWrap = false;
            Widgets.Label(labelRect, thing.LabelCapNoCount);
            Text.WordWrap = true;
            Text.Anchor = TextAnchor.UpperLeft;

            // gear icon
            if ((thing.def.MadeFromStuff || thing.TryGetQuality(out _))
                && !thing.def.IsArt
                && Widgets.ButtonImage(gearIconRect, TexButton.Gear))
            {
                Find.WindowStack.Add(new Dialog_StuffAndQuality(thing, _currentLoadout));
            }

            // count
            DrawCountField(countRect, thing);

            // delete
            if (Mouse.IsOver(deleteRect))
                GUI.DrawTexture(row, TexUI.HighlightTex);
            if (Widgets.ButtonImage(deleteRect, _iconClear))
            {
                _currentLoadout.RemoveItem(thing);
            }

            if (!drawShadow)
            {
                // Tooltips && Highlights
                Widgets.DrawHighlightIfMouseover(row);
                if (Event.current.type == EventType.MouseDown)
                {
                    TooltipHandler.ClearTooltipsFrom(labelRect);
                }
                else
                {
                    TooltipHandler.TipRegion(labelRect, string.Concat(thing.GetWeightTip(), '\n', UIText.DragToReorder.Translate()));
                }
                TooltipHandler.TipRegion(deleteRect, UIText.Delete.Translate());
            }
        }

        private void DrawItemsInLoadout(Rect canvas)
        {
            // set up content canvas
            Rect listRect = new Rect(0, 0, canvas.width - GenUI.ScrollBarWidth, _scrollViewHeight);
            // darken whole area
            GUI.DrawTexture(canvas, _darkBackground);
            Widgets.BeginScrollView(canvas, ref _slotScrollPosition, listRect);
            // Set up reorder functionality
            int reorderableGroup = ReorderableWidget.NewGroup(
                delegate (int from, int to)
                {
                    ReorderItems(from, to);
                    UtilityDraw.ResetDrag();
                }
                , ReorderableDirection.Vertical
                , -1
                , (index, pos) =>
                {
                    Vector2 position = UtilityDraw.GetPostionForDrag(windowRect.ContractedBy(Margin), new Vector2(canvas.xMin, canvas.yMin), index, GenUI.ListSpacing);
                    Rect dragRect = new Rect(position, new Vector2(listRect.width, GenUI.ListSpacing));
                    Find.WindowStack.ImmediateWindow(Rand.Int, dragRect, WindowLayer.Super,
                        () =>
                        {
                            GUI.DrawTexture(dragRect.AtZero(), SolidColorMaterials.NewSolidColorTexture(Theme.MilkySlicky.BackGround));
                            GUI.color = Theme.MilkySlicky.ForeGround;
                            DrawItem(dragRect.AtZero(), _currentLoadout.CachedList[index], 0, true);
                            GUI.color = Color.white;
                        }, false);
                }
                );

            float curY = 0f;
            for (int i = 0; i < _currentLoadout.CachedList.Count; i++)
            {
                // create row rect
                Rect row = new Rect(0f, curY, listRect.width, GenUI.ListSpacing);
                curY += GenUI.ListSpacing;

                // alternate row background
                if (i % 2 == 0)
                    GUI.DrawTexture(row, _darkBackground);

                DrawItem(row, _currentLoadout.CachedList[i], reorderableGroup);
                GUI.color = Color.white;
            }

            if (Event.current.type == EventType.Layout)
            {
                _scrollViewHeight = curY + GenUI.ListSpacing;
            }
            Widgets.EndScrollView();
        }

        private void DrawItemsInCategory(Rect canvas)
        {
            GUI.DrawTexture(canvas, _darkBackground);

            Rect viewRect = new Rect(canvas);
            viewRect.height = _source.Count * GenUI.ListSpacing;
            viewRect.width -= GenUI.GapWide;

            Widgets.BeginScrollView(canvas, ref _availableScrollPosition, viewRect.AtZero());
            for (int i = 0; i < _source.Count; i++)
            {
                Color baseColor = GUI.color;

                // gray out weapons not in stock
                if (_source[i].isGreyedOut)
                    GUI.color = Color.gray;

                Rect row = new Rect(0f, i * GenUI.ListSpacing, canvas.width, GenUI.ListSpacing);
                Rect labelRect = new Rect(row);
                TooltipHandler.TipRegion(row, _source[i].thingDef.GetWeightAndBulkTip());

                labelRect.xMin += GenUI.GapTiny;
                if (i % 2 == 0)
                    GUI.DrawTexture(row, _darkBackground);

                int j = i;
                UtilityDraw.DrawLineButton
                    (labelRect
                    , _source[j].thingDef.LabelCap
                    , _source[j].thingDef
                    , (target) => _currentLoadout.AddItem(target));

                GUI.color = baseColor;
            }
            Widgets.EndScrollView();
        }

        private void ReorderItems(int oldIndex, int newIndex)
        {
            if (oldIndex != newIndex)
            {
                _currentLoadout.CachedList.Insert(newIndex, _currentLoadout.CachedList[oldIndex]);
                _currentLoadout.CachedList.RemoveAt((oldIndex >= newIndex) ? (oldIndex + 1) : oldIndex);
            }
        }

        private void DrawSourceIcon(SourceSelection sourceSelected, Texture2D texButton, ref WidgetRow row, string tip)
        {
            if (row.ButtonIcon(texButton, tip, GenUI.MouseoverColor))
            {
                SetSource(sourceSelected);
                _availableScrollPosition = Vector2.zero;
            }
        }

        #endregion Methods

        private class SelectableItem
        {
            public ThingDef thingDef;
            public bool isGreyedOut;
        }
    }
}
