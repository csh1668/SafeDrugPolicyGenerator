using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace BestDrugPolicyGen
{
    public class Mod : Verse.Mod
    {

        internal static bool ModActivated_OnlyIfToleranceBelow = false;

        private static Settings ModSettings;

        public Mod(ModContentPack contentPack):base(contentPack)
        {
            ModActivated_OnlyIfToleranceBelow = ModLister.AllInstalledMods.Any(x =>
                x.Active && x.PackageId.ToLower().Contains("onlyiftolerancebelow"));

            ModSettings = GetSettings<Settings>();
        }

        public override string SettingsCategory()
        {
            return "BDP_Category".Translate();
        }

        public override void DoSettingsWindowContents(Rect inRect)
        {
            base.DoSettingsWindowContents(inRect);
            ModSettings.DoWindowContents(inRect);
        }
    }
}
