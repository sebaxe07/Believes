using System.Collections;
using System.Collections.Generic;
using Pada1.BBCore;
using Pada1.BBCore.Tasks;
using BBUnity.Actions;
using UnityEngine;
using Npc;
using UnityEngine.AI;

[Action("Enemy/LightAttackPlayer")]
[Help("Use the light attack on the player")]
public class LightAttackPlayer : GOAction {

    ///<value>OutPut Found game object Parameter.</value>
    [InParam("foundGameObject")]
    [Help("Found game object")]
    public GameObject foundGameObject;

    [InParam("enemyScript")]
    [Help("Enemy script")]
    public Enemy enemyScript;

    [InParam("RobotType")]
    [Help("Robot type")]
    public RobotType robotType;
    private NavMeshAgent navAgent;
    public override void OnStart() {
        navAgent = gameObject.GetComponent<UnityEngine.AI.NavMeshAgent>();
        if (navAgent == null) {
            // Debug.LogWarning("The " + gameObject.name + " game object does not have a Nav Mesh Agent component to navigate. One with default values has been added", gameObject);
            navAgent = gameObject.AddComponent<UnityEngine.AI.NavMeshAgent>();
        }
    }

    public override TaskStatus OnUpdate() {
        if (enemyScript == null) {
            // Debug.LogError("Enemy script is null");
            return TaskStatus.FAILED;
        }
        enemyScript.SendLightAttack();

        if (robotType == RobotType.Support || robotType == RobotType.Agile) {

            // Try to get away from the player by moving backwards after attacking
            navAgent.updateRotation = false;
            Vector3 randomOffset = new Vector3(Random.Range(-2f, 2f), 0, Random.Range(-2f, 2f));
            Vector3 destination = gameObject.transform.position - gameObject.transform.forward * 10 + randomOffset;
            navAgent.SetDestination(destination);

#if UNITY_5_6_OR_NEWER
            navAgent.isStopped = false;
#else
            navAgent.Resume();
#endif
        }
        return TaskStatus.COMPLETED;
    }

    public override void OnAbort() {
        if (navAgent == null) {
            return;
        }
        navAgent.updateRotation = true;
    }

}