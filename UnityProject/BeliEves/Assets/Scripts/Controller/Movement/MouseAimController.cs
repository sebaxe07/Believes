using System.Collections;
using System.Collections.Generic;
using AimIK.Behaviour;
using Pada1.BBCore.Actions;
using UnityEngine;

public class MouseAimController : MonoBehaviour
{
    // Start is called before the first frame update
    [SerializeField] private AimIKBehaviour3D HeadAimIK;
    [SerializeField] private Camera _camera;
    [SerializeField] private LayerMask _groundMask;


    public void Update() {
        var (success, position) = GetMousePosition();
        if (success) {
            HeadAimIK.Target.position = position;
        }
    }

    private (bool success, Vector3 position) GetMousePosition() {
        var ray = _camera.ScreenPointToRay(Input.mousePosition);
        var position = Vector3.zero;
        var success = false;
        if (Physics.Raycast(ray, out var hitInfo, Mathf.Infinity, _groundMask)) {
            // The Raycast hit something, return with the position.
            position = hitInfo.point;
            position.y = 3.0f;  
            success = true;
        }
        // The Raycast did not hit anything.
        return (success, position);
    }
}
