using UnityEngine;
using UnityEngine.InputSystem;

public class CameraShakeTester : MonoBehaviour
{
    [SerializeField] private CameraShakeController cameraShake;

    private void Update()
    {
        if (Keyboard.current != null &&
            Keyboard.current.tKey.wasPressedThisFrame)
        {
            if (cameraShake != null)
                cameraShake.Shake(0.25f, 0.35f);
        }
    }
}