using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;

public class AIController : CharacterClass
{
    [Header("Vision Cone"), SerializeField]
    private float coneRadius = 5f;
    [Range(0, 1)]
    public float angleth = 5f;

    [Header("Area Check"), SerializeField]
    private float areaRadius = 3f;

    [Header("Agro Check"), SerializeField]
    private float aggroRadius = 7f;

    [SerializeField]
    protected float _attackrange = 2f;

    protected float _attackTimer = 50;

    protected Transform player;

    private SphereCollider playerCheckCollider;

    protected AIBrain currentAction;

    [SerializeField]
    private float rotationSpeed = 5f;

    protected NavMeshAgent agent;


    [SerializeField]
    private HealthCollectible healthDrop;

    [SerializeField]
    private SoulCollectible soulDrop;

    [SerializeField]
    protected float healthDropChance;

    private List<AISpot> spotList;

    [SerializeField]
    private int maxAmountInGroup;

    [SerializeField]
    private float minIdleTime;  // Duration for idle state

    [SerializeField]
    private float minIdleRadius;  // Radius within which the AI can roam while idling

    [Header("Boid Settings")]
    public float boidNeighborRadius = 5f;
    public float separationWeight = 1.5f;
    public float alignmentWeight = 1f;
    public float cohesionWeight = 1f;


    protected virtual void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        agent.stoppingDistance = _attackrange;

        playerCheckCollider = GetComponent<SphereCollider>();
        playerCheckCollider.radius = aggroRadius;


        SetBrain(AIBrain.Idle);
    }

    protected virtual void Update()
    {
        _attackTimer += Time.deltaTime;
        agent.speed = _speed;

        if (currentAction == AIBrain.Patrol || currentAction == AIBrain.Idle || currentAction == AIBrain.Chase)
        {
            //ApplyBoidBehavior();
        }
    }
    #region AI Brain

    public enum AIBrain
    {
        Idle,
        Patrol,
        Chase,
        Combat,
        Die
    }

    protected void SetBrain(AIBrain newState)
    {
        //if (currentAction == newState) return;
        currentAction = newState;
        StopCoroutine(OnIdle());
        StopCoroutine(OnPatrol());
        StopCoroutine(OnCombat());
        StopCoroutine(OnChasing());

        Debug.Log(currentAction.ToString());
        switch (currentAction)
        {
            case AIBrain.Idle:
                StartCoroutine(OnIdle());
                break;
            case AIBrain.Die:
                OnDie();
                break;
            case AIBrain.Patrol:
                StartCoroutine(OnPatrol());
                break;
            case AIBrain.Chase:
                StartCoroutine(OnChasing());
                break;
            case AIBrain.Combat:
                StartCoroutine(OnCombat());
                break;
            default:
                break;
        }
    }
    protected virtual IEnumerator OnIdle()
    {
        float startTime = Time.time;

        float idleTime = UnityEngine.Random.Range(minIdleTime, minIdleTime * 2);
        float idleRadius = UnityEngine.Random.Range(minIdleRadius, minIdleRadius * 2);


        while (AIBrain.Idle == currentAction && Time.time - startTime < idleTime)
        {
            Vector3 randomDirection = UnityEngine.Random.insideUnitSphere * idleRadius;
            randomDirection += transform.position;
            NavMeshHit hit;
            NavMesh.SamplePosition(randomDirection, out hit, idleRadius, 1);
            Vector3 finalPosition = hit.position;

            agent.SetDestination(finalPosition);

            while (Vector3.Distance(transform.position, finalPosition) > agent.stoppingDistance)
            {
                yield return null;
            }

            // Smooth random rotation
            float randomRotation = UnityEngine.Random.Range(0f, 360f);
            Quaternion targetRotation = Quaternion.Euler(0, randomRotation, 0);
            yield return StartCoroutine(SmoothRotate(targetRotation));

            // Random wait time
            float waitTime = UnityEngine.Random.Range(0.5f, 1.5f);

            if (IsPlayerInView())
                SetBrain(AIBrain.Chase);

            yield return new WaitForSeconds(waitTime);


        }
        SetBrain(AIBrain.Patrol);
    }

    protected virtual IEnumerator OnCombat()
    {
        while (AIBrain.Combat == currentAction)
        {
            //anim.SetTrigger("Attack");

            Vector3 target = player.position - transform.position;
            target.y = 0;
            Quaternion targetRotation = Quaternion.LookRotation(target);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);

            if (Vector3.Distance(this.transform.position, player.position) > _attackrange)
            {
                SetBrain(AIBrain.Chase);
            }
            else
            {
                AttackPlayer();
            }
            yield return null;
        }
        yield return null;
    }
    protected virtual IEnumerator OnPatrol()
    {

        FindAISpots();

        while (AIBrain.Patrol == currentAction)
        {
            Debug.Log(spotList.Count);
            if (spotList.Count > 0)
            {
                AISpot targetSpot = spotList[UnityEngine.Random.Range(0, spotList.Count)];
                Quaternion targetRotation = Quaternion.LookRotation(targetSpot.transform.position);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);

                agent.SetDestination(targetSpot.transform.position);

                while (Vector3.Distance(transform.position, targetSpot.transform.position) > agent.stoppingDistance)
                {
                    yield return null;
                }

                // Check for nearby AIs
                Collider[] hitColliders = Physics.OverlapSphere(transform.position, areaRadius);
                int aiCount = 0;
                foreach (var hitCollider in hitColliders)
                {
                    if (hitCollider.GetComponent<AIController>() && hitCollider.GetComponent<AIController>() != this)
                    {
                        aiCount++;
                    }
                }

                if (aiCount < maxAmountInGroup)
                {
                    SetBrain(AIBrain.Idle);
                }
            }


            if (IsPlayerInView())
                SetBrain(AIBrain.Chase);

            yield return null;
        }
        yield return null;
    }
    protected virtual IEnumerator OnChasing()
    {
        while (AIBrain.Chase == currentAction)
        {
            Vector3 target = player.position - transform.position;
            target.y = 0;
            Quaternion targetRotation = Quaternion.LookRotation(target);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);

            if (Vector3.Distance(this.transform.position, player.position) > _attackrange)
            {
                agent.SetDestination(player.position);
            }
            else
            {
                SetBrain(AIBrain.Combat);
                Debug.Log("Combat");
            }

            if (LoseAgro(player.position))
            {
                SetBrain(AIBrain.Patrol);
            }

            yield return null;
        }
        yield return null;
    }
    protected virtual IEnumerator OnGetHit()
    {
        yield return null;
    }
    protected virtual void OnDie()
    {

        Instantiate(soulDrop, transform.position, Quaternion.identity);

        if (UnityEngine.Random.Range(0f,1f) <= healthDropChance)
        {
            Instantiate(healthDrop, transform.position + Vector3.forward, Quaternion.identity);
        }
        
        Destroy(this.gameObject, 1f);
    }
    public virtual void AttackPlayer() { }

    private IEnumerator SmoothRotate(Quaternion targetRotation)
    {
        float rotationDuration = 0.2f; // Duration of the rotation
        float time = 0f;

        Quaternion startRotation = transform.rotation;

        while (time < rotationDuration)
        {
            transform.rotation = Quaternion.Slerp(startRotation, targetRotation, time / rotationDuration);
            time += Time.deltaTime;
            yield return null;
        }

        transform.rotation = targetRotation;
    }

    public override void GetHit(int damageAmount, CharacterClass attacker, SpellBook spell)
    {
        if (attacker.GetComponent<PlayerController>())
        {
            base.GetHit(damageAmount, attacker, spell);
            player = attacker.transform;
            SetBrain(AIBrain.Chase);
        }
        if (health <= 0)
        {
            SetBrain(AIBrain.Die);
        }
    }
    #endregion

    #region Check Functions
    void OnDrawGizmos()
    {
        if (player != null)
        {
            Gizmos.matrix = Handles.matrix = transform.localToWorldMatrix;
            if (VisionConeCheck(player.position))
                Handles.color = Color.red;
            if (AreaCheck(player.position))
                Handles.color = Color.blue;
            if (LoseAgro(player.position))
                Handles.color = Color.black;
            Gizmos.color = Handles.color;
        }
        Handles.DrawWireDisc(Vector3.zero, Vector3.up, coneRadius);
        Handles.DrawWireDisc(Vector3.zero, Vector3.up, areaRadius);
        Handles.DrawWireDisc(Vector3.zero, Vector3.up, aggroRadius);

        float p = angleth;
        float x = Mathf.Sqrt(1 - p * p);

        Vector3 vRight = new Vector3(x, 0, p) * coneRadius;
        Vector3 vLeft = new Vector3(-x, 0, p) * coneRadius;

        Gizmos.DrawRay(Vector3.zero, vLeft);
        Gizmos.DrawRay(Vector3.zero, vRight);
    }

    private Vector3 GetFlatDirection(Vector3 targetPosition, out float flatDistance)
    {
        Vector3 vecToTargetWorld = targetPosition - transform.position;
        Vector3 vecToTarget = transform.InverseTransformVector(vecToTargetWorld);
        Vector3 flatDir = vecToTarget;
        flatDir.y = 0;
        flatDistance = flatDir.magnitude;
        return flatDir;
    }

    public bool AreaCheck(Vector3 position)
    {
        float flatDistance;
        GetFlatDirection(position, out flatDistance);

        // Distance check
        return flatDistance <= areaRadius;
    }

    public bool VisionConeCheck(Vector3 position)
    {
        float flatDistance;
        Vector3 flatDir = GetFlatDirection(position, out flatDistance);
        flatDir.Normalize();

        // Angle check
        if (flatDir.z < angleth)
            return false;

        // Distance check
        return flatDistance <= coneRadius;
    }

    public bool LoseAgro(Vector3 position)
    {
        float flatDistance;
        GetFlatDirection(position, out flatDistance);

        // Distance check
        return flatDistance > aggroRadius;
    }

    public bool IsPlayerInView()
    {
        if (player == null)
            return false;
        return VisionConeCheck(player.position) || AreaCheck(player.position);
    }

    void FindAISpots()
    {
        AISpot[] allSpots = FindObjectsOfType<AISpot>();

        // Initialize spotList
        spotList = new List<AISpot>();

        // Filter based on tier and add to spotList
        foreach (AISpot spot in allSpots)
        {
            if (spot.tier == tier)
            {
                spotList.Add(spot);
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<PlayerController>())
        {
            player = other.GetComponent<PlayerController>().transform;
        }
    }
    #endregion

    #region Boid Behavior
    private void ApplyBoidBehavior()
    {
        Vector3 separation = Vector3.zero;
        Vector3 alignment = Vector3.zero;
        Vector3 cohesion = Vector3.zero;

        int neighborCount = 0;

        Collider[] hitColliders = Physics.OverlapSphere(transform.position, boidNeighborRadius);
        foreach (var hitCollider in hitColliders)
        {
            AIController neighbor = hitCollider.GetComponent<AIController>();
            if (neighbor != null && neighbor != this && neighbor.currentAction == currentAction)
            {
                // Separation
                Vector3 toNeighbor = transform.position - neighbor.transform.position;
                separation += toNeighbor / toNeighbor.sqrMagnitude;

                // Alignment
                alignment += neighbor.agent.velocity;

                // Cohesion
                cohesion += neighbor.transform.position;

                neighborCount++;
            }
        }

        if (neighborCount > 0)
        {
            // Average out values
            separation /= neighborCount;
            alignment /= neighborCount;
            cohesion /= neighborCount;

            // Calculate direction to center of mass of neighbors
            cohesion = (cohesion - transform.position).normalized;

            // Apply weights
            Vector3 boidDirection = separation * separationWeight + alignment * alignmentWeight + cohesion * cohesionWeight;

            // Apply the resulting boid direction to the AI
            if (boidDirection != Vector3.zero)
            {
                Quaternion boidRotation = Quaternion.LookRotation(boidDirection);
                transform.rotation = Quaternion.Slerp(transform.rotation, boidRotation, rotationSpeed * Time.deltaTime);
                agent.SetDestination(transform.position + boidDirection);
            }
        }
    }
    #endregion

}
