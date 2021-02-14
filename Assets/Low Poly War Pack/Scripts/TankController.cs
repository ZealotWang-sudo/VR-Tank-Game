using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody), typeof(AudioSource))]
public class TankController : MonoBehaviour
{
    [System.Serializable]
    public class TankSetUp
    {
        public float speed = 1;
        public float turnSpeed = 180f;
        public float headRot = 2f;
        public float turretRot = 2f;
        public GameObject tankHead;
        public GameObject tankTurret;
        public GameObject bullet;

        [HideInInspector]
        public GameObject player;
        public Transform playerSlot;

        public Transform getOutPoint;
        public Camera Cam;

        public float bulletDamage;

        public bool particleBullets;

        public KeyCode getOutOfTank;

    }

    public TankSetUp tankSetUp;


    Rigidbody rb;

    [System.Serializable]
    public class AudioParams
    {
        //Audio Parameters
        public float min = 1f;
        public float max = -1f;
        public float newMin = 0.8f;
        public float newMax = 1.3f;
    }

    public AudioParams audioParams;


    //Input Parameters
    float vertical;
    float horizontal;
    public float health;

    Quaternion HeadRot;
    Animator anim;
    Vector3 lastPosition = Vector3.zero;

    bool hideCursor = true;
    bool interactingWith = false;
    bool startedEngine = false;

    public AudioSource tankMovingSource;
    public AudioSource fireSource;
    public AudioClip fireSound;

    // Use this for initialization
    void Start()
    {
        Assignments();
        interactingWith = false;
        tankSetUp.Cam.enabled = false;
    }
    void Assignments()
    {
        rb = GetComponent<Rigidbody>();
        anim = GetComponent<Animator>();
        HeadRot = tankSetUp.tankHead.transform.rotation;
    }



    public bool canGetOut;
    // Update is called once per frame
    void Update()
    {
        if (interactingWith)
        {
            HideCursor();
            vertical = -Input.GetAxis("Vertical");
            horizontal = Input.GetAxis("Horizontal");

            if (Input.GetKeyDown(KeyCode.Mouse0))
            {
                FireGun();
            }

            if (Input.GetKeyDown(tankSetUp.getOutOfTank) && canGetOut)
            {
                GetOutOfTank();
            }

            Sound();
        }
    }

    void GetOutOfTank()
    {
        tankSetUp.player.transform.parent = null;
        tankSetUp.player.transform.position = tankSetUp.getOutPoint.position;
		//The player will reset to world 0,0,0 rotation
		tankSetUp.player.transform.rotation = Quaternion.Euler (Vector3.zero);
        interactingWith = false;
        anim.SetFloat("Speed", 0);
        tankSetUp.player.SetActive(true);
        tankMovingSource.Stop();
        canGetOut = false;
    }

    IEnumerator CoolDownGetInButton()
    {
        yield return new WaitForSeconds(.2f);
        canGetOut = true;
    }

    void FireGun()
    {
        anim.SetTrigger("Fire");
        GameObject BulletClone = Instantiate(tankSetUp.bullet, tankSetUp.tankTurret.transform.GetChild(0).transform.position, tankSetUp.tankTurret.transform.GetChild(0).transform.rotation);
        BulletClone.AddComponent<Rigidbody>();
        BulletClone.GetComponent<Bullets>().instantiateParticles = tankSetUp.particleBullets;
        BulletClone.GetComponent<Rigidbody>().AddForce(tankSetUp.tankTurret.transform.GetChild(0).transform.forward * 100, ForceMode.Impulse);
        BulletClone.transform.parent = null;
    }
    void FixedUpdate()
    {
        if (interactingWith)
        {
            Move();
            Turn();
            RotateXTankHead();
            RotateYTankTurret();
        }
    }

    void Sound()
    {
        if (interactingWith)
        {
            if (!startedEngine)
            {
                tankMovingSource.Play();
                startedEngine = true;
            }

            if (startedEngine)
            {
                var audioPitch = vertical - Mathf.Abs(horizontal);

				tankMovingSource.pitch = audioPitch.Remap(audioParams.min, audioParams.max, audioParams.newMin, audioParams.newMax);
            }
        }
    }

    void Move()
    {
        Vector3 movement = transform.forward * vertical * tankSetUp.speed * Time.deltaTime;
        rb.MovePosition(rb.position + movement);

        float currentSpeed = Mathf.Abs((((transform.position - lastPosition).magnitude) / Time.deltaTime));

        lastPosition = transform.position;
		currentSpeed.Remap (0, tankSetUp.speed, 0, 2);

        if (vertical < 0.1)
        {
			anim.SetFloat("Speed", currentSpeed);
        }

        if (vertical > -0.1)
        {
            anim.SetFloat("Speed", -currentSpeed);
        }

        if (horizontal >= 0.1)
        {
            anim.SetFloat("Speed", -1);
        }

        if (horizontal <= -0.1)
        {
            anim.SetFloat("Speed", 1);
        }
    }

    void Turn()
    {
        float turn = horizontal * tankSetUp.turnSpeed * Time.deltaTime;
        Quaternion turnRot = Quaternion.Euler(0f, turn, 0f);
        rb.MoveRotation(rb.rotation * turnRot);
    }

    void RotateXTankHead()
    {
        float xRot = Input.GetAxis("Mouse X") * tankSetUp.headRot;
        HeadRot = Quaternion.Euler(0, xRot, 0f);
        tankSetUp.tankHead.transform.rotation *= HeadRot;
    }
    void RotateYTankTurret()
    {
        float yRot = Input.GetAxis("Mouse Y") * tankSetUp.turretRot;
        HeadRot = Quaternion.Euler(yRot, 0f, 0f);
        tankSetUp.tankTurret.transform.rotation *= HeadRot;
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

        Debug.Log(" Take Damage");
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
        tankSetUp.Cam.GetComponent<Camera>().enabled = true;
        tankSetUp.player = player;
        tankSetUp.player.transform.parent = tankSetUp.playerSlot;
        tankSetUp.player.transform.position = tankSetUp.playerSlot.position;
        tankSetUp.player.SetActive(false);

        tankSetUp.Cam.enabled = true;
        interactingWith = true;
        StartCoroutine(CoolDownGetInButton());
    }
}
