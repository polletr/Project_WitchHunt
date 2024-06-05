using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[RequireComponent(typeof(CharacterController))]
public class PlayerController : CharacterClass
{
    [HideInInspector]
    public CharacterController c;
    
    public float combatCooldown = 5f;
    float combatTimer = 0;

    [SerializeField]
    protected float _rotationSpeed;
    public float RotationSpeed { get { return _speed; } }

    public AttackType attackType;

    public TemporaryDataContainer tempData;

    private InputManager inputManager;

    private bool isMoving;

    private PlayerSpellCastManager spellCastManager;
    [SerializeField]
    private GameEvent onHealthChanged;

    public Vector3 MouseWorldPosition { get; private set; }
    public Quaternion PlayerRotation { get; private set; }

    private LayerMask groundLayer;

    [HideInInspector]
    public bool canInteract;

    [HideInInspector]
    public Interactable interactableObj;


    private void Awake()
    {
        groundLayer = LayerMask.GetMask("Ground");
        health = tempData.startHealth;
        maxHealth = tempData.startHealth;
        spellCastManager = GetComponent<PlayerSpellCastManager>();
        onHealthChanged.Raise(health);
        inputManager = GetComponent<InputManager>();
        c = GetComponent<CharacterController>();
        ChangeState(new PlayerMoveState());

    }

    private void Update()
    {
        if (isInCombat)
        {
           combatTimer += Time.deltaTime;
            if (combatTimer >= combatCooldown)
            {
                isInCombat = false;
                combatTimer = 0;
            }
        }

        if (inputManager.Movement != new Vector2(0f, 0f) || !isMoving)
        {
            HandleMove(inputManager.Movement);
            isMoving = true;
        }

        if (inputManager.Movement == new Vector2(0f, 0f) && isMoving)
        {
            isMoving = false;
        }

        currentState?.StateUpdate();
    }
    private void FixedUpdate() => currentState?.StateFixedUpdate();



    #region character Actions
    public void HandleMove(Vector2 dir)
    {
        currentState?.HandleMovement(dir);
    }
    
    public void HandleInteract()
    {
        if (interactableObj != null)
            currentState?.HandleInteract();
    }

    public void CancelInteract()
    {
        currentState?.StopInteract();
    }

    public void HandleBaseAttack()
    {
        if (tempData.baseSpell != null)
            spellCastManager.CastBaseSpell();

    }
    public void HandleSpecialAttack()
    {
        if (tempData.specialSpell != null)
            spellCastManager.CastSpecialSpell();
    }
    public void HandleUltimateAttack()
    {
        if (tempData.ultimateSpell != null)
            spellCastManager.CastUltimateSpell();
    }

    #endregion

    public override void GetHit(float damageAmount,GameObject attacker, SpellBook spell)
    {
        base.GetHit(damageAmount, attacker, spell);
        if (health <= 0)
        {
            Debug.Log("Dead");
            //Time.timeScale = 0f;
        }

    }

    public enum AttackType
    {
        Base,
        Special,
        Ultimate
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<Interactable>())
        {
            if (other.GetComponent<Interactable>().canInteract)
                interactableObj = other.GetComponent<Interactable>();
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if (other.GetComponent<Interactable>())
        {
            interactableObj = null;
        }
    }

    private void RemoveUltimateSpell()
    {
        //Turn it on later
        //tempData.ultimateSpell = null;
    }

    public void HandlePointerDirection(Vector2 mousePos)
    {
        Vector3 newMousePos = Input.mousePosition;
        Ray ray = Camera.main.ScreenPointToRay(newMousePos);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, Mathf.Infinity, groundLayer))
        {
            MouseWorldPosition = hit.point;
        }
        Vector3 target = MouseWorldPosition;
        Vector3 direction = target - transform.position;
        direction.y = 0;
        PlayerRotation = Quaternion.LookRotation(direction);
    }

    public void RotateToTarget()
    {
        transform.rotation = Quaternion.Slerp(gameObject.transform.rotation, PlayerRotation, RotationSpeed);
    }


    public void HandleControllerDirection(Vector2 move)
    {
        //calculate the PlayerRotation based on the movement of the joystick
    }

    public void HandleCancelBaseAttack()
    {
        currentState?.HandleAttackCancel();
    }


    protected override void TakeDamage(float damage)
    {
        base.TakeDamage(damage);
       onHealthChanged.Raise(health);
    }

    public override void Heal(float healAmount)
    {
        base.Heal(healAmount);
        onHealthChanged.Raise(health);
    }

}
