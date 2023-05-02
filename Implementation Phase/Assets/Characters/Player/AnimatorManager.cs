using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LON {
    public class AnimatorManager : MonoBehaviour
    {
        PlayerManager playerManager;
        public Animator anim;
        InputManager inputManager;
        PlayerMovement playerMovement;
        int vertical;
        int horizontal;
        public bool canRotate;

        public void Initialize()
        {
            playerManager = GetComponentInParent<PlayerManager>();
            anim = GetComponent<Animator>();
            inputManager = GetComponentInParent<InputManager>();
            playerMovement = GetComponentInParent<PlayerMovement>();
            vertical = Animator.StringToHash("Vertical");
            horizontal = Animator.StringToHash("Horizontal");
        }

        public void UpdateAnimatorValues(float verticalMovement, float horizontalMovement)
        {
            #region Vertical
            float v = 0;
            if (Mathf.Abs(verticalMovement) > 0.55f)
            {
                v = Mathf.Sign(verticalMovement);
            }
            else if (Mathf.Abs(verticalMovement) > 0)
            {
                v = Mathf.Sign(verticalMovement) * 0.5f;
            }
            #endregion

            #region Horizontal
            float h = 0;
            if (Mathf.Abs(horizontalMovement) > 0.55f)
            {
                h = Mathf.Sign(horizontalMovement);
            }
            else if (Mathf.Abs(horizontalMovement) > 0)
            {
                h = Mathf.Sign(horizontalMovement) * 0.5f;
            }
            #endregion

            anim.SetFloat(vertical, v, 0.1f, Time.deltaTime);
            anim.SetFloat(horizontal, h, 0.1f, Time.deltaTime);
        }

        public void PlayTargetAnimation(string targetAnim, bool isBusy)
        {
            anim.applyRootMotion = isBusy;
            anim.SetBool("isBusy", isBusy);
            anim.CrossFade(targetAnim, 0.2f);
        }
        public void CanRotate()
        {
            canRotate = true;
        }

        public void StopRotate()
        {
            canRotate = false;
        }
        public void OnAnimatorMove()
        {
            if (playerManager.isBusy == false)
                return;
            
            float delta = Time.deltaTime;
            playerMovement.rigidbody.drag = 0;
            Vector3 deltaPosition = anim.deltaPosition;
            deltaPosition.y = 0;
            Vector3 velocity = deltaPosition / delta;
            playerMovement.rigidbody.velocity = velocity;
        }
    }
}
