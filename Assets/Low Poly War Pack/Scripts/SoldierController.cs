using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoldierController : MonoBehaviour
{
    [System.Serializable]
    public class SoliderSetUp
    {
        public float speed;

        [Space(10)]
        public float turnSpeed;

        [Space(10)]
        public float health;
        public KeyCode interactionKeyCode;
        public List<SoldierUI_ID> uiGuns = new List<SoldierUI_ID>();
        public UnityEngine.UI.Text healthText;
    }

    public SoliderSetUp soldierSetUp;
    public WeaponController[] weapons;
    public int currentWeapon;

    [System.Serializable]
    public class CameraSetUp
    {
        public GameObject camera;
        public GameObject cameraSpinnerX;
        public Vector3 oldPosition;
        public Vector3 newPosition;
    }
    public CameraSetUp cameraSetUp;

    [System.Serializable]

    public class IkSetUp
    {
        public bool ik = false;

        public GameObject ikAimPos;
    }

    [SerializeField]
    IkSetUp ikSetUp;

    float vertical;
    float horizontal;

    Quaternion rotationX;
    Quaternion rotationZ;

    public GameObject interactingObject;
    Ray ray;
    AudioSource audioSource;
    Animator anim;
    bool canShoot = true;

    [HideInInspector]
    public bool shotHappend = false;
    public bool canMove = true;
    public bool canInteract = true;
    bool isRunning;
    bool hideCursor = true;
    bool interacting = false;

    Transform leftHandTarget;

    public UnityEngine.UI.Text maxAmmo;
    public UnityEngine.UI.Text currentAmmo;

    public bool parachuting;
    public float parachuteLandingHeight;

    // Use this for initialization
    void Start()
    {
        anim = GetComponent<Animator>();

        currentWeapon = 0;
        anim.SetBool(weapons[currentWeapon].weaponId, true);
        weapons[currentWeapon].gameObject.SetActive(true);
        audioSource = transform.GetComponent<AudioSource>();

        LeftHandIk();

        SortUI();
    }

    void SortUI()
    {
        foreach (var item in soldierSetUp.uiGuns)
        {
            if (item.ID == weapons[currentWeapon].weaponId)
            {
                item.Active(true);
            }

            else
            {
                item.Active(false);
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (canMove)
        {
            if (maxAmmo != null && currentAmmo != null)
            {
                maxAmmo.text = weapons[currentWeapon].weapon.magazine.ToString();
                currentAmmo.text = (weapons[currentWeapon].weapon.magazine - shotCount).ToString();
            }

            if (!interacting)
            {
                vertical = Input.GetAxis("Vertical");
                horizontal = Input.GetAxis("Horizontal");

                Move(vertical, horizontal);
                LookAround();
                HideCursor();
                WeaponSwitch();

                if (Input.GetKey(KeyCode.LeftShift))
                {
                    isRunning = true;
                }

                else
                {
                    isRunning = false;
                }

                if (Input.GetKeyDown(KeyCode.R))
                {
                    anim.SetTrigger("Reload");
                }
            }

            if (transform.parent == null)
            {
                interacting = false;
            }

            if (canInteract && interactingObject != null)
            {
                if (Input.GetKeyDown(soldierSetUp.interactionKeyCode))
                {
                    if (interactingObject.GetComponent<PlaneController>() && interactingObject.GetComponent<PlaneController>().enabled)
                    {
                        interacting = true;
                        interactingObject.GetComponent<PlaneController>().SortPlayer(gameObject);
                    }

                    if (interactingObject.GetComponent<TankController>() && interactingObject.GetComponent<TankController>().enabled)
                    {
                        interacting = true;
                        interactingObject.GetComponent<TankController>().SortPlayer(gameObject);
                    }

                    if (interactingObject.GetComponent<CarController>() && interactingObject.GetComponent<CarController>().enabled)
                    {
                        interacting = true;
                        interactingObject.GetComponent<CarController>().SortPlayer(gameObject);
                    }

                    if (interactingObject.GetComponent<TurretController>() && interactingObject.GetComponent<TurretController>().enabled)
                    {
                        interacting = true;
                        interactingObject.GetComponent<TurretController>().SortPlayer(gameObject);
                    }
                }
            }
        }
    }

    void Parachute()
    {
        //Check if you are close to the floor
        parachuting = !GroundCheck(parachuteLandingHeight);
        
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

    void WeaponSwitch()
    {
        for (int i = 0; i < weapons.Length; i++)
        {
            if (weapons[i] == weapons[currentWeapon])
            {
                continue;
            }

            if (Input.GetKeyDown(weapons[i].weaponSwitchKey))
            {
                Debug.Log(weapons[i].weaponSwitchKey + ": - If the keyPad buttons do not work for you, change it to whatever you want to use. You can do this by clicking on the gun and changing the keycode");

                StartCoroutine(WaitForChangeWeapon(weapons[currentWeapon].weaponSwitchTime, currentWeapon));
                currentWeapon = i;
                shotCount = 0;
                anim.ResetTrigger("Fire");
                SortAnimator();
            }
        }
    }

    void SortAnimator()
    {
        foreach (var item in weapons)
        {
            //If it is not the new weapons ID
            if (item.weaponId != weapons[currentWeapon].weaponId)
            {
                anim.SetBool(item.weaponId, false);
            }

            //if it is the new weapons ID
            else
            {
                anim.SetBool(item.weaponId, true);
            }
        }

        SortUI();
    }

    IEnumerator WaitForChangeWeapon(float time, int oldValue)
    {
        yield return new WaitForSeconds(time);
        LeftHandIk();
        weapons[oldValue].gameObject.SetActive(false);
        weapons[currentWeapon].gameObject.SetActive(true);
    }

    void LeftHandIk()
    {
        if (weapons[currentWeapon].leftHandPosition != null)
        {
            leftHandTarget = weapons[currentWeapon].leftHandPosition;
        }
    }

    void LateUpdate()
    {
        if (canMove)
        {
            if (Input.GetMouseButton(1))
            {
                Aim(true);
            }

            else
            {
                Aim(false);
            }
        }
    }

    void Move(float vertical, float horizontal)
    {
        anim.SetBool("isRunning", false);
        anim.SetFloat("Vertical", vertical * soldierSetUp.speed);
        anim.SetFloat("Horizontal", horizontal * soldierSetUp.speed);

        if (isRunning)
        {
            anim.SetBool("isRunning", true);
            anim.SetFloat("Vertical", vertical * soldierSetUp.speed);
            anim.SetFloat("Horizontal", horizontal * soldierSetUp.speed);
        }
    }

    void LookAround()
    {
        float xRot = Input.GetAxis("Mouse X") * soldierSetUp.turnSpeed;
        rotationX = Quaternion.Euler(0, xRot, 0f);

        //cameraSetUp.cameraSpinnerY.transform.rotation *= rotationX;

        transform.rotation *= rotationX;

        float zRot = Input.GetAxis("Mouse Y") * soldierSetUp.turnSpeed;
        rotationZ = Quaternion.Euler(-zRot, 0, 0f);
        cameraSetUp.cameraSpinnerX.transform.rotation *= rotationZ;
    }

    void OnAnimatorIK()
    {
        if (ikSetUp.ik)
        {
            if (ikSetUp.ikAimPos != null)
            {
                //We add an offset to the ik so that we can make the gun aim in the correct location
                anim.SetLookAtPosition(ikSetUp.ikAimPos.transform.position);
                anim.SetLookAtWeight(1, 1, 1, .2f);
            }

            else
            {
                Debug.LogError("Ik AimPosition is not assigned");
            }

            if (weapons.Length > 0)
            {
                if (leftHandTarget != null)
                {
                    anim.SetIKPosition(AvatarIKGoal.LeftHand, leftHandTarget.position);
                    anim.SetIKRotation(AvatarIKGoal.LeftHand, leftHandTarget.rotation);
                    anim.SetIKPositionWeight(AvatarIKGoal.LeftHand, 1);
                    anim.SetIKRotationWeight(AvatarIKGoal.LeftHand, 1);
                }

                else
                {
                    anim.SetIKPositionWeight(AvatarIKGoal.LeftHand, 0);
                    anim.SetIKRotationWeight(AvatarIKGoal.LeftHand, 0);
                }
            }

            else
            {
                Debug.LogError("Ik LeftHand is not assigned");
            }
        }

        else
        {
            anim.SetIKPositionWeight(AvatarIKGoal.LeftHand, 0);
        }
    }

    public void DamageHealth(float ammount)
    {
        soldierSetUp.health -= ammount;
        soldierSetUp.healthText.text = soldierSetUp.health.ToString();

        if (soldierSetUp.health <= 0)
        {
            soldierSetUp.healthText.text = "0";
            Death();
        }
    }

    float lastfired;

    void Aim(bool value)
    {
        anim.SetBool("Aiming", value);

        if (value)
        {
            anim.SetBool(weapons[currentWeapon].weaponId, true);

            LerpCamera(true);

            if (Input.GetMouseButton(0) && canShoot)
            {
                if (Time.time - lastfired > weapons[currentWeapon].weapon.fireRate)
                {
                    lastfired = Time.time;
                    Fire();
                }
            }
        }

        else
        {
            LerpCamera(false);
        }
    }

    void LerpCamera(bool value)
    {
        if (value)
        {
            var distance = Vector3.Distance(cameraSetUp.camera.transform.localPosition, cameraSetUp.camera.transform.localPosition + cameraSetUp.newPosition);

            if (distance > 0.1f)
            {
                cameraSetUp.camera.transform.localPosition = Vector3.Lerp(cameraSetUp.oldPosition, cameraSetUp.newPosition, Time.deltaTime * 0.1f);
            }
        }

        else
        {
            var distance = Vector3.Distance(cameraSetUp.newPosition, cameraSetUp.camera.transform.localPosition);

            if (distance > 0.1f)
            {
                cameraSetUp.camera.transform.localPosition = Vector3.Lerp(cameraSetUp.newPosition, cameraSetUp.oldPosition, Time.deltaTime * 0.1f);
            }
        }
    }

    int shotCount;

    void Fire()
    {
        var cWeapon = weapons[currentWeapon];

        if (weapons[currentWeapon].weaponFlash != null)
        {
            //Move the weapon flash to the correct location
            cWeapon.weaponFlash.transform.parent = cWeapon.weaponEnd.transform;
            cWeapon.weaponFlash.transform.position = cWeapon.weaponEnd.transform.position;
            cWeapon.weaponFlash.transform.rotation = cWeapon.weaponEnd.transform.rotation;
            cWeapon.weaponFlash.GetComponentInChildren<Animator>().SetTrigger("Fire");
        }

        anim.SetTrigger("Fire");

        audioSource.PlayOneShot(cWeapon.weaponSound);
        shotCount++;

        RaycastHit hit;

        if (Physics.Raycast(cameraSetUp.camera.transform.position, cameraSetUp.camera.transform.forward, out hit, Mathf.Infinity))
        {
            if (hit.collider.gameObject.GetComponent<SoldierEnemy>())
            {
                hit.collider.gameObject.GetComponent<SoldierEnemy>().DamageHealth(cWeapon.weapon.weaponDamage);
            }

            if (hit.collider.gameObject.GetComponent<TurretController>())
            {
                hit.collider.gameObject.GetComponent<TurretController>().TakeDamage(cWeapon.weapon.weaponDamage);
            }

            if (cWeapon.areaDamage)
            {
                DamageArea(hit.point, cWeapon.areaRadius, cWeapon.weapon.weaponDamage);
            }

            if (cWeapon.hitEffect != null)
            {
                Instantiate(cWeapon.hitEffect, hit.point, Quaternion.identity);
            }
        }

        if (cWeapon.Projecticle != null)
        {
            var clone = Instantiate(cWeapon.Projecticle,
                cWeapon.weaponEnd.position,
                cWeapon.weaponEnd.rotation);

            if (!clone.GetComponent<Rigidbody>())
            {
                clone.AddComponent<Rigidbody>();
            }

            Vector3 direction = (hit.point - cWeapon.weaponEnd.transform.position).normalized;

            clone.GetComponent<Rigidbody>().AddForce(direction * cWeapon.projectileSpeed);
        }

        shotHappend = true;

        if (shotCount >= cWeapon.weapon.magazine)
        {
            anim.SetTrigger("Reload");
            canShoot = false;
            shotCount = 0;

            StartCoroutine(WaitForReload());
        }
    }

    void DamageArea(Vector3 originPoint, float radius, float damage)
    {
        var collidersInRange = Physics.OverlapSphere(originPoint, radius);

        foreach (var item in collidersInRange)
        {
            if (item.GetComponent<SoldierEnemy>())
            {
                var distanceDivide = Vector3.Distance(item.transform.position, originPoint);

                item.GetComponent<SoldierEnemy>().DamageHealth(weapons[currentWeapon].weapon.weaponDamage / distanceDivide);
            }
        }
    }

    IEnumerator TurnOffShotHappend()
    {
        yield return new WaitForSeconds(weapons[currentWeapon].weaponSound.length);
        shotHappend = false;
    }

    IEnumerator WaitForReload()
    {
        yield return new WaitForSeconds(weapons[currentWeapon].reloadSpeed);
        canShoot = true;
    }

    void Death()
    {
        anim.SetTrigger("Death");

        ikSetUp.ik = false;
        anim.enabled = false;
        this.enabled = false;
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

    KeyCode[] windowKeyCodes = {
        KeyCode.Alpha0,
        KeyCode.Alpha1,
        KeyCode.Alpha2,
        KeyCode.Alpha3,
        KeyCode.Alpha4,
        KeyCode.Alpha5,
        KeyCode.Alpha6,
        KeyCode.Alpha7,
        KeyCode.Alpha8,
        KeyCode.Alpha9,
    };

    KeyCode[] macKeyCodes = {
        KeyCode.Keypad0,
        KeyCode.Keypad1,
        KeyCode.Keypad2,
        KeyCode.Keypad3,
        KeyCode.Keypad4,
        KeyCode.Keypad5,
        KeyCode.Keypad6,
        KeyCode.Keypad7,
        KeyCode.Keypad8,
        KeyCode.Keypad9,
    };

    [ContextMenu("Swap Keycodes Pc vs Mac")]
    void ChangeKeyBindingsIfMac()
    {
        bool isWinEditor = Application.platform == RuntimePlatform.WindowsEditor;
        bool isOSXEditor = Application.platform == RuntimePlatform.OSXEditor;

        if(isWinEditor)
        {
            for (int i = 0; i < weapons.Length; i++)
            {
                weapons[i].GetComponent<WeaponController>().weaponSwitchKey = windowKeyCodes[i];
            }
        }

        if (isOSXEditor)
        {
            for (int i = 0; i < weapons.Length; i++)
            {
                weapons[i].GetComponent<WeaponController>().weaponSwitchKey = macKeyCodes[i];
            }
        }
    }
}

