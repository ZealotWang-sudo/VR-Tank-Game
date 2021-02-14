using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace BNG {
    public class LocomotionManager : MonoBehaviour {

        [Header("Locomotion Type")]
        /// <summary>
        /// Default locomotion to use if nothing stored in playerprefs. 0 = Teleport. 1 = SmoothLocomotion
        /// </summary>
        [Tooltip("Default locomotion to use if nothing stored in playerprefs. 0 = Teleport. 1 = SmoothLocomotion")]
        public LocomotionType DefaultLocomotion = LocomotionType.Teleport;

        LocomotionType selectedLocomotion = LocomotionType.Teleport;
        public LocomotionType SelectedLocomotion {
            get { return selectedLocomotion; }
        }

        /// <summary>
        /// If true, locomotion type will be saved and loaded from player prefs
        /// </summary>
        [Header("Save / Loading")]
        [Tooltip("If true, locomotion type will be saved and loaded from player prefs")]
        public bool LoadLocomotionFromPrefs = false;

        [Header("Input")]
        [Tooltip("The key(s) to use to toggle locomotion type")]
        public List<ControllerBinding> locomotionToggleInput = new List<ControllerBinding>() { ControllerBinding.None };

        [Tooltip("The action used to toggle locomotion type")]
        public InputActionReference LocomotionToggleAction;

        BNGPlayerController player;
        PlayerTeleport teleport;
        SmoothLocomotion smoothLocomotion;

        void Start() {
            player = GetComponent<BNGPlayerController>();
            teleport = GetComponent<PlayerTeleport>();

            // Load Locomotion Preference
            if (LoadLocomotionFromPrefs) {
                ChangeLocomotion(PlayerPrefs.GetInt("LocomotionSelection", 0) == 0 ? LocomotionType.Teleport : LocomotionType.SmoothLocomotion, false);
            }
            else {
                ChangeLocomotion(DefaultLocomotion, false);
            }
        }

        void OnEnable() {
            if(LocomotionToggleAction) {
                LocomotionToggleAction.action.Enable();
                LocomotionToggleAction.action.performed += OnLocomotionToggle;
            }
        }

        void OnDisable() {
            if (LocomotionToggleAction) {
                LocomotionToggleAction.action.Disable();
                LocomotionToggleAction.action.performed -= OnLocomotionToggle;
            }
        }

        public void OnLocomotionToggle(InputAction.CallbackContext context) {
            // Toggle the locomotion
            ChangeLocomotion(SelectedLocomotion == LocomotionType.SmoothLocomotion ? LocomotionType.Teleport : LocomotionType.SmoothLocomotion, LoadLocomotionFromPrefs);
        }

        public void UpdateTeleportStatus() {
            teleport.enabled = SelectedLocomotion == LocomotionType.Teleport;
        }

        public void ChangeLocomotion(LocomotionType locomotionType, bool save) {
            ChangeLocomotionType(locomotionType);

            if (save) {
                PlayerPrefs.SetInt("LocomotionSelection", locomotionType == LocomotionType.Teleport ? 0 : 1);
            }

            UpdateTeleportStatus();
        }

        public void ChangeLocomotionType(LocomotionType loc) {

            // Make sure Smooth Locomotion is available
            if (smoothLocomotion == null) {
                smoothLocomotion = GetComponent<SmoothLocomotion>();
            }

            selectedLocomotion = loc;

            if (teleport == null) {
                teleport = GetComponent<PlayerTeleport>();
            }

            toggleTeleport(selectedLocomotion == LocomotionType.Teleport);
            toggleSmoothLocomotion(selectedLocomotion == LocomotionType.SmoothLocomotion);
        }

        void toggleTeleport(bool enabled) {
            if (enabled) {
                teleport.EnableTeleportation();
            }
            else {
                teleport.DisableTeleportation();
            }
        }

        void toggleSmoothLocomotion(bool enabled) {
            if (smoothLocomotion) {
                smoothLocomotion.enabled = enabled;
            }
        }

        public void ToggleLocomotionType() {
            // Toggle based on last value
            if (selectedLocomotion == LocomotionType.SmoothLocomotion) {
                ChangeLocomotionType(LocomotionType.Teleport);
            }
            else {
                ChangeLocomotionType(LocomotionType.SmoothLocomotion);
            }
        }
    }
}