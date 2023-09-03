using ComAbilities.Types;
using CommandSystem.Commands.RemoteAdmin.Broadcasts;
using Exiled.API.Features;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ComAbilities.Objects
{
    public class CompDict
    {
        private readonly static ComAbilities Instance = ComAbilities.Instance;
        private readonly static Dictionary<Player, CompManager> _playerComputers = new();

        public void CleanUp()
        {
            foreach (CompManager computer in _playerComputers.Values)
            {
                computer.KillAll();
            }
            _playerComputers.Clear();
        }

        public void Remove(Player key)
        {
            _playerComputers[key].KillAll();
            _playerComputers.Remove(key);
        }
        public bool Contains(Player key)
        {
            return _playerComputers.ContainsKey(key);
        }
        public CompManager? Get(Player key)
        {
            CompManager comp = _playerComputers[key];
            return _playerComputers[key];
        }
        public bool Get(Player key, out CompManager? compManager)
        {
            CompManager comp = _playerComputers[key];
            compManager = comp;
            return _playerComputers is not null;
        }
        public CompManager GetOrError(Player key)
        {
            CompManager comp = _playerComputers[key];
            if (comp == null) throw new Exception($"Player {key} not found");
            return _playerComputers[key];
        }

        public void Add(Player key)
        {
            _playerComputers.Add(key, new CompManager(key));
        }

        public CompManager AddReturn(Player key)
        {
            _playerComputers.Add(key, new CompManager(key));
            return _playerComputers[key];
        }
    }

}
