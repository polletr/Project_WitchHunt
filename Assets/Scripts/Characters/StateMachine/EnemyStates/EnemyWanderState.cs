using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem.XR.Haptics;

public class EnemyWanderState : EnemyBaseState
{
    readonly NavMeshAgent agent;
    readonly Vector3 startPoint;
    readonly float wanderRadius;

    public EnemyWanderState(Enemy enemy, Animator animator, NavMeshAgent agent, float wanderRadius) : base(enemy, animator)
    {
        this.agent = agent;
        this.startPoint = enemy.transform.position;
        this.wanderRadius = wanderRadius;
    }

    public override void OnEnter()
    {
        agent.speed = enemy.Speed;
        enemy.wanderTimer.Start();
    }

    public override void OnExit()
    {
        enemy.wanderTimer.Stop();
    }


    public override void Update()
    {
        if (HasReachedDestination())
        {
            var randomDirection = Random.insideUnitSphere * wanderRadius;
            randomDirection += startPoint;
            NavMeshHit hit;
            NavMesh.SamplePosition(randomDirection, out hit, wanderRadius, 1);
            var finalPosition = hit.position;

            agent.SetDestination(finalPosition);
        }

        if (agent.velocity.magnitude > 0f && animator.GetCurrentAnimatorStateInfo(0).shortNameHash != WalkHash)
        {
            animator.CrossFade(WalkHash, crossFadeDuration);
        }
        else if (agent.velocity.magnitude <= 0f && animator.GetCurrentAnimatorStateInfo(0).shortNameHash != IdleHash)
        {
            animator.CrossFade(IdleHash, crossFadeDuration);
        }
    }

    bool HasReachedDestination()
    {
        return !agent.pathPending
                && agent.remainingDistance <= agent.stoppingDistance
                && (!agent.hasPath || agent.velocity.sqrMagnitude == 0f);
    }
}
