using UnityEngine;
using static Assets.Scripts.GameOptions;

namespace Assets.Scripts.UI
{
    internal class ToggleGroupForSizesOfSquare : ToggleGroupForEnum<SizesOfSquare>
    {
        [ContextMenu("Initialize")]
        protected override void Initialize()
        {
            base.Initialize();
            var toggle = System.Array.Find(GetComponentsInChildren<ToggleForEnum<SizesOfSquare>>(), t => t.Enum == DefaultSizeOfSquare);
            if (toggle) toggle.isOn = true;
        }

        protected override string GetName(SizesOfSquare sizesOfSquare) =>
            (int)sizesOfSquare + "x" + (int)sizesOfSquare;
    }
}
