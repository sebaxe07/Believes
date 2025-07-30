using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Utilities.Input {
    public static class InputEnabler {
        private static string _exclusiveClassLockerHash = null;
        private static readonly object _lock = new object();
        
        private static List<string> _disabledActions = new List<string>();
        
        public static void EnableActions(List<string> actions, PlayerInput playerInput, string exclusiveClassLockerHash = null) {
            lock (_lock) {
                if(_exclusiveClassLockerHash != null && _exclusiveClassLockerHash != exclusiveClassLockerHash) return;
                if (_exclusiveClassLockerHash != null && _exclusiveClassLockerHash == exclusiveClassLockerHash) {
                    _exclusiveClassLockerHash = null;
                }
                foreach (var action in actions) {
                    var actionMethod = playerInput.actions[action];
                    actionMethod.Enable();
                }
                _disabledActions.Clear();
            }
        }

        public static void DisableActions(List<string> actions, PlayerInput playerInput, string exclusiveClassLockerHash = null) {
            lock (_lock) {
                if (_exclusiveClassLockerHash == null && exclusiveClassLockerHash != null) {
                    _exclusiveClassLockerHash = exclusiveClassLockerHash;
                }
                else if (_exclusiveClassLockerHash != null && _exclusiveClassLockerHash != exclusiveClassLockerHash) return;
                if(_exclusiveClassLockerHash!=null)DisableOverwrite(actions, playerInput);
                
                foreach (var action in actions) {
                    var actionMethod = playerInput.actions[action];
                    _disabledActions.Add(action);
                    actionMethod.Disable();
                }
            }
        }

        private static void DisableOverwrite(List<string> actions, PlayerInput playerInput) {
            foreach (var action in actions) {
                if (_disabledActions.Contains(action)) {
                    _disabledActions.Remove(action);
                }
            }

            foreach (var action in _disabledActions) {
                var actionMethod = playerInput.actions[action];
                actionMethod.Enable();
            }
            _disabledActions.Clear();
        }
        
        public static void ReleaseExclusivity(string exclusiveClassLockerHash){
            lock (_lock) {
                if (_exclusiveClassLockerHash != exclusiveClassLockerHash) return;
                _exclusiveClassLockerHash = null;
            }
        }
    }
}