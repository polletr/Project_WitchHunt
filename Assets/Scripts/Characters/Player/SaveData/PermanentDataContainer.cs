using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu]

public class PermanentDataContainer : ScriptableObject
{
    public int totalSouls;
    public List<SpellBook> spellBooksUnlocked;
    public int rune = 0;
    public float cooldownReduction = 0;
    public float healthBonus;
    public int baseAttackTier = 1;
    public BaseSpellBook prefBaseSpell;
    public float templeSoulsDropRate;


}



