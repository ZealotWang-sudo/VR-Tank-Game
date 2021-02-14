using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponController : MonoBehaviour
{
	[Tooltip("This key you want to ")]
    public KeyCode weaponSwitchKey = KeyCode.Alpha1;

	[Tooltip("This must be the same name as the one used in the animator for it to work")]
    public string weaponId;

	[Tooltip("The weapon Scriptable object that you can make, right click in the project and click (Create/PolyPerfect/NewWeapon")]
    public Weapon weapon;

	[Tooltip("The tip of the weapon, make an empty gameobject and move it to the end of the weapon")]
    public Transform weaponEnd;

	[Tooltip("So that left hand can reach to the IK point, makes it look like it is holding the weapon")]
    public Transform leftHandPosition;

	[Tooltip("The weapon flash that will appear at the end of the gun")]
    public GameObject weaponFlash;

	[Tooltip("How fast to change the weapon to the next chosen one")]
    public float weaponSwitchTime = 0.8f;

    [Tooltip("If you put a gameobject in this slot, it will be fired out of the end of the weapon")]
    public GameObject Projecticle;
	[Tooltip("If you put a gameobject in this slot, it will be spawned at the position of the hit")]
	public GameObject hitEffect;

	[Tooltip("How fast the Projectile will travel")]
    public float projectileSpeed;

	[Tooltip("How fast the weapon will reload")]
    public float reloadSpeed = 2f;

    [Tooltip("The sound the weapon will make when shot")]
    public AudioClip weaponSound;

	[Tooltip("Will the weapon damage around the point of impact e.g. Bazzoka")]

    public bool areaDamage;
	[Tooltip("How far away from the point of impact will the damage effect")]
    public float areaRadius;

	[Tooltip("This is used to make the weapon point in the correct direction")]
	public Vector3 weaponIKOffset;
}

