using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealableObject : MonoBehaviour {

    public enum HealableObjectType {
        Bridge,
        Platform,
        Stairs,
        Elevator
    }

    [SerializeField] private HealableObjectType healableObjectType;
    [SerializeField] private GameObject healableObject;
    [SerializeField] private float maxHealth = 100;
    [SerializeField] private float currentHealth = 0;
    [SerializeField] private float healableAmount = 10;

    [SerializeField] public ParticleSystem healingParticles;
    private Vector3 initialLocalScale;
    private Vector3 particleInitialLocalScale;

    [Header("Bridge")]
    [SerializeField] private float maxSizeToGrow;

    [Header("Platform")]
    [SerializeField] private Vector3 initialPosition;
    [SerializeField] private Vector3 targetPosition;
    private Vector3 currentPosition;

    [Header("Stairs")]
    [SerializeField] private float maxStairsHeight;
    [SerializeField] private GameObject ParticlePivot;

    [Header("Elevator")]
    [SerializeField] private float maxElevatorHeight;
    private Vector3 initialElevatorPosition;


    private Coroutine healOverTimeCoroutine;
    private Coroutine UnHealOverTimeCoroutine;
    private GameObject healingCollider;

    void Start() {
        // Store initial local position and scale to reset each frame
        initialLocalScale = healableObject.transform.localScale;
        initialElevatorPosition = healableObject.transform.position;
        initialPosition = healableObject.transform.position;
        if (ParticlePivot != null) particleInitialLocalScale = ParticlePivot.transform.localScale;
    }

    // Check if a collider of type SpecialAttackCollider and has the tag "Heal" is colliding with the object
    private void OnTriggerEnter(Collider other) {
        if (other.gameObject.CompareTag("Heal")) {
            StopUnHealing();
            healingCollider = other.gameObject;
            healingParticles.Play();
            healOverTimeCoroutine = StartCoroutine(HealOverTime(healableAmount, healableObjectType));
        }
    }

    // If stops colliding with the object, stop the coroutine
    private void OnTriggerExit(Collider other) {
        if (other.gameObject.CompareTag("Heal")) {
            StopHealing();
            if (healableObjectType.Equals(HealableObjectType.Platform)) {
                healingParticles.Stop();
                return;
            }
            if (currentHealth < maxHealth || HealableObjectType.Elevator == healableObjectType) {
                UnHealOverTimeCoroutine = StartCoroutine(UnHealOverTime(healableAmount, healableObjectType));
                healingParticles.Play();
            }
        }
    }

    private IEnumerator HealOverTime(float healAmount, HealableObjectType type) {
        float healRate = healAmount / 1f; // Heal amount per second
        while (currentHealth < maxHealth) {
            float deltaHealth = healRate * Time.deltaTime;
            currentHealth += deltaHealth;
            currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth); // Ensure currentHealth does not exceed maxHealth

            switch (type) {
                case HealableObjectType.Bridge:
                    HealBridge();
                    break;
                case HealableObjectType.Platform:
                    HealPlatform();
                    break;
                case HealableObjectType.Stairs:
                    HealStairs();
                    break;
                case HealableObjectType.Elevator:
                    HealElevator();
                    break;

            }

            yield return null;
        }

        if (currentHealth >= maxHealth) {
            currentHealth = maxHealth;
            StopHealing();
        }
    }

    private IEnumerator UnHealOverTime(float healAmount, HealableObjectType type) {
        float healRate = healAmount / 1f; // UnHeal amount per second
        if (healableObjectType.Equals(HealableObjectType.Platform)) {
            // If the object is a platform, do not unheal
            healingParticles.Stop();
            yield break;
        }
        while (currentHealth > 0) {
            float deltaHealth = healRate * Time.deltaTime;
            currentHealth -= deltaHealth;
            currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth); // Ensure currentHealth does not exceed maxHealth

            switch (type) {
                case HealableObjectType.Bridge:
                    HealBridge();
                    break;
                case HealableObjectType.Platform:
                    // Do nothing as the platform stays in the same position                    
                    break;
                case HealableObjectType.Stairs:
                    HealStairs();
                    break;
                case HealableObjectType.Elevator:
                    HealElevator();
                    break;
            }

            yield return null;
        }

        if (currentHealth == 0) {
            Debug.LogWarning("health 0");
            StopUnHealing();
        }
    }

    private void HealBridge() {
        // Calculate the scale factor based on the current healt, the max health and the max size to grow
        float targetScaleFactor = Mathf.Lerp(initialLocalScale.y, maxSizeToGrow, currentHealth / maxHealth);
        Vector3 targetScale = new Vector3(targetScaleFactor, initialLocalScale.y, initialLocalScale.z);

        // Smoothly interpolate the scale towards the target scale
        healableObject.transform.localScale = Vector3.Lerp(healableObject.transform.localScale, targetScale, 0.1f);
    }

    private void HealPlatform() {


        // Calculate the new position based on the current healt, the max health and the target position
        Vector3 newPosition = Vector3.Lerp(initialPosition, targetPosition, currentHealth / maxHealth);

        // Smoothly interpolate the position towards the target position
        healableObject.transform.position = Vector3.Lerp(healableObject.transform.position, newPosition, 0.1f);

        // Update the current position
        currentPosition = newPosition;

        // If the current position is the target position, change the target position to the initial position
        if (currentPosition == targetPosition) {
            Vector3 temp = targetPosition;
            targetPosition = initialPosition;
            initialPosition = temp;

            // Health back to 0
            currentHealth = 0;
        }
    }
    private void HealStairs() {
        // Calculate the scale factor based on the current healt, the max health and the max size to grow
        float targetScaleFactor = Mathf.Lerp(initialLocalScale.y, maxStairsHeight, currentHealth / maxHealth);
        Vector3 targetScale = new Vector3(initialLocalScale.x, targetScaleFactor, initialLocalScale.z);
        // Smoothly interpolate the scale towards the target scale
        healableObject.transform.localScale = Vector3.Lerp(healableObject.transform.localScale, targetScale, 0.1f);


        // Calculate the scale factor based on the current healt, the max health and the max size to grow of the particle system
        float targetScaleFactorParticle = Mathf.Lerp(particleInitialLocalScale.z, 0.1f, currentHealth / maxHealth);
        Vector3 targetScaleParticle = new Vector3(particleInitialLocalScale.x, particleInitialLocalScale.y, targetScaleFactorParticle);

        // Smoothly interpolate the scale towards the target scale
        ParticlePivot.transform.localScale = Vector3.Lerp(ParticlePivot.transform.localScale, targetScaleParticle, 0.1f);
    }

    private void HealElevator() {
        // Calculate the y position based on the current healt, the max health and the max size to go up
        float targetYPosition = Mathf.Lerp(initialElevatorPosition.y, maxElevatorHeight, currentHealth / maxHealth);
        Vector3 targetPosition = new Vector3(initialElevatorPosition.x, targetYPosition, initialElevatorPosition.z);

        // Smoothly interpolate the position towards the target position
        healableObject.transform.position = Vector3.Lerp(healableObject.transform.position, targetPosition, 0.1f);
    }
    private void StopHealing() {
        if (healOverTimeCoroutine != null) {
            StopCoroutine(healOverTimeCoroutine);
            healOverTimeCoroutine = null;
        }
        healingParticles.Stop();
    }

    private void StopUnHealing() {
        if (UnHealOverTimeCoroutine != null) {
            StopCoroutine(UnHealOverTimeCoroutine);
            UnHealOverTimeCoroutine = null;
        }
        Debug.LogWarning("Stop UnHealing");
        healingParticles.Stop();
    }


    private void LateUpdate() {
        UpdateEdgePosition();
        if (healingCollider != null) {
            // check if is still enabled
            if (!healingCollider.activeSelf && UnHealOverTimeCoroutine == null && currentHealth != 0) {
                StopHealing();
                if (HealableObjectType.Platform == healableObjectType) {
                    healingParticles.Stop();
                    return;
                }
                if (currentHealth < maxHealth || HealableObjectType.Elevator == healableObjectType) {
                    UnHealOverTimeCoroutine = StartCoroutine(UnHealOverTime(healableAmount, healableObjectType));
                    healingParticles.Play();
                }
            }
        }
    }
    private void UpdateEdgePosition() {
        // Get the mesh and its bounds
        MeshRenderer meshRenderer = GetComponent<MeshRenderer>();
        Bounds bounds = meshRenderer.localBounds;


        Vector3 edgePosition = Vector3.zero;
        // Base on the type of object, calculate the edge position
        switch (healableObjectType) {
            case HealableObjectType.Bridge:

                // Calculate the middle right position
                edgePosition = new(
                   (bounds.min.x + bounds.max.x) / 2,                               // Right side on the x-axis
                   bounds.min.y,         // Middle on the y-axis
                   (bounds.min.z + bounds.max.z) / 2          // Center on the z-axis
               );


                edgePosition = transform.TransformPoint(edgePosition);

                // Place the particle system at the edge
                healingParticles.transform.position = edgePosition;

                return;
            case HealableObjectType.Platform:
                // Nothing to do
                break;
            case HealableObjectType.Stairs:
                // Calculate the right top position
                edgePosition = new(
                   (bounds.min.x + bounds.max.x) / 2,         // Middle on the x-axis
                   bounds.max.y,                              // Top side on the y-axis
                   bounds.min.z                               // Front on the z-axis
                );

                edgePosition = transform.TransformPoint(edgePosition);

                ParticlePivot.transform.position = edgePosition;
                return;
            case HealableObjectType.Elevator:

                break;
        }

    }


}
