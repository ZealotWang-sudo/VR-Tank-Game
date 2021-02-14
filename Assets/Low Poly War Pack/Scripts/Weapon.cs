using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(menuName = "PolyPerfect/New Weapon")]
public class Weapon : ScriptableObject
{
    [Tooltip("How many shots can this weapon do before needing to reload")]
    public int magazine;

    [Tooltip("How much damage will this weapon do")]
    public float weaponDamage;

    [Tooltip("How fast this weapon fires")]
    public float fireRate;
}

