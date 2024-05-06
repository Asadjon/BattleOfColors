using UnityEngine;
using static Assets.Scripts.GameOptions;

namespace Assets.Scripts.UI
{
    internal class ToggleGroupForGameTypes : ToggleGroupForEnum<GameTypes>
    {
        [ContextMenu("Initialize")]
        protected override void Initialize()
        {
            base.Initialize();
            var toggle = System.Array.Find(GetComponentsInChildren<ToggleForEnum<GameTypes>>(), t => t.Enum == DefaultGameType);
            if (toggle) toggle.isOn = true;
        }

        protected override string GetName(GameTypes gameTypes)
        {
            switch (gameTypes)
            {
                case GameTypes.WithColor: return "Whith color";
                case GameTypes.WithNumber: return "Whith number";
                default: return string.Empty;
            }
        }
    }
}