using UnityEngine;
using UnityEngine.AI;

public class EnemyChaseState : EnemyBaseState
{
    readonly NavMeshAgent agent;
    readonly Transform player;

    private bool isMoving;

    public EnemyChaseState(Enemy enemy, Animator animator, NavMeshAgent agent, Transform player) : base(enemy, animator)
    {
        this.agent = agent;
        this.player = player;
    }

    public override void OnEnter()
    {

    }



    public override void Update()
    {
        agent.speed = enemy.Speed * enemy.runMultiplier;

        agent.SetDestination(player.position);

        if (agent.velocity.magnitude > 0f && !isMoving)
        {
            isMoving = true;
            animator.CrossFade(RunHash, crossFadeDuration);
        }
        else if (agent.velocity.magnitude <= 0f && isMoving)
        {
            isMoving = false;

            animator.CrossFade(IdleHash, crossFadeDuration);
        }



    }
}
