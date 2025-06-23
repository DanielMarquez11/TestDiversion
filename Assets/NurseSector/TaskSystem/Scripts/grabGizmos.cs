using System;
using System.Collections;
using System.Linq;
using UnityEngine;

public class grabGizmos : MonoBehaviour
{
    private float bounds;
    private Vector3 parentCenter;
    private Vector3 grabOnBounds;
    private Vector3 grabTransform;

    private Vector3 grabDirection;

    private Vector3 itemTop;
    private Vector3 upDirection;

    private Vector3 newItemTop;
    private Vector3 newUpDirection;

    private Vector3 newItemFront;
    private Vector3 frontDirection;

    [Range(1, 100)]
    [SerializeField]
    private int extendMultiplier = 1;

    private bool isPickedUp = false;
    private bool isAligned = false;
    private int rotationCount = 0;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        CalculateVectors();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (isPickedUp && !isAligned)
        {
            if (this.transform.parent.transform.parent != null)
                alignGrabVector(this.transform.parent.gameObject, this.transform.parent.transform.parent.gameObject);
        }
    }

    public void beginPickup()
    {
        if (!isPickedUp)
        {
            isPickedUp = true;
            isAligned = false;
            rotationCount = 0;
            alignInHand();
        }
    }

    public void drop()
    {
        if (isPickedUp)
        {
            isPickedUp = false;
            isAligned = false;
            rotationCount = 0;
        }
    }

    void CalculateVectors()
    {
        //get the distance from the center of the object to the bounding box of the object in the direction of the transfrom of this object relative to its parent
        bounds = this.transform.parent.GetComponent<Collider>().bounds.extents.magnitude;

        //find the center of the parents collider
        parentCenter = this.transform.parent.GetComponent<Collider>().bounds.center;

        //a vector position that is on the bounds of the object, to find the direction that should be facing the palm
        grabOnBounds = parentCenter + ((this.transform.position - parentCenter).normalized * bounds);
        grabTransform = this.transform.parent.GetComponent<Collider>().ClosestPoint(grabOnBounds);
        grabDirection = (grabOnBounds - grabTransform).normalized;

        // Calculate itemTop to be a vector from the object to the up direction of the grab helper
        itemTop = parentCenter + this.transform.up.normalized;
        upDirection = (itemTop - this.transform.parent.transform.position).normalized;

        //define new vector3 for newItemFront, which is at the front of the object in relation to the top as itemTop and the grabPoint as right
        newItemFront = this.transform.parent.transform.position + Vector3.Cross(grabDirection, upDirection);
        frontDirection = (newItemFront - this.transform.parent.transform.position).normalized;

        //define new vector3 for newItemTop, which is at the top of the object in relation to the front as newItemFront and the grabPoint as right to ensure that top is perpendicular to front and grabDirection
        newItemTop = this.transform.parent.transform.position - Vector3.Cross(grabDirection, frontDirection);
        newUpDirection = (newItemTop - this.transform.parent.transform.position).normalized;
    }

    void OnDrawGizmosSelected()
    {
        if (!isPickedUp && !isAligned)
        {
            CalculateVectors();
        }

        Gizmos.color = Color.red;

        //Gizmos.DrawLine(grabOnBounds, grabTransform);

        Gizmos.color = Color.red;
        Gizmos.DrawRay(grabTransform, grabDirection * bounds * extendMultiplier);

        Gizmos.DrawSphere(grabOnBounds, 0.01f);
        Gizmos.DrawSphere(grabTransform, 0.01f);

        Gizmos.color = Color.yellow;
        Gizmos.DrawSphere(this.transform.parent.GetComponent<Collider>().ClosestPoint(newItemFront), 0.01f);
        Gizmos.DrawRay(grabTransform, frontDirection * bounds * extendMultiplier);

        //Gizmos.color = Color.cyan;
        //Gizmos.DrawRay(gameObject.transform.parent.position, grabDirection * bounds * extendMultiplier);

        Gizmos.color = Color.green;
        Gizmos.DrawRay(grabTransform, newUpDirection * bounds * extendMultiplier);
        Gizmos.DrawSphere(this.transform.parent.GetComponent<Collider>().ClosestPoint(newItemTop), 0.01f);
    }

    public void alignInHand()
    {
        CalculateVectors();
        GameObject pickupItem = gameObject.transform.parent.gameObject;
        GameObject handTransform = pickupItem.transform.parent.gameObject;
        CalculateVectors();
        resetLocation(pickupItem);
        CalculateVectors();

        alignGrabVector(pickupItem, handTransform);

    }

    private void resetLocation(GameObject pickupItem)
    {
        pickupItem.transform.rotation = Quaternion.identity;
        pickupItem.transform.localRotation = Quaternion.identity;
        CalculateVectors();
    }

    private void alignGrabVector(GameObject pickupItem, GameObject handTransform)
    {

        rotationCount++;
        CalculateVectors();

        Vector3 targetForwardWorld = -handTransform.transform.forward;
        Vector3 targetUpWorld = handTransform.transform.up;
        Quaternion targetWorldRotation = Quaternion.LookRotation(targetForwardWorld, targetUpWorld);
        Quaternion localOffsetRotation = Quaternion.LookRotation(grabDirection, newUpDirection);
        pickupItem.transform.rotation = targetWorldRotation * Quaternion.Inverse(localOffsetRotation) * pickupItem.transform.rotation;


        CalculateVectors();

        if (Vector3.Dot(grabDirection, -handTransform.transform.forward) < .999f
        || Vector3.Dot(newUpDirection, handTransform.transform.up) < .999f)
        {
            //Debug.Log("Not aligned | grab: " + Vector3.Dot(grabDirection, -handTransform.transform.forward) + " | up: " + Vector3.Dot(newUpDirection, handTransform.transform.up) + " | forward: " + Vector3.Dot(frontDirection, handTransform.transform.right));
            isAligned = false;
        }
        else
        {
            Debug.Log("Aligned in " + rotationCount + " rotation cycles | grab: " + Vector3.Dot(grabDirection, -handTransform.transform.forward) + " | up: " + Vector3.Dot(newUpDirection, handTransform.transform.up) + " | forward: " + Vector3.Dot(frontDirection, handTransform.transform.right));
            isAligned = true;
            positionToGrabTransform();
        }

    }

    void positionToGrabTransform()
    {
        CalculateVectors();

        //targetDistance between hand and object with regard to grabOnBounds
        var initialOffset = gameObject.transform.parent.transform.parent.position - (gameObject.transform.parent.position + (gameObject.transform.parent.transform.parent.position - grabOnBounds));
        var initialOffsetToGrabTransform = gameObject.transform.parent.transform.parent.position - (gameObject.transform.parent.position + (gameObject.transform.parent.transform.parent.position - grabTransform));     //targetDistance between hand and object with regard to grabTransform
        CalculateVectors();
        StartCoroutine(MoveObjectToHand(gameObject.transform.parent.transform.parent, initialOffset, initialOffsetToGrabTransform));
    }

    IEnumerator MoveObjectToHand(Transform playerHand, Vector3 initialOffset, Vector3 finalOffset)
    {
        float moveTime = .2f; // Time to complete the movement

        float elapsedTime = 0f;

        Vector3 startPosition = playerHand.position - initialOffset;
        Vector3 finalPosition = playerHand.position - finalOffset;

        while (elapsedTime < moveTime)
        {
            CalculateVectors();

            startPosition = playerHand.transform.position - initialOffset;
            finalPosition = playerHand.transform.position - finalOffset;

            gameObject.transform.parent.position = Vector3.Lerp(startPosition, finalPosition, elapsedTime / moveTime);
            elapsedTime += Time.deltaTime;
            yield return null; // Wait for the next frame
        }

        gameObject.transform.parent.position = playerHand.transform.position - finalOffset;

        var animBegin = FindObjectsByType<ItemInteraction>(FindObjectsSortMode.None).Select(x => x.GetComponent<ItemInteraction>()).FirstOrDefault();
        if (animBegin != null && this.gameObject.transform.parent.gameObject.transform.parent != null)
        {
            animBegin.beginPickupAnimation();
            animBegin.toggleHeldItemColliders(false);
        }

    }
}
