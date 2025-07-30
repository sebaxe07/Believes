using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Breakable : MonoBehaviour
{
    // Start is called before the first frame update

    private void OnTriggerEnter(Collider other){
        if(other.TryGetComponent<Player.SpecialAttackCollider>(out var player)){
            BreakWall();
        }
    }

    private void BreakWall(){
        StartCoroutine(ShakeAndDestroy());
    }
    private IEnumerator ShakeAndDestroy() {
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
        Destroy(gameObject); // Remove the GameObject
    }

}
