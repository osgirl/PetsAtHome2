using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class BirdController : MonoBehaviour
{
    private CharacterController characterController;
    [SerializeField]
    private Vector3 movement, direction;
    [SerializeField]
    private float speed, rotateSpeed, stepSpeed;
    private Action execution;
    [SerializeField]
    private Transform cameraSocket, focusCarousel, birdSocket;
    Camera camera;
    [SerializeField]
    private bool testMode;
    void Start(){
        camera = Camera.main;
        characterController = GetComponent<CharacterController>();
    }
    void Update()
    {
		movement = new Vector3(Input.GetAxis("Horizontal"),0,Input.GetAxis("Vertical"));
		direction = new Vector3(Input.GetAxis("Mouse X"),0, Input.GetAxis("Mouse Y"));
		transform.Rotate(0, movement.z - direction.z, 0);
		characterController.Move(transform.TransformDirection(Vector3.forward) * (movement.z + direction.z + 2.5f) * speed);
		birdSocket.localRotation = Quaternion.RotateTowards(birdSocket.localRotation,Quaternion.Euler((movement.z + direction.z)*22.5f,0, (movement.z - direction.z)*-22.5f),Time.deltaTime * stepSpeed);
		//birdSocket.localRotation = Quaternion.Euler((movement.z + direction.z)*22.5f,0, (movement.z - direction.z)*-22.5f);
    }
}
