using System.Collections;
using System.Collections.Generic;
using Pada1.BBCore;
using Pada1.BBCore.Tasks;
using BBUnity.Actions;
using UnityEngine;
using Npc;
using UnityEngine.AI;
using Utilities;

// Create a type of robot to be selected in the robotType parameter
// Create a type of robot to be selected in the robotType parameter
public enum RobotType {
    Support,
    Tank,
    Agile
}


[Action("Enemy/HeavyAttackPlayer")]
[Help("Use the heavy attack on the player")]
public class HeavyAttackPlayer : GOAction {

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
    private Coroutine circleMovementCoroutine;
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
        enemyScript.SendHeavyAttack();

        if (robotType == RobotType.Support) {
            // Have a 33% chance of doing the closeup movement
            if (Random.Range(0, 3) == 0) {
                // Try to get closer to the player after attacking if the robot is a support type
                navAgent.SetDestination(foundGameObject.transform.position);
#if UNITY_5_6_OR_NEWER
                navAgent.isStopped = false;
#else
            navAgent.Resume();
#endif
            }
        }
        else if (robotType == RobotType.Agile) {
            // Try to move around the player after attacking if the robot is an agile type, in a circle around the player
            if (circleMovementCoroutine != null) {
                CoroutineRunner.Instance.StopCoroutine(circleMovementCoroutine);
            }
            circleMovementCoroutine = CoroutineRunner.Instance.RunCoroutineAndGet(MoveInCircleAroundPlayer(foundGameObject.transform));
#if UNITY_5_6_OR_NEWER
            navAgent.isStopped = false;
#else
            navAgent.Resume();
#endif
        }
        else if (robotType == RobotType.Tank) {
            // Try to close the distance to the player after attacking if the robot is a tank type
            navAgent.SetDestination(foundGameObject.transform.position);
#if UNITY_5_6_OR_NEWER
            navAgent.isStopped = false;
#else
            navAgent.Resume();
#endif
        }

        return TaskStatus.COMPLETED;
    }

    public override void OnAbort() {
        if (circleMovementCoroutine != null) {
            CoroutineRunner.Instance.StopCoroutine(circleMovementCoroutine);
        }
        if (navAgent == null) {
            return;
        }
        navAgent.updateRotation = true;

#if UNITY_5_6_OR_NEWER
        navAgent.isStopped = true;
#else
        navAgent.Stop();
#endif
    }

    private IEnumerator MoveInCircleAroundPlayer(Transform playerTransform) {
        // Radius of the circle is the distance from the player
        float radius = Vector3.Distance(playerTransform.position, navAgent.transform.position);

        float speed = 0.2f;
        float angle = 0f;

        while (true) {
            angle += speed * Time.deltaTime;
            float x = playerTransform.position.x + Mathf.Cos(angle) * radius;
            float z = playerTransform.position.z + Mathf.Sin(angle) * radius;
            Vector3 newPosition = new Vector3(x, navAgent.transform.position.y, z);
            if (navAgent == null) {
                yield break;
            }
            if (navAgent.isOnNavMesh == true) {
                navAgent.SetDestination(newPosition);
                navAgent.transform.LookAt(playerTransform);
            }
            else {
                yield break;
            }
            yield return null;
        }
    }

}