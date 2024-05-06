using System.Collections;
using UnityEngine;
using static Assets.Scripts.GameOptions;

namespace Assets.Scripts.UI
{
    internal class ToggleForGameTypes : ToggleForEnum<GameTypes>
    {
        protected override string ConvertEnumValueToString(GameTypes gameTypes)
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