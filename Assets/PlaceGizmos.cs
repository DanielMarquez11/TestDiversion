using System;
using UnityEngine;

public class PlaceGizmos : MonoBehaviour
{
    [Header("Gizmo Settings:")]
    [Range(1, 100)]
    [SerializeField]
    private int extendMultiplier = 1;

    [Header("Base Axis Options:")]

    [SerializeField] private bool invertUp;
    [SerializeField] private bool invertForward;
    [SerializeField] private bool invertRight;

    private Vector3 objectCenter;
    private Vector3 topFace;
    private Vector3 frontFace;
    private Vector3 rightFace;
    private Vector3 topPoint;
    private Vector3 frontPoint;
    private Vector3 rightPoint;
    private Vector3 topDistance;
    private Vector3 frontDistance;
    private Vector3 rightDistance;
    private Vector3 placementDirection1;

    private Vector3 placementPoint;

    private Vector3 placementDirection2;

    [Header("Placement Axis Override Options:")]
    [SerializeField] private bool overrideStandUpright = false;
    [SerializeField] private bool overrideSecondaryDirection = false;
    [SerializeField] private bool masterOverrideManualMode = false;
    [Tooltip("MasterOverrideManualMode needs to be on for this to work. Forces the object to use the centerpoint of the collider instead of the objects transform pivot.")]
    [SerializeField] private bool forceUseColliderCenter = false;


    private bool isPlaced = false;
    private bool isAligned = false;
    private int rotationCount = 0;

    private GameObject slotObject;

    private Vector3 placementDistance1;
    private Vector3 placementDistance2;

    private bool withinStandingRatio = false;
    private float standingRatio = 0.5f;

    private bool lockedInSlot = false;

    private void Start()
    {
        calculateFaces();
    }

    void FixedUpdate()
    {
        if (slotObject != null && slotObject.GetComponent<deliverySlot>() != null)
        {
            if(masterOverrideManualMode == false && slotObject.GetComponent<deliverySlot>().masterOverrideMode == false)
            {
                if (isPlaced && !isAligned)
                {
                    alignInslot();
                }
            }
        }
    }

    public void beginPlace(GameObject slot, bool lockInSlot = false)
    {
        if(isPlaced)
        {
            return;
        }
    
        lockedInSlot = lockInSlot;
        isPlaced = true;
        isAligned = false;
        rotationCount = 0;
        slotObject = slot;

        if(masterOverrideManualMode == true || slotObject.GetComponent<deliverySlot>().masterOverrideMode == true)
        {
            this.transform.rotation = slotObject.transform.rotation;
            if(forceUseColliderCenter)
            {
                calculateFaces();
                var offset = slotObject.transform.position - objectCenter;
                this.transform.position += offset;
            }
            else
            {
                this.transform.position = slotObject.transform.position;
            }
            return;
        }

        if(slotObject.GetComponent<deliverySlot>().forceUpright)
        {
            overrideStandUpright = !overrideStandUpright;
        }
        slotObject.GetComponent<deliverySlot>().calculateSlotVectors();
        beginAlignment();
    }

    private void lockInSlot()
    {
        this.gameObject.GetComponent<Collider>().enabled = false; //disable collider when locking in slot
        this.gameObject.layer = LayerMask.NameToLayer("Default");
        this.gameObject.tag = "Untagged";
    }

    private void beginAlignment()
    {
        calculateFaces();
        resetRotation();
        calculateFaces();
        alignInslot();
    }

    private void resetRotation()
    {
        this.transform.rotation = Quaternion.identity;
        this.transform.localRotation = Quaternion.identity;
        calculateFaces();
    }

    public void unPlace()
    {
        if(isPlaced)
        {
            isPlaced = false;
            isAligned = false;
            rotationCount = 0;
            slotObject = null;
        }
    }

    private void OnDrawGizmosSelected()
    {
        Collider objectCollider = GetComponent<Collider>();
        calculateFaces();
        if (objectCollider != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(objectCenter, 0.005f);
            Gizmos.color = Color.cyan;
            Gizmos.DrawSphere(topPoint, 0.005f);
            Gizmos.color = Color.magenta;
            Gizmos.DrawSphere(frontPoint, 0.005f);
            Gizmos.color = Color.black;
            Gizmos.DrawSphere(rightPoint, 0.005f);
            Gizmos.color = Color.red;
            Gizmos.DrawRay(objectCenter, topDistance);
            Gizmos.color = Color.green;
            Gizmos.DrawRay(objectCenter, frontDistance);
            Gizmos.color = Color.blue;
            Gizmos.DrawRay(objectCenter, rightDistance);

            Gizmos.color = Color.yellow;
            Gizmos.DrawSphere(placementPoint, 0.0075f);
            Gizmos.color = Color.yellow;
            Gizmos.DrawRay(placementPoint, placementDistance1 * extendMultiplier);
            Gizmos.color = Color.blue;
            Gizmos.DrawRay(placementPoint, placementDistance2 * extendMultiplier);
        }
    }

    private void calculateFaces()
    {
        Collider objectCollider = GetComponent<Collider>();

        Physics.SyncTransforms();

        int modifyUp = invertUp ? -1 : 1;
        int modifyForward = invertForward ? -1 : 1;
        int modifyRight = invertRight ? -1 : 1;

        objectCenter = objectCollider.bounds.center;
        topFace = objectCenter + (modifyUp * -this.transform.up * objectCollider.bounds.extents.magnitude);
        frontFace = objectCenter + (modifyForward * -this.transform.forward * objectCollider.bounds.extents.magnitude);
        rightFace = objectCenter + (modifyRight * -this.transform.right * objectCollider.bounds.extents.magnitude);

        topPoint = objectCollider.ClosestPoint(topFace);
        frontPoint = objectCollider.ClosestPoint(frontFace);
        rightPoint = objectCollider.ClosestPoint(rightFace);

        topDistance = topPoint - objectCenter;
        frontDistance = frontPoint - objectCenter;
        rightDistance = rightPoint - objectCenter;

        var longestDistance = Mathf.Max(topDistance.magnitude, frontDistance.magnitude, rightDistance.magnitude);
        var middleDistance = Mathf.Min(Mathf.Max(topDistance.magnitude, frontDistance.magnitude), Mathf.Max(topDistance.magnitude, rightDistance.magnitude), Mathf.Max(frontDistance.magnitude, rightDistance.magnitude));
        var shortestDistance = Mathf.Min(topDistance.magnitude, frontDistance.magnitude, rightDistance.magnitude);

        withinStandingRatio = ((middleDistance + shortestDistance) / 2) >= (longestDistance * standingRatio);

        if((overrideStandUpright == false && withinStandingRatio == false) || (overrideStandUpright == true && withinStandingRatio == true))
        {
            findPrimaryPlacementVector(shortestDistance);
        }
        else{
            findPrimaryPlacementVector(longestDistance);
        }

        if (!overrideSecondaryDirection)
        {
            findSecondaryPlacementVector(middleDistance, false);
        }
        else
        {
            if((overrideStandUpright == false && withinStandingRatio == false) || (overrideStandUpright == true && withinStandingRatio == true))
            {
                findSecondaryPlacementVector(longestDistance, true);
            }
            else
            {
                findSecondaryPlacementVector(shortestDistance, true);
            }
        }

        placementDistance1 = placementDirection1;
        placementDistance2 = placementDirection2;

        placementDirection1 = placementDirection1.normalized;
        placementDirection2 = placementDirection2.normalized;
        
    }
    void findPrimaryPlacementVector(float compareDistance)
    {
        if (compareDistance == topDistance.magnitude)
        {
            placementDirection1 = topDistance;
            placementPoint = topPoint;
            return;
        }

        if (compareDistance == frontDistance.magnitude)
        {
            placementDirection1 = frontDistance;
            placementPoint = frontPoint;
            return;
        }

        if (compareDistance == rightDistance.magnitude)
        {
            placementDirection1 = rightDistance;
            placementPoint = rightPoint;
            return;
        }
    }

    void findSecondaryPlacementVector(float compareDistance, bool alternateDirection)
    {
        if (compareDistance == topDistance.magnitude)
        {
            placementDirection2 = topDistance;
            if (placementDirection1 != placementDirection2 && !alternateDirection)
            {
                return;
            }
        }
        if (compareDistance == frontDistance.magnitude)
        {
            placementDirection2 = frontDistance;
            if (placementDirection1 != placementDirection2 && !alternateDirection)
            {
                return;
            }
        }
        if (compareDistance == rightDistance.magnitude)
        {
            placementDirection2 = rightDistance;
            if (placementDirection1 != placementDirection2 && !alternateDirection)
            {
                return;
            }
        }
    }

    private void alignInslot()
    {
        rotationCount++;
        calculateFaces();

        var slotPlacementDirection1 = slotObject.GetComponent<deliverySlot>().placementDirection1;
        var slotPlacementDirection2 = slotObject.GetComponent<deliverySlot>().placementDirection2;

        Vector3 targetForwardWorld = -slotPlacementDirection1;
        Vector3 targetUpWorld = slotPlacementDirection2;
        Quaternion targetWorldRotation = Quaternion.LookRotation(targetForwardWorld, targetUpWorld);
        Quaternion localOffsetRotation = Quaternion.LookRotation(placementDirection1, placementDirection2);
        this.transform.rotation = targetWorldRotation * Quaternion.Inverse(localOffsetRotation) * this.transform.rotation;

        calculateFaces();

        if (Vector3.Dot(placementDirection1, -slotPlacementDirection1) < .99f
        || Vector3.Dot(placementDirection2, slotPlacementDirection2) < .99f
        )
        {
            if(rotationCount > 50){
                Debug.Log("Failed to align in slot after too many rotations, placing in slot anyway");
                isAligned = true;
                placeInSlotWithOffset();
            }
        }
        else
        {
            Debug.Log("Aligned in: " + rotationCount + " rotation cycles | placement1: " + Vector3.Dot(placementDirection1, -slotPlacementDirection1) + " | placement2: " + Vector3.Dot(placementDirection2, slotPlacementDirection2));
            isAligned = true;
            placeInSlotWithOffset();
        }
    }

    void placeInSlotWithOffset()
    {
        var offset2 = slotObject.transform.position - placementPoint;
        this.transform.position += offset2;
        if(lockedInSlot)
        {
            lockInSlot();
        }
    }
}