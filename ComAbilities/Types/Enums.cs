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
using Utf8Json.Resolvers.Internal;

namespace ComAbilities.Types
{
    public enum MessageType
    {
        Error,
        Info
    }

    public enum GoToType
    {
        SCP,
        TrackedPlayer
    }
    public enum TrackerState
    {
        Nonexistent,
        Empty,
        Full,
        Selected,
        SelectedFull
    }
    public enum Elements
    {
        AvailableAbilities,
        ActiveAbilities,
        Message,
        Trackers
    }
    public enum DisplayTypes
    {
        Main,
        Tracker
    }
    public enum AllHotkeys
    {
        Reload,
        HoldReload,
        Throw,
        GunFlashlight,
        ADS,
        Noclip // not implemented
    }
}
