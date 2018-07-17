using UnityEngine;
using UnityEngine.UI;

namespace MyGame.UI
{
    public class UIFloatingStats : UIFloatingCanvas
    {
        static float transitionDelta = 15f;
        static float stoppingDistance = 0.01f;

        public Image healthFill;
        Camera facingCam;
        float targetFill;

        private void Awake()
        {
            facingCam = transform.GetComponentInChildren<Canvas>().worldCamera;
            
            if (facingCam == null)
                facingCam = Camera.main;
        }

        private void Update()
        {
            float distance = Mathf.Abs(targetFill - healthFill.fillAmount);

            if (distance > stoppingDistance)
                healthFill.fillAmount = Mathf.Lerp(healthFill.fillAmount, targetFill, distance * transitionDelta * GameTime.deltaTime);

            FaceCamera();
        }

        public void SetTargetFill(float fill)
        {
            targetFill = fill;
        }

        private void FaceCamera()
        {
            Vector3 relVec = facingCam.transform.position - transform.position;
            relVec = Vector3.ProjectOnPlane(-relVec, Vector3.up);
            Quaternion lookRotation = Quaternion.LookRotation(relVec);
            transform.rotation = lookRotation;
        }
    }
}

