using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class BaseNavigation : MonoBehaviour
{
    public enum EState
    {
        Idle                = 0,
        FindingPath         = 1,
        FollowingPath       = 2,

        Failed_NoPathExists = 100
    }

    [Header("Path Following")]
    [SerializeField] protected float DestinationReachedThreshold = 0.25f;
    [SerializeField] protected float MaxMoveSpeed = 5f;
    [SerializeField] protected float RotationSpeed = 120f;

    [Header("Animation")]
    [SerializeField] protected Animator AnimController;

    [Header("Debug Tools")]
    [SerializeField] protected bool DEBUG_UseMoveTarget;
    [SerializeField] protected Transform DEBUG_MoveTarget;
    [SerializeField] protected bool DEBUG_ShowHeading;

    public Vector3 Destination { get; private set; }
    public EState State { get; private set; } = EState.Idle;

    public bool IsFindingOrFollowingPath => State == EState.FindingPath || State == EState.FollowingPath;
    public bool IsAtDestination
    {
        get
        {
            if (State != EState.Idle)
                return false;

            Vector3 vecToDestination = Destination - transform.position;
            vecToDestination.y = 0f;

            return vecToDestination.magnitude <= DestinationReachedThreshold;
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        Initialise();
    }

    // Update is called once per frame
    void Update()
    {
        if (DEBUG_UseMoveTarget)
            SetDestination(DEBUG_MoveTarget.position);

        if (State == EState.FindingPath)
            Tick_Pathfinding();

        Tick_Default();

        if (AnimController != null)
            Tick_Animation();
    }

    void FixedUpdate()
    {
        if (State == EState.FollowingPath)
            Tick_PathFollowing();
    }

    public bool SetDestination(Vector3 newDestination)
    {
        // location is already our destination?
        Vector3 destinationDelta = newDestination - Destination;
        destinationDelta.y = 0f;
        if (IsFindingOrFollowingPath && (destinationDelta.magnitude <= DestinationReachedThreshold))
            return true;
        
        // are we already near the destination
        destinationDelta = newDestination - transform.position;
        destinationDelta.y = 0f;
        if (destinationDelta.magnitude <= DestinationReachedThreshold)
            return true;

        Destination = newDestination;

        return RequestPath();
    }

    public abstract void StopMovement();

    public abstract bool FindNearestPoint(Vector3 searchPos, float range, out Vector3 foundPos);

    protected abstract void Initialise();

    protected abstract bool RequestPath();

    protected virtual void OnBeganPathFinding()
    {
        State = EState.FindingPath;
    }

    protected virtual void OnPathFound()
    {
        State = EState.FollowingPath;
    }

    protected virtual void OnFailedToFindPath()
    {
        State = EState.Failed_NoPathExists;
    }

    protected virtual void OnReachedDestination()
    {
        State = EState.Idle;
    }

    protected abstract void Tick_Default();
    protected abstract void Tick_Pathfinding();
    protected abstract void Tick_PathFollowing();
    protected abstract void Tick_Animation();
}
