using System.Collections;
using UnityEngine;
using static Assets.Scripts.GameOptions;

namespace Assets.Scripts.UI
{
    internal class ToggleForGameLevels : ToggleForEnum<GameLevels>
    {
        protected override string ConvertEnumValueToString(GameLevels gameLevels) => gameLevels.ToString();
    }
}