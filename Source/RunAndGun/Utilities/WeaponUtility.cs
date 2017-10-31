using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace RunAndGun.Utilities
{
    static class WeaponUtility
    {
        public static List<ThingStuffPair> getAllWeapons()
        {
            List<ThingStuffPair> allWeaponPairs;

            Predicate<ThingDef> isWeapon = (ThingDef td) => td.equipmentType == EquipmentType.Primary && td.canBeSpawningInventory && !td.weaponTags.NullOrEmpty<string>();

            allWeaponPairs = ThingStuffPair.AllWith(isWeapon);
            foreach (ThingDef thingDef in from td in DefDatabase<ThingDef>.AllDefs
                                          where isWeapon(td)
                                          select td)
            {
                float num = allWeaponPairs.Where((ThingStuffPair pa) => pa.thing == thingDef).Sum((ThingStuffPair pa) => pa.Commonality);
                float num2 = thingDef.generateCommonality / num;
                if (num2 != 1f)
                {
                    for (int i = 0; i < allWeaponPairs.Count; i++)
                    {
                        ThingStuffPair thingStuffPair = allWeaponPairs[i];
                        if (thingStuffPair.thing == thingDef)
                        {
                            allWeaponPairs[i] = new ThingStuffPair(thingStuffPair.thing, thingStuffPair.stuff, thingStuffPair.commonalityMultiplier * num2);
                        }
                    }
                }
            }
            return filterForType(allWeaponPairs, false);
        }
        internal static List<ThingStuffPair> filterForType(List<ThingStuffPair> list, bool allowThingDuplicates)
        {
            List<ThingStuffPair> returnList = new List<ThingStuffPair>();
            List<ThingDef> things = new List<ThingDef>();
            foreach (ThingStuffPair weapon in list)
            {
                if (!weapon.thing.PlayerAcquirable)
                    continue;
                if ((allowThingDuplicates | !things.Contains(weapon.thing)))
                {
                    returnList.Add(weapon);
                    things.Add(weapon.thing);
                }

            }
            return returnList;
        }

        internal static void getHeaviestWeapons(List<ThingStuffPair> list, out float weightMelee, out float weightRanged)
        {
            weightMelee = float.MinValue;
            weightRanged = float.MinValue;
            foreach (ThingStuffPair weapon in list)
            {
                if (!weapon.thing.PlayerAcquirable)
                    continue;
                float mass = weapon.thing.GetStatValueAbstract(StatDefOf.Mass);
                if (weapon.thing.IsRangedWeapon)
                {
                    if (mass > weightRanged)
                        weightRanged = mass;
                }
                else if (weapon.thing.IsMeleeWeapon)
                {
                    if (mass > weightMelee)
                        weightMelee = mass;
                }
            }
        }
    }
}
