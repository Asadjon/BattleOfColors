using System.Collections;
using UnityEngine;
using static Assets.Scripts.GameOptions;

namespace Assets.Scripts.UI
{
    internal class ToggleForSizesOfSquare : ToggleForEnum<SizesOfSquare>
    {
        protected override string ConvertEnumValueToString(SizesOfSquare sizesOfSquare) =>
            (int)sizesOfSquare + "x" + (int)sizesOfSquare;
    }
}