using Pada1.BBCore.Tasks;
using Pada1.BBCore;
using UnityEngine;

namespace BBUnity.Actions {
    /// <summary>
    /// It is an action to move towards the given goal using a NavMeshAgent.
    /// </summary>
    [Action("Navigation/SafetyMoveToLastLocation")]
    [Help("Moves the game object towards a given target by using a NavMeshAgent")]
    public class SafetyMoveToLastLocation : GOAction {
        ///<value>Input target game object towards this game object will be moved Parameter.</value>
        [InParam("target")]
        [Help("Target game object towards this game object will be moved")]
        public GameObject target;

        [InParam("InvestigateParticle")]
        [Help("Investigate Particle")]
        public ParticleSystem InvestigateParticle;

        private UnityEngine.AI.NavMeshAgent navAgent;

        private Transform targetTransform;
        private Vector3 lastPosition;

        /// <summary>Initialization Method of SafetyMoveToLastLocation.</summary>
        /// <remarks>Check if GameObject object exists and NavMeshAgent, if there is no NavMeshAgent, the default one is added.</remarks>
        public override void OnStart() {
            if (target == null) {
                // Debug.LogError("The movement target of this game object is null", gameObject);
                return;
            }
            targetTransform = target.transform;
            lastPosition = targetTransform.position;

            navAgent = gameObject.GetComponent<UnityEngine.AI.NavMeshAgent>();
            if (navAgent == null) {
                // Debug.LogWarning("The " + gameObject.name + " game object does not have a Nav Mesh Agent component to navigate. One with default values has been added", gameObject);
                navAgent = gameObject.AddComponent<UnityEngine.AI.NavMeshAgent>();
            }
            InvestigateParticle.Play();
            navAgent.SetDestination(lastPosition);

#if UNITY_5_6_OR_NEWER
            navAgent.isStopped = false;
#else
            navAgent.Resume();
#endif
        }

        /// <summary>Method of Update of SafetyMoveToLastLocation.</summary>
        /// <remarks>Verify the status of the task, if there is no objective fails, if it has traveled the road or is near the goal it is completed
        /// y, the task is running, if it is still moving to the target.</remarks>
        public override TaskStatus OnUpdate() {
            if (target == null)
                return TaskStatus.FAILED;
            if (!navAgent.pathPending && navAgent.remainingDistance <= navAgent.stoppingDistance)
                return TaskStatus.COMPLETED;

            return TaskStatus.RUNNING;
        }
        /// <summary>Abort method of SafetyMoveToLastLocation </summary>
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
    }
}
