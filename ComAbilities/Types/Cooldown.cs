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
    public class Cooldown
    {
        public bool Active => IsActive();

        private DateTimeOffset? _startedAt { get; set; }
        private float? _length { get; set; }

        public void Start(float time)
        {
            _startedAt = new DateTimeOffset();
            this._length = time;
        }
        public float? GetETA()
        {
            if (_startedAt == null) return null;
            if (_length == null) return null;
            return (new DateTimeOffset().ToUnixTimeMilliseconds() + _length - _startedAt.Value.ToUnixTimeMilliseconds());
        }
        public long? RunningFor()
        {
            if (_startedAt == null) return null;

            return new DateTimeOffset().ToUnixTimeMilliseconds() - _startedAt.Value.ToUnixTimeMilliseconds();
        }
        private bool IsActive()
        {
            if (this._startedAt == null) return false;
            if (this._length == null) return false;
            DateTimeOffset now = new DateTimeOffset();
            return (now.ToUnixTimeMilliseconds() - _startedAt.Value.ToUnixTimeMilliseconds() > _length);
        }
    }
}
