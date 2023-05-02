using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LON {
    public class PlayerMovement : MonoBehaviour
    {
        PlayerManager playerManager;
        Transform cameraObject;
        InputManager inputManager;
        public Vector3 moveDirection;

        [HideInInspector]
        public Transform myTransform;
        [HideInInspector]
        public AnimatorManager animatorManager;

        public new Rigidbody rigidbody;
        public GameObject normalCamera;

        [Header("Ground & Air Detection")]
        [SerializeField]
        float groundDetectionRayStartPoint = 0.5f;
        [SerializeField]
        float minimumDistanceNeededToBeginFall = 1f;
        [SerializeField]
        float groundDirectionRayDistance = -0.2f;
        LayerMask ignoreForGroundCheck;
        public float inAirTimer;

        [Header("Movement")]
        [SerializeField]
        float movementSpeed = 5;
        [SerializeField]
        float rotationSpeed = 10;
        float fallingSpeed = 45;

        void Start()
        {
            playerManager = GetComponent<PlayerManager>();
            rigidbody = GetComponent<Rigidbody>();
            inputManager = GetComponent<InputManager>();
            animatorManager = GetComponentInChildren<AnimatorManager>();
            cameraObject = Camera.main.transform;
            myTransform = transform;
            animatorManager.Initialize();
            playerManager.isOnGround = true;
            ignoreForGroundCheck = ~(1 << 8 | 1 << 11);
        }

        Vector3 normalVector;
        Vector3 targetPosition;

        private void HandleRotation(float delta)
        {
            Vector3 targetDir = Vector3.zero;
            float moveOverrisde = inputManager.moveAmount;

            targetDir = cameraObject.forward * inputManager.vertical;
            targetDir += cameraObject.right * inputManager.horizontal;

            targetDir.Normalize();
            targetDir.y = 0;

            if(targetDir == Vector3.zero)
                targetDir = myTransform.forward;

            float rs = rotationSpeed;

            Quaternion tr = Quaternion.LookRotation(targetDir);
            Quaternion targetRotation = Quaternion.Slerp(myTransform.rotation, tr, rs * delta);

            myTransform.rotation = targetRotation;
        }
        
        public void HandleMovement(float delta) {
            if(playerManager.isBusy)
                return;

            moveDirection = cameraObject.forward * inputManager.vertical;
            moveDirection += cameraObject.right * inputManager.horizontal;
            moveDirection.Normalize();
            moveDirection.y = 0;

            float speed = movementSpeed;

            moveDirection *= speed;

            Vector3 projectedVelocity = Vector3.ProjectOnPlane(moveDirection, normalVector);
            rigidbody.velocity = projectedVelocity;

            animatorManager.UpdateAnimatorValues(inputManager.moveAmount, 0);
            if (animatorManager.canRotate)
            {
                HandleRotation(delta);
            }
        }

        public void HandleFalling(float delta, Vector3 moveDirection) {
            playerManager.isOnGround = false;

            // Raycast to detect any obstacles in front of the player
            RaycastHit hit;
            Vector3 origin = myTransform.position;
            origin.y += groundDetectionRayStartPoint;

            if(Physics.Raycast(origin, myTransform.forward, out hit, 0.4f))
            {
                // If an obstacle is detected, stop moving
                moveDirection = Vector3.zero;
            }

            if(playerManager.isInAir)
            {
                // Apply falling force to the player's rigidbody
                rigidbody.AddForce(-Vector3.up * fallingSpeed);
                rigidbody.AddForce(moveDirection * fallingSpeed / 5f);
            }

            Vector3 dir = moveDirection;
            dir.Normalize();
            origin = origin + dir * groundDirectionRayDistance;

            targetPosition = myTransform.position;

            // Draw a ray to visualize the ground detection
            Debug.DrawRay(origin, -Vector3.up * minimumDistanceNeededToBeginFall, Color.red, 0.1f, false);

            if (Physics.Raycast(origin, -Vector3.up, out hit, minimumDistanceNeededToBeginFall, ignoreForGroundCheck))
            {
                // If the ground is detected, set the player on the ground
                normalVector = hit.normal;
                Vector3 tp = hit.point;
                playerManager.isOnGround = true;
                targetPosition.y = tp.y;

                if(playerManager.isInAir)
                {
                    // Play the landing animation if the player was in the air for more than 0.5 seconds
                    if(inAirTimer > 0.5f)
                    {
                        animatorManager.PlayTargetAnimation("Land", true);
                        inAirTimer = 0;
                    }
                    else
                    {
                        animatorManager.PlayTargetAnimation("Movement", false);
                        inAirTimer = 0;
                    }
                    playerManager.isInAir = false;
                }
            }
            else {
                // If the ground is not detected, set the player in the air
                if (playerManager.isOnGround)
                {
                    playerManager.isOnGround = false;
                }

                if(playerManager.isInAir == false)
                {
                    // Play the falling animation if the player is not busy
                    if(playerManager.isBusy == false)
                    {
                        animatorManager.PlayTargetAnimation("Fall", true);
                    }

                    Vector3 vel = rigidbody.velocity;
                    vel.Normalize();
                    rigidbody.velocity = vel * (movementSpeed / 2);
                    playerManager.isInAir = true;
                }
            }

            if(playerManager.isOnGround)
            {
                // If the player is on the ground, move the player to the target position
                if(playerManager.isBusy || inputManager.moveAmount > 0)
                {
                    myTransform.position = Vector3.Lerp(myTransform.position, targetPosition, Time.deltaTime);
                }
                else
                {
                    myTransform.position = targetPosition;
                }
            }
        }


    }
}
