using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class IceSpike_SpecialSpell : SpecialSpellBook
{

    private BoxCollider damageTrigger;

    [SerializeField]
    private float spellTimer;

    private float limitUp;

    [SerializeField]
    private float speed;

    private float startPosY;

    protected override void CastSpell(int tier)
    {
        damageTrigger = GetComponentInChildren<BoxCollider>();
        limitUp = 0.5f;
        startPosY = transform.position.y;
    }

    protected override void Update()
    {
        base.Update();
        if (transform.position.y < limitUp && timer < spellTimer)
        {
            transform.Translate(Vector3.up * speed * Time.deltaTime);
        }
        else if (timer >= spellTimer)
        {
            damageTrigger.enabled = false;

            transform.Translate(Vector3.down * speed * Time.deltaTime);
            if (transform.position.y <= startPosY )
            {
                Destroy(gameObject);
            }
        }
        //Stop when reaching upper limit

        if(transform.position.y >= limitUp)
        {
            damageTrigger.enabled = false;
        }
    }


    private void OnTriggerEnter(Collider other)
    {

        if (other.GetComponent<CharacterClass>())
        {
            other.GetComponent<CharacterClass>().GetHit(damage, charAttacker);
            Debug.Log("Got Hit");

        }

        //Check if it is an enemy to call a damage function
        //Call an explosion or the after effect for the destroyed object.
    }

}