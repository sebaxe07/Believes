using System.Collections;
using System.Collections.Generic;
using Pada1.BBCore;
using Pada1.BBCore.Tasks;
using BBUnity.Actions;
using UnityEngine;
using Npc;

[Action("Enemy/SpiderAttack")]
[Help("Use the spider attack on the player")]
public class SpiderAttack : GOAction {

    ///<value>OutPut Found game object Parameter.</value>
    [InParam("foundGameObject")]
    [Help("Found game object")]
    public GameObject foundGameObject;

    [InParam("enemyScript")]
    [Help("Enemy script")]
    public NPCEnemy enemyScript;

    public override TaskStatus OnUpdate() {
        if (enemyScript == null) {
            // Debug.LogError("Enemy script is null");
            return TaskStatus.FAILED;
        }
        enemyScript.Explode();
        return TaskStatus.COMPLETED;
    }

}