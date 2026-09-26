using BepInEx;
using UnityEngine;
using UnityEngine.InputSystem;
using System.Runtime.InteropServices;

namespace WASDFly
{
    [BepInPlugin("com.poopoovr.gorillatag.wasdfly", "WASDFly", "1.0.0")]
    public class Plugin : BaseUnityPlugin
    {
        [DllImport("user32.dll")]
        static extern short GetAsyncKeyState(int vKey);

        bool isEnabled = false;
        bool wasKDown = false;
        
        float FlySpeed = 10f;
        Vector3 lastPosition = Vector3.zero;

        void Update()
        {
            try
            {
                bool isKDown = (GetAsyncKeyState(0x4B) & 0x8000) != 0; // 'K' key
                if (isKDown && !wasKDown) isEnabled = !isEnabled;
                wasKDown = isKDown;

                if (isEnabled) WASDFly();
            }
            catch { }
        }

        void WASDFly()
        {
            bool W = (GetAsyncKeyState(0x57) & 0x8000) != 0;
            bool A = (GetAsyncKeyState(0x41) & 0x8000) != 0;
            bool S = (GetAsyncKeyState(0x53) & 0x8000) != 0;
            bool D = (GetAsyncKeyState(0x44) & 0x8000) != 0;
            bool Space = (GetAsyncKeyState(0x20) & 0x8000) != 0;
            bool Ctrl = (GetAsyncKeyState(0x11) & 0x8000) != 0; 
            bool Shift = (GetAsyncKeyState(0x10) & 0x8000) != 0;
            bool Alt = (GetAsyncKeyState(0x12) & 0x8000) != 0;

            bool LeftArrow = (GetAsyncKeyState(0x25) & 0x8000) != 0;
            bool RightArrow = (GetAsyncKeyState(0x27) & 0x8000) != 0;
            bool UpArrow = (GetAsyncKeyState(0x26) & 0x8000) != 0;
            bool DownArrow = (GetAsyncKeyState(0x28) & 0x8000) != 0;

            if (W || A || S || D || Space || Ctrl)
                GorillaTagger.Instance.rigidbody.linearVelocity = Vector3.zero;

            if (GorillaTagger.Instance != null && GorillaTagger.Instance.mainCamera != null)
            {
                Transform parentTransform = GorillaTagger.Instance.mainCamera.transform.parent;
                float turnSpeed = 250f;
                
                if (LeftArrow) parentTransform.eulerAngles += new Vector3(0, -turnSpeed, 0) * Time.deltaTime;
                if (RightArrow) parentTransform.eulerAngles += new Vector3(0, turnSpeed, 0) * Time.deltaTime;
                if (UpArrow) parentTransform.eulerAngles += new Vector3(-turnSpeed, 0, 0) * Time.deltaTime;
                if (DownArrow) parentTransform.eulerAngles += new Vector3(turnSpeed, 0, 0) * Time.deltaTime;
                
                Mouse mouse = Mouse.current;
                if (mouse != null && mouse.rightButton.isPressed)
                {
                    const float mouseLookSensitivity = 0.2f;
                    Vector2 delta = mouse.delta.ReadValue();
                    Vector3 euler = parentTransform.eulerAngles;
                    float pitch = euler.x > 180f ? euler.x - 360f : euler.x;
                    float yaw = euler.y + delta.x * mouseLookSensitivity;

                    pitch = Mathf.Clamp(pitch - delta.y * mouseLookSensitivity, -90f, 90f);
                    parentTransform.rotation = Quaternion.Euler(pitch, yaw, euler.z);
                }

                float speed = FlySpeed;
                if (Shift) speed *= 2f;
                else if (Alt) speed /= 2;

                if (W) GorillaTagger.Instance.rigidbody.transform.position += parentTransform.forward * (Time.deltaTime * speed);
                if (S) GorillaTagger.Instance.rigidbody.transform.position += parentTransform.forward * (Time.deltaTime * -speed);
                if (A) GorillaTagger.Instance.rigidbody.transform.position += parentTransform.right * (Time.deltaTime * -speed);
                if (D) GorillaTagger.Instance.rigidbody.transform.position += parentTransform.right * (Time.deltaTime * speed);
                if (Space) GorillaTagger.Instance.rigidbody.transform.position += new Vector3(0f, Time.deltaTime * speed, 0f);
                if (Ctrl) GorillaTagger.Instance.rigidbody.transform.position += new Vector3(0f, Time.deltaTime * -speed, 0f);

                if (VRRig.LocalRig != null && VRRig.LocalRig.head != null && GorillaTagger.Instance.headCollider != null)
                    VRRig.LocalRig.head.rigTarget.transform.rotation = GorillaTagger.Instance.headCollider.transform.rotation;
            }

            if (!W && !A && !S && !D && !Space && !Ctrl && lastPosition != Vector3.zero)
                GorillaTagger.Instance.rigidbody.transform.position = lastPosition;
            else
                lastPosition = GorillaTagger.Instance.rigidbody.transform.position;
        }
    }
}
