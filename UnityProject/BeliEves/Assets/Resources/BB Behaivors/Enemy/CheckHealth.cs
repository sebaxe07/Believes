using UnityEngine;
using Pada1.BBCore;
using Pada1.BBCore.Framework;
using Pada1.BBCore.Tasks;
using BBUnity.Conditions;
using System;
using Utilities.Events.EventsLayout;
using Npc;
using Events;

[Condition("Custom/CheckHealth")]
[Help("Checks if the player has swapped bodies and updates the reference accordingly.")]
public class CheckHealth : GOCondition {

    [InParam("foundGameObject")]
    [Help("The game object to check if the player has swapped bodies")]
    public GameObject foundGameObject;

    [InParam("enemyScript")]
    [Help("Enemy script")]
    public Enemy enemyScript;


    private float influence;
    public override bool Check() {
        if (enemyScript.Yolo) {
            return false;
        }
        influence = enemyScript.GetInfluence();
        // If less than 20% health return true
        if (enemyScript.HealCoroutine != null) {
            return true;
        }

        if (influence < 0.25f) {
            return true;
        }
        else {
            return false;
        }
    }

}
