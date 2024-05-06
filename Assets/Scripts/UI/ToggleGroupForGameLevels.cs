using UnityEngine;
using static Assets.Scripts.GameOptions;

namespace Assets.Scripts.UI
{
    internal class ToggleGroupForGameLevels : ToggleGroupForEnum<GameLevels>
    {

        [ContextMenu("Initialize")]
        protected override void Initialize()
        {
            base.Initialize();
            var toggle = System.Array.Find(GetComponentsInChildren<ToggleForEnum<GameLevels>>(), t => t.Enum == DefaultGameLevel);
            if (toggle) toggle.isOn = true;
        }

        protected override string GetName(GameLevels gameLevels) => gameLevels.ToString();
    }
}
