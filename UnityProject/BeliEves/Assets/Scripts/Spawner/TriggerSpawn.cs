using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class TriggerSpawn : MonoBehaviour {
    // Start is called before the first frame update
    [SerializeField] private List<SpawnerPoint> spawnerPoints;
    [SerializeField] private Collider _collider; // The collider to visualize
    [SerializeField] private bool debug = true;
    
    private void OnTriggerEnter(Collider other) {
        if(other.TryGetComponent<Player.Player>(out var player)) {
            foreach (var spawnerPoint in spawnerPoints) {
                spawnerPoint.TriggerSpawn();
            }
        }
    }

    public void SetSpawnerPoints(List<SpawnerPoint> spawnerPoints) {
        this.spawnerPoints.AddRange(spawnerPoints);
    }

    private void OnDrawGizmos() {
        if (!debug || _collider == null) return;
        Gizmos.color = Color.green; // Color for the visualization
        if (_collider is SphereCollider sphereCollider) {
            Vector3 center = sphereCollider.transform.position + sphereCollider.center;
            Gizmos.DrawWireSphere(center, sphereCollider.radius * sphereCollider.transform.lossyScale.x);
        } 
    }

}
