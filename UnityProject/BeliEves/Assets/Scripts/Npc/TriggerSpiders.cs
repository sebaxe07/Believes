using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Experimental.GlobalIllumination;

public class TriggerSpiders : MonoBehaviour {

    public Light spotLight;
    public Animator animator;
    public GameObject dieAnimation;

    private void OnTriggerEnter(Collider other) {
        if (other.TryGetComponent<Player.Player>(out Player.Player player)) {
            // Reduce the radius of the light to 0 smoothly
            StartCoroutine(ReduceLightRadius());
            // Destroy the collider so the player can't trigger the spiders again
            Destroy(GetComponent<Collider>());
            animator.gameObject.GetComponent<BehaviorExecutor>().enabled = false;
            // set the height of the parent game object to 0 
            Destroy(animator.gameObject.GetComponent<NavMeshAgent>());
            Die();
        }
    }

    private IEnumerator ReduceLightRadius() {
        // Reduce the inner / outter Spot Angle of the light to 0
        while (spotLight.spotAngle > 0) {
            spotLight.spotAngle -= 5;
            // Flash the light randomly to make it look like it's turning off
            spotLight.intensity = Random.Range(0, 500);

            yield return new WaitForSeconds(0.1f);
        }

        // Turn off the light
        spotLight.enabled = false;
        // Die after the light has been turned off

    }
    private void Die() {
        animator.Play("Destroyed");

        // wait for the animation to finish before destroying the game object and instantiating the die animation
        StartCoroutine(DestroyAfterAnimation());
    }

    private IEnumerator DestroyAfterAnimation() {
        yield return new WaitForSeconds(animator.GetCurrentAnimatorStateInfo(0).length + 1f);
        Instantiate(dieAnimation, transform.position + new Vector3(0, 1.5947f, 0), Quaternion.identity);
        Destroy(animator.gameObject);
    }

}
