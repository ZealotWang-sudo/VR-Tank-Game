
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody), typeof(AudioSource))]
public class PlaneController : MonoBehaviour
{
    [System.Serializable]
    public class PlaneSetUp
    {

        public float currentSpeed = 20f;
        public float maxSpeed = 90f;
        public float minSpeed = 0f;
        public float turnSpeed = 180f;
        public float lookSpeed = 2f;
        public float breakSpeed;
        public float accelerationSpeed;
        public float health;
        public float bulletDamage;

        public GameObject cam;
        public GameObject bullet;
        [HideInInspector]
        public GameObject player;

        public Transform playerSlot;
        public Transform[] firePoints;
        public Transform planeNose;
        public Transform getOutPoint;

        public GameObject particleSystem;
        public GameObject parachute;

    }

    public PlaneSetUp planeSetUp;


    Animator anim;
    Rigidbody rb;

    Quaternion camRot;

    RaycastHit hit;

    public AudioSource planeFlying;
    public AudioSource fireSource;
    public AudioClip fireSound;

    //Audio Parameters
    float min = 5f;
    float max = -5f;
    float newMin = 0.8f;
    float newMax = 1.3f;

    //Input Parameters
    float vertical;
    float horizontal;


    public float groundCheckHeight;
    public float preGroundCheckHeight;
    float audioPitch;


    bool accelerating = false;
    bool startedEngine = false;
    bool canLand;
    public bool bulletExplosion;

    public KeyCode GetOutKey;

    bool hideCursor = true;
    bool interactingWith, hasFlown;

    public bool canGetOut;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.useGravity = false;
        anim = GetComponentInChildren<Animator>();
        planeSetUp.cam.transform.GetChild(0).GetComponent<Camera>().enabled = false;
        rb.isKinematic = true;
    }

    void Update()
    {
        if (interactingWith)
        {
            hasFlown = true;

            var inputHorizontal = Input.GetAxis("Mouse X");
            var inputVertical = Input.GetAxis("Mouse Y");
            //MouseInput
            vertical = inputVertical;
            horizontal = inputHorizontal;

            anim.SetFloat("Speed", planeSetUp.currentSpeed.Remap(0, planeSetUp.maxSpeed, .1f, 2f));

            if (Input.GetKeyDown(KeyCode.Mouse0))
            {
                FireGun();
                if (Input.GetMouseButton(0))
                {
                    InvokeRepeating("FireGun", 1, 0.2f);
                }
            }

            if (Input.GetMouseButtonUp(0))
            {
                CancelInvoke();
            }

            HideCursor();
            Tricks();
            Sound();

            anim.SetBool("LegsAway", !GroundCheck(preGroundCheckHeight));

            if (Input.GetKeyDown(GetOutKey) && canGetOut)
            {
                GetOutOfPlane();
            }
        }
    }

    void Sound()
    {
        if (interactingWith)
        {
            if (!startedEngine)
            {
                planeFlying.Play();
                startedEngine = true;
            }

            if (startedEngine)
            {
                audioPitch = Mathf.Abs(horizontal) - vertical;

                planeFlying.pitch = audioPitch.Remap(min, max, newMin, newMax);
            }
        }
    }

    void GetOutOfPlane()
    {
        planeSetUp.player.transform.parent = null;
        planeSetUp.player.transform.position = planeSetUp.getOutPoint.position;
        //The player will reset to 0,0,0 rotation
        planeSetUp.player.transform.rotation = Quaternion.Euler(Vector3.zero);

        planeSetUp.player.SetActive(true);
        interactingWith = false;
        planeSetUp.cam.transform.GetChild(0).GetComponent<Camera>().enabled = false;
    }

    IEnumerator DestroyPlane()
    {
        yield return new WaitForSeconds(1f);

        Destroy(this.gameObject);
    }

    void FireGun()
    {
        foreach (Transform item in planeSetUp.firePoints)
        {
            GameObject BulletClone = Instantiate(planeSetUp.bullet, item.position, item.rotation);

            if (!BulletClone.GetComponent<Bullets>())
            {
                BulletClone.AddComponent<Bullets>();
            }

            BulletClone.GetComponent<Bullets>().instantiateParticles = bulletExplosion;

            if (!BulletClone.GetComponent<Rigidbody>())
            {
                BulletClone.AddComponent<Rigidbody>();
            }

            BulletClone.GetComponent<Rigidbody>().AddForce(item.transform.forward * 100, ForceMode.Impulse);

            BulletClone.transform.parent = null;
            BulletClone.transform.localScale = new Vector3(1, 1, 1);



            BulletClone.AddComponent<Bullets>();

            BulletClone.GetComponent<Bullets>().instantiateParticles = bulletExplosion;

            if (!BulletClone.GetComponent<Rigidbody>())
            {
                BulletClone.AddComponent<Rigidbody>();
            }

            BulletClone.GetComponent<Rigidbody>().AddForce(item.transform.forward * 100, ForceMode.Impulse);

            BulletClone.transform.parent = null;
            BulletClone.transform.localScale = new Vector3(1, 1, 1);


            fireSource.PlayOneShot(fireSound);
        }
    }

    void FixedUpdate()
    {
        if (interactingWith)
        {
            Move();
        }

        if (!interactingWith & hasFlown & !canLand)
        {
            Crash();
        }
    }

    void Tricks()
    {
        if (horizontal >= 0.2f && Input.GetKeyDown(KeyCode.E))
        {
            anim.SetTrigger("barrelRollL");
        }

        if (horizontal <= -0.2f && Input.GetKeyDown(KeyCode.Q))
        {
            anim.SetTrigger("barrelRollR");
        }
    }

    void Crash()
    {
        planeSetUp.currentSpeed -= Time.deltaTime * planeSetUp.accelerationSpeed;
        planeSetUp.currentSpeed = Mathf.Clamp(planeSetUp.currentSpeed, planeSetUp.minSpeed, planeSetUp.maxSpeed);
        Vector3 movement = planeSetUp.planeNose.forward * planeSetUp.currentSpeed * Time.deltaTime;
        rb.MovePosition(rb.position + movement);

        if (!startTimer)
        {
            startTimer = true;
            StartCoroutine(CrashTime());
        }

    }

    bool startTimer = false;

    IEnumerator CrashTime()
    {
        var parachute = Instantiate(planeSetUp.parachute, transform.position, Quaternion.identity);
        parachute.GetComponent<ParachuteController>().SortPlayer(planeSetUp.player);
        parachute.GetComponent<ParachuteController>().CollidersEnabled(false);


        yield return new WaitForSeconds(Random.Range(2, 5));

        if (planeSetUp.particleSystem != null)
        {
            Instantiate(planeSetUp.particleSystem, transform.position, Quaternion.identity);
        }

        Destroy(this.gameObject);
    }

    void Move()
    {
        if (Input.GetKey(KeyCode.W))
        {
            accelerating = true;
            planeSetUp.currentSpeed += Time.deltaTime * planeSetUp.accelerationSpeed;
        }
        else
        {
            accelerating = false;
        }

        planeSetUp.currentSpeed = Mathf.Clamp(planeSetUp.currentSpeed, planeSetUp.minSpeed, planeSetUp.maxSpeed);
        Vector3 movement = planeSetUp.planeNose.forward * planeSetUp.currentSpeed * Time.deltaTime;
        rb.MovePosition(rb.position + movement);

        var turnSpeed = planeSetUp.currentSpeed.Remap(planeSetUp.minSpeed, planeSetUp.maxSpeed, planeSetUp.turnSpeed / 4, planeSetUp.turnSpeed);
        float turn = horizontal * turnSpeed * Time.deltaTime;

        Quaternion turnRot = Quaternion.Euler(0f, turn, 0f);
        rb.MoveRotation(rb.rotation * turnRot);

        //Makes the animator controls a little smoother
        var inputDamping = planeSetUp.currentSpeed.Remap(planeSetUp.minSpeed, planeSetUp.maxSpeed, 2, 1f);

        anim.SetFloat("Horizontal", horizontal, inputDamping, Time.deltaTime);
        anim.SetFloat("Vertical", -vertical, inputDamping, Time.deltaTime);

        canLand = GroundCheck(groundCheckHeight);

        if (!canLand)
        {
            var minSpeed = (rb.mass / planeSetUp.currentSpeed);

            if (Input.GetKey(KeyCode.S) && planeSetUp.currentSpeed > minSpeed)
            {
                planeSetUp.currentSpeed -= Time.deltaTime * planeSetUp.accelerationSpeed;
                planeSetUp.currentSpeed = Mathf.Clamp(planeSetUp.currentSpeed, minSpeed, planeSetUp.maxSpeed);
            }
        }
        else
        {
            if (!accelerating)
            {
                planeSetUp.currentSpeed -= Time.deltaTime * planeSetUp.breakSpeed;
            }

            anim.SetFloat("Horizontal", Mathf.Lerp(anim.GetFloat("Horizontal"), 0, Time.deltaTime));
            anim.SetFloat("Vertical", Mathf.Clamp(anim.GetFloat("Vertical"), 0, 1));
        }
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
        if (planeSetUp.health <= 0)
        {
            BlowUp();
        }

        planeSetUp.health -= ammount;
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
        planeSetUp.player = player;
        player.transform.parent = planeSetUp.playerSlot;
        player.transform.position = planeSetUp.playerSlot.position;
        player.SetActive(false);

        planeSetUp.cam.transform.GetChild(0).GetComponent<Camera>().enabled = true;

        interactingWith = true;
        rb.isKinematic = true;
        StartCoroutine(CoolDownGetInButton());
    }

    bool GroundCheck(float height)
    {
        RaycastHit hit;

        Debug.DrawRay((transform.position + transform.forward * 2), Vector3.down * height);

        if (Physics.Raycast((transform.position + transform.forward * 2), Vector3.down, out hit, height))
        {
            return true;
        }
        else
        {
            return false;

        }
    }

    IEnumerator CoolDownGetInButton()
    {
        yield return new WaitForSeconds(.2f);
        canGetOut = true;
    }
}

