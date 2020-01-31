using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System.Globalization;
using System.Resources;
using Verse;
using RimWorld;
using Harmony;
using RPGIResource;
using RPG_Inventory_Remake.Loadout;
using RPG_Inventory_Remake.Tests.Resources;
using System.Diagnostics;
using TSP = RimWorld.ThingStuffPairWithQuality;


namespace RPG_Inventory_Remake.Tests
{
    public static class Maker<T> where T : class
    {
        public static List<T> Make()
        {
            if (typeof(T) == typeof(Apparel))
            {
                return MakeApparels();
            }
            else if (typeof(T) == typeof(ThingWithComps))
            {
                return MakeThingWithComps();
            }
            else if (typeof(T) == typeof(Thing))
            {
                return MakeItems();
            }
            else
            {
                throw new ArgumentException(ErrorMessage.WrongArgumentType, nameof(T));
            }
        }

        private static List<T> MakeApparels()
        {
            Trace.WriteLine("In makeapparels");
            List<T> apparels = new List<T>();
            ResourceSet apparelStrings = ApparelNames.ResourceManager.GetResourceSet(CultureInfo.InvariantCulture, true, false);
            ResourceSet stuffStrings = StuffNames.ResourceManager.GetResourceSet(CultureInfo.InvariantCulture, true, false);

            Trace.WriteLine("Load resources");
            IDictionaryEnumerator apparelEnumerator = apparelStrings.GetEnumerator();
            foreach (DictionaryEntry app in apparelStrings)
            {
                Trace.WriteLine(app.Value as string);
                foreach (DictionaryEntry s in stuffStrings)
                {
                    Trace.WriteLine(s.Value as string);
                    foreach (QualityCategory qc in Enum.GetValues(typeof(QualityCategory)))
                    {
                        Apparel apparel = new Apparel() { def = MakeDef(app.Value as string) };
                        apparel.SetStuffDirect(MakeDef(s.Value as string));
                        Traverse.Create(apparel).Field("comps").SetValue(new List<ThingComp>()).Method("Add", new CompQuality()).GetValue();
                        apparel.TryGetComp<CompQuality>().SetQuality(qc, ArtGenerationContext.Outsider);
                        apparels.Add(apparel as T);
                    }
                }
            }
            return apparels;
        }

        private static List<T> MakeThingWithComps()
        {
            List<T> weapons = new List<T>();
            ResourceSet weaponStrings = WeaponStrings.ResourceManager.GetResourceSet(CultureInfo.CurrentCulture, false, false);
            ResourceSet stuffStrings = StuffNames.ResourceManager.GetResourceSet(CultureInfo.CurrentCulture, false, false);
            foreach (DictionaryEntry app in weaponStrings)
            {
                foreach (DictionaryEntry s in stuffStrings)
                {
                    foreach (QualityCategory qc in Enum.GetValues(typeof(QualityCategory)))
                    {
                        ThingWithComps newApp = (ThingWithComps)MakeThingSimple(new TSP()
                        {
                            thing = MakeDef(app.Value as string),
                            stuff = MakeDef(s.Value as string),
                            quality = qc
                        });
                        weapons.Add(newApp as T);
                    }
                }
            }
            return weapons;
        }

        private static List<T> MakeItems()
        {
            List<T> Items = new List<T>();
            ResourceSet itemStrings = ItemStrings.ResourceManager.GetResourceSet(CultureInfo.CurrentCulture, false, false);
            foreach (DictionaryEntry i in itemStrings)
            {
                Thing thing = new Thing() { def = MakeDef(i.Value as string) };
                Items.Add(thing as T);
            }
            return Items;
        }

        private static Thing MakeThingSimple(TSP pair)
        {
            ThingWithComps thing = new ThingWithComps();
            CompQuality compQuality = new CompQuality
            {
                parent = thing
            };

            Traverse.Create(thing).Field("comps").SetValue(new List<ThingComp>()).Method("Add", compQuality).GetValue();
            Trace.WriteLine(thing.AllComps.Count);
            thing.def = pair.thing;
            thing.SetStuffDirect(pair.stuff);
            if (thing.TryGetComp<CompQuality>() == null)
            {
                Trace.WriteLine(thing.AllComps[0].GetType());
                Trace.WriteLine(thing.AllComps[0] as CompQuality == null ? "null" : "no");
                Trace.WriteLine(typeof(CompQuality));
                Trace.WriteLine("quality is null");
            }
            thing.TryGetComp<CompQuality>().SetQuality(pair.Quality, ArtGenerationContext.Outsider);
            return thing;
        }

        private static ThingDef MakeDef(string name)
        {
            return new ThingDef() { defName = name };
        }
    }
}
