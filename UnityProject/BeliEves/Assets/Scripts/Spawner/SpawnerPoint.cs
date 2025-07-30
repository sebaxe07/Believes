using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Events;
using Events.EventsLayout;
using Npc;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.PlayerLoop;
using Utilities.Events.EventsLayout;
using Pool;
using Utilities;
using Utilities.Pool;

public class SpawnerPoint : MonoBehaviour {
    [SerializeField] private bool spawnWithTrigger = false;
    [SerializeField] private GameObject prefabToSpawn; // Prefab to spawn
    [SerializeField] private List<Transform> waypoints; // List of waypoints for NavMeshAgent
    [SerializeField] private float distanceToPlayer;
    [SerializeField] private float spawnCooldown = 5f; // Time between spawns
    [SerializeField] private LayerMask obstacleLayer; // Layer for spawn obstacles
    [SerializeField] private float spawnRadius = 1f; // Radius to check for obstacles

    [Header("Waypoint Generation")]

    [SerializeField] private GameObject waypointPrefab;
    [SerializeField] private bool generateWaypoints = false;
    [SerializeField] private Vector2 generationAreaSize = new Vector2(10f, 10f);
    [SerializeField] private int numberOfWaypoints = 5;
    [SerializeField] private float minDistanceBetweenWaypoints = 2f;

    [Header("despawn settings")]
    public bool despawnWithDistance = false;
    public float despawnDistance = 90f;
    
    private GameObject _player;
    private bool hasSpawned = false;
    private float lastSpawnTime;
    private EventBodySwitch _bodySwitchChannel;
    
    private List<GameObject> _spawnedObjects = new List<GameObject>();

    private void Start() {
        Reinitialize();
    }

    public void Reinitialize() {
        try {
            //PoolManager.CreateNewPool("SpawnerNPC" + prefabToSpawn.name, prefabToSpawn, typeof(Npc.Npc), 100, 50);
            PoolManager.CreateNewPool("SpawnerNPC" + prefabToSpawn.name, prefabToSpawn, null, 25, 10);
        }
        catch (Exception e) {
            // ignored
        }
        //print("poolID" + "SpawnerNPC" + this.GetHashCode());

        _player = FindObjectOfType<Player.Player>().gameObject;
        _bodySwitchChannel = (EventBodySwitch)EventBroker.TryToAddEventChannel("bodySwitch", ScriptableObject.CreateInstance<EventBodySwitch>());
        _bodySwitchChannel.Subscribe(SwitchPlayerReference);

        if (generateWaypoints) {
            GenerateWaypoints();
        }
    }
    private void OnDrawGizmos() {
        Gizmos.color = Color.yellow; // Color for the visualization
        Vector3 center = transform.position;
        Gizmos.DrawWireSphere(center, 1f);
    }

    public void EnableWaypointsGeneration(Vector2 areaSize, int numberOfWaypoints, float minDistanceBetweenWaypoints) {
        generateWaypoints = true;
        generationAreaSize = areaSize;
        this.numberOfWaypoints = numberOfWaypoints;
        this.minDistanceBetweenWaypoints = minDistanceBetweenWaypoints;
    }

    private void GenerateWaypoints() {
        //Debug.Log("Generating waypoints");
        //Debug.Log("numberOfWaypoints: " + numberOfWaypoints);
        
        waypoints = new List<Transform>();
        for (int i = 0; i < numberOfWaypoints; i++) {
            Vector3 randomPoint = GetRandomNavMeshPoint();
            if (randomPoint != Vector3.zero) {
                GameObject waypoint = Instantiate(waypointPrefab, randomPoint, Quaternion.identity);
                waypoint.transform.parent = transform;
                waypoints.Add(waypoint.transform);
            }
        }
        
        //Debug.Log("Generating waypoints after " + waypoints.Count + " waypoints");
    }

    private Vector3 GetRandomNavMeshPoint() {
        for (int attempts = 0; attempts < 30; attempts++) {
            Vector3 randomPoint = transform.position + new Vector3(
                UnityEngine.Random.Range(-generationAreaSize.x / 2, generationAreaSize.x / 2),
                0,
                UnityEngine.Random.Range(-generationAreaSize.y / 2, generationAreaSize.y / 2)
            );
            
            if (NavMesh.SamplePosition(randomPoint,  out var hit, Mathf.Infinity, NavMesh.AllAreas)) {
                if (!IsWaypointTooClose(hit.position)) {
                    return hit.position;
                }
            }
        }
        return Vector3.zero;
    }

    private bool IsWaypointTooClose(Vector3 position) {
        foreach (Transform waypoint in waypoints) {
            if (Vector3.Distance(position, waypoint.position) < minDistanceBetweenWaypoints) {
                return true;
            }
        }
        return false;
    }

    private void SwitchPlayerReference() {
        _player = FindObjectOfType<Player.Player>().gameObject;
    }


    private void Spawn() {
        
        //Debug.LogWarning("spawning NPCENEMY at " + transform.position);
        if (prefabToSpawn.TryGetComponent<Npc.Npc>(out var NpcPrefab)) {
            var spawnedObject = PoolManager.GetPoolObject<Npc.Npc>("SpawnerNPC" + prefabToSpawn.name, transform.position); // Instantiate the prefab
            _spawnedObjects.Add(spawnedObject.gameObject);
            if(despawnWithDistance)AddDistanceObserver(spawnedObject.gameObject);
            
            if (spawnedObject.TryGetComponent<Npc.Npc>(out var AgentSpawned)) {
                var waypointsList = waypoints.ToList();
                waypointsList = waypointsList.OrderBy(x => UnityEngine.Random.value).ToList();
                var waypointsArray = waypointsList.ToArray();
                AgentSpawned.SetWaypoints(waypointsArray); // Set the first waypoint as the destination
            }
        }
        else {
            var spawnedObject = PoolManager.GetPoolObject<NPCEnemy>("SpawnerNPC" + prefabToSpawn.name, transform.position); // Instantiate the prefab
            _spawnedObjects.Add(spawnedObject.gameObject);
            if(despawnWithDistance)AddDistanceObserver(spawnedObject.gameObject);
        }

    }

    private void AddDistanceObserver(GameObject spawnedObject) {
        var distanceObserver = spawnedObject.AddComponent<DistanceObserver>();
        distanceObserver.range = despawnDistance;
        distanceObserver.objectA = spawnedObject;
        distanceObserver.objectB = _player;
        distanceObserver.Callback = new Action((() => {
            spawnedObject.SetActive(false);
            hasSpawned = false;
        }));
    }

    public void Despawn() {
        hasSpawned = false;
        foreach (var obj in _spawnedObjects) {
            if(obj.TryGetComponent<Enemy>(out var e))obj.SetActive(false);
        }
        _spawnedObjects.Clear();
    }

    public void TriggerSpawn() {
        if (hasSpawned) return;
        Spawn();
        hasSpawned = true;
    }

    public void ReleaseSpawner() {
        hasSpawned = false;
    }


    public void SetSpawnWithTrigger(bool spawnWithTrigger) {
        this.spawnWithTrigger = spawnWithTrigger;
    }

    public void SetPrefab(GameObject gameObject) {
        this.prefabToSpawn = gameObject;
    }

}
