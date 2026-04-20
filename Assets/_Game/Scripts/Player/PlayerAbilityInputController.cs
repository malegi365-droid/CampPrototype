using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(AbilityController))]
public class PlayerAbilityInputController : MonoBehaviour
{
    private AbilityController abilityController;

    private void Awake()
    {
        abilityController = GetComponent<AbilityController>();
    }

    private void Update()
    {
        Keyboard kb = Keyboard.current;
        if (kb == null || abilityController == null)
            return;

        if (kb.digit1Key.wasPressedThisFrame || kb.numpad1Key.wasPressedThisFrame)
        {
            bool used = abilityController.UseHeavyShot();
            if (!used)
            {
                Debug.Log("Heavy Shot not used.");
            }
        }

        if (kb.digit2Key.wasPressedThisFrame || kb.numpad2Key.wasPressedThisFrame)
        {
            bool used = abilityController.UseFocusBreak();
            if (!used)
            {
                Debug.Log("Focus Break not used.");
            }
        }
    }
}