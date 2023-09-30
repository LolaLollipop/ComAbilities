using ComAbilities.Types;
using Exiled.API.Enums;
using Exiled.API.Features;
using Exiled.API.Features.Roles;
using Exiled.API.Interfaces;
using PlayerRoles;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Permissions;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace ComAbilities.Objects
{

    internal record IndexIterator<T> (int Index, T Value);
    /// <summary>
    /// Helper functions
    /// </summary>
    internal static class Helper
    {
        public static Dictionary<RoleTypeId, string> RoleColors = new()
        {
            { RoleTypeId.ChaosRepressor, "#0D7D35" },
            { RoleTypeId.ChaosMarauder,  "#006826" },
            { RoleTypeId.ChaosConscript, "#008F1C" },
            { RoleTypeId.ChaosRifleman, "#008F1C" },
            { RoleTypeId.ClassD, "#FF8E00" },
            { RoleTypeId.NtfPrivate, "#70C3FF" },
            { RoleTypeId.NtfSergeant, "#0096FF" },
            { RoleTypeId.NtfSpecialist, "#0096FF" },
            { RoleTypeId.NtfCaptain, "#003DCA" },
            { RoleTypeId.Scientist, "#FFFF7C" },
            { RoleTypeId.FacilityGuard, "#5B6370" }
        };
        public static string? GetRoleColor(RoleTypeId role) => RoleColors.TryGetValue(role, out var color) ? color : null;

        public static T? GetClosest<T>(Vector3 position, IEnumerable<T> objects)
            where T: IPosition
        {
            T? chosen = default;
            float closestDistance = Mathf.Infinity;

            foreach (T posObject in objects)
            {
                Vector3 distanceDirection = posObject.Position - position;

                float squaredDistance = distanceDirection.sqrMagnitude;
                if (squaredDistance < closestDistance)
                {
                    closestDistance = squaredDistance;
                    chosen = posObject;
                }
            }

            return chosen;
        }

        public static string GetCleanText(string text)
        {
            string cleanText = text.Replace("</noparse>", "</nopa​rse>"); // zero width space is inserted
            return $"<noparse>{cleanText}</noparse>";
        }

        public static Player[] GetSCPs() => Player.Get(Side.Scp).ToArray();

        public static int GetETA(float cost, float current, float regenRate)
        {
            float x = (cost - current) / regenRate;
            return (int)Math.Min(Math.Round(x), 0);
        }

        public static string FormatError(string message)
        {
            return $"<align=center>Hello world </align>";
        }

        public static string ErrorNotEnoughAux(Ability ability, PlayerRoles.PlayableScps.Scp079.Scp079AuxManager auxManager)
        {
            return FormatError($"[{ability.Name.ToUpper()}] NOT ENOUGH AUX POWER. ETA: {auxManager.GenerateETA(ability.AuxCost)} SECONDS");
        }

        public static float GetETA(Scp079Role role, float cost)
        {
            if (role == null) throw new Exception("No role");
            if (cost <= role.Energy) return 0.5f;
            float regenSpeed = role.EnergyRegenerationSpeed;
            return (float)Math.Max(0.5, (role.Energy - cost) / regenSpeed);
        }

        public static IEnumerable<IndexIterator<T>> GetIndexIterator<T>(IEnumerable<T> enumerator)
        {
            return enumerator.Select((v, i) => new IndexIterator<T>(i, v));
        }
    }
}
