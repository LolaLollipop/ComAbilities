namespace Exiled.ComAbilitiesEvents
{
    using ComAbilities;
    using ComAbilities.Abilities;
    using ComAbilities.Objects;
    using ComAbilities.Types;

    using Exiled.API.Enums;
    using Exiled.API.Extensions;
    using Exiled.API.Features;
    using Exiled.API.Features.Doors;
    using Exiled.API.Features.Items;
    using Exiled.Events.EventArgs.Interfaces;
    using Exiled.Events.EventArgs.Player;
    using MEC;
    using PlayerRoles;
    using RueI;
    using Scp914;


    using System.Collections.Generic;
    using UnityEngine;
    using KeycardPermissions = Interactables.Interobjects.DoorUtils.KeycardPermissions;
    using Scp079Role = API.Features.Roles.Scp079Role;

    internal sealed class PlayerHandler : MonoBehaviour
    {
        private static ComAbilities Instance => ComAbilities.Instance;
        private static CompDict compDict => Instance.CompDict;

        public void OnChangingItem(ChangingItemEventArgs ev)
        {
            if (ev.Player == null || ev.Player.Role == null) return;

            if (ev.Player.Role == RoleTypeId.Scp106 && ev.Player.SessionVariables.ContainsKey(Hologram.SessionVariable))
            {
                if (ev.Item.Type != ItemType.Medkit) return;
                CompManager compManager = compDict.GetOrError(ev.Player);
                if (compManager.Hologram.ConfirmationPressed)
                {
                    compManager.Hologram.ChangeBack();
                }
                else
                {
                    compManager.Hologram.ActivateConfirmation();
                }
            }
            if (ev.Player.Role == RoleTypeId.Scp079)
            {
               /* Scp079Role role = ev.Player.Role.Cast<Scp079Role>();
                if (Guards.SignalLost(role)) { ev.IsAllowed = false; return; }
                CompManager compManager = Instance.CompDict.GetOrError(ev.Player);

                AllHotkeys? hotkey = ev.Item.Type switch // convert hotkey from HotkeyButton to FullHotkeys (support for all actions)
                {
                    ItemType.GunCOM15 => AllHotkeys.PrimaryFirearm,
                    ItemType.GunCOM18 => AllHotkeys.SecondaryFirearm,
                    ItemType.Medkit => AllHotkeys.Medical,
                    ItemType.GrenadeFlash => AllHotkeys.Grenade,
                    ItemType.KeycardJanitor => AllHotkeys.Keycard,
                    _ => null
                };

                if (compManager.DisplayManager.SelectedScreen == DisplayTypes.Tracker)
                {
                    compManager.PlayerTracker.HandleInputs(hotkey);
                    return;
                }
                if (hotkey == null || !compManager.Hotkey.TryGetValue(hotkey.Value, out Ability ability)) return;
                if (ability is ICooldownAbility rateLimitedAbility)
                {
                    if (Guards.OnCooldown(rateLimitedAbility, out string errorCooldown)) {
                        compManager.TryShowErrorHint(errorCooldown);
                        ev.IsAllowed = false;
                        return;
                    }
                }
                if (Guards.NotEnoughAuxDisplay(role, ability.AuxCost, out string response))
                {
                    compManager.TryShowErrorHint(response);
                    ev.IsAllowed = false;
                    return;
                }
                IHotkeyAbility? hotkeyAbility = ability as IHotkeyAbility;
                hotkeyAbility?.Trigger();
                ev.IsAllowed = false; */
            }
        }

        public void OnShot(ShotEventArgs ev)
        {
            if (ev.Player == null || ev.Player.Role == null) return;

            if (ev.Target != null)
            {
                if (ev.Target.SessionVariables[Hologram.SessionVariable] is not null and (bool)true)
                {
                    RaycastHit hit = ev.RaycastHit;
                    Physics.Raycast(hit.point, hit.normal * -1, out RaycastHit newHit);
                }
            }
        }

        public void OnLeft(LeftEventArgs ev)
        {
            if (ev.Player == null || ev.Player.Role == null) return;

            if (Instance.CompDict.Contains(ev.Player)) Instance.CompDict.Remove(ev.Player);
        }

        public void OnChangingRole(ChangingRoleEventArgs ev)
        {
            if (ev.Player == null || ev.Player.Role == null) return;

            Player player = ev.Player;
            if (!player.SessionVariables.ContainsKey(Hologram.SessionVariable))
            {
                if (Instance.CompDict.Contains(player)) Instance.CompDict.Remove(player);
            }

            if (ev.NewRole == RoleTypeId.Scp079)
            {
                //  ev.Player.ReferenceHub.roleManager.ServerSetRole(RoleTypeId.Scp079, RoleChangeReason.RemoteAdmin);
                Log.Debug("Player role is now 079 - attempting to create new CompDict");
                Instance.CompDict.Add(player);

            }
        }

        public void OnSpawning(SpawningEventArgs ev)
        {
            IEnumerable<Door> doors = Door.List.Where(x => x.Type == DoorType.Scp914Door);
            PlayerDisplay display = new(ev.Player);
            SetElement element = new(500, 5, "Hello world!!!!!\n\n\n\n\n\n\n\n\n\n\n\n\n999");
            SetElement anotherElement = new(400, 5, "hi");
            display.Add(element, anotherElement);
            display.Update();
            foreach (Door door in doors)
            {
                try
                {
                   // ev.Player.SendFakeSyncVar(door.Base.netIdentity, typeof(DoorVariant), nameof(DoorVariant.NetworkTargetState), false);
                } catch(Exception e)
                {
                    Log.Debug(e);
                }
            }

            ev.Player.SendFakeSyncVar(Scp914Controller.Singleton.netIdentity, typeof(Scp914Controller), nameof(Scp914Controller.Network_knobSetting), Scp914KnobSetting.OneToOne);
            MirrorExtensions.SendFakeTargetRpc(ev.Player, Scp914Controller.Singleton.netIdentity, typeof(Scp914Controller), nameof(Scp914Controller.RpcPlaySound), 1);

            Player player = ev.Player;
            if (player == null || player.Role == null) return;

            if (player.Role == RoleTypeId.Scp106 && player.SessionVariables.ContainsKey(Hologram.SessionVariable))
            {
                player.AddItem(ItemType.Painkillers);
            }

            CoroutineHandle coroutineHandle = Timing.CallDelayed(5000, () =>
            {

            });

            if (player.Role == RoleTypeId.Scp079)
            {
                Timing.CallDelayed(15, () =>
                {
                    Log.Debug(Timing.IsRunning(coroutineHandle));
                    Firearm gun = (Firearm)Item.Create(ItemType.GunCOM15);
                    gun.AddAttachment(InventorySystem.Items.Firearms.Attachments.AttachmentName.Flashlight);
                    player.AddItem(gun);
                    player.CurrentItem = gun;
                    PlayerDisplay display = new(ev.Player);
                    SetElement element = new(500, 5, "Hello world!!!!!\n\n\n\n\n\n\n\n\n\n\n\n\n999");
                  //  SetElement anotherElement = new(400, 5, "hi");
                    display.Add(element);
                    display.Update();
                });
            }
        }

        public void OnInteractingDoor(InteractingDoorEventArgs ev)
        {
            if (ev.Player == null || ev.Player.Role == null) return;

            if (!Instance.Config.DoComputerPerms) return;

            Player player = ev.Player;
            if (player.Role is Scp079Role role)
            {
                if (Helper.CanOpenDoor(player, ev.Door))
                {

                }
            }
        }
        public void OnReloadingWeapon(ReloadingWeaponEventArgs ev) => ReceiveInput(ev, AllHotkeys.Reload);
        public void OnUnloadingWeapon(UnloadingWeaponEventArgs ev) => ReceiveInput(ev, AllHotkeys.HoldReload);
        public void OnTogglingWeaponFlashlight(TogglingWeaponFlashlightEventArgs ev) => ReceiveInput(ev, AllHotkeys.GunFlashlight);
        public void OnDroppingItem(DroppingItemEventArgs ev) { if (ev.IsThrown) ReceiveInput(ev, AllHotkeys.Throw); }


        internal void DenyHologram<T>(T ev)
        where T : IDeniableEvent, IPlayerEvent
        {
            if (ev.Player == null || ev.Player.Role == null) return;

            if (ev.Player.Role == RoleTypeId.Scp106 && ev.Player.SessionVariables[Hologram.SessionVariable] != null)
            {
                ev.IsAllowed = false;
            }
        }
        internal void ReceiveInput<T>(T ev, AllHotkeys hotkey)
        where T: IDeniableEvent, IPlayerEvent
        {
            if (ev.Player == null) return;
            if (ev.Player.Role != RoleTypeId.Scp079) return;

            CompManager compManager = Instance.CompDict.GetOrError(ev.Player);
            compManager.HandleInput(hotkey);
            ev.IsAllowed = false; 
        }
    }
}