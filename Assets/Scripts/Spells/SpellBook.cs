using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class SpellBook : MonoBehaviour
{
    [HideInInspector]
    public int tier = 1;
    [HideInInspector]
    public float cooldown;
    protected GameObject charAttacker;
    public castType castOrigin;

    protected float damage;

    protected float timer;

    protected float range;

    protected float radius;

    protected float duration;

    protected float projectileSpeed;

    public bool canUseBaseSpell = true;

    public SpellStatsContainer spellData;

    public Sprite spellIcon;

    #region StatusEffect
    [Header("Status Effect")]
    protected float statusEffectTimer;
    protected float statusEffectDamage;
    protected float statusEffectChance;

    [SerializeField]
    protected StatusEffect statusEffect;

    #endregion

    [Header("Clips")]
    public AudioClip castClip;
    public AudioClip hitClip;
    public AudioClip endClip;
    public AudioClip travelClip;

    // Start is called before the first frame update
    protected virtual void Awake()
    {
        gameObject.layer = LayerMask.NameToLayer("Spell");
        SetDataFromSpellContainer();
        CastSpell(tier);
    }
    public virtual void UpgradeTier()
    {
        if (tier < 3)
        {
            tier++;
        }
    }

    protected virtual void Start()
    {

    }

    protected virtual void Update()
    {
        timer += Time.deltaTime;

    }

    protected virtual void CastSpell(int tier)
    {

    }

    public virtual void Shoot(Vector3 direction,GameObject attacker)
    {
        charAttacker = attacker;

    }

    protected void SetDataFromSpellContainer()
    {
        spellData.SetTierData(tier);
        damage = spellData.currentTierData.damage;
        statusEffectTimer = spellData.currentTierData.statusEffectTimer;
        statusEffectDamage = spellData.currentTierData.statusEffectDamage;
        statusEffectChance = spellData.currentTierData.statusEffectChance;

        range = spellData.currentTierData.range;
        duration = spellData.currentTierData.duration;
        cooldown = spellData.currentTierData.cooldown;
        projectileSpeed = spellData.currentTierData.speed;
        radius = spellData.currentTierData.radius;
    }
    public float ReturnDuration()
    {
        return duration;
    }

    protected void SetStatusEffect(GameObject target)
    {
        if (target.GetComponentInChildren<StatusEffect>())
        {
            Destroy(target.GetComponentInChildren<StatusEffect>().gameObject);
        }

        if (Random.value <= statusEffectChance)
        {
            StatusEffect effect = Instantiate(statusEffect, target.transform);
            effect.ActivateStatusEffect(statusEffectTimer, statusEffectDamage);
        }

    }


    public enum castType
    {
        groundPos,
        skyToGroundPos,
        self,
        projectile
    }
}
