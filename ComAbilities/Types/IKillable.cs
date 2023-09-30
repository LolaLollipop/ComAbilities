using ComAbilities.Objects;
using Exiled.API.Enums;
using Exiled.API.Features;
using Exiled.API.Features.Roles;
using GameCore;
using MEC;
using PlayerRoles;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Utf8Json.Resolvers.Internal;

namespace ComAbilities.Types
{
    /// <summary>
    /// Allows for easy cooldowns and timed waits
    /// </summary>
    public interface IKillable
    {
        public void CleanUp();
    }
}
