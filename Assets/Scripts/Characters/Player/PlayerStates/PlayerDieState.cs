using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerDieState : BaseState
{
    float clipLength;

    bool UIOn = false;
    public override void EnterState()
    {
        base.EnterState();
        //animate 

        timer = 0f;
        clipLength = 1.5f;//player.anim.GetCurrentAnimatorClipInfo(0)[0].clip.length;


    }
    public override void ExitState()
    {

    }

    public override void StateFixedUpdate()
    {

    }

    public override void StateUpdate()
    {
        timer += Time.deltaTime;    
        if (timer >= clipLength && !UIOn)
        {
            player.OnPlayerDie.Raise(new Empty());

            UIOn = true;
        }

    }

    public override void HandleSpecialAttack()
    {

    }

    public override void HandleAttack()
    {

    }
    public override void StopInteract()
    {

    }


}
