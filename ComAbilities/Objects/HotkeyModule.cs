using ComAbilities.Abilities;
using ComAbilities.Types;

namespace ComAbilities.Objects
{
    using Exiled.API.Enums;
    using Exiled.API.Features;
    using Exiled.API.Features.Roles;
    using System.Text;

    /// <summary>
    /// Handles hotkeys for a player
    /// </summary>
    public sealed class HotkeyModule
    {
        private readonly ComAbilities Instance = ComAbilities.Instance;

        private Dictionary<AllHotkeys, IHotkeyAbility> _hotkeysDict { get; set; } = new();

        public struct HotkeyStruct
        {
            public IHotkeyAbility a;
            public Ability b;
        }
        public void Register(IHotkeyAbility ability)
        {

                _hotkeysDict.Add(ability.HotkeyButton, ability);
        }

        public void HandleInput(AllHotkeys hotkey)
        {
            if (_hotkeysDict.TryGetValue(hotkey, out IHotkeyAbility ability)) {
                ability.Trigger();
            }
        }
    }

}
    