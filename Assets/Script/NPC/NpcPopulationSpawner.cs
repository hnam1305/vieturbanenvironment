using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[Serializable]
public class NpcResidentData
{
    public string residentName;
    public string deliveryAddress;
    public Transform homeEntrance;
    public NpcBehaviorType behaviorType;
}

public class NpcPopulationSpawner : MonoBehaviour
{
    [SerializeField] private NpcController npcPrefab;
    [SerializeField] private Transform player;
    [SerializeField, Min(1)] private int populationSize = 8;
    [SerializeField, Min(0f)] private float spawnRadius = 40f;
    [SerializeField, Min(0.1f)] private float sampleRadius = 4f;
    [SerializeField, Min(1f)] private float hideDistance = 80f;
    [SerializeField, Min(1f)] private float showDistance = 60f;
    [SerializeField] private Transform[] houseEntrances;
    [SerializeField] private Transform[] shopSpots;
    [SerializeField] private Transform[] parkSpots;
    [SerializeField] private List<NpcResidentData> residents = new List<NpcResidentData>();

    private readonly List<NpcController> activeResidents = new List<NpcController>();

    public IReadOnlyList<NpcController> ActiveResidents { get { return activeResidents; } }

    private void Start()
    {
        SpawnPopulation();
    }

    private void Update()
    {
        if (player == null)
        {
            return;
        }

        foreach (NpcController resident in activeResidents)
        {
            if (resident == null)
            {
                continue;
            }

            float distance = Vector3.Distance(player.position, resident.transform.position);
            if (!resident.IsDormant && distance > hideDistance)
            {
                resident.SetDormant(true, resident.HasPendingDelivery);
            }
            else if (resident.IsDormant && distance < showDistance)
            {
                resident.SetDormant(false, false);
            }
        }
    }

    public NpcController GetRandomResident()
    {
        List<NpcController> availableResidents = new List<NpcController>();
        foreach (NpcController resident in activeResidents)
        {
            if (resident != null && resident.isActiveAndEnabled)
            {
                availableResidents.Add(resident);
            }
        }

        if (availableResidents.Count == 0)
        {
            return null;
        }
        return availableResidents[UnityEngine.Random.Range(0, availableResidents.Count)];
    }

    private void SpawnPopulation()
    {
        if (npcPrefab == null || residents.Count == 0 || houseEntrances == null || houseEntrances.Length < 2)
        {
            Debug.Log("npc population needs a prefab, residents and house entrances", this);
            return;
        }

        List<NpcResidentData> spawnList = new List<NpcResidentData>();
        foreach (NpcResidentData resident in residents)
        {
            if (resident != null && resident.homeEntrance != null)
            {
                spawnList.Add(resident);
            }
        }

        if (spawnList.Count == 0)
        {
            Debug.Log("npc population has no residents with a home entrance", this);
            return;
        }

        int count = Mathf.Min(Mathf.Max(1, populationSize), spawnList.Count);
        for (int index = 0; index < count; index++)
        {
            int selectedIndex = UnityEngine.Random.Range(0, spawnList.Count);
            NpcResidentData residentData = spawnList[selectedIndex];
            spawnList.RemoveAt(selectedIndex);

            if (!TryGetSpawnPosition(out Vector3 spawnPosition))
            {
                Debug.Log("npc population could not find a spawn position on the nav mesh", this);
                continue;
            }

            NpcController resident = Instantiate(npcPrefab, spawnPosition, Quaternion.identity, transform);
            resident.Configure(residentData.behaviorType, residentData.residentName, residentData.deliveryAddress,
                residentData.homeEntrance, houseEntrances, shopSpots, parkSpots);
            activeResidents.Add(resident);
        }
    }

    private bool TryGetSpawnPosition(out Vector3 spawnPosition)
    {
        Vector3 center = transform.position;
        if (player != null)
        {
            center = player.position;
        }

        for (int attempt = 0; attempt < 20; attempt++)
        {
            Vector2 offset = UnityEngine.Random.insideUnitCircle * spawnRadius;
            Vector3 candidate = center + new Vector3(offset.x, 0f, offset.y);
            if (NavMesh.SamplePosition(candidate, out NavMeshHit hit, sampleRadius, NavMesh.AllAreas))
            {
                spawnPosition = hit.position;
                return true;
            }
        }

        spawnPosition = Vector3.zero;
        return false;
    }
}
