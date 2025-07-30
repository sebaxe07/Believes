using UnityEngine;
using Pada1.BBCore;
using Pada1.BBCore.Framework;
using Pada1.BBCore.Tasks;
using BBUnity.Conditions;
using System;
using Utilities.Events.EventsLayout;
using Events;

[Condition("Custom/CheckPlayer")]
[Help("Checks if the player has swapped bodies and updates the reference accordingly.")]
public class CheckPlayer : GOCondition {

    [InParam("foundGameObject")]
    [Help("The game object to check if the player has swapped bodies")]
    public GameObject foundGameObject;


    public override bool Check() {
        // Check if the player has swapped bodies
        GameObject currentBody = GameObject.FindObjectOfType<Player.Player>().gameObject;


        if (foundGameObject == null) {
            //Debug.LogWarning("Found game object is null " + gameObject.name);
            return true;
        }

        if (currentBody != foundGameObject) {

            //Debug.LogWarning("Current body " + currentBody.name + " is not the same as found body" + foundGameObject.name);
            return true;
        }
        else {
            //Debug.LogWarning("Current body " + currentBody.name + " is the same as found body" + foundGameObject.name);
            return false;
        }
    }

}
