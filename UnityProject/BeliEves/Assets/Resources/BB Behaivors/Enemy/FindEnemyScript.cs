using System.Collections;
using System.Collections.Generic;
using Pada1.BBCore;
using Pada1.BBCore.Tasks;
using BBUnity.Actions;
using UnityEngine;
using Npc;

[Action("Enemy/FindEnemyScript")]
[Help("Find the enemy script and set it as the target")]
public class FindEnemyScript : GOAction {

    ///<value>OutPut Found game object Parameter.</value>
    [OutParam("foundScript")]
    [Help("Found script object")]
    public Enemy foundScript;

    public override void OnStart() {
        //Debug.LogError("Finding player");
        foundScript = gameObject.GetComponent<Npc.Enemy>();

        //Debug.LogError("Found script: " + foundScript.name);
    }

    public override TaskStatus OnUpdate() {
        return TaskStatus.COMPLETED;
    }
}