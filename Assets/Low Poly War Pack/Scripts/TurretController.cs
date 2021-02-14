using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TurretController : MonoBehaviour
{
    [System.Serializable]
    public class WeaponSetUp
    {
        public float speed = 1;
        public float turnSpeed = 180f;
        public float headRot = 2f;
        public float turretRot = 2f;
        public GameObject GunHead;
        public GameObject GunTurret;
        public GameObject bullet;

        [HideInInspector]
        public GameObject player;

        public Transform playerSlot;

        public float bulletDamage;

        public KeyCode stopInteractingKey;

    }

    public WeaponSetUp weaponSetUp;
    public float health;

    public Camera cam;
    Animator anim;
    float Horizontal;
    Quaternion HeadRot;
    bool hideCursor = true;

    bool interactingWith;

	public bool autoFire;
	float time;
	public float shotTime = 2f;

    public bool canGetOut;

    // Use this for initialization
    void Start()
    {
        cam.enabled = false;
        anim = GetComponent<Animator>();
        HeadRot = weaponSetUp.GunHead.transform.rotation;
    }

    // Update is called once per frame
    void Update()
    {
		if (interactingWith) {
			Horizontal = Input.GetAxis ("Horizontal");

			if (Input.GetKeyDown (KeyCode.Mouse0)) {
				FireGun ();
			}

			HideCursor ();

			if (Input.GetKeyDown (weaponSetUp.stopInteractingKey) && canGetOut)
            {
				StopInteracting ();
			}
		}
		else 
		{
			if (autoFire) 
			{
				time += Time.deltaTime;

				if (time > shotTime) 
				{
					FireGun();
					time = 0;
				}
			}
		}
    }

    void FixedUpdate()
    {
        if (interactingWith)
        {
            Turn();
            RotateTurret();
        }
    }

    void Turn()
    {
        float turn = Horizontal * weaponSetUp.turnSpeed * Time.deltaTime;
        Quaternion turnRot = Quaternion.Euler(0f, turn, 0f);
        weaponSetUp.GunHead.transform.rotation = weaponSetUp.GunHead.transform.rotation * turnRot;
    }

    void RotateTurret()
    {
        float yRot = Input.GetAxis("Mouse Y") * weaponSetUp.turretRot;
        HeadRot = Quaternion.Euler(yRot, 0f, 0f);
        weaponSetUp.GunTurret.transform.rotation *= HeadRot;
    }

    void FireGun()
    {
        anim.SetTrigger("Fire");
        GameObject BulletClone = Instantiate(weaponSetUp.bullet, weaponSetUp.GunTurret.transform.GetChild(0).GetChild(0).transform.position, weaponSetUp.GunTurret.transform.GetChild(0).GetChild(0).transform.rotation);
        BulletClone.AddComponent<Rigidbody>();
        BulletClone.GetComponent<Rigidbody>().AddForce(weaponSetUp.GunTurret.transform.GetChild(0).GetChild(0).transform.forward * 100, ForceMode.Impulse);
        BulletClone.transform.parent = null;

    }

    void HideCursor()
    {
        if (!hideCursor)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

            if (Input.GetMouseButtonUp(0))
            {
                hideCursor = true;
            }
        }

        else
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;

            if (Input.GetKeyUp(KeyCode.Escape))
            {
                hideCursor = false;
            }
        }
    }

    public void TakeDamage(float ammount)
    {
        if (health <= 0)
        {
            BlowUp();
        }

        health -= ammount;
    }

    void BlowUp()
    {
        Destroy(this.gameObject);
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Player")
        {
            other.GetComponent<SoldierController>().canInteract = true;
            other.GetComponent<SoldierController>().interactingObject = gameObject;
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.tag == "Player")
        {
            other.GetComponent<SoldierController>().canInteract = false;
            other.GetComponent<SoldierController>().interactingObject = null;
        }
    }

    public void SortPlayer(GameObject player)
    {
        cam.enabled = true;
        weaponSetUp.player = player;
        player.transform.parent = weaponSetUp.playerSlot;
        player.transform.position = weaponSetUp.playerSlot.position;

        player.SetActive(false);

        interactingWith = true;
        StartCoroutine(CoolDownGetInButton());
    }

    void StopInteracting()
    {
        weaponSetUp.player.transform.parent = null;
        interactingWith = false;
        anim.SetFloat("Speed", 0);
        weaponSetUp.player.SetActive(true);
    }

    IEnumerator CoolDownGetInButton()
    {
        yield return new WaitForSeconds(.2f);
        canGetOut = true;
    }

}
