using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BreakablePlatform : MonoBehaviour
{
    private bool isBroken = false;
    [SerializeField] private bool isFragile = true;
    // Start is called before the first frame update

    private void OnTriggerEnter(Collider other) {
        Player.Player player;
        if(!isBroken && (( other.transform.parent !=null && other.transform.parent.gameObject.TryGetComponent<Player.Player>(out player)) 
               || other.gameObject.TryGetComponent<Player.Player>(out  player)) && (isFragile || 
               (other.TryGetComponent<Player.HeavyAttackCollider>(out var collider)&& other.transform.parent.gameObject.name.Contains("TankRobot")))
           ){
            isBroken = true;
            BreakWall();
        }
    }

    private void BreakWall(){
        StartCoroutine(ShakeAndDestroy());
    }
    private IEnumerator ShakeAndDestroy()
    {
        Vector3 originalPosition = transform.position;
        float shakeDuration = 0.5f;
        float shakeMagnitude = 0.1f;
        float elapsed = 0.0f;

        while (elapsed < shakeDuration)
        {
            float x = Random.Range(-shakeMagnitude, shakeMagnitude);
            float y = Random.Range(-shakeMagnitude, shakeMagnitude);
            transform.position = new Vector3(originalPosition.x + x, originalPosition.y, originalPosition.z + y);
            elapsed += Time.deltaTime;
            yield return null;
        }

        transform.position = originalPosition; // Reset position

        float fallDuration = 1.0f; // Duration for the platform to fall
        float fallSpeed = 2.0f; // Speed of the fall
        Vector3 fallDirection = Vector3.down; // Direction of the fall
        float elapsedFallTime = 0.0f;

        while (elapsedFallTime < fallDuration)
        {
            transform.position += fallDirection * fallSpeed * Time.deltaTime;
            elapsedFallTime += Time.deltaTime;
            yield return null;
        }

        Destroy(gameObject); // Remove the GameObject
    }

}
