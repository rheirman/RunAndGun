using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Harmony;
using System.Reflection;
using Verse;
using Verse.Sound;
using RimWorld;
using HugsLib;
using HugsLib.Settings;



namespace RunAndGun.Harmony
{
    [StaticConstructorOnStartup]
    class HarmonyBase
    {
        static HarmonyBase()
        {
            var harmony = HarmonyInstance.Create("RunAndGun.Harmony");
            harmony.PatchAll(Assembly.GetExecutingAssembly());
        }
    }
}
