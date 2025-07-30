using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Utilities.Events.EventsLayout;

public class SpawnerManager : MonoBehaviour
{
    private GameObject[] _spawnPoints;
    private SpawnerPoint[] _spawners;
    
    private void Start() {
        _spawnPoints = GameObject.FindGameObjectsWithTag("SpawnPoint");
        _spawners = new SpawnerPoint[1];
    }
}
