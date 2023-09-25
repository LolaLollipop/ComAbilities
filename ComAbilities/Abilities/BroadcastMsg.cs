using ComAbilities.Objects;
using ComAbilities.Types;
using Exiled.API.Features;
using ComAbilities.Localizations;

namespace ComAbilities.Abilities
{
    public sealed class BroadcastMsg : Ability, ICooldownAbility
    {
        private readonly static ComAbilities Instance = ComAbilities.Instance;

        private static BroadcastMessageT BroadcastMessageT => Instance.Localization.BroadcastMessage;
        private static BroadcastMessageConfig _config => Instance.Config.BroadcastMessage;

        public BroadcastMsg(CompManager compManager) : base(compManager) { }

        public override string Name { get; } = BroadcastMessageT.Name;
        public override string Description { get; } = BroadcastMessageT.Description;
        public override float AuxCost { get; } = _config.AuxCost;
        public override int ReqLevel { get; } = _config.Level;
        public override string DisplayText => string.Format(BroadcastMessageT.DisplayText, AuxCost);
        public override bool Enabled { get; } = _config.Enabled;
 
        
        public float CooldownLength { get; } = _config.Cooldown;
        public bool OnCooldown => _cooldown.Active;
        private Cooldown _cooldown { get; } = new();

        public int GetETA()
        {
            if (_cooldown == null) throw new Exception("Attempt to get ETA of a null rateLimitTask");
            float? eta = _cooldown.GetETA();
            if (!eta.HasValue) throw new Exception("Attempt to get ETA of a null rateLimitTask");
            return (int)eta;
        }
        public void Trigger(string content)
        {
            //  value.Length <= maxChars ? value : value.Substring(0, maxChars) + "...";
            string playerName = Helper.GetCleanText(CompManager.AscPlayer.DisplayNickname);
            string messageName = playerName.Length <= _config.MaxPlayerNameLength ? playerName : playerName.Substring(0, _config.MaxPlayerNameLength) + "...";

            Exiled.API.Features.Broadcast broadcast = new(string.Format(BroadcastMessageT.BroadcastFormat, messageName, Helper.GetCleanText(content), (ushort)_config.MessageDuration));
            foreach (Player scp in Helper.GetSCPs())
            {
                scp.Broadcast(broadcast);
            }
        }
        public void UpdateText()
        {
            // _cooldownTask.Run();
        }

        public override void KillTasks() { }
    }
}
