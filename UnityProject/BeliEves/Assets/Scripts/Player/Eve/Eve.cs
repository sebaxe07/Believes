using System;
using Events;
using Events.EventsLayout;
using ScriptableObjects.Attack;
using Unity.VisualScripting;
using UnityEngine;

namespace Player {
    public class Eve : Player {
        private GameObject _prefabInstance; 
        private EveAttack _eveAttack;

        [SerializeField]private float minTimeForLong = 0.5f;
        private float _startingTime =0;
        
        private RaycastHit _hit;
        private BasicEventChannel _stopEveFlightActionEventChannel;
        private void Awake() {
            _eveAttack = Resources.Load<EveAttack>("Settings/AttackSettings/EveAttack");
            var prefab = Resources.Load<GameObject>("Vfx/SelectedArea");
            _prefabInstance = Instantiate(prefab);
            _prefabInstance.SetActive(false);
            
            _stopEveFlightActionEventChannel = ScriptableObject.CreateInstance<BasicEventChannel>();
            _stopEveFlightActionEventChannel = EventBroker.TryToAddEventChannel("heavyAttackStop", _stopEveFlightActionEventChannel);
        }

        protected new void Update() {
            LightAttackAim();
            if(_hit.transform!=null && Vector3.Distance(_hit.transform.position, transform.position)> _eveAttack.dashDistance)_prefabInstance.SetActive(false);
            base.Update();

            if (HeavyAttackAction.WasPressedThisFrame()) _startingTime = Time.time; // Record the time when the button is pressed.

            if (HeavyAttackAction.WasReleasedThisFrame() && Time.time - _startingTime >= minTimeForLong) {
                _startingTime = 0f; // Reset the starting time.
                _stopEveFlightActionEventChannel.RaiseEvent(); // Trigger the action.
            }
        }
        private void LightAttackAim() {
            if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out _hit, Mathf.Infinity, LayerMask.GetMask("Npc"))) {
                if(_prefabInstance.activeInHierarchy) return;
                
                Vector3 hitPoint = _hit.point; // Punto colpito sul suolo
                Vector3 playerPosition = transform.position; // Posizione del giocatore

                float distance = Vector3.Distance(new Vector3(hitPoint.x, 0, hitPoint.z), 
                    new Vector3(playerPosition.x, 0, playerPosition.z));
                if (distance > _eveAttack.dashDistance) return;
                
                _prefabInstance.transform.SetParent(_hit.transform);
                _prefabInstance.SetActive(true);
                _prefabInstance.name = "SelectedArea";
                //Debug.Log(_prefabInstance.GetHashCode());
                
                
                Vector3 worldPosition = new Vector3(
                    _hit.collider.bounds.center.x, 
                    _hit.collider.bounds.min.y, 
                    _hit.collider.bounds.center.z
                );
                Transform fatherTransform = _prefabInstance.transform.parent;
                
                Bounds bounds = _hit.collider.bounds;
                Vector3 basePosition = fatherTransform.InverseTransformPoint(worldPosition);
                _prefabInstance.transform.localPosition = basePosition;
                
            }
            else if (_prefabInstance != null){
                 _prefabInstance.SetActive(false);
                 //Destroy(_prefabInstance);
            }
        }
    }
}