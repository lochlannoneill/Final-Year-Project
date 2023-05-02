using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LON {
    public class CameraManager : MonoBehaviour
    {
        public Transform targetTransform;
        public Transform cameraTransform;
        public Transform cameraPivotTransform;
        private Transform myTransform;
        private Vector3 cameraTransformPosition;
        private LayerMask ignoreLayers;
        private Vector3 cameraFollowVelocity = Vector3.zero;

        public static CameraManager singleton;

        public float lookSpeed = 0.1f;
        public float followSpeed = 0.1f;
        public float pivotSpeed = 0.03f;
        private float targetPosition;
        private float defaultPosition;
        private float lookAngle;
        private float pivotAngle;
        public float minimumPivot = -35;
        public float maximumPivot = 35;

        public float cameraSphereRadius = 0.2f;
        public float cameraCollisionOffset = 0.2f;
        public float minimumCollisionOffset = 0.2f;

        private void Awake() {
            singleton = this;
            myTransform = transform;
            defaultPosition = cameraTransform.localPosition.z;
            ignoreLayers = ~(1 << 8 | 1 << 9 | 1 << 10);
        }

        public void HandleFollow(float deltatime) {
            Vector3 targetPosition = Vector3.SmoothDamp(
                myTransform.position, targetTransform.position, ref cameraFollowVelocity, deltatime / followSpeed
            );
            myTransform.position = targetPosition;

            HandleCollisions(deltatime);
        }

        public void HandleRotation(float delta, float input_mouseX, float input_mouseY) 
        {
            float smoothTime = 0.3f;

            // Calculate new look and pivot angles
            lookAngle += (input_mouseX * lookSpeed) / delta;
            pivotAngle -= (input_mouseY * pivotSpeed) / delta;

            // Clamp the pivot angle between minimum and maximum pivot angles
            pivotAngle = Mathf.Clamp(pivotAngle, minimumPivot, maximumPivot);

            // Set the look angle to the Y component of the rotation vector
            Vector3 rotation = new Vector3(0f, lookAngle, 0f);
            Quaternion targetRotation = Quaternion.Euler(rotation);
            myTransform.rotation = Quaternion.Slerp(myTransform.rotation, targetRotation, smoothTime);

            // Set the pivot angle to the X component of the rotation vector
            rotation = new Vector3(pivotAngle, 0f, 0f);
            targetRotation = Quaternion.Euler(rotation);
            cameraPivotTransform.localRotation = Quaternion.Slerp(cameraPivotTransform.localRotation, targetRotation, smoothTime);
        }

        private void HandleCollisions(float deltaTime)
        {
            float newTargetPosition = defaultPosition;
            RaycastHit hit;

            // Calculate direction of camera from pivot and normalize it
            Vector3 cameraDirection = cameraTransform.position - cameraPivotTransform.position;
            cameraDirection.Normalize();

            // Check if there is an object in the camera's path and update target position accordingly
            if (Physics.SphereCast(cameraPivotTransform.position, cameraSphereRadius, cameraDirection, out hit, Mathf.Abs(newTargetPosition), ignoreLayers))
            {
                float distanceToHit = Vector3.Distance(cameraPivotTransform.position, hit.point);
                newTargetPosition = -(distanceToHit - cameraCollisionOffset);
            }

            // If the target position is too close to the pivot, update it to the minimum collision offset
            if (Mathf.Abs(newTargetPosition) < minimumCollisionOffset)
            {
                newTargetPosition = -minimumCollisionOffset;
            }

            // Interpolate between the current camera position and the target position and update camera position
            Vector3 cameraLocalPos = cameraTransform.localPosition;
            float cameraZPos = Mathf.Lerp(cameraLocalPos.z, newTargetPosition, deltaTime / 0.2f);
            cameraTransform.localPosition = new Vector3(cameraLocalPos.x, cameraLocalPos.y, cameraZPos);
        }

    }
}