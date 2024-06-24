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
    private GameEvent<float> onHealthChanged;
    [SerializeField]
    public EmptyGameEvent OnPlayerDie;
    [SerializeField]
    private EmptyGameEvent OnPlayerPickSpell;

    public Vector3 AimWorldPosition { get; private set; }
    public Quaternion PlayerRotation { get; private set; }

    [Header("Audio Clip")]
    [SerializeField] private AudioClip getHitClip;
    [SerializeField] private AudioClip magicGetHitClip;

    private LayerMask groundLayer;

    [HideInInspector]
    public bool canInteract;

    [HideInInspector]
    public Interactable interactableObj;
    [HideInInspector]
    public SpellWeapon spellWeapon;

    [SerializeField]
    private float healthBonusEachStage;

    public DoubleFloatEvent OnUILoading;

    private void Awake()
    {
        groundLayer = LayerMask.GetMask("Ground");
        health = tempData.startHealth + GameManager.Instance.pData.healthBonus * healthBonusEachStage;
        maxHealth = health;
        initialSpeed = _speed;
        onHealthChanged.Raise(health);

        SetDamageMultiplier();
        initialDamageMultiplier = damageMultiplier;
        
        spellCastManager = GetComponent<PlayerSpellCastManager>();
        spellWeapon = GetComponent<SpellWeapon>();
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
        {
            spellTarget = AimWorldPosition;
            spellCastManager.CastSpecialSpell();
        }
    }
    public void HandleUltimateAttack()
    {
        if (tempData.ultimateSpell != null)
        {
            spellTarget = AimWorldPosition;
            spellCastManager.CastUltimateSpell();
            Invoke(nameof(RemoveUltimateSpell),4f);
        }
    }

    #endregion

    public override void GetHit(float damageAmount,GameObject attacker, SpellBook spell)
    {
        //AudioEvent.PlayGetHit.Invoke(getHitClip);
        base.GetHit(damageAmount, attacker, spell);
        if (isAlive && health <= 0)
        {
            ChangeState(new PlayerDieState());
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
            {
                interactableObj = other.GetComponent<Interactable>();
                interactableObj.ActivateInteractable();
            }
        }

        if (other.CompareTag("Detector"))
        {
           other.GetComponentInParent<Village>().TriggerTracker();
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if (other.GetComponent<Interactable>())
        {
            interactableObj = null;
            other.GetComponent<Interactable>().DeactivateInteractable();
        }

        if (other.CompareTag("Detector"))
        {
            other.GetComponentInParent<Village>().EndTracker();
        }
    }

    private void RemoveUltimateSpell()
    {
        //Turn it on later
        tempData.ultimateSpell = null;
        OnPlayerPickSpell.Raise(new Empty());
    }

    public void HandlePointerDirection(Vector2 mousePos)
    {
        Vector3 newMousePos = Input.mousePosition;
        Ray ray = Camera.main.ScreenPointToRay(newMousePos);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, Mathf.Infinity, groundLayer))
        {
            AimWorldPosition = hit.point;
        }
        Vector3 target = AimWorldPosition;
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

        if (!isAlive)
        {
        }
    }

    public override void Heal(float healAmount)
    {
        base.Heal(healAmount);
        onHealthChanged.Raise(health);
    }

    private void SetDamageMultiplier()
    {
        if (GameManager.Instance.pData.rune < 3)
        {
            damageMultiplier = 1 + GameManager.Instance.pData.rune * 0.1f;
        }
        else
        {
            damageMultiplier = 1 + GameManager.Instance.pData.rune * 0.15f;
        }
    }

}
