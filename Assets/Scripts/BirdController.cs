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
    private float speed, defaultSpeed, forwardFloat, rotate speed, rotationValue = 20f, turnSpeed;
    private Action execution;
    [SerializeField]
    private Transform cameraSocket, birdSocket;
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

		forwardFloat = defaultSpeed;
		if (keyboardMovement.magnitude > 0) {
			transform.Rotate(0, keyboardMovement.x * turnSpeed, 0);
			forwardFloat += keyboardMovement.z * speed * (1 - Mathf.Abs(keyboardMovement.x)/2);
			birdSocket.localRotation = Quaternion.RotateTowards(birdSocket.localRotation,Quaternion.Euler(keyboardMovement.z*rotationValue,0, keyboardMovement.x *-rotationValue),Time.deltaTime * rotateSpeed);
		} else {
			transform.Rotate(0, (leftJoystick.z - rightJoystick.z) * turnSpeed / 2f, 0);
			forwardFloat += (leftJoystick.z + rightJoystick.z) * speed / 2f;
			birdSocket.localRotation = Quaternion.RotateTowards(birdSocket.localRotation,Quaternion.Euler((leftJoystick.z + rightJoystick.z)*rotationValue / 2f,0, (leftJoystick.z - rightJoystick.z) * -rotationValue /2f),Time.deltaTime * rotateSpeed);
		}
		characterController.Move(transform.TransformDirection(Vector3.forward) * forwardFloat);

    }
}
