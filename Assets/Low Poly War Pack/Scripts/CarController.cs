using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class CarController : MonoBehaviour
{
    [System.Serializable]
    public class CarSetUp
    {
        public float speed = 1;
        public float turnSpeed = 180f;
        public float headRot = 2f;
        public GameObject camRot;
        public Camera camera;

        [HideInInspector]
        public GameObject player;

        public Transform playerSlot;

        public Transform getOutPoint;

        public KeyCode getOutKey;
    }
    public CarSetUp carSetUp;
    public float health;

    Rigidbody rb;
    float vertical;
    float Horizontal;
    Quaternion HeadRot;
    Quaternion turretRot;
    bool hideCursor = true;
    bool interactingWith = false;


    public bool canGetOut;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        carSetUp.camera.enabled = false;
        rb.isKinematic = false;
    }

    void Update()
    {
        if (interactingWith)
        {
            HideCursor();
            vertical = -Input.GetAxis("Vertical");
            Horizontal = Input.GetAxis("Horizontal");

            if (Input.GetKeyDown(carSetUp.getOutKey) && canGetOut)
            {
                GetOutOfJeep();
            }
        }
    }
    void FixedUpdate()
    {
        if (interactingWith)
        {
            Move();
            Turn();
            LookAround();
        }
    }
    void Move()
    {
        Vector3 movement = transform.forward * vertical * carSetUp.speed * Time.deltaTime;
        rb.MovePosition(rb.position + movement);
    }
    void Turn()
    {
        float turn = Horizontal * carSetUp.turnSpeed * Time.deltaTime;
        Quaternion turnRot = Quaternion.Euler(0f, turn, 0f);
        rb.MoveRotation(rb.rotation * turnRot);
    }

    void LookAround()
    {
        float xRot = Input.GetAxis("Mouse X") * carSetUp.headRot;

        HeadRot = Quaternion.Euler(0, xRot, 0f);
        carSetUp.camRot.transform.rotation *= HeadRot;
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
        carSetUp.camera.enabled = true;
        carSetUp.player = player;
        carSetUp.player.transform.parent = carSetUp.playerSlot;
        carSetUp.player.transform.position = carSetUp.playerSlot.position;
        carSetUp.player.SetActive(false);
        rb.isKinematic = false;

        interactingWith = true;
        StartCoroutine(CoolDownGetInButton());
    }

    void GetOutOfJeep()
    {
        rb.isKinematic = true;
        rb.velocity = Vector3.zero;
        carSetUp.player.transform.parent = null;
        carSetUp.player.transform.position = carSetUp.getOutPoint.position;
		//The player will reset to world 0,0,0 rotation
		carSetUp.player.transform.rotation = Quaternion.Euler (Vector3.zero);
        interactingWith = false;
        rb.isKinematic = true;
        carSetUp.player.SetActive(true);
    }

    IEnumerator CoolDownGetInButton()
    {
        yield return new WaitForSeconds(.2f);
        canGetOut = true;
    }

}
