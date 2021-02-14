using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.AI;

public class SoldierEnemy : MonoBehaviour
{
	[Tooltip("The health of the soldier")]
    public float health;
	float maxHealth;
	
	[Tooltip("The health bar if you want one above the soldiers head")]
    public Image healthBar;
	
	[Tooltip("Do you want the ragdoll to be enabled when it dies?")]
	public bool ragdollDeath;

	public string weaponId;
	public WeaponController [] weapons;

	public AudioSource audioSource;

	WeaponController currentWeapon;
	SoldierController playerScript;

    Animator anim;
    Rigidbody[] rigidbodys;
	Collider [] colldiers;
	float time;

	[Range(0,1), Tooltip("1 = full damage of the weapon, 0.5 = Half damage of weapon, 0 = No Damage")]
	public float difficultyMultiplier = .5f;

    // Use this for initialization
    void Start()
    {
		// Make the weapon visable
		foreach (var item in weapons)
		{
			if (item.weaponId == weaponId) 
			{
				currentWeapon = item;
				item.gameObject.SetActive (true);
			} 
			else 
			{
				item.gameObject.SetActive (false);

			}
		}

		maxHealth = health;

		playerScript = GameObject.FindGameObjectWithTag ("Player").GetComponent<SoldierController>();

        rigidbodys = GetComponentsInChildren<Rigidbody>();

        foreach (Rigidbody item in rigidbodys)
        {
            item.mass = 0f;
        }


        anim = GetComponent<Animator>();
    }

    void OnTriggerStay(Collider other)
    {
		if (other.tag == "Player" && health > 0 && playerScript.soldierSetUp.health > 0)
        {
			anim.SetBool (weaponId, true);
			transform.LookAt (playerScript.transform);
			time += Time.deltaTime;

			if (time > currentWeapon.weapon.fireRate) 
			{
				ShootPlayer ();
				time = 0f;
			}
        }
    }

	void OnTriggerExit()
	{
		anim.SetBool (weaponId, false);
	}

	[ContextMenu("ShootPlayer")]
	public void ShootPlayer()
	{
		anim.SetTrigger ("Fire");

		playerScript.DamageHealth(Mathf.Round(currentWeapon.weapon.weaponDamage * difficultyMultiplier));

		Fire ();
	}
	void Fire()
	{
		var cWeapon = currentWeapon;

		if(cWeapon.weaponFlash != null)
		{        
			//Move the weapon flash to the correct location
			cWeapon.weaponFlash.transform.parent = cWeapon.weaponEnd.transform;
			cWeapon.weaponFlash.transform.position = cWeapon.weaponEnd.transform.position;
			cWeapon.weaponFlash.transform.rotation = cWeapon.weaponEnd.transform.rotation;
			cWeapon.weaponFlash.GetComponentInChildren<Animator>().SetTrigger("Fire");
		}

		anim.SetTrigger("Fire");

		if (audioSource != null) 
		{   
			audioSource.PlayOneShot (cWeapon.weaponSound);
		}
	}


    public void DamageHealth(float ammount)
    {
        health -= ammount;

        anim.SetTrigger("TakeHit");

        if (healthBar != null)
        {
			//Make it so that the health bar can be above 100
			healthBar.fillAmount = health / maxHealth;
        }


        if (health <= 0)
        {
            anim.SetTrigger("Death");
            if (ragdollDeath)
            {
				foreach (Rigidbody item in rigidbodys)
				{
					item.mass = 10;
				}
                anim.enabled = false;
				this.enabled = false;
            }
        }
    }
}
