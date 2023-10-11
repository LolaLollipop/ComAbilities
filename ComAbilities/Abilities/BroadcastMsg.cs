using ComAbilities.Objects;
using ComAbilities.Types;
using Exiled.API.Features;
using ComAbilities.Localizations;

namespace ComAbilities.Abilities
{
    public sealed class BroadcastMsg : Ability, ICooldownAbility
    {
        private static BroadcastMessageT Translation => Instance.Localization.BroadcastMessage;
        private static BroadcastMessageConfig Config => Instance.Config.BroadcastMessage;

        private readonly Cooldown cooldown = new();

        public BroadcastMsg(CompManager compManager) : base(compManager) { }

        public override string Name => Translation.Name;
        public override string Description => Translation.Description;
        public override float AuxCost => Config.AuxCost;
        public override int ReqLevel => Config.Level;
        public override string DisplayText => string.Format(Translation.DisplayText, AuxCost);
        public override bool Enabled => Config.Enabled;
 
        public float CooldownLength => Config.Cooldown;
        public bool OnCooldown => cooldown.Active;

        public float GetDisplayETA() => cooldown.GetDisplayETA();

        public void Trigger(string content)
        {
            //  value.Length <= maxChars ? value : value.Substring(0, maxChars) + "...";
            string playerName = Helper.GetCleanText(CompManager.AscPlayer.DisplayNickname);
            string messageName = playerName.Length <= Config.MaxPlayerNameLength ? playerName : playerName.Substring(0, Config.MaxPlayerNameLength) + "...";

            Exiled.API.Features.Broadcast broadcast = new(string.Format(Translation.BroadcastFormat, messageName, Helper.GetCleanText(content), (ushort)Config.MessageDuration));
            foreach (Player scp in Helper.GetSCPs())
            {
                scp.Broadcast(broadcast);
            }
        }

        public override void CleanUp() { }
    }
}
