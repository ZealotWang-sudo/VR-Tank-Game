using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;

namespace BNG {

    public class VREmulator : MonoBehaviour {

        [Header("Enable / Disable : ")]
        [Tooltip("Use Emulator if true and HMDIsActive is false")]
        public bool EmulatorEnabled = true;

        [Header("Key Bindings : ")]
        public KeyCode PlayerUp = KeyCode.LeftAlt;
        public KeyCode PlayerDown = KeyCode.LeftControl;

        public KeyCode RightTrigger = KeyCode.Z;
        public KeyCode RightGrip = KeyCode.X;
        public KeyCode RightThumbNear = KeyCode.C;

        public KeyCode LeftTrigger = KeyCode.V;
        public KeyCode LeftGrip = KeyCode.B;
        public KeyCode LeftThumbNear = KeyCode.N;


        float mouseRotationX;
        float mouseRotationY;

        [Header("Camera Look : ")]

        public float MouseSensitivityX = 1.25f;
        public float MouseSensitivityY = 1.25f;

        public float MinimumCameraY = -90f;
        public float MaximumCameraY = 90f;

        Transform mainCameraTransform;
        Transform leftControllerTranform;
        Transform rightControllerTranform;

        Transform leftHandAnchor;
        Transform rightHandAnchor;

        BNGPlayerController player;
        SmoothLocomotion smoothLocomotion;
        bool didFirstActivate = false;

        Grabber grabberLeft;
        Grabber grabberRight;

        private float _originalPlayerYOffset = 1.65f;

        [Header("Shown for Debug : ")]
        public bool HMDIsActive;

        public Vector3 LeftControllerPosition = new Vector3(-0.2f, -0.2f, 0.5f);
        public Vector3 RightControllerPosition = new Vector3(0.2f, -0.2f, 0.5f);

        void Start() {
            mainCameraTransform = GameObject.Find("CameraRig").transform;
            leftHandAnchor = GameObject.Find("LeftHandAnchor").transform;
            rightHandAnchor = GameObject.Find("RightHandAnchor").transform;

            leftControllerTranform = GameObject.Find("LeftControllerAnchor").transform;
            rightControllerTranform = GameObject.Find("RightControllerAnchor").transform;

            player = FindObjectOfType<BNGPlayerController>();

            if(player) {
                // Use this to keep our head up high
                player.ElevateCameraIfNoHMDPresent = true;
                _originalPlayerYOffset = player.ElevateCameraHeight;

                smoothLocomotion = player.GetComponentInChildren<SmoothLocomotion>();

                if (smoothLocomotion == null) {
                    Debug.Log("No Smooth Locomotion component found. Will not be able to use SmoothLocomotion without calling it manually.");
                }
                else if (smoothLocomotion.MoveAction == null) {
                    Debug.Log("Smooth Locomotion Move Action has not been assigned. Make sure to assign this in the inspector if you want to be able to move around using the VR Emulator.");
                }
            }
        }

        public void OnBeforeRender() {
            HMDIsActive = InputBridge.Instance.HMDActive;

            // Ready to go
            if (EmulatorEnabled && !HMDIsActive) {
                UpdateControllerPositions();
            }
        }

        void onFirstActivate() {
            UpdateControllerPositions();            

            didFirstActivate = true;
        }

        void Update() {

            //// Considerd absent if specified or unknown status
            //bool userAbsent = XRDevice.userPresence == UserPresenceState.NotPresent || XRDevice.userPresence == UserPresenceState.Unknown;
            // Updated to show in Debug Settings
            HMDIsActive = InputBridge.Instance.HMDActive;

            // Ready to go
            if (EmulatorEnabled && !HMDIsActive) {

                if(!didFirstActivate) {
                    onFirstActivate();
                }

                CheckHeadControls();

                UpdateControllerPositions();

                CheckPlayerControls();
            }

            // Device came online after emulator had started
            if(EmulatorEnabled && didFirstActivate && HMDIsActive) {
                ResetAll();
            }
        }

        public void CheckHeadControls() {

            // Hold right mouse button down to move camera around
            if(Input.GetMouseButton(1)) {

                // Move Camera on Y Axis
                mouseRotationY += Input.GetAxis("Mouse Y") * MouseSensitivityY;
                mouseRotationY = Mathf.Clamp(mouseRotationY, MinimumCameraY, MaximumCameraY);
                mainCameraTransform.localEulerAngles = new Vector3(-mouseRotationY, mainCameraTransform.localEulerAngles.y, 0);

                // Player controller on X axis
                player.transform.Rotate(0, Input.GetAxis("Mouse X") * MouseSensitivityX, 0);

                // Lock Camera and cursor
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
            else {
                // Unlock Camera
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
        }

        /// <summary>
        /// Overwrite InputBridge inputs with our own bindings
        /// </summary>
        public void UpdateInputs() {

            // Only override controls if no hmd is active and this script is enabled
            if (EmulatorEnabled == false || HMDIsActive) {
                return;
            }

            // Make sure grabbers are assigned
            checkGrabbers();

            var prevVal = InputBridge.Instance.LeftTrigger;
            InputBridge.Instance.LeftTrigger = Input.GetKey(LeftTrigger) ? 1f : 0;
            InputBridge.Instance.LeftTriggerDown = prevVal < InputBridge.Instance.DownThreshold && InputBridge.Instance.LeftTrigger >= InputBridge.Instance.DownThreshold;
            InputBridge.Instance.LeftTriggerUp = prevVal >= InputBridge.Instance.DownThreshold && InputBridge.Instance.LeftTrigger == 0;

            prevVal = InputBridge.Instance.LeftGrip;
            InputBridge.Instance.LeftGrip = Input.GetKey(LeftGrip) ? 1f : 0;
            InputBridge.Instance.LeftGripDown = prevVal < InputBridge.Instance.DownThreshold && InputBridge.Instance.LeftGrip >= InputBridge.Instance.DownThreshold;
            InputBridge.Instance.LeftThumbNear = Input.GetKey(LeftThumbNear);

            prevVal = InputBridge.Instance.RightTrigger;
            InputBridge.Instance.RightTrigger = Input.GetKey(RightTrigger) ? 1f : 0;
            InputBridge.Instance.RightTriggerDown = prevVal < InputBridge.Instance.DownThreshold && InputBridge.Instance.RightTrigger >= InputBridge.Instance.DownThreshold;
            InputBridge.Instance.RightTriggerUp = prevVal >= InputBridge.Instance.DownThreshold && InputBridge.Instance.RightTrigger == 0;

            prevVal = InputBridge.Instance.RightGrip;
            InputBridge.Instance.RightGrip = Input.GetKey(RightGrip) ? 1f : 0;
            InputBridge.Instance.RightGripDown = prevVal < InputBridge.Instance.DownThreshold && InputBridge.Instance.RightGrip >= InputBridge.Instance.DownThreshold;
            InputBridge.Instance.RightThumbNear = Input.GetKey(RightThumbNear);
        }

        public void CheckPlayerControls() {

            // Player Up / Down
            if(Input.GetKey(PlayerUp)) {
                player.ElevateCameraHeight = Mathf.Clamp(player.ElevateCameraHeight + Time.deltaTime, 0.2f, 5f);
            }
            else if (Input.GetKey(PlayerDown)) {
                player.ElevateCameraHeight = Mathf.Clamp(player.ElevateCameraHeight - Time.deltaTime, 0.2f, 5f);
            }

            // Player Move Forward / Back, Snap Turn
            if(smoothLocomotion != null && smoothLocomotion.enabled == false) {
                // Manually allow player movement if the smooth locomotion component is disabled
                smoothLocomotion.CheckControllerReferences();
                smoothLocomotion.UpdateInputs();
                smoothLocomotion.MoveCharacter();
            }
        }

        public virtual void UpdateControllerPositions() {
            leftControllerTranform.transform.localPosition = LeftControllerPosition;
            leftControllerTranform.transform.localEulerAngles = Vector3.zero;

            rightControllerTranform.transform.localPosition = RightControllerPosition;
            rightControllerTranform.transform.localEulerAngles = Vector3.zero;
        }

        void checkGrabbers() {
            // Find Grabber Left
            if (grabberLeft == null || !grabberLeft.isActiveAndEnabled) {
                Grabber[] grabbers = FindObjectsOfType<Grabber>();

                for (var x = 0; x < grabbers.Length; x++) {
                    if (grabbers[x] != null && grabbers[x].isActiveAndEnabled && grabbers[x].HandSide == ControllerHand.Left) {
                        grabberLeft = grabbers[x];
                    }
                }
            }

            // Find Grabber Right
            if (grabberRight == null || !grabberRight.isActiveAndEnabled) {
                Grabber[] grabbers = FindObjectsOfType<Grabber>();
                for (var x = 0; x < grabbers.Length; x++) {
                    if (grabbers[x] != null && grabbers[x].isActiveAndEnabled && grabbers[x].HandSide == ControllerHand.Right) {
                        grabberRight = grabbers[x];
                    }
                }
            }
        }

        public virtual void ResetHands() {
            leftControllerTranform.transform.localPosition = Vector3.zero;
            leftControllerTranform.transform.localEulerAngles = Vector3.zero;

            rightControllerTranform.transform.localPosition = Vector3.zero;
            rightControllerTranform.transform.localEulerAngles = Vector3.zero;
        }

        public virtual void ResetAll() {

            ResetHands();

            // Reset Camera
            mainCameraTransform.localEulerAngles = Vector3.zero;

            // Reset Player
            if (player) {
                player.ElevateCameraHeight = _originalPlayerYOffset;
            }

            didFirstActivate = false;
        }

        void OnEnable() {
            // Subscribe to input events
            InputBridge.OnInputsUpdated += UpdateInputs;

            Application.onBeforeRender += OnBeforeRender;
        }

        void OnDisable() {

            Application.onBeforeRender -= OnBeforeRender;

            if (isQuitting) {
                return;
            }

            // Reset Hand Positions
            ResetAll();

            // Unsubscribe from input events
            InputBridge.OnInputsUpdated -= UpdateInputs;
        }

        bool isQuitting = false;
        void OnApplicationQuit() {
            isQuitting = true;
        }
    }
}
