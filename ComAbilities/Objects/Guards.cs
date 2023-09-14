using ComAbilities.Localizations;
using ComAbilities.Types;
using CommandSystem.Commands.RemoteAdmin.Broadcasts;
using Exiled.API.Features;
using Exiled.API.Features.Roles;
using PlayerRoles;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ComAbilities.Objects
{
    internal static class Guards
    {
        private static readonly ComAbilities Instance = ComAbilities.Instance;
        private static ErrorsT Errors => Instance.Localization.Errors;

        public static bool NotEnabled(IAbilityConfig config, out string response)
        {
            response = "";
            if (!config.Enabled)
            {
                response = Errors.NotEnabled; 
                return true;
            }
            return false;
        }

        public static bool NotComputer(Role role, out string response)
        {
            response = "";
            if (role != RoleTypeId.Scp079)
            {
                response = Errors.WrongRole;
                return true;
            }
            return false;
        }

        public static bool SignalLost(Scp079Role role, out string response)
        {
            response = "";
            if (role.IsLost)
            {
                response = Errors.SignalLost;
                return true;
            }
            return false;
        }

        public static bool SignalLost(Scp079Role role)
        {
            if (role.IsLost)
            {
                return true;
            } else
            {
                return false;
            }
        }

        public static bool NotEnoughAux(Scp079Role role, float cost, out string response)
        {
            response = "";
            if (role.Energy < cost)
            {
                response = string.Format(Errors.NotEnoughAux, CompManager.GetETA(role, cost));
                return true;
            }
            return false;
        }

        public static bool NotEnoughAuxDisplay(Scp079Role role, float cost, out string response)
        {
            response = "";
            if (role.Energy < cost)
            {
                response = string.Format(Errors.DisplayNotEnoughAux, role.AuxManager.GenerateETA(cost));
                return true;
            }
            return false;
        }

        public static bool InvalidLevel(Scp079Role role, int level, out string response)
        {
            response = "";
            if (role.Level < level)
            {
                response = string.Format(Errors.InsufficientLevel, level);
                return true;
            }
            return false;
        }

        public static bool OnCooldown(ICooldownAbility ability, out string response)
        {
            response = "";
            if (ability.OnCooldown)
            {
                response = string.Format(Errors.OnCooldown, ability.GetETA());
                return true;
            }
            return false;
        }

        public static bool OnCooldownDisplay(ICooldownAbility ability, out string response)
        {
            response = "";
            if (ability.OnCooldown)
            {
                response = string.Format(Errors.DisplayOnCooldown, ability.GetETA());
                return true;
            }
            return false;
        }
    }
}
