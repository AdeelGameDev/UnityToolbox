using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
public class PinchScrollDetection : MonoBehaviour 
{
	public float speed = 0.01f;
	private float prevMagnitude = 0;
	private int touchCount = 0;
	private void Start () 
	{
		// mouse scroll
		var scrollAction = new InputAction(binding: "<Mouse>/scroll");
		scrollAction.Enable();
		scrollAction.performed += ctx => CameraZoom(ctx.ReadValue<Vector2>().y * speed);

		// pinch gesture
		var touch0contact = new InputAction
		(
			type: InputActionType.Button,
			binding: "<Touchscreen>/touch0/press"
		);
		touch0contact.Enable();
		var touch1contact = new InputAction
		(
			type: InputActionType.Button,
			binding: "<Touchscreen>/touch1/press"
		);
		touch1contact.Enable();

		touch0contact.performed += _ => touchCount++;
		touch1contact.performed += _ => touchCount++;
		touch0contact.canceled += _ => 
		{
			touchCount--;
			prevMagnitude = 0;
		};
		touch1contact.canceled += _ => 
		{
			touchCount--;
			prevMagnitude = 0;
		};

		var touch0pos = new InputAction
		(
			type: InputActionType.Value,
			binding: "<Touchscreen>/touch0/position"
		);
		touch0pos.Enable();
		var touch1pos = new InputAction
		(
			type: InputActionType.Value,
			binding: "<Touchscreen>/touch1/position"
		);
		touch1pos.Enable();
		touch1pos.performed += _ => 
		{
			if(touchCount < 2)
				return;
			var magnitude = (touch0pos.ReadValue<Vector2>() - touch1pos.ReadValue<Vector2>()).magnitude;
			if(prevMagnitude == 0)
				prevMagnitude = magnitude;
			var difference = magnitude - prevMagnitude;
			prevMagnitude = magnitude;
			CameraZoom(-difference * speed);
		};
	}


	private void CameraZoom(float increment) => Camera.main.fieldOfView = Mathf.Clamp(Camera.main.fieldOfView + increment, 20, 60);

}
