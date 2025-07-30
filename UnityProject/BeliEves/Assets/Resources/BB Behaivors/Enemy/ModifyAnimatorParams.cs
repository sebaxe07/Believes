using System.Collections;
using System.Collections.Generic;
using Pada1.BBCore;
using Pada1.BBCore.Tasks;
using BBUnity.Actions;
using UnityEngine;
using Ami.BroAudio;


public enum SoundAction {
    Play,
    Stop
}
[Action("Enemy/ModifyAnimatorParams")]
[Help("ModifyAnimatorParams")]
public class ModifyAnimatorParams : GOAction {

    [InParam("Animator")]
    public Animator animator;

    [InParam("paramName")]
    public string paramName;

    [InParam("paramValue")]
    public float paramValue;

    [InParam("SoundAction")]
    public SoundAction soundAction;

    public override void OnStart() {
        if (animator == null) {
            // Debug.LogError("Animator component not found!");
            return;
        }

        animator.SetFloat(paramName, paramValue);

        if (soundAction == SoundAction.Play) {
            animator.GetComponent<SoundSource>().Play(gameObject.transform);
        }
        else {
            animator.GetComponent<SoundSource>().Stop();
        }
    }

    public override TaskStatus OnUpdate() {
        return TaskStatus.COMPLETED;
    }
}