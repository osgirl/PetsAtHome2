using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class BirdController : MonoBehaviour
{
    private CharacterController characterController;
    [SerializeField]
    private Vector3 leftJoystick, rightJoystick, keyboardMovement;
    [SerializeField]
    private float speed, rotateSpeed, stepSpeed, invertFloat, scaleValue, rotationValue = 20f;
    private Action execution;
    [SerializeField]
    private Transform cameraSocket, focusCarousel, birdSocket;
    private Camera camera;
    [SerializeField]
    private bool testMode;
    void Start(){
        camera = Camera.main;
        characterController = GetComponent<CharacterController>();
    }
    void Update()
    {
		leftJoystick = new Vector3(Input.GetAxis("Left Joystick X"),0,Input.GetAxis("Left Joystick Y"));
		rightJoystick = new Vector3(Input.GetAxis("Right Joystick X"),0, Input.GetAxis("Right Joystick Y"));
		keyboardMovement = new Vector3(Input.GetAxis("Horizontal NEW"),0,Input.GetAxis("Vertical NEW"));

		if (keyboardMovement.magnitude > 0) {
			transform.Rotate(0, keyboardMovement.x, 0);
			characterController.Move(transform.TransformDirection(Vector3.forward) * speed);
			birdSocket.localRotation = Quaternion.RotateTowards(birdSocket.localRotation,Quaternion.Euler(keyboardMovement.z*rotationValue,0, keyboardMovement.x *-rotationValue),Time.deltaTime * stepSpeed);
		} else {
			transform.Rotate(0, leftJoystick.z - rightJoystick.z, 0);
			characterController.Move(transform.TransformDirection(Vector3.forward) * (leftJoystick.z + rightJoystick.z + 2.5f) * speed);
			birdSocket.localRotation = Quaternion.RotateTowards(birdSocket.localRotation,Quaternion.Euler((leftJoystick.z + rightJoystick.z)*22.5f,0, (leftJoystick.z - rightJoystick.z)*-22.5f),Time.deltaTime * stepSpeed);
		}

    }
}
