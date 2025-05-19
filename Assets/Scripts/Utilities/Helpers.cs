using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Users;

    public class Helpers : MonoBehaviour
    {
        public enum ControlSchemeType { KeyboardMouse, Xbox, PlayStation, Unknown }

        public ControlSchemeType GetCurrentScheme()
        {
            var gamepad = Gamepad.current;
            if (gamepad == null) { return ControlSchemeType.Unknown; }

            if (gamepad is UnityEngine.InputSystem.DualShock.DualShockGamepad) { return ControlSchemeType.PlayStation; }
            if (gamepad is UnityEngine.InputSystem.DualShock.DualSenseGamepadHID) { return ControlSchemeType.PlayStation; }
            
            if (gamepad is UnityEngine.InputSystem.XInput.XInputController) { return ControlSchemeType.Xbox; }

            return ControlSchemeType.Unknown;
        }
    }
