using ComAbilities.RueI.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RueI
{
    using Exiled.Events.Features;
    public static class Events
    {
        public static Event<RateLimitFinishedEventArgs> RateLimitFinished { get; set; } = new();

        internal static void OnRateLimitFinished(RateLimitFinishedEventArgs ev) => RateLimitFinished.InvokeSafely(ev);
    }
}
