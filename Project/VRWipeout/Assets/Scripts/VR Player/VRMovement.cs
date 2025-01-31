﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;

public class VRMovement : MonoBehaviour
{
    [Header("Movement")]
    public float Speed;
    public float RotationSpeed;

    [Header("Sprinting")]
    public GameObject SweatEffect;
    public GameObject SpeedEffect;
    private float SprintValue;
    private float OrignalValue;
    public bool canSprint;
    public float Cooldown;
    public float SpintingTime;
    public bool isSprinting;

    [Header("Ground Dectection")]
    public Transform groundCheckTransform;
    public LayerMask groundMask;
    public float groundDistance;
    public bool isGrounded;

    [Header("Jumping")]
    public float JumpForce;
    

    [Header("XR")]
    public XRNode inputSource;
    private Vector2 inputAxis;
    private Vector2 rotationAxis;
    public XRNode RotationInput;
    private XRNode jumpInput;
    private XRRig Rig;
    private Rigidbody RB;

    [Header("Transforms")]
    public Transform Cam;

    private void Start()
    {
        //Access our private components
        Rig = GetComponent<XRRig>();
        RB = GetComponent<Rigidbody>();

        jumpInput = inputSource;

        //Speed Variables
        OrignalValue = Speed;
        SprintValue *= Speed;
    }

    private void Update()
    {
        FollowHeadset();

        InputDevice device = InputDevices.GetDeviceAtXRNode(inputSource);
        device.TryGetFeatureValue(CommonUsages.primary2DAxis, out inputAxis);

        InputDevice rotationDevice = InputDevices.GetDeviceAtXRNode(RotationInput);
        rotationDevice.TryGetFeatureValue(CommonUsages.primary2DAxis, out rotationAxis);
    }

    private void FixedUpdate()
    {
        //Position
        Quaternion headYaw = Quaternion.Euler(0, Rig.cameraGameObject.transform.eulerAngles.y, 0);
        Vector3 direction = headYaw * new Vector3(inputAxis.x, RB.velocity.y, inputAxis.y) * Time.fixedDeltaTime * Speed;
        RB.MovePosition(RB.position + direction);

        //Rotation
        Vector3 rotation = rotationAxis * Time.fixedDeltaTime * RotationSpeed;
        transform.Rotate(transform.localRotation.x, rotation.x, transform.localRotation.x);

        //Jump
        InputDevice jumpController = InputDevices.GetDeviceAtXRNode(jumpInput);
        jumpController.TryGetFeatureValue(CommonUsages.primaryButton, out bool jumpPressed);
        if (jumpPressed)
            Jump();

        groundCheck();
    }

    public bool Jump()
    {
        if (isGrounded)
        {
            RB.AddForce(Vector3.up * JumpForce * Time.fixedDeltaTime, ForceMode.Impulse);
            return true;
        }
        else
        {
            return false;
        }
    }

    void FollowHeadset()
    {
        CapsuleCollider capsuleCollider = GetComponent<CapsuleCollider>();
        capsuleCollider.height = Rig.cameraInRigSpaceHeight;
        Cam.transform.localPosition = new Vector3(0, 1, 0);
        Vector3 capsuleCenter = transform.InverseTransformPoint(Rig.cameraGameObject.transform.position);
        capsuleCollider.center = new Vector3(capsuleCenter.x, capsuleCollider.height / 2 + capsuleCollider.radius, capsuleCenter.z);
    }  

    //GroundDetection
    void groundCheck()
    {
        CapsuleCollider capsuleCollider = GetComponent<CapsuleCollider>();
        groundCheckTransform.transform.position = capsuleCollider.bounds.min;

        isGrounded = Physics.CheckSphere(groundCheckTransform.position, groundDistance, groundMask);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.transform.CompareTag("Death"))
        {
            var respawnscript = FindObjectOfType<Respawn>();
            respawnscript.PlayerDeath();
        }
    }

    //Camera Effects
    public void Sprinting(bool on)
    {
        if (on == true)
        {
            isSprinting = true;
            Speed = SprintValue;
            CamEffects(SpeedEffect, Color.white, true);
            SpintingTime += 0.01f;

            if (SpintingTime > 10)
            {
                StartCoroutine(SprintTime());
            }
        }
        if (on == false)
        {
            isSprinting = false;
            Speed = OrignalValue;
            CamEffects(SpeedEffect, Color.white, false);
            SpintingTime = 0f;
        }
    }
    IEnumerator SprintTime()
    {
        CamEffects(SweatEffect, Color.blue, true);
        CamEffects(SpeedEffect, Color.blue, false);
        Speed = OrignalValue;
        canSprint = false;

        yield return new WaitForSeconds(10);
        canSprint = true;
        CamEffects(SweatEffect, Color.blue, false);
    }

    //CameraEffects
    public void CamEffects(GameObject Effect, Color CamColor, bool active)
    {
        Effect.SetActive(active);
    }
}
