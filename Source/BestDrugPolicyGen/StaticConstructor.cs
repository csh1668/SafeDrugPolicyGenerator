using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace BestDrugPolicyGen
{
    [StaticConstructorOnStartup]
    public static class StaticConstructor
    {
        static StaticConstructor()
        {
            SafeDrugPolicyGenerator.Init();
            SafeDrugPolicyGenerator.UpdateSafeDrugPolicy();
            DefGenerator.AddImpliedDef(SafeDrugPolicyGenerator.GeneratedDrugPolicyDef);
        }
    }
}
