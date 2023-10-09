using ComAbilities.Objects;
using ComAbilities.Types;
using Exiled.API.Features;
using ComAbilities.Localizations;

namespace ComAbilities.Abilities
{
    public sealed class BroadcastMsg : Ability, ICooldownAbility
    {
        private static BroadcastMessageT BroadcastMessageT => Instance.Localization.BroadcastMessage;
        private static BroadcastMessageConfig config => Instance.Config.BroadcastMessage;

        private Cooldown cooldown { get; } = new();

        public BroadcastMsg(CompManager compManager) : base(compManager) { }

        public override string Name => BroadcastMessageT.Name;
        public override string Description => BroadcastMessageT.Description;
        public override float AuxCost => config.AuxCost;
        public override int ReqLevel => config.Level;
        public override string DisplayText => string.Format(BroadcastMessageT.DisplayText, AuxCost);
        public override bool Enabled => config.Enabled;
 
        public float CooldownLength => config.Cooldown;
        public bool OnCooldown => cooldown.Active;

        public float GetDisplayETA() => cooldown.GetDisplayETA();

        public void Trigger(string content)
        {
            //  value.Length <= maxChars ? value : value.Substring(0, maxChars) + "...";
            string playerName = Helper.GetCleanText(CompManager.AscPlayer.DisplayNickname);
            string messageName = playerName.Length <= config.MaxPlayerNameLength ? playerName : playerName.Substring(0, config.MaxPlayerNameLength) + "...";

            Exiled.API.Features.Broadcast broadcast = new(string.Format(BroadcastMessageT.BroadcastFormat, messageName, Helper.GetCleanText(content), (ushort)config.MessageDuration));
            foreach (Player scp in Helper.GetSCPs())
            {
                scp.Broadcast(broadcast);
            }
        }

        public override void CleanUp() { }
    }
}
