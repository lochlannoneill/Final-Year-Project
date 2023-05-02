using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LON {
    public class PlayerManager : MonoBehaviour
    {
        PlayerMovement playerMovement;
        InputManager inputManager;
        CameraManager cameraManager;
        Animator anim;

        [Header("Flags")]
        public bool isBusy;
        public bool isInAir;
        public bool isOnGround;

        private void Awake() {
            // placeholder
        }

        void Start()
        {
            cameraManager = CameraManager.singleton;
            inputManager = GetComponent<InputManager>();
            anim = GetComponentInChildren<Animator>();
            playerMovement = GetComponent<PlayerMovement>();
        }

        void Update()
        {
            float deltaTime = Time.deltaTime;

            isBusy = anim.GetBool("isBusy"); // halts input while falling

            inputManager.Input(deltaTime);
            playerMovement.HandleMovement(deltaTime);
            playerMovement.HandleFalling(deltaTime, playerMovement.moveDirection);
        }

        private void FixedUpdate() {
            float deltaTime = Time.fixedDeltaTime;

            if (cameraManager != null) {
                // Update the camera's position to follow the target
                cameraManager.HandleFollow(deltaTime);

                // Handle the rotation of the camera based on user input
                float mouseX = inputManager.mouseX;
                float mouseY = inputManager.mouseY;
                cameraManager.HandleRotation(deltaTime, mouseX, mouseY);
            }
        }

        // Resetting flags in LateUpdate() instead of Update() to avoid animation glitches
        private void LateUpdate() {
            // inputManager.rollFlag = false;
            // inputManager.sprintFlag = false;
            // isSprinting = inputManager.b_Input;

            if (isInAir)
            {
                playerMovement.inAirTimer = playerMovement.inAirTimer + Time.deltaTime;
            }

        }

    }
}
