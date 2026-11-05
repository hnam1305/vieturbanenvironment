using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public enum NpcBehaviorType
{
    Wanderer,
    Shopper
}

public class NpcController : MonoBehaviour
{
    [SerializeField] private NpcBehaviorType behaviorType;
    [SerializeField, Range(0f, 1f)] private float wandererHomeVisitChance = 0.2f;
    [SerializeField, Min(0f)] private float minTimeInside = 10f;
    [SerializeField, Min(0f)] private float maxTimeInside = 30f;
    [SerializeField, Min(0f)] private float minTimeAtActivity = 5f;
    [SerializeField, Min(0f)] private float maxTimeAtActivity = 15f;
    [SerializeField, Min(0f)] private float arrivalTolerance = 0.4f;

    private NavMeshAgent agent;
    private Animator animator;
    private Renderer[] visualRenderers;
    private Transform[] houseEntrances;
    private Transform[] shopSpots;
    private Transform[] parkSpots;
    private Transform homeEntrance;
    private Transform currentDestination;
    private DestinationType currentDestinationType;
    private string residentName;
    private string deliveryAddress;
    private bool isWaiting;
    private bool isDormant;
    private bool isInHouse;
    private bool hasPendingDelivery;

    private enum DestinationType
    {
        Home,
        Activity
    }

    public string ResidentName { get { return residentName; } }
    public string DeliveryAddress { get { return deliveryAddress; } }
    public Transform DeliveryPoint { get { return homeEntrance; } }
    public bool IsDormant { get { return isDormant; } }
    public bool HasPendingDelivery { get { return hasPendingDelivery; } }

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponentInChildren<Animator>();
        visualRenderers = GetComponentsInChildren<Renderer>(true);
    }

    public void Configure(NpcBehaviorType type, string name, string address, Transform residentHome,
        Transform[] houses, Transform[] shops, Transform[] parks)
    {
        behaviorType = type;
        residentName = name;
        deliveryAddress = address;
        homeEntrance = residentHome;
        houseEntrances = houses;
        shopSpots = shops;
        parkSpots = parks;
    }

    private void Start()
    {
        if (agent == null || !agent.isOnNavMesh)
        {
            Debug.Log("npc needs a nav mesh agent on the nav mesh", this);
            enabled = false;
            return;
        }

        if (GetValidCount(houseEntrances) < 2 || homeEntrance == null)
        {
            Debug.Log("npc needs a home and at least two house entrances", this);
            enabled = false;
            return;
        }

        if (behaviorType == NpcBehaviorType.Wanderer && GetValidCount(parkSpots) == 0)
        {
            Debug.Log("wanderer needs at least one park spot", this);
            enabled = false;
            return;
        }

        if (behaviorType == NpcBehaviorType.Shopper && GetValidCount(shopSpots) == 0)
        {
            Debug.Log("shopper needs at least one shop spot", this);
            enabled = false;
            return;
        }

        SetNextActivityDestination(null);
    }

    private void Update()
    {
        if (isDormant || agent == null)
        {
            return;
        }

        if (isWaiting || currentDestination == null || agent.pathPending || !agent.hasPath)
        {
            UpdateWalkingAnimation();
            return;
        }

        if (agent.pathStatus == NavMeshPathStatus.PathComplete &&
            agent.remainingDistance <= agent.stoppingDistance + arrivalTolerance)
        {
            if (currentDestinationType == DestinationType.Home)
            {
                StartCoroutine(EnterHouseAndLeaveLater());
            }
            else
            {
                StartCoroutine(WaitAtActivity());
            }
        }

        UpdateWalkingAnimation();
    }

    public void SetDormant(bool dormant, bool keepAtDeliveryPoint)
    {
        if (isDormant == dormant)
        {
            return;
        }

        if (dormant)
        {
            isDormant = true;
            if (keepAtDeliveryPoint && homeEntrance != null && agent.isOnNavMesh)
            {
                StopAgent();
                currentDestination = null;
                agent.Warp(homeEntrance.position);
                transform.rotation = homeEntrance.rotation;
                isInHouse = false;
            }

            agent.isStopped = true;
            SetVisible(false);
            return;
        }

        if (agent.isOnNavMesh)
        {
            agent.isStopped = false;
            SetVisible(!isInHouse);
            isDormant = false;
            if (!isInHouse && currentDestination != null)
            {
                agent.SetDestination(currentDestination.position);
            }
        }
    }

    public void SetHasPendingDelivery(bool pending)
    {
        hasPendingDelivery = pending;
        if (!pending || homeEntrance == null || agent == null || !agent.isOnNavMesh)
        {
            return;
        }

        StopAllCoroutines();
        isWaiting = false;
        currentDestination = null;
        isInHouse = false;
        agent.Warp(homeEntrance.position);
        agent.isStopped = isDormant;
        transform.rotation = homeEntrance.rotation;
        SetVisible(!isDormant);
    }

    private IEnumerator WaitAtActivity()
    {
        isWaiting = true;
        currentDestination = null;
        StopAgent();
        yield return new WaitForSeconds(GetRandomWait(minTimeAtActivity, maxTimeAtActivity));
        isWaiting = false;

        if (behaviorType == NpcBehaviorType.Shopper || Random.value < wandererHomeVisitChance)
        {
            SetHomeDestination(null);
        }
        else
        {
            SetNextActivityDestination(null);
        }
    }

    private IEnumerator EnterHouseAndLeaveLater()
    {
        isWaiting = true;
        isInHouse = true;
        Transform enteredEntrance = currentDestination;
        currentDestination = null;
        StopAgent();
        SetVisible(false);
        yield return new WaitForSeconds(GetRandomWait(minTimeInside, maxTimeInside));

        Transform exitEntrance = GetRandomTransform(houseEntrances, enteredEntrance);
        if (exitEntrance == null || !agent.Warp(exitEntrance.position))
        {
            Debug.Log("npc could not reach a house entrance", this);
            SetVisible(!isDormant);
            isInHouse = false;
            agent.isStopped = isDormant;
            isWaiting = false;
            SetNextActivityDestination(null);
            yield break;
        }

        transform.rotation = exitEntrance.rotation;
        isInHouse = false;
        SetVisible(!isDormant);
        agent.isStopped = isDormant;
        isWaiting = false;
        SetNextActivityDestination(null);
    }

    private void SetNextActivityDestination(Transform exclude)
    {
        Transform[] activities = parkSpots;
        if (behaviorType == NpcBehaviorType.Shopper)
        {
            activities = shopSpots;
        }
        SetDestination(GetRandomTransform(activities, exclude), DestinationType.Activity);
    }

    private void SetHomeDestination(Transform exclude)
    {
        SetDestination(homeEntrance, DestinationType.Home);
    }

    private void SetDestination(Transform destination, DestinationType type)
    {
        if (destination == null || !agent.SetDestination(destination.position))
        {
            Debug.Log("npc could not set a destination on the nav mesh", this);
            currentDestination = null;
            return;
        }
        currentDestination = destination;
        currentDestinationType = type;
    }

    private Transform GetRandomTransform(Transform[] candidates, Transform exclude)
    {
        int count = GetValidCount(candidates, exclude);
        if (count == 0)
        {
            count = GetValidCount(candidates);
            exclude = null;
        }
        if (count == 0)
        {
            return null;
        }

        int choice = Random.Range(0, count);
        foreach (Transform candidate in candidates)
        {
            if (candidate == null || candidate == exclude)
            {
                continue;
            }
            if (choice == 0)
            {
                return candidate;
            }
            choice--;
        }
        return null;
    }

    private int GetValidCount(Transform[] candidates, Transform exclude = null)
    {
        if (candidates == null)
        {
            return 0;
        }
        int count = 0;
        foreach (Transform candidate in candidates)
        {
            if (candidate != null && candidate != exclude)
            {
                count++;
            }
        }
        return count;
    }

    private float GetRandomWait(float min, float max)
    {
        float lowerBound = Mathf.Max(0f, Mathf.Min(min, max));
        float upperBound = Mathf.Max(lowerBound, Mathf.Max(min, max));
        return Random.Range(lowerBound, upperBound);
    }

    private void StopAgent()
    {
        agent.isStopped = true;
        agent.ResetPath();
    }

    private void UpdateWalkingAnimation()
    {
        if (animator != null && agent != null && HasAnimatorParameter("isWalking", AnimatorControllerParameterType.Bool))
        {
            animator.SetBool("isWalking", agent.velocity.sqrMagnitude > 0.01f);
        }
    }

    private bool HasAnimatorParameter(string parameterName, AnimatorControllerParameterType parameterType)
    {
        foreach (AnimatorControllerParameter parameter in animator.parameters)
        {
            if (parameter.name == parameterName && parameter.type == parameterType)
            {
                return true;
            }
        }
        return false;
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
