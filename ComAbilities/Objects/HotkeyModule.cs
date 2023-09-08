using ComAbilities.Abilities;
using ComAbilities.Types;

namespace ComAbilities.Objects
{
    using Exiled.API.Enums;
    using Exiled.API.Features;
    using Exiled.API.Features.Roles;
    using System.Data;
    using System.Globalization;
    using System.Text;

    /// <summary>
    /// Handles hotkeys for a player
    /// </summary>
    public abstract class HotkeyModule
    {
        private readonly ComAbilities Instance = ComAbilities.Instance;

        private Dictionary<AllHotkeys, IHotkeyAbility> _hotkeysDict { get; set; } = new();

        public void Register(IHotkeyAbility ability)
        {
            _hotkeysDict.Add(ability.HotkeyButton, ability);
        }

       /* public bool TryHandleInput(AllHotkeys hotkey, out string response)
        {
            response = "";

            if (_hotkeysDict.TryGetValue(hotkey, out IHotkeyAbility ability)) return true;

            if (ability is ICooldownAbility rateLimitedAbility)
            {
                if (Guards.OnCooldown(rateLimitedAbility, out response))
                {
                    //TryShowErrorHint(errorCooldown);
                    return;
                }
            }
            if (Guards.NotEnoughAuxDisplay(role, ability.AuxCost, out response))
            {
                //compManager.TryShowErrorHint(response);
                return false ;
            }
            IHotkeyAbility? hotkeyAbility = ability as IHotkeyAbility;
            hotkeyAbility?.Trigger();
        } */
    }

}
    