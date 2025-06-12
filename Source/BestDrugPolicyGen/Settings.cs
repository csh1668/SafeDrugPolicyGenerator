using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using RimWorld;
using UnityEngine;
using Verse;

namespace BestDrugPolicyGen
{
    public class Settings : ModSettings
    {
        public static float additionalDaysFrequency = 0f;
        public static float onlyIfMoodBelow = 1f;
        public static float onlyIfJoyBelow = 1f;

        /// <summary>
        /// Integration for "Only If 'Tolerance' Below in Drug Policy"
        /// </summary>
        public static float onlyIfToleranceBelowMultiplier = 1f;

        public override void ExposeData()
        {
            Scribe_Values.Look(ref additionalDaysFrequency, "BestDrugPolicyGen_AdditionalDays", 0f);
            Scribe_Values.Look(ref onlyIfMoodBelow, "BestDrugPolicyGen_onlyIfMoodBelow", 1f);
            Scribe_Values.Look(ref onlyIfJoyBelow, "BestDrugPolicyGen_onlyIfJoyBelow", 1f);

            if (BestDrugPolicyGen.Mod.ModActivated_OnlyIfToleranceBelow)
                Scribe_Values.Look(ref onlyIfToleranceBelowMultiplier, "BestDrugPolicyGen_onlyIfToleranceBelowMultiplier", 1f);

            base.ExposeData();
        }

        public void DoWindowContents(Rect inRect)
        {
            Listing_Standard ls = new Listing_Standard();
            ls.Begin(inRect);

            ls.Label("BDP_additionalDays".Translate(additionalDaysFrequency));
            additionalDaysFrequency = ls.Slider((int)additionalDaysFrequency, 0, 25);

            ls.Label("BDP_onlyIfJoyBelow".Translate(onlyIfJoyBelow * 100));
            onlyIfJoyBelow = ls.Slider(Mathf.Round(onlyIfJoyBelow * 100) / 100f, 0, 1);

            ls.Label("BDP_onlyIfMoodBelow".Translate(onlyIfMoodBelow * 100));
            onlyIfMoodBelow = ls.Slider(Mathf.Round(onlyIfMoodBelow * 100) / 100f, 0, 1);

            if (BestDrugPolicyGen.Mod.ModActivated_OnlyIfToleranceBelow)
            {
                ls.Label(onlyIfToleranceBelowMultiplier > 1.0f
                    ? "BDP_onlyIfToleranceBelowMultiplier".Translate("BDP_Anyway".Translate())
                    : "BDP_onlyIfToleranceBelowMultiplier".Translate(onlyIfToleranceBelowMultiplier.ToString("F1")), tooltip: "BDP_onlyIfToleranceBelowMultiplierTooltip".Translate());
                onlyIfToleranceBelowMultiplier = ls.Slider(Mathf.Round(onlyIfToleranceBelowMultiplier * 100) / 100f, 0, 1.1f);
            }

            var button = ls.ButtonText("BDP_applyButton".Translate());
            if (button)
            {
                SafeDrugPolicyGenerator.UpdateSafeDrugPolicy();
                Messages.Message("BDP_appliedMessage".Translate(), MessageTypeDefOf.PositiveEvent);
            }

            // 현재 게임 실행 중일 경우
            if (Current.Game != null)
            {
                var button2 = ls.ButtonText("BDP_addToCurSaveButton".Translate());
                if (button2)
                {
                    SafeDrugPolicyGenerator.UpdateSafeDrugPolicy();
                    Current.Game.drugPolicyDatabase?.NewDrugPolicyFromDef(
                        SafeDrugPolicyGenerator.GeneratedDrugPolicyDef);
                    Messages.Message("BDP_addedToCurSaveMessage".Translate(), MessageTypeDefOf.PositiveEvent);
                }
            }
            
            ls.End();
        }
    }
}
