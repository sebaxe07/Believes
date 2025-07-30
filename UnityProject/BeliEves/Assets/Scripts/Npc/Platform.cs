using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Utilities.Interactable;

public class Platform : Interactable {
    public Transform[] points;
    public float speed = 2f;
    private int currentPointIndex = 0;
    private bool isMoving = false;
    private bool playerOnPlatform = false;

    void Update() {
        if (isMoving) {
            MovePlatform();
        }
    }

    private void MovePlatform() {
        if (points.Length == 0) return;

        Transform targetPoint = points[currentPointIndex];
        transform.position = Vector3.MoveTowards(transform.position, targetPoint.position, speed * Time.deltaTime);

        if (Vector3.Distance(transform.position, targetPoint.position) < 0.1f) {
            currentPointIndex = (currentPointIndex + 1) % points.Length;
        }
    }


    public override void Interact() {
        if (playerOnPlatform) {
            isMoving = !isMoving;
        }
    }

    protected override void OnTriggerEnterCallback() {
        interactable.ShowHelpText("Press ", " to activate the platform");
        playerOnPlatform = true;
    }

    protected override void OnTriggerExitCallback() {
        interactable.HideHelpText();
        playerOnPlatform = false;
    }

}