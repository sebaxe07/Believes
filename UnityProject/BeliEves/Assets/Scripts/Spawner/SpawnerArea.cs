using System;
using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;
using Utilities.Events.EventsLayout;


public class SpawnerArea : MonoBehaviour {
    [Header("Spawner Settings")]
    [SerializeField] private GameObject prefab;
    [SerializeField] private int numberOfSpawnerPoints = 5; // Number of spawnerPoints to generate
    [SerializeField] private Vector2 areaSize; // Size of the area to spawn points

    [Header("Spawner Points Settings")]
    [SerializeField] private SpawnerPoint spawnerPointPrefab;
    [SerializeField] private float spawnCooldown = 2f;
    [SerializeField] private float spawnerPointRadius = 1f; // Radius of the spawnerPoints
    [SerializeField] private LayerMask obstacleLayer;
    
    
    [Header("Waypoint Generation")]
    [SerializeField] private int numberOfWaypoints = 5;
    [SerializeField] private float minDistanceBetweenWaypoints = 2f;

    [SerializeField] private bool spawnWithTrigger = false;
    [SerializeField] private bool debug = false;
    
    
    [SerializeField] private List<SpawnerPoint> spawnerPoints = new List<SpawnerPoint>();
    [SerializeField] private List<TriggerSpawn> triggerSpawn;
    
    [Header("despawn settings")]
    [SerializeField] private bool despawnWithDistance = false;
    [SerializeField] private float despawnDistance = 90f;
    [SerializeField] private List<TriggerDespawn> triggersDespawn = new List<TriggerDespawn>();
    

    public void Start() {
        //Debug.Log(numberOfWaypoints + " Area" + this.GetHashCode());
        if(numberOfWaypoints>0)spawnerPointPrefab.EnableWaypointsGeneration(areaSize, numberOfWaypoints, minDistanceBetweenWaypoints);
        spawnerPointPrefab.despawnWithDistance = despawnWithDistance;
        spawnerPointPrefab.despawnDistance = despawnDistance;
        SpawnSpawnerPoints();
    }
    
    private void OnDrawGizmos() {
        if(!debug) return;
        
        Gizmos.color = Color.green;

        // Calculate the four corners of the rectangle in the horizontal plane (XZ)
        Vector3 bottomLeft = transform.position - new Vector3(areaSize.x / 2, 0, areaSize.y / 2);
        Vector3 bottomRight = transform.position + new Vector3(areaSize.x / 2, 0, -areaSize.y / 2);
        Vector3 topLeft = transform.position + new Vector3(-areaSize.x / 2, 0, areaSize.y / 2);
        Vector3 topRight = transform.position + new Vector3(areaSize.x / 2, 0, areaSize.y / 2);

        // Draw the rectangle edges in the XZ plane
        Gizmos.DrawLine(bottomLeft, bottomRight);
        Gizmos.DrawLine(bottomRight, topRight);
        Gizmos.DrawLine(topRight, topLeft);
        Gizmos.DrawLine(topLeft, bottomLeft);
    }

    
    private void SpawnSpawnerPoints() {
        for (int i = 0; i < numberOfSpawnerPoints; i++) {

            // Randomly spawn spawnerPoints within the specified area
            Vector3 spawnPosition = new Vector3(UnityEngine.Random.Range(-areaSize.x / 2, areaSize.x / 2), this.transform.position.y, UnityEngine.Random.Range(-areaSize.y / 2, areaSize.y / 2));
            // Change position to world position
            spawnPosition = this.transform.TransformPoint(spawnPosition);
            // Make sure the point is over the ground
            if (NavMesh.SamplePosition(spawnPosition,  out var hit, Mathf.Infinity, NavMesh.AllAreas)) {
                spawnPosition = hit.position;
            }
            var spawnerPoint = Instantiate(spawnerPointPrefab, spawnPosition, Quaternion.identity);
            spawnerPoint.SetPrefab(this.prefab);
            spawnerPoint.SetSpawnWithTrigger(spawnWithTrigger);
            spawnerPoints.Add(spawnerPoint);
        }

        for (int i = 0; i < triggerSpawn.Count; i++) {
            triggerSpawn[i].SetSpawnerPoints(spawnerPoints);
        }

        foreach (var triggerDespawn in triggersDespawn) {
            triggerDespawn.SetSpawnerPoints(spawnerPoints);
        }
    }

    public List<SpawnerPoint> GetSpawnerPoints() {
        return spawnerPoints;
    }

    public void ReleaseSpawner() {
        for (int i = 0; i < spawnerPoints.Count; i++) {
            spawnerPoints[i].ReleaseSpawner();
        }
    }
}
