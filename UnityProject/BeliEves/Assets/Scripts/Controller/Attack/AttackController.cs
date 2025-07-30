using System;
using System.Collections;
using Events;
using Events.EventsLayout;
using UnityEngine;
using Utilities.Events.EventsLayout;

namespace Controller {
    public enum AttackEnum {
        None,
        LightAttack,
        HeavyAttack,
        SpecialAttack
    }
    public abstract class AttackController {
        protected Func<Vector3> GetPlayerDirection;
        protected AttackEnum AttackEnum = AttackEnum.None;

        //events
        private BasicEventChannel _lightAttackEvent;
        private BasicEventChannel _heavyAttackEvent;
        private BasicEventChannel _specialAttackEvent;
        private BasicEventChannel _CreateLightAttackEvent;
        private BasicEventChannel _CreateHeavyAttackEvent;
        private BasicEventChannel _CreateSpecialAttackEvent;
        private BasicEventChannel _CleanStatesEvent;
        private EventAttackHit _hitEvent;
        protected readonly EventWithBool _parryEvent;

        protected Player.Player playerScript;
        protected Npc.Enemy enemyScript;
        private Action _hitCallback;

        protected GameObject Player;
        private bool _subscribeToEvents;

        protected ProjectileSpawner _projectileSpawner;

        protected MoveTowardsEvent _moveTowardsEventChannel;

        protected Animator animator;

        protected AttackController(Func<Vector3> getPlayerDirection, bool subscribeToEvents = true) {
            GetPlayerDirection = getPlayerDirection;

            _subscribeToEvents = subscribeToEvents;

            _moveTowardsEventChannel = (MoveTowardsEvent)EventBroker.TryToAddEventChannel("movePlayerTowardsEvent", ScriptableObject.CreateInstance<MoveTowardsEvent>());
            _parryEvent = (EventWithBool)EventBroker.TryToAddEventChannel("ParryController", ScriptableObject.CreateInstance<EventWithBool>());

            if (_subscribeToEvents) {
                _lightAttackEvent = EventBroker.TryToAddEventChannel("lightAttackTriggerEvent", ScriptableObject.CreateInstance<BasicEventChannel>());
                _lightAttackEvent.Subscribe(LightAttackTrigger);

                _heavyAttackEvent = EventBroker.TryToAddEventChannel("heavyAttackTriggerEvent", ScriptableObject.CreateInstance<BasicEventChannel>());
                _heavyAttackEvent.Subscribe(HeavyAttackTrigger);

                _specialAttackEvent = EventBroker.TryToAddEventChannel("specialAttackTriggerEvent", ScriptableObject.CreateInstance<BasicEventChannel>());
                _specialAttackEvent.Subscribe(SpecialAttackTrigger);

                _CreateLightAttackEvent = EventBroker.TryToAddEventChannel("createLightAttackEvent", ScriptableObject.CreateInstance<BasicEventChannel>());
                _CreateLightAttackEvent.Subscribe(CreateLightAttack);

                _CreateHeavyAttackEvent = EventBroker.TryToAddEventChannel("createHeavyAttackEvent", ScriptableObject.CreateInstance<BasicEventChannel>());
                _CreateHeavyAttackEvent.Subscribe(CreateHeavyAttack);

                _CreateSpecialAttackEvent = EventBroker.TryToAddEventChannel("createSpecialAttackEvent", ScriptableObject.CreateInstance<BasicEventChannel>());
                _CreateSpecialAttackEvent.Subscribe(CreateSpecialAttack);

                _CleanStatesEvent = EventBroker.TryToAddEventChannel("cleanStatesEvent", ScriptableObject.CreateInstance<BasicEventChannel>());
                _CleanStatesEvent.Subscribe(CleanStates);

                _hitEvent = (EventAttackHit)EventBroker.TryToAddEventChannel("attackHitEvent", ScriptableObject.CreateInstance<EventAttackHit>());
                _hitCallback = new Action(() => AttackHit(_hitEvent.gameObject, _hitEvent.hitObj));
                _hitEvent.Subscribe(_hitCallback);
            }

        }
        protected abstract void LightAttackTrigger();
        protected abstract void HeavyAttackTrigger();
        protected abstract void SpecialAttackTrigger();
        protected abstract void AttackHit(GameObject player, GameObject hitObj);

        protected abstract void CreateLightAttack();
        protected abstract void CreateHeavyAttack();
        protected abstract void CreateSpecialAttack();
        protected abstract void CleanStates();


        public void EnemyLightAttack() {
            LightAttackTrigger();
        }
        public void EnemyHeavyAttack() {
            HeavyAttackTrigger();
        }
        public void EnemySpecialAttack() {
            SpecialAttackTrigger();
        }

        public void EnemyCreateLightAttack() {
            CreateLightAttack();
        }
        public void EnemyCreateHeavyAttack() {
            CreateHeavyAttack();
        }
        public void EnemyCreateSpecialAttack() {
            CreateSpecialAttack();
        }
        public void EnemyCleanStates() {
            CleanStates();
        }

        public void EnemyAttackHit(GameObject player, GameObject hitObj) {
            AttackHit(player, hitObj);
        }

        public void SetGameObject(GameObject player) {
            Player = player;
            playerScript = Player.GetComponent<Player.Player>();
            enemyScript = Player.GetComponent<Npc.Enemy>();
        }

        public void DestroyAttackController() {
            _lightAttackEvent.Unsubscribe(LightAttackTrigger);
            _heavyAttackEvent.Unsubscribe(HeavyAttackTrigger);
            _specialAttackEvent.Unsubscribe(SpecialAttackTrigger);
            _CreateLightAttackEvent.Unsubscribe(CreateLightAttack);
            _CreateHeavyAttackEvent.Unsubscribe(CreateHeavyAttack);
            _CreateSpecialAttackEvent.Unsubscribe(CreateSpecialAttack);
            _CleanStatesEvent.Unsubscribe(CleanStates);
            _hitEvent.Unsubscribe(_hitCallback);
        }

        public void SetProjectileSpawner(ProjectileSpawner projectileSpawner) {
            _projectileSpawner = projectileSpawner;
        }

        protected IEnumerator _attackCoolDownCoroutine(float seconds, bool player, int attackType) {
            yield return new WaitForSeconds(seconds);
            if (player) {
                playerScript.LightCooldown = false;
                playerScript.HeavyCooldown = false;
                playerScript.SpecialCooldown = false;

            }
            else {
                enemyScript.LightCooldown = false;
                enemyScript.HeavyCooldown = false;
                enemyScript.SpecialCooldown = false;
            }
        }
        protected IEnumerator _attackDurationCoroutine(GameObject gameObject, float seconds, bool player) {
            yield return new WaitForSeconds(seconds);
            gameObject.SetActive(false);
            if (player) playerScript.isAttackActive = false;
            else enemyScript.isAttackActive = false;
            AttackEnum = AttackEnum.None;
        }
    }
}