using UnityEngine;
using UnityEngine.InputSystem;

namespace Adeel.UnityToolbox.Samples
{
    /// <summary>
    /// Two-finger pinch and mouse-wheel zoom, applied to the main camera's field of
    /// view.
    /// </summary>
    /// <remarks>
    /// Requires the Unity Input System package (com.unity.inputsystem) and an
    /// active Touchscreen or Mouse device. Written with the Input System's
    /// standalone <see cref="InputAction"/> API, so no .inputactions asset is needed.
    /// </remarks>
    public class PinchScrollDetection : MonoBehaviour
    {
        [Tooltip("How much one unit of scroll or pixel of pinch changes the field of view.")]
        public float speed = 0.01f;

        [Tooltip("Clamped range for the field of view.")]
        public Vector2 fieldOfViewLimits = new Vector2(20f, 60f);

        private float _previousMagnitude;
        private int _touchCount;
        private Camera _camera;

        private InputAction _scrollAction;
        private InputAction _touch0Contact;
        private InputAction _touch1Contact;
        private InputAction _touch0Position;
        private InputAction _touch1Position;

        private void Start()
        {
            _camera = Camera.main;

            if (_camera == null)
            {
                Debug.LogError("PinchScrollDetection: no camera tagged 'MainCamera' in the scene.", this);
                enabled = false;
                return;
            }

            // Mouse wheel
            _scrollAction = new InputAction(binding: "<Mouse>/scroll");
            _scrollAction.performed += context => Zoom(context.ReadValue<Vector2>().y * speed);
            _scrollAction.Enable();

            // Pinch gesture: two touches down means a pinch is in progress.
            _touch0Contact = new InputAction(type: InputActionType.Button, binding: "<Touchscreen>/touch0/press");
            _touch1Contact = new InputAction(type: InputActionType.Button, binding: "<Touchscreen>/touch1/press");
            _touch0Contact.Enable();
            _touch1Contact.Enable();

            _touch0Contact.performed += _ => _touchCount++;
            _touch1Contact.performed += _ => _touchCount++;

            _touch0Contact.canceled += _ => OnTouchReleased();
            _touch1Contact.canceled += _ => OnTouchReleased();

            _touch0Position = new InputAction(type: InputActionType.Value, binding: "<Touchscreen>/touch0/position");
            _touch1Position = new InputAction(type: InputActionType.Value, binding: "<Touchscreen>/touch1/position");
            _touch0Position.Enable();
            _touch1Position.Enable();

            _touch1Position.performed += _ =>
            {
                if (_touchCount < 2)
                {
                    return;
                }

                float magnitude = (_touch0Position.ReadValue<Vector2>() - _touch1Position.ReadValue<Vector2>()).magnitude;

                if (_previousMagnitude <= 0f)
                {
                    _previousMagnitude = magnitude;
                    return;
                }

                float difference = magnitude - _previousMagnitude;
                _previousMagnitude = magnitude;
                Zoom(-difference * speed);
            };
        }

        private void OnTouchReleased()
        {
            _touchCount = Mathf.Max(0, _touchCount - 1);
            _previousMagnitude = 0f;
        }

        private void Zoom(float increment)
        {
            _camera.fieldOfView = Mathf.Clamp(
                _camera.fieldOfView + increment,
                fieldOfViewLimits.x,
                fieldOfViewLimits.y);
        }

        private void OnDisable()
        {
            _scrollAction?.Disable();
            _touch0Contact?.Disable();
            _touch1Contact?.Disable();
            _touch0Position?.Disable();
            _touch1Position?.Disable();
        }

        private void OnDestroy()
        {
            _scrollAction?.Dispose();
            _touch0Contact?.Dispose();
            _touch1Contact?.Dispose();
            _touch0Position?.Dispose();
            _touch1Position?.Dispose();
        }
    }
}
