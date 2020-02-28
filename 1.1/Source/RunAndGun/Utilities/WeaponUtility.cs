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
        public static List<ThingDef> getAllWeapons()
        {
            List<ThingDef> allWeapons = new List<ThingDef>();

            Predicate<ThingDef> isWeapon = (ThingDef td) => td.equipmentType == EquipmentType.Primary && !td.weaponTags.NullOrEmpty<string>() && !td.destroyOnDrop;
            foreach (ThingDef thingDef in from td in DefDatabase<ThingDef>.AllDefs
                                          where isWeapon(td)
                                          select td)
            {
                allWeapons.Add(thingDef);
            }
            return allWeapons;
        }

        internal static void getHeaviestWeapons(List<ThingDef> list, out float weightMelee, out float weightRanged)
        {
            weightMelee = float.MinValue;
            weightRanged = float.MinValue;
            foreach (ThingDef weapon in list)
            {
                if (!weapon.PlayerAcquirable)
                    continue;
                float mass = weapon.GetStatValueAbstract(StatDefOf.Mass);
                if (weapon.IsRangedWeapon)
                {
                    if (mass > weightRanged)
                        weightRanged = mass;
                }
                else if (weapon.IsMeleeWeapon)
                {
                    if (mass > weightMelee)
                        weightMelee = mass;
                }
            }
        }
    }
}
