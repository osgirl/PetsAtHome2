using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class BirdController : MonoBehaviour
{
    private CharacterController characterController;
    [SerializeField]
    private Vector3 movement,TestMode1Pos, TestMode2Pos;
    [SerializeField]
    private float speed, rotateSpeed;
    private Action execution;
    [SerializeField]
    private Transform cameraSocket, focusCarousel;
    Camera camera;
    [SerializeField]
    private bool testMode;
    void Start(){
        camera = Camera.main;
        characterController = GetComponent<CharacterController>();
        execution = TestMode1;
    }
    void Update()
    {
        if (Input.GetButtonDown("Submit")){
            SetTestMode();
        } 
        execution.Invoke();
    }
    void SetTestMode(){
        if(testMode){
                execution = TestMode1;
                cameraSocket.localRotation = Quaternion.Euler(90,0,0);
                camera.gameObject.transform.localPosition = TestMode1Pos;
                camera.orthographic = true;
            } else {
                execution = TestMode2;
                cameraSocket.localRotation = Quaternion.Euler(25,0,0);
                camera.gameObject.transform.localPosition = TestMode2Pos;
                camera.orthographic = false;
                focusCarousel.localRotation = Quaternion.identity;
            }
            testMode = !testMode;
    }
    void TestMode1(){
        movement = new Vector3 (Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));
        movement *= speed;
        characterController.Move(transform.TransformDirection(movement));
        focusCarousel.LookAt(focusCarousel.position + transform.TransformDirection(new Vector3(Input.GetAxis("Mouse X"), 0, Input.GetAxis("Mouse Y"))));
    }
    void TestMode2(){
        transform.Rotate(0, Input.GetAxis("Horizontal") * rotateSpeed, 0);
        characterController.Move(transform.TransformDirection(Vector3.forward) * speed * Input.GetAxis("Vertical"));
        
    }
}
