using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using UnityEngine;
using Verse;

namespace BestDrugPolicyGen
{
    public static class SafeDrugPolicyGenerator
    {
        public static DrugPolicyDef GeneratedDrugPolicyDef = new DrugPolicyDef();

        private enum SafeDoseCondition
        {
            AlwaysSafe = 0, NeverSafe, MightSafe
        }

        private static MethodInfo Set_OnlyIfToleranceBelow;

        public static void Init()
        {
            GeneratedDrugPolicyDef.label = "BestDrugPolicyLabel".Translate();
            GeneratedDrugPolicyDef.defName = "BestDrugPolicy";
            var drugPolicyEntries = new List<DrugPolicyEntry>();

            foreach (var drug in DefDatabase<ThingDef>.AllDefsListForReading.Where(x =>
                         x.category == ThingCategory.Item && x.IsDrug))
            {
                var condition = GetSafeDoseCondition(drug, out float safeDoseInterval);
                var drugPolicyEntry = new DrugPolicyEntry()
                {
                    drug = drug
                };
                switch (condition)
                {
                    case SafeDoseCondition.AlwaysSafe:
                        drugPolicyEntry.allowedForJoy = true;
                        drugPolicyEntries.Add(drugPolicyEntry);
                        break;
                    case SafeDoseCondition.NeverSafe:
                        continue;
                    case SafeDoseCondition.MightSafe:
                        drugPolicyEntry.allowScheduled = true;
                        drugPolicyEntry.daysFrequency = Mathf.CeilToInt(safeDoseInterval);
                        drugPolicyEntries.Add(drugPolicyEntry);
                        break;
                }
            }

            GeneratedDrugPolicyDef.entries = drugPolicyEntries;

            Set_OnlyIfToleranceBelow = GenTypes.GetTypeInAnyAssembly("DrugPolicyEnhanced.ExposedPrepatcherFields")
                ?.GetMethod("Set_OnlyIfToleranceBelow");
        }

        private static SafeDoseCondition GetSafeDoseCondition(ThingDef drug, out float safeDoseInterval)
        {
            var toleranceGiver = DrugStatsUtility.GetToleranceGiver(drug);
            safeDoseInterval = DrugStatsUtility.GetSafeDoseInterval(drug);
            float num = (toleranceGiver != null)
                ? (DrugStatsUtility.GetDrugComp(drug).minToleranceToAddict / toleranceGiver.severity)
                : 0f;
            if (safeDoseInterval == 0f)
            {
                return SafeDoseCondition.AlwaysSafe;
            }
            return num < 1f ? SafeDoseCondition.NeverSafe : SafeDoseCondition.MightSafe;
        }

        public static void UpdateSafeDrugPolicy()
        {
            if (GeneratedDrugPolicyDef == null) return;

            float? currentAdditionalDays = GeneratedDrugPolicyDef.entries.FirstOrDefault(x => x.drug.defName == "Beer")?.daysFrequency - 1;
            foreach (var entry in GeneratedDrugPolicyDef.entries)
            {
                if (entry.allowScheduled && currentAdditionalDays != null)
                {
                    entry.daysFrequency = Mathf.Clamp(entry.daysFrequency + Settings.additionalDaysFrequency - (float)currentAdditionalDays, 1, 25);
                }
                if (Settings.onlyIfJoyBelow < 1)
                {
                    entry.onlyIfJoyBelow = Settings.onlyIfJoyBelow;
                }
                if (Settings.onlyIfMoodBelow < 1)
                {
                    entry.onlyIfMoodBelow = Settings.onlyIfMoodBelow;
                }

                // Integration "Only if tolerance below"
                if (Mod.ModActivated_OnlyIfToleranceBelow && Set_OnlyIfToleranceBelow != null && !(Settings.onlyIfToleranceBelowMultiplier > 1.0f))
                {
                    var baseValue = DrugStatsUtility.GetDrugComp(entry.drug).minToleranceToAddict;
                    Set_OnlyIfToleranceBelow.Invoke(null,
                        new[] { (object)entry, baseValue * Settings.onlyIfToleranceBelowMultiplier });
                }
            }
        }
    }
}
