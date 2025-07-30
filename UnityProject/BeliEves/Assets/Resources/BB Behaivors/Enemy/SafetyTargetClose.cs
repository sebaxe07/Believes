using UnityEngine;
using Pada1.BBCore;
using Pada1.BBCore.Framework;
using Pada1.BBCore.Tasks;
using BBUnity.Conditions;
using System;
using Utilities.Events.EventsLayout;
using Events;

[Condition("Custom/SafetyTargetClose")]
[Help("Checks if the target is close to the game object and has a safety check.")]
public class SafetyTargetClose : GOCondition {

    [InParam("target")]
    [Help("Target to check the distance")]
    public GameObject target;

    ///<value>Input maximun distance Parameter to consider that the target is close.</value>
    [InParam("closeDistance")]
    [Help("The maximun distance to consider that the target is close")]
    public float closeDistance;

    /// <summary>
    /// Checks whether a target is close depending on a given distance,
    /// calculates the magnitude between the gameobject and the target and then compares with the given distance.
    /// </summary>
    /// <returns>True if the magnitude between the gameobject and de target is lower that the given distance.</returns>
    public override bool Check() {
        if (target == null) {
            // // Debug.LogWarning("Found game object is null");
            return false;
        }

        // Draw the raycast for visualization
        /*         if ((gameObject.transform.position - target.transform.position).sqrMagnitude < closeDistance * closeDistance)
                    // Debug.DrawRay(gameObject.transform.position, target.transform.position - gameObject.transform.position, Color.blue, 1.0f); */

        return (gameObject.transform.position - target.transform.position).sqrMagnitude < closeDistance * closeDistance;
    }


}
