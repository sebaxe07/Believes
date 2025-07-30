using System;
using System.Collections;
using System.Collections.Generic;
using Controller.Animations;
using Unity.VisualScripting;
using UnityEngine;

public class AnimationInitialStateSetter : MonoBehaviour
{
    [SerializeField] private String State = "";
    AnimationController controller = AnimationController.Instance;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (controller != null && controller.GetState() != State) {
            controller.ChangeState(State); 
            Destroy(this);
        }
    }
}
