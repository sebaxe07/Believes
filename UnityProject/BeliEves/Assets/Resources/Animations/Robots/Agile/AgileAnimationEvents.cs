using System.Collections;
using System.Collections.Generic;
using Ami.BroAudio;
using Controller.Attack;
using Events;
using Events.EventsLayout;
using Npc;
using ScriptableObjects.Attack;
using UnityEngine;

public class AgileAnimationEvents : MonoBehaviour {

    public Enemy enemyScript;
    private AgileAttackSettings _agileAttackSettings = null;
    private BasicEventChannel _CreateLightAttackEvent;
    private BasicEventChannel _CreateHeavyAttackEvent;
    private BasicEventChannel _CreateSpecialAttackEvent;
    private BasicEventChannel _CleanStatesEvent;

    private void Start() {
        _agileAttackSettings = Resources.Load<AgileAttackSettings>("Settings/AttackSettings/AgileAttack");
        // Get the component from the parent

        _CreateLightAttackEvent = EventBroker.TryToAddEventChannel("createLightAttackEvent", ScriptableObject.CreateInstance<BasicEventChannel>());

        _CreateHeavyAttackEvent = EventBroker.TryToAddEventChannel("createHeavyAttackEvent", ScriptableObject.CreateInstance<BasicEventChannel>());

        _CreateSpecialAttackEvent = EventBroker.TryToAddEventChannel("createSpecialAttackEvent", ScriptableObject.CreateInstance<BasicEventChannel>());

        _CleanStatesEvent = EventBroker.TryToAddEventChannel("cleanStatesEvent", ScriptableObject.CreateInstance<BasicEventChannel>());

    }

    public void JumpSound() {
        BroAudio.Play(_agileAttackSettings.jumpSound);
    }

    public void LightAttackSoundL() {
        if (enemyScript != null) {
            enemyScript.CreateEnemyLightAttack();
        }
        else {
            _CreateLightAttackEvent.RaiseEvent();
        }
    }

    public void LightAttackSoundR() {
        if (enemyScript != null) {
            enemyScript.CreateEnemyLightAttack();
        }
        else {
            _CreateLightAttackEvent.RaiseEvent();
        }
    }

    public void HeavyAttackSound() {
        BroAudio.Play(_agileAttackSettings.grapplingShootSound, transform);
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
        BroAudio.Play(_agileAttackSettings.lightPrepareAttackSound);
    }

    public void Steps() {
        BroAudio.Play(_agileAttackSettings.steps, transform.position);
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
