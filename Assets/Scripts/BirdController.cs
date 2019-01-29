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
    private float speed, defaultSpeed, forwardFloat, rotateSpeed, rotationValue = 20f, turnSpeed = 4f;
    [SerializeField]
    private Transform birdSocket;
	public static float speedReference;
    void Start(){
        characterController = GetComponent<CharacterController>();
    }
    void Update()
    {
        forwardFloat = defaultSpeed;
		keyboardMovement = new Vector3(Input.GetAxis("Horizontal NEW"),0,Input.GetAxis("Vertical NEW"));
		if (keyboardMovement.magnitude > 0) {
			transform.Rotate(0, keyboardMovement.x * turnSpeed, 0);
			forwardFloat += keyboardMovement.z * speed * (1 - Mathf.Abs(keyboardMovement.x)/2);
			birdSocket.localRotation = Quaternion.RotateTowards(birdSocket.localRotation,Quaternion.Euler((keyboardMovement.z*rotationValue)*0.2f,0, keyboardMovement.x *-rotationValue),Time.deltaTime * rotateSpeed);
		} else {
            leftJoystick = new Vector3(Input.GetAxis("Left Joystick X"),0,Input.GetAxis("Left Joystick Y"));
		    rightJoystick = new Vector3(Input.GetAxis("Right Joystick X"),0, Input.GetAxis("Right Joystick Y"));
			transform.Rotate(0, (leftJoystick.z - rightJoystick.z) * turnSpeed / 2f, 0);
			forwardFloat += (leftJoystick.z + rightJoystick.z) * speed / 2f;
			birdSocket.localRotation = Quaternion.RotateTowards(birdSocket.localRotation,Quaternion.Euler((leftJoystick.z + rightJoystick.z)*rotationValue / 2f,0, (leftJoystick.z - rightJoystick.z) * -rotationValue /2f),Time.deltaTime * rotateSpeed);
		}
		characterController.Move(transform.TransformDirection(Vector3.forward) * forwardFloat);
		speedReference = characterController.velocity.magnitude;

    }
}
