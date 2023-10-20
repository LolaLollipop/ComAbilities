using Exiled.API.Features;
using Exiled.Events.EventArgs.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ComAbilities.RueI.Events
{
    public class RateLimitFinishedEventArgs : EventArgs, IPlayerEvent
    {
        /// <inheritdoc>/>
        public Player Player { get; }

        public RateLimitFinishedEventArgs(Player player)
        {
            Player = player;
        }
    }
}
