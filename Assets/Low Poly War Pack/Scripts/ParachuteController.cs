using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[RequireComponent(typeof(Rigidbody))]
public class ParachuteController : MonoBehaviour
{
    Rigidbody rBody;

    Animator anim;

    [System.Serializable]
    public class CameraSetUp
    {
        public GameObject camera;

        public GameObject cameraSpinnerX;
    }

    public CameraSetUp cameraSetUp;

    [System.Serializable]
    public class ParachuteSetUp
    {
        public float windResistance = 5;

        public float speed;
        public float turnSpeed = 20;
        public GameObject player;

        public Transform playerSlot;
        public GameObject ParachutePlayerModel;
    }
    public ParachuteSetUp parachuteSetUp;

    Quaternion rotationX;

    Quaternion rotationZ;

    float vertical;
    float horizontal;

    public bool grounded = false;

    SoldierController SoldierController;

    // Use this for initialization
    void OnEnable()
    {
        rBody = GetComponent<Rigidbody>();
        anim = GetComponentInChildren<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        vertical = Mathf.Lerp(vertical, Input.GetAxis("Vertical"), 1f * Time.deltaTime);
        horizontal = Mathf.Lerp(horizontal, Input.GetAxis("Horizontal"), 1f * Time.deltaTime);

        anim.SetFloat("Horizontal", horizontal);
        anim.SetFloat("Vertical", vertical);

        OpenParachute();

        //ControlParachute();

        LookAround();

    }

    public void CollidersEnabled(bool value)
    {
        var colliders = GetComponentsInChildren<Collider>();

        for (int i = 0; i < colliders.Length; i++)
        {
            if(!colliders[i].isTrigger)
            {
                colliders[i].enabled = value;
            }
        }
    }

    void LookAround()
    {
        float xRot = Input.GetAxis("Mouse X") * parachuteSetUp.turnSpeed;
        rotationX = Quaternion.Euler(0, xRot, 0f);
        transform.rotation *= rotationX;

        float zRot = Input.GetAxis("Mouse Y") * parachuteSetUp.turnSpeed;
        rotationZ = Quaternion.Euler(-zRot, 0, 0f);
        cameraSetUp.cameraSpinnerX.transform.rotation *= rotationZ;
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Ground")
        {
            StartCoroutine(DestroyParachute());
            parachuteSetUp.player.transform.parent = null;
            parachuteSetUp.player.transform.rotation = new Quaternion(0, -transform.rotation.y, 0, 0);
            parachuteSetUp.player.GetComponent<SoldierController>().canMove = true;
            parachuteSetUp.player.GetComponent<CharacterController>().enabled = true;
            parachuteSetUp.ParachutePlayerModel.SetActive(false);
            parachuteSetUp.player.SetActive(true);


        }
    }

    IEnumerator DestroyParachute()
    {
        yield return new WaitForSeconds(0.2f);
        Destroy(this.gameObject);
    }

    public void SortPlayer(GameObject player)
    {
        parachuteSetUp.player = player;
        parachuteSetUp.player.transform.parent = parachuteSetUp.playerSlot;
        parachuteSetUp.player.transform.position = parachuteSetUp.playerSlot.position;
        parachuteSetUp.player.GetComponent<SoldierController>().canMove = false;
        parachuteSetUp.ParachutePlayerModel.SetActive(true);
        parachuteSetUp.player.SetActive(false);

        parachuteSetUp.player.GetComponent<CharacterController>().enabled = false;
        cameraSetUp.camera.GetComponent<Camera>().enabled = true;
    }

    void OpenParachute()
    {
        rBody.drag = parachuteSetUp.windResistance;
    }

    void ControlParachute()
    {
        Vector3 movement = transform.GetChild(0).transform.forward * parachuteSetUp.speed * Time.deltaTime;
        rBody.MovePosition(rBody.position + movement);
    }
}
