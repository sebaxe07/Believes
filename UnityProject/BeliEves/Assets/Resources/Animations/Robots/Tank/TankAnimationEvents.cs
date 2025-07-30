using System.Collections;
using System.Collections.Generic;
using Ami.BroAudio;
using Controller.Attack;
using Events;
using Events.EventsLayout;
using Npc;
using ScriptableObjects.Attack;
using UnityEngine;

public class TankAnimationEvents : MonoBehaviour {

    public Enemy enemyScript;
    private TankAttack _tankAttack = null;
    private BasicEventChannel _CreateLightAttackEvent;
    private BasicEventChannel _CreateHeavyAttackEvent;
    private BasicEventChannel _CreateSpecialAttackEvent;
    private BasicEventChannel _CleanStatesEvent;

    private void Start() {
        _tankAttack = Resources.Load<TankAttack>("Settings/AttackSettings/TankAttack");
        // Get the component from the parent

        _CreateLightAttackEvent = EventBroker.TryToAddEventChannel("createLightAttackEvent", ScriptableObject.CreateInstance<BasicEventChannel>());

        _CreateHeavyAttackEvent = EventBroker.TryToAddEventChannel("createHeavyAttackEvent", ScriptableObject.CreateInstance<BasicEventChannel>());

        _CreateSpecialAttackEvent = EventBroker.TryToAddEventChannel("createSpecialAttackEvent", ScriptableObject.CreateInstance<BasicEventChannel>());

        _CleanStatesEvent = EventBroker.TryToAddEventChannel("cleanStatesEvent", ScriptableObject.CreateInstance<BasicEventChannel>());
    }

    public void JumpSound() {
        BroAudio.Play(_tankAttack.jumpSound);
    }

    public void LightAttackSound() {
        if (enemyScript != null) {
            enemyScript.CreateEnemyLightAttack();
        }
        else {
            _CreateLightAttackEvent.RaiseEvent();
        }
    }

    public void HeavyAttackSound() {
        BroAudio.Play(_tankAttack.heavyAttackSound, transform.position);

        if (enemyScript != null) {
            enemyScript.CreateEnemyHeavyAttack();
        }
        else {
            _CreateHeavyAttackEvent.RaiseEvent();
        }
    }

    public void SpecialAttackSound() {
        if (enemyScript != null) {
            enemyScript.CreateEnemySpecialAttack();
        }
        else {
            _CreateSpecialAttackEvent.RaiseEvent();
        }
    }

    public void LightPrepareAttackSound() {
        BroAudio.Play(_tankAttack.lightPrepareAttackSound);
    }

    public void Steps() {
        BroAudio.Play(_tankAttack.steps, transform.position);
    }

    public void CleanStates() {
        if (enemyScript != null) {
            enemyScript.CleanStates();
        }
        else {
            _CleanStatesEvent.RaiseEvent();
        }
    }

}
