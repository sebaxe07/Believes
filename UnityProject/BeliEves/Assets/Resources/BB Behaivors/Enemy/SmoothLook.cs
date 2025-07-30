using System.Collections;
using System.Collections.Generic;
using Pada1.BBCore;
using Pada1.BBCore.Tasks;
using BBUnity.Actions;
using UnityEngine;
using Npc;
using UnityEngine.AI;

[Action("Enemy/SmoothLook")]
[Help("Rotates the transform so the forward vector of the game object points at target's current position")]
public class SmoothLook : GOAction {
    ///<value>Input Target game object Parameter.</value>
    [InParam("target")]
    [Help("Target game object")]
    public GameObject target;

    private Transform targetTransform;

    /// <summary>Initialization Method of LookAt.</summary>
    /// <remarks>Check if you have an objective gameObject associated with it.</remarks>
    public override void OnStart() {
        if (target == null) {
            // Debug.LogError("The look target of this game object is null", gameObject);
            return;
        }
        targetTransform = target.transform;


    }

    /// <summary>Method of Update of LookAt.</summary>
    /// <remarks>In each Update Check the position of the target GameObject and rotate the vector where it points, the task fails
    /// if you have no objective Gameobject associated with it.</remarks>
    public override TaskStatus OnUpdate() {
        if (target == null)
            return TaskStatus.FAILED;

        // Smoothly rotate towards the target point. Returning Running until the rotation is complete.
        // Adjust lookPos to only affect the Y-axis.
        Vector3 lookPos = targetTransform.position;
        lookPos.y = gameObject.transform.position.y;  // Lock the Y-axis

        // Check if the angle is close enough to consider rotation complete.
        if (Vector3.Angle(gameObject.transform.forward, lookPos - gameObject.transform.position) < 1) {
            return TaskStatus.COMPLETED;
        }
        else {
            // Rotate smoothly on the Y axis only.
            Quaternion targetRotation = Quaternion.LookRotation(lookPos - gameObject.transform.position);
            gameObject.transform.rotation = Quaternion.Slerp(gameObject.transform.rotation, targetRotation, Time.deltaTime * 20);

            return TaskStatus.RUNNING;
        }

    }
}