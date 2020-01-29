using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System.Globalization;
using System.Resources;
using Verse;
using RimWorld;
using RPG_Inventory_Remake.Loadout;
using RPG_Inventory_Remake.Tests.Resources;
using TSP = RimWorld.ThingStuffPairWithQuality;


namespace RPG_Inventory_Remake.Tests
{
    public static class Maker<T> where T : Type
    {
        public static List<T> Make()
        {
            if (typeof(T) == typeof(Apparel))
            {
                return MakeApparel();
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
                throw new ArgumentException();
            }
        }

        private static List<T> MakeApparel()
        {
            List<T> apparels = new List<T>();
            Apparel apparel = new Apparel();
            ResourceSet resources = ApparelNames.ResourceManager.GetResourceSet(CultureInfo.CurrentCulture, false, false);
            foreach (DictionaryEntry app in resources)
            {
                Apparel newApp = (Apparel)MakeThingSimple(new TSP()
                {
                    thing = MakeDef(app.Value as string)
                });
            }
            apparels.Add(apparel as T);
            return apparels;
        }

        private static List<T> MakeThingWithComps()
        {
            throw new Exception();
        }

        private static List<T> MakeItems()
        {
            throw new Exception();
        }

        private static Thing MakeThingSimple(TSP pair)
        {
            Thing thing = new Thing();
            thing.def = pair.thing;
            thing.SetStuffDirect(pair.stuff);
            thing.TryGetComp<CompQuality>().SetQuality(QualityCategory.Awful, ArtGenerationContext.Outsider);
            return thing;
        }

        private static ThingDef MakeDef(string name)
        {
            return new ThingDef() { defName = name };
        }
    }
}
