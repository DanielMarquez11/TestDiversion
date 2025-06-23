using UnityEngine;

public class deliverySlot : MonoBehaviour
{
    /* --- not yet used ---
    [Header("Item->Slot Settings:")]
    [SerializeField]
    private GameObject[] itemsToInclude;
    [SerializeField]
    private bool enforceWhitelist;
    [SerializeField]
    private GameObject[] itemsToExclude;
    [SerializeField]
    private bool enforceBlacklist;

    private GameObject itemInSlot;
    */

    public bool masterOverrideMode = false;

    [Header("Gizmo Settings:")]
    [Range(1, 10)]
    [SerializeField]
    private int ExtendMultiplier = 1;

    //Slot Placement Variables
    private GameObject slotObject;
    private GameObject slotParentObject;
    private Collider slotParentCollider;

    private Vector3 parentToSlotVector;
    private Vector3 closestPoint;
    private bool alternateMode;
    private Vector3 slotToClosestPoint;
    private Vector3 alternateDirection;
    private Vector3 alternateColliderPoint;

    [HideInInspector]
    public Vector3 placementDirection1;
    [HideInInspector]
    public Vector3 placementDirection2;
    [HideInInspector]
    public Vector3 placementPoint;

    public bool forceUpright = false;

    [SerializeField]
    private bool invertDirection2 = false;

    private void initializeSlot()
    {
        if(slotObject == null)
        {
            slotObject = this.gameObject;
        }

        if(slotParentObject == null)
        {
            slotParentObject = slotObject.transform.parent.gameObject;
            slotParentCollider = slotParentObject.GetComponent<Collider>();
        }

        if(slotParentCollider == null)
        {
            slotParentObject = slotParentObject.transform.parent.gameObject;
            slotParentCollider = slotParentObject.GetComponent<Collider>();
        }
    }
    public void calculateSlotVectors()
    {
        parentToSlotVector = slotObject.transform.position - slotParentCollider.bounds.center;
        closestPoint = slotParentCollider.ClosestPoint(slotObject.transform.position);

        slotToClosestPoint = slotObject.transform.position - closestPoint;

        if(slotToClosestPoint.magnitude == 0f)
        {
            alternateMode = true;

            //Get the closest point on the object in each direction from the slot
            Vector3 outsideUp = slotParentCollider.ClosestPoint(slotObject.transform.position + (slotParentObject.transform.up * slotParentCollider.bounds.extents.magnitude));
            Vector3 outsideRight = slotParentCollider.ClosestPoint(slotObject.transform.position + (slotParentObject.transform.right * slotParentCollider.bounds.extents.magnitude));
            Vector3 outsideForward = slotParentCollider.ClosestPoint(slotObject.transform.position + (slotParentObject.transform.forward * slotParentCollider.bounds.extents.magnitude));
            Vector3 outsideDown = slotParentCollider.ClosestPoint(slotObject.transform.position + (-slotParentObject.transform.up * slotParentCollider.bounds.extents.magnitude));
            Vector3 outsideLeft = slotParentCollider.ClosestPoint(slotObject.transform.position + (-slotParentObject.transform.right * slotParentCollider.bounds.extents.magnitude));
            Vector3 outsideBack = slotParentCollider.ClosestPoint(slotObject.transform.position + (-slotParentObject.transform.forward * slotParentCollider.bounds.extents.magnitude));
            Vector3[] colliderPoints = { outsideUp, outsideRight, outsideForward, outsideDown, outsideLeft, outsideBack };

            //Get direction vectors compared to slot position
            Vector3 outsideUpToSlot = outsideUp - slotObject.transform.position;
            Vector3 outsideRightToSlot = outsideRight - slotObject.transform.position;
            Vector3 outsideForwardToSlot = outsideForward - slotObject.transform.position;
            Vector3 outsideDownToSlot = outsideDown - slotObject.transform.position;
            Vector3 outsideLeftToSlot = outsideLeft - slotObject.transform.position;
            Vector3 outsideBackToSlot = outsideBack - slotObject.transform.position;

            //Find the shortest direction with the shortest distance
            Vector3[] directions = { outsideUpToSlot, outsideRightToSlot, outsideForwardToSlot, outsideDownToSlot, outsideLeftToSlot, outsideBackToSlot };
            float[] distances = { outsideUpToSlot.magnitude, outsideRightToSlot.magnitude, outsideForwardToSlot.magnitude, outsideDownToSlot.magnitude, outsideLeftToSlot.magnitude, outsideBackToSlot.magnitude };

            float shortestDistance = directions[0].magnitude; // Initialize with the first value

            var shortestDistanceIndex = 0;
            for (int i = 1; i < distances.Length; i++)
            {
                if (Mathf.Min(shortestDistance, directions[i].magnitude) == directions[i].magnitude)
                {
                    shortestDistance = directions[i].magnitude;
                    shortestDistanceIndex = i;
                }
            }

            alternateDirection = directions[shortestDistanceIndex];
            alternateColliderPoint = colliderPoints[shortestDistanceIndex];

            placementDirection1 = alternateDirection.normalized;
            placementPoint = alternateColliderPoint;
        }
        else{
            alternateMode = false;
            placementDirection1 = slotToClosestPoint.normalized;
            placementPoint = closestPoint;
        }

        int modifyDirection2 = invertDirection2 ? -1 : 1;

        placementDirection2 = Vector3.Cross(placementDirection1, modifyDirection2 * -this.transform.right);

        if(placementDirection2.normalized.magnitude == 0f){
            placementDirection2 = Vector3.Cross(placementDirection1, modifyDirection2 * -this.transform.forward);
        }
    }

    void Start()
    {
        initializeSlot();
        calculateSlotVectors();
    }

    private void OnDrawGizmosSelected()
    {
        initializeSlot();
        calculateSlotVectors();

        Gizmos.color = Color.green;

        float direction2GizmoLength;

        if (alternateMode)
        {
            Gizmos.DrawSphere(alternateColliderPoint, 0.001f);
            Gizmos.color = Color.blue;
            Gizmos.DrawRay(slotObject.transform.position, alternateDirection * ExtendMultiplier);
            direction2GizmoLength = alternateDirection.magnitude;
        }
        else
        {
            Gizmos.DrawRay(closestPoint, slotToClosestPoint * ExtendMultiplier);
            Gizmos.DrawSphere(this.transform.position, 0.001f);
            direction2GizmoLength = slotToClosestPoint.magnitude;
        }

        Gizmos.DrawSphere(closestPoint, 0.0025f);
        Gizmos.color = Color.green;
        Gizmos.DrawRay(slotParentCollider.bounds.center, parentToSlotVector);

        Gizmos.color = Color.yellow;
        Gizmos.DrawRay(placementPoint, placementDirection2 * direction2GizmoLength * ExtendMultiplier);
    }
}
