using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class Walk : MonoBehaviour
{
    [SerializeField] private Transform leftFootTarget;
    [SerializeField] private Transform rightFootTarget;
    [SerializeField] private Transform sepine;
    [SerializeField] private Transform hips;
    [SerializeField] private AnimationCurve horizontalCurve;
    [SerializeField] private AnimationCurve verticalCurve;
    [SerializeField] private float hipOffset;  
    [SerializeField] private float raycastDistance = 2f;  
    [SerializeField] private float moveSpeed = 5f; 
    [SerializeField] private float rotationSmoothTime; 
    [SerializeField] private LayerMask ground;

    private Vector3 leftTargetOffSet;
    private Vector3 rightTargetOffSet;
    private Vector3 lastPosition; 
    
    private float leftLegLast = 0;
    private float rightLegLast = 0;
    private float leftFootDistance;
    private float rightFootDistance;
    private float currentAngle; 
    private float currentAngleVelocity;
    private float lastHipsHeight; 

    private Camera cam;
     
    private bool isMoving;  
    

    private CharacterController characterController; 

    private void Start()
    {
      
        leftFootDistance = hips.position.y - leftFootTarget.position.y;
        rightFootDistance = hips.position.y - rightFootTarget.position.y;
        leftTargetOffSet = leftFootTarget.localPosition;
        rightTargetOffSet = rightFootTarget.localPosition;

        
        lastHipsHeight = hips.position.y;
        lastPosition = transform.position;  

       
        characterController = GetComponent<CharacterController>();
        cam=Camera.main;
    }

    private void Update()
    {
        HandleMovement(); 
        AlignHipsWithGround(); 

        if (isMoving)
        {
            AnimateFeet(); 
        }
        else
        {
            ResetFeetToGround();
        }
    }

    private void HandleMovement()
    {
        Vector3 movement = new Vector3(Input.GetAxisRaw("Horizontal"), 0, Input.GetAxisRaw("Vertical")).normalized;
        if (movement.magnitude >= 0.1f)
        {
            isMoving = true;
            float targetAngle = Mathf.Atan2(movement.x, movement.z) * Mathf.Rad2Deg + cam.transform.eulerAngles.y;
            currentAngle = Mathf.SmoothDampAngle(currentAngle, targetAngle, ref currentAngleVelocity, rotationSmoothTime);
            transform.rotation = Quaternion.Euler(0, currentAngle, 0);
            Vector3 rotatedMovement = Quaternion.Euler(0, targetAngle, 0) * Vector3.forward;
            characterController.Move(rotatedMovement * moveSpeed * Time.deltaTime);
        }
        else
        {
            isMoving = false;
        }
    }


    private void AnimateFeet()
    {
        float leftLegForwardMovement = horizontalCurve.Evaluate(Time.time);
        float rightLegForwardMovement = horizontalCurve.Evaluate(Time.time - 0.6f);

      
        leftFootTarget.localPosition = leftTargetOffSet + this.transform.InverseTransformVector(leftFootTarget.forward) * leftLegForwardMovement +
                                       this.transform.InverseTransformVector(leftFootTarget.up) * verticalCurve.Evaluate(Time.time + 0.3f);
        rightFootTarget.localPosition = rightTargetOffSet + this.transform.InverseTransformVector(rightFootTarget.forward) * rightLegForwardMovement +
                                        this.transform.InverseTransformVector(rightFootTarget.up) * verticalCurve.Evaluate(Time.time - 0.3f);

     
        AdjustFootToGround(leftFootTarget, leftLegForwardMovement, ref leftLegLast);
        AdjustFootToGround(rightFootTarget, rightLegForwardMovement, ref rightLegLast);
    }

    private void AdjustFootToGround(Transform footTarget, float legForwardMovement, ref float legLast)
    {
        float legDirection = legForwardMovement - legLast;
        if (legDirection < 0)
        {
            RaycastHit hit;
            if (Physics.Raycast(footTarget.position + footTarget.up, -footTarget.up, out hit, raycastDistance,ground))
            {
                footTarget.position = hit.point;
            }
        }
        legLast = legForwardMovement;
    }

    private void ResetFeetToGround()
    {
        RaycastHit hit;

       
        if (Physics.Raycast(leftFootTarget.position + Vector3.up * raycastDistance, Vector3.down, out hit, raycastDistance,ground))
        {
            leftFootTarget.position = new Vector3(leftFootTarget.position.x, hit.point.y, leftFootTarget.position.z);
        }

        RaycastHit hit2;

      
        if (Physics.Raycast(rightFootTarget.position + Vector3.up * raycastDistance, Vector3.down, out hit2, raycastDistance,ground))
        {
            rightFootTarget.position = new Vector3(rightFootTarget.position.x, hit2.point.y, rightFootTarget.position.z);
        }

       
        leftFootTarget.localPosition = new Vector3(leftTargetOffSet.x, leftFootTarget.localPosition.y, leftTargetOffSet.z);
        rightFootTarget.localPosition = new Vector3(rightTargetOffSet.x, rightFootTarget.localPosition.y, rightTargetOffSet.z);

       
        if (leftFootTarget.position.y < rightFootTarget.position.y)
        {
            rightFootTarget.position = new Vector3(rightFootTarget.position.x, leftFootTarget.position.y,
                rightFootTarget.position.z);
        }
        else
        {
            leftFootTarget.position = new Vector3(leftFootTarget.position.x, rightFootTarget.position.y,
                leftFootTarget.position.z);
        }
    }

    private void AlignHipsWithGround()
    {
        RaycastHit hipsHit;
        if (Physics.Raycast(hips.position + Vector3.up * raycastDistance, Vector3.down, out hipsHit, Mathf.Infinity,ground))
        {
            float currentHeight = hips.position.y;
            float hitHeight = hipsHit.point.y;

           
            float heightDifference = currentHeight - hitHeight;
            if (heightDifference != 0)
            {
                hips.position = new Vector3(hips.position.x, hips.position.y - heightDifference + hipOffset, hips.position.z);
                lastHipsHeight = hips.position.y;
            }
        }
    }
}
