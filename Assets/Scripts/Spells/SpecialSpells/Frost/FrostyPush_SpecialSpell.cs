using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class FrostyPush_SpecialSpell : SpecialSpellBook
{
    private float pushRadius;

    [SerializeField]
    private float pushForce = 10f;

    private SphereCollider damageCollider;

    protected override void CastSpell(int tier)
    {
        pushRadius = spellData.currentTierData.radius;


        damageCollider = GetComponent<SphereCollider>();
        damageCollider.radius = pushRadius;
        //CastPushSpell();
    }


    protected override void Update()
    {
        base.Update();
        

        if (timer >= duration)
        {
           Destroy(gameObject);
        }
    }

    IEnumerator PushEnemy(Transform agent, Vector3 force)
    {
        float elapsedTime = 0f;

       //agent.enabled = false; // Disable NavMeshAgent to manually move the enemy

        while (elapsedTime < duration)
        {
            agent.transform.position += force * Time.deltaTime;
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        agent.gameObject.GetComponent<CharacterClass>().GetHit(damage , charAttacker, this);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<CharacterClass>() && charAttacker != other)
        {
            Transform agent = other.GetComponent<CharacterClass>().transform;
            if (agent != null)
            {
                Vector3 direction = other.transform.position - transform.position;
                direction.y = 0; // Ignore vertical push

                StartCoroutine(PushEnemy(agent, direction.normalized * pushForce));
                SetStatusEffect(other.gameObject);

            }
        }

    }
}
