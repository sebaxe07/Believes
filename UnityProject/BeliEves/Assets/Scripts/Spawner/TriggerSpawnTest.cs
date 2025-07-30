using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TriggerSpawnTest : MonoBehaviour
{
    // Start is called before the first frame update
    [SerializeField] private SpawnerPoint _spawnerPoint;

    private void OnTriggerEnter(Collider other) {
        if(other.TryGetComponent<Player.Player>(out var player)) {
            _spawnerPoint.TriggerSpawn();
        }
    }
}
