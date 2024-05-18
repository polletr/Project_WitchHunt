using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Interactable : MonoBehaviour
{
    [HideInInspector]
    public bool canInteract;

    public float waitTime;
    // Start is called before the first frame update
    protected virtual void Start()
    {
        canInteract = true;
    }

    public virtual void Interact()
    {

    }


}
