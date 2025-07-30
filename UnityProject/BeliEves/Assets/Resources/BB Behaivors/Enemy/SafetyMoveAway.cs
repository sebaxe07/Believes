using Pada1.BBCore.Tasks;
using Pada1.BBCore;
using UnityEngine;
using Npc;

namespace BBUnity.Actions {
    /// <summary>
    /// It is an action to move towards the given goal using a NavMeshAgent.
    /// </summary>
    [Action("Navigation/SafetyMoveAway")]
    [Help("Moves the game object towards a given target by using a NavMeshAgent")]
    public class SafetyMoveAway : GOAction {
        ///<value>Input target game object towards this game object will be moved Parameter.</value>
        [InParam("target")]
        [Help("Target game object towards this game object will be moved")]
        public GameObject target;

        [InParam("enemyScript")]
        [Help("Enemy script")]
        public Enemy enemyScript;


        private UnityEngine.AI.NavMeshAgent navAgent;

        private Transform targetTransform;
        private Transform _closestRobot;

        private float offsetDistance = 3f;

        /// <summary>Initialization Method of SafetyMoveAway.</summary>
        /// <remarks>Check if GameObject object exists and NavMeshAgent, if there is no NavMeshAgent, the default one is added.</remarks>
        public override void OnStart() {
            if (target == null) {
                // Debug.LogError("The movement target of this game object is null", gameObject);
                return;
            }
            targetTransform = target.transform;

            navAgent = gameObject.GetComponent<UnityEngine.AI.NavMeshAgent>();
            if (navAgent == null) {
                // Debug.LogWarning("The " + gameObject.name + " game object does not have a Nav Mesh Agent component to navigate. One with default values has been added", gameObject);
                navAgent = gameObject.AddComponent<UnityEngine.AI.NavMeshAgent>();
            }

            SearchSupport();



#if UNITY_5_6_OR_NEWER
            navAgent.isStopped = false;
#else
            navAgent.Resume();
#endif

        }

        /// <summary>Method of Update of SafetyMoveAway.</summary>
        /// <remarks>Verify the status of the task, if there is no objective fails, if it has traveled the road or is near the goal it is completed
        /// y, the task is running, if it is still moving to the target.</remarks>
        public override TaskStatus OnUpdate() {
            if (enemyScript.Yolo)
                return TaskStatus.COMPLETED;

            CheckDistance();

            if (target == null)
                return TaskStatus.FAILED;
            if (!navAgent.pathPending && navAgent.remainingDistance <= 3)
                return TaskStatus.COMPLETED;
            else if (navAgent.destination != _closestRobot?.position)
                if (_closestRobot != null) {
                    if (_closestRobot.gameObject.activeInHierarchy) {
                        Vector3 direction = (_closestRobot.position - gameObject.transform.position).normalized;
                        Vector3 offsetPosition = _closestRobot.position - direction * offsetDistance;

                        navAgent.updateRotation = true;
                        navAgent.SetDestination(offsetPosition);
                    }
                    else {
                        // Debug.LogWarning("The closest robot is not active in the scene, searching for another robot");
                        SearchSupport();
                    }
                }
                else {
                    // Debug.LogWarning("The closest robot is null searching for another robot");
                    SearchSupport();
                }
            return TaskStatus.RUNNING;
        }
        /// <summary>Abort method of SafetyMoveAway </summary>
        /// <remarks>When the task is aborted, it stops the navAgentMesh.</remarks>
        public override void OnAbort() {

#if UNITY_5_6_OR_NEWER
            if (navAgent != null)
                navAgent.isStopped = true;
#else
            if (navAgent != null)
                navAgent.Stop();
#endif

        }


        private void SearchSupport() {
            // // Debug.LogWarning("Searching for support");
            // Search for any support type robots in the scene
            GameObject[] supportRobots = GameObject.FindGameObjectsWithTag("SupportRobot");
            // Filter out the robots that do not have the NavMeshAgent component
            supportRobots = System.Array.FindAll(supportRobots, robot => robot.GetComponent<UnityEngine.AI.NavMeshAgent>() != null);
            // Filter out the robots that are not active in the scene
            supportRobots = System.Array.FindAll(supportRobots, robot => robot.activeInHierarchy);
            // Filter out myself from the list of robots
            supportRobots = System.Array.FindAll(supportRobots, robot => robot != gameObject);
            // Get the closest robot to the enemy
            if (supportRobots.Length != 0) {

                GameObject closestRobot = supportRobots[0];
                float closestDistance = Vector3.Distance(gameObject.transform.position, closestRobot.transform.position);
                foreach (GameObject robot in supportRobots) {
                    float distance = Vector3.Distance(gameObject.transform.position, robot.transform.position);
                    if (distance < closestDistance) {
                        closestDistance = distance;
                        closestRobot = robot;
                    }
                }
                // Navigate to the closest robot
                _closestRobot = closestRobot.transform;

                Vector3 direction = (_closestRobot.position - gameObject.transform.position).normalized;
                Vector3 offsetPosition = _closestRobot.position - direction * offsetDistance;

                navAgent.updateRotation = true;
                navAgent.SetDestination(offsetPosition);

            }
            else {
                // No support robots in the scene, Yolo
                enemyScript.Yolo = true;
            }
        }


        private void CheckDistance() {
            if (Vector3.Distance(gameObject.transform.position, _closestRobot.position) < 5) {
                // Get the support script of the closest robot
                Enemy closestRobotScript = _closestRobot.GetComponent<Enemy>();
                if (!closestRobotScript.SpecialCooldown) {
                    closestRobotScript.TriggerHeal();
                }
            }

        }
    }
}
