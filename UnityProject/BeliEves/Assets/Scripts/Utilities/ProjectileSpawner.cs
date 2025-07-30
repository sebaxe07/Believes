using UnityEngine;

public class ProjectileSpawner : MonoBehaviour {
    public GameObject SpawnProjectile(GameObject prefab, Vector3 position, Quaternion rotation) {
        return Instantiate(prefab, position, rotation);
    }
}