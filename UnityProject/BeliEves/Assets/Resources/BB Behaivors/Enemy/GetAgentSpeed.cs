using System.Collections;
using System.Collections.Generic;
using Pada1.BBCore;
using Pada1.BBCore.Tasks;
using BBUnity.Actions;
using UnityEngine;

[Action("Enemy/GetAgentSpeed")]
[Help("GetAgentSpeed")]
public class GetAgentSpeed : GOAction {

    [InParam("rigidbody")]
    public Rigidbody rigidbody;

    [OutParam("Speed")]
    public float speed;

    public override void OnStart() {
        if (rigidbody == null) {
            //Debug.LogError("navMeshAgent component not found!");
            return;
        }

        speed = rigidbody.velocity.magnitude;
        /*         Debug.LogError("Speed: " + speed);
                Debug.LogError("Velocity: " + rigidbody.velocity);
                Debug.LogError("Magnitude: " + rigidbody.velocity.magnitude); */
    }

    public override TaskStatus OnUpdate() {
        return TaskStatus.COMPLETED;
    }
}