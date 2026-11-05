using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class NpcController : MonoBehaviour
{
    [SerializeField] private Transform[] houseEntrances;
    [SerializeField, Min(0f)] private float minTimeInside = 10f;
    [SerializeField, Min(0f)] private float maxTimeInside = 30f;
    [SerializeField, Min(0f)] private float arrivalTolerance = 0.4f;

    private NavMeshAgent agent;
    private Renderer[] visualRenderers;
    private Transform currentDestination;
    private bool isWaitingAtHouse;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        visualRenderers = GetComponentsInChildren<Renderer>(true);
    }

    private void Start()
    {
        if (agent == null)
        {
            Debug.Log("npc controller requires navmeshagent on the same gameobject", this);
            enabled = false;
            return;
        }

        if (!agent.isOnNavMesh)
        {
            Debug.Log("npc must start on a baked surface", this);
            enabled = false;
            return;
        }

        if (GetValidEntranceCount() < 2)
        {
            Debug.Log("npc controller need at least two valid house entrance transforms.", this);
            enabled = false;
            return;
        }

        SetNewDestination(null);
    }

    private void Update()
    {
        if (isWaitingAtHouse || currentDestination == null || agent.pathPending || !agent.hasPath)
        {
            return;
        }

        if (agent.pathStatus == NavMeshPathStatus.PathComplete && agent.remainingDistance <= agent.stoppingDistance + arrivalTolerance)
        {
            StartCoroutine(EnterHouseAndLeaveLater());
        }
    }

    private IEnumerator EnterHouseAndLeaveLater()
    {
        isWaitingAtHouse = true;
        currentDestination = null;
        agent.isStopped = true;
        agent.ResetPath();
        SetVisible(false);

        float minWait = Mathf.Max(0f, Mathf.Min(minTimeInside, maxTimeInside));
        float maxWait = Mathf.Max(minWait, Mathf.Max(minTimeInside, maxTimeInside));
        yield return new WaitForSeconds(Random.Range(minWait, maxWait));

        Transform exitEntrance = GetRandomEntrance(null);
        if (exitEntrance == null || !agent.Warp(exitEntrance.position))
        {
            Debug.Log("could not place npc at a house entrance on the nav mesh surface", this);
            SetVisible(true);
            agent.isStopped = false;
            isWaitingAtHouse = false;
            SetNewDestination(null);
            yield break;
        }

        transform.rotation = exitEntrance.rotation;
        SetVisible(true);
        agent.isStopped = false;
        isWaitingAtHouse = false;
        SetNewDestination(exitEntrance);
    }

    private void SetNewDestination(Transform exclude)
    {
        Transform entrance = GetRandomEntrance(exclude);
        if (entrance == null || !agent.SetDestination(entrance.position))
        {
            Debug.Log("can not not set npc destination as unreachable entrance", this);
            currentDestination = null;
            return;
        }

        currentDestination = entrance;
    }

    private Transform GetRandomEntrance(Transform exclude)
    {
        int validCount = GetValidEntranceCount(exclude);
        if (validCount == 0)
        {
            return null;
        }

        int choice = Random.Range(0, validCount);
        for (int i = 0; i < houseEntrances.Length; i++)
        {
            Transform entrance = houseEntrances[i];
            if (entrance == null || entrance == exclude)
            {
                continue;
            }

            if (choice == 0)
            {
                return entrance;
            }
            choice--;
        }

        return null;
    }

    private int GetValidEntranceCount(Transform exclude = null)
    {
        if (houseEntrances == null)
        {
            return 0;
        }

        int count = 0;
        foreach (Transform entrance in houseEntrances)
        {
            if (entrance != null && entrance != exclude)
            {
                count++;
            }
        }

        return count;
    }

    private void SetVisible(bool visible)
    {
        foreach (Renderer visualRenderer in visualRenderers)
        {
            if (visualRenderer != null)
            {
                visualRenderer.enabled = visible;
            }
        }
    }
}
