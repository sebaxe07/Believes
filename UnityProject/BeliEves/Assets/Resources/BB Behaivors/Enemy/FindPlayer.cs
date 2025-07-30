using System.Collections;
using System.Collections.Generic;
using Pada1.BBCore;
using Pada1.BBCore.Tasks;
using BBUnity.Actions;
using UnityEngine;

[Action("Enemy/FindPlayer")]
[Help("Find the player and set it as the target")]
public class FindPlayer : GOAction {

    ///<value>OutPut Found game object Parameter.</value>
    [OutParam("foundGameObject")]
    [Help("Found game object")]
    public GameObject foundGameObject;

    public override void OnStart() {
        //Debug.LogError("Finding player");
        foundGameObject = GameObject.FindObjectOfType<Player.Player>().gameObject;
        //Debug.LogError("Found player: " + foundGameObject.name);
    }

    public override TaskStatus OnUpdate() {
        return TaskStatus.COMPLETED;
    }
}