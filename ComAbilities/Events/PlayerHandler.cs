namespace Exiled.ComAbilitiesEvents
{
    using AdminToys;
    using ComAbilities;
    using ComAbilities.Abilities;
    using ComAbilities.Objects;
    using ComAbilities.Types;
    using ComAbilities.UI;
    using Exiled.API.Enums;
    using Exiled.API.Extensions;
    using Exiled.API.Features;
    using Exiled.API.Features.Items;
    using Exiled.API.Structs;
    using Exiled.Events.EventArgs.Interfaces;
    using Exiled.Events.EventArgs.Player;
    using Exiled.Events.EventArgs.Scp079;
    using HarmonyLib;
    using MapGeneration;
    using MEC;
    using Mirror;
    using PlayerRoles;
    using PlayerRoles.PlayableScps.Scp079.GUI;
    using Scp914;

    // using PluginAPI.Core;
    using System.Collections.Generic;
    using UnityEngine;
    using YamlDotNet.Core.Tokens;
    using static UnityEngine.GraphicsBuffer;
    using KeycardPermissions = Interactables.Interobjects.DoorUtils.KeycardPermissions;
    using Scp079Role = API.Features.Roles.Scp079Role;

    internal sealed class PlayerHandler : MonoBehaviour
    {
        private readonly ComAbilities Instance = ComAbilities.Instance;
        /* private Dictionary<HotkeyButton, Ability> _hotkeyDict = GetHotkeys();

         private Dictionary<HotkeyButton, Ability> GetHotkeys()
         {
             Dictionary<HotkeyButton, Ability> _dict = new();
             Assembly assembly = Assembly.GetExecutingAssembly();
             foreach (Type type in assembly.GetTypes())
             {
                 if ( !(type.BaseType == typeof(Ability) && type.BaseType is IHotkeyAbility) ) continue;
                 Attribute[] attrib = (Attribute[])type.GetCustomAttributes(typeof(HotkeyAttribute), true);
                 if (attrib.Length > 0 && (type is IHotkeyAbility))
                 {
                     _dict.Add((type as IHotkeyAbility)!.hotkeyButton, type);
                 }
             }
         } */

        public void OnChangingItem(ChangingItemEventArgs ev)
        {
            if (ev.Player == null || ev.Player.Role == null) return;

            if (ev.Player.Role == RoleTypeId.Scp106 && ev.Player.SessionVariables.ContainsKey(Hologram.SessionVariable))
            {
                if (ev.Item.Type != ItemType.Medkit) return;
                CompManager compManager = Instance.CompDict.GetOrError(ev.Player);
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

            if (Instance.CompDict.Contains(ev.Player))
            {
                Instance.CompDict.Remove(ev.Player);
            }
        }

        public void OnChangingRole(ChangingRoleEventArgs ev)
        {
            if (ev.Player == null || ev.Player.Role == null) return;

            Player player = ev.Player;
            if (Instance.CompDict.Contains(player) && !player.SessionVariables.ContainsKey(Hologram.SessionVariable))
            {
                Instance.CompDict.Remove(player);
            }

            if (ev.NewRole == RoleTypeId.Scp079)
            {
                //  ev.Player.ReferenceHub.roleManager.ServerSetRole(RoleTypeId.Scp079, RoleChangeReason.RemoteAdmin);
                Log.Debug("Player role is now 079 - attempting to create new CompDict");
                Instance.CompDict.Add(player);
                Timing.CallDelayed(3, () =>
                {
                    Scp079IntroCutscene.IsPlaying = false;
                });

            }
        }

        public void OnSpawning(SpawningEventArgs ev)
        {
            //ev.Player.SendFakeSyncVar(ev.Player.NetworkIdentity, typeof(Scp914Controller), nameof(Scp914Controller.Network_knobSetting), Scp914KnobSetting.OneToOne);
            Timing.CallDelayed(5, () =>
            {
                Log.Debug("Showing display");
                //try
                //{
                    PlayerDisplay display = new(ev.Player);
                    display.CreateElement(-500, "<size=50px><line-height=20px>Player Displayer Example<br>New Lines");
                    display.CreateElement(-200, "<size=40px>You are looking at an example");
                    display.CreateElement(-200, "<size=40px>Another eaxmpl e gasp");
                    display.CreateElement(-700, "<align=left>pmnixls");
                    Log.Debug("Hi chat");
                    display.Update();
          
                //} catch(Exception e)
                //{
                //}
            });
            Timing.CallDelayed(10, () =>
            {
                try
                {
                    ev.Player.SendFakeSyncVar(Scp914Controller.Singleton.netIdentity, typeof(Scp914Controller), nameof(Scp914Controller.Network_knobSetting), Scp914KnobSetting.OneToOne);
                } catch(Exception e)
                {
                    Log.Debug(e);
                }
                PlayerDisplay display = new(ev.Player);
                display.CreateElement(-500, "<size=50px><line-height=20px>Player Displayer Example\nNew Lines");
                // display.CreateElement(-200, "<size=40px>You are looking at an example");
                display.CreateElement(-400, "<size=40px>You are looking at an example");
                display.CreateElement(-700, "<align=left>pmnixls");
                Log.Debug("Hi chat");
                display.Update();

                //} catch(Exception e)
                //{
                //}
            });
            //  Hint hint = new(sb.ToString(), 5000);
            //  ev.Player.ShowHint(hint);
            if (ev.Player == null || ev.Player.Role == null) return;
            if (ev.Player.Role == RoleTypeId.Scp106 && ev.Player.SessionVariables.ContainsKey(Hologram.SessionVariable))
            {
                ev.Player.AddItem(ItemType.Painkillers);
            }
            if (ev.Player.Role == RoleTypeId.Scp079)
            {
                Timing.CallDelayed(15, () =>
                {

                    Firearm gun = (Firearm)Item.Create(ItemType.GunCOM15);
                    gun.AddAttachment(InventorySystem.Items.Firearms.Attachments.AttachmentName.Flashlight);
                    ev.Player.AddItem(gun);
                    ev.Player.CurrentItem = gun;
                });
            }
        }

        public void OnInteractingDoor(InteractingDoorEventArgs ev)
        {
            if (ev.Player == null || ev.Player.Role == null) return;

            if (!Instance.Config.DoComputerPerms)
            {
                return;
            }
            Player player = ev.Player;
            if (player.Role == PlayerRoles.RoleTypeId.Scp079)
            {
                KeycardPermissions computerPermissions = new KeycardPermissions();
                KeycardPermissions doorPerms = ev.Door.RequiredPermissions.RequiredPermissions;

                Scp079Role role = player.Role.As<Scp079Role>();
                int accessLevel = role.Level;
                foreach (KeyValuePair<KeycardPermissions, int> pair in Instance.Config.DoorPermissions)
                {
                    if (accessLevel >= pair.Value)
                    {
                        computerPermissions |= pair.Key;
                    }
                }
                if (!computerPermissions.HasFlag(doorPerms))
                {
                    ev.IsAllowed = false;
                    ev.Door.PlaySound(DoorBeepType.PermissionDenied);
                    CompManager compManager = Instance.CompDict.GetOrError(player);
                    compManager.TryShowErrorHint("- DOOR ACCESS DENIED -");
                }
            }
        }
        public void OnReloadingWeapon(ReloadingWeaponEventArgs ev) => ReceiveInput(ev, AllHotkeys.Reload);
        public void OnUnloadingWeapon(UnloadingWeaponEventArgs ev) => ReceiveInput(ev, AllHotkeys.HoldReload);
        public void OnTogglingWeaponFlashlight(TogglingWeaponFlashlightEventArgs ev) => ReceiveInput(ev, AllHotkeys.GunFlashlight);
        public void OnDroppingItem(DroppingItemEventArgs ev) { if (ev.IsThrown) ReceiveInput(ev, AllHotkeys.GunFlashlight); }


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
            if (ev.Player.Role != RoleTypeId.Scp079) return;
            CompManager compManager = Instance.CompDict.GetOrError(ev.Player);
            compManager.HandleInput(hotkey);
            ev.IsAllowed = false; 
        }
    }
}