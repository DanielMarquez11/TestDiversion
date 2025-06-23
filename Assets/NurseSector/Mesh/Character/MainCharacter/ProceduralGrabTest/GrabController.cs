using UnityEngine;

public class GrabController : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public Animator animator;

    [Range(1,5)]
    public int fingerNum;

    void Start()
    {
        //animator = this.GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        //Add a test input (using getkeydown), to trigger the Finger1Touch animation event
        if (Input.GetKeyDown(KeyCode.F))
        {
            animator.SetBool("Finger1Touch", true);
            animator.SetBool("Finger2Touch", true);
            animator.SetBool("Finger4Touch", true);
            animator.SetBool("Finger5Touch", true);
        }
    }

    void OnTriggerEnter(Collider other){
        
        //make sure the layer of the object is the InteractableItem layer
        // LayerMask.NameToLayer("InteractableItem");

        if(other.gameObject.tag == "FingerTouchCollider"){

            //Find if the animator state is the grab state
            //animator.GetCurrentAnimatorStateInfo(0).IsName("Grab");
            //Debug.Log("Finger : " + fingerNum + "  |  Trigger Enter: " + other.name);
            animator.SetBool($"Finger{fingerNum}Touch", true);
        }

    }
    void OnTriggerStay(Collider other) {
        if(other.gameObject.tag == "FingerTouchCollider"){
            //Debug.Log("Finger : " + fingerNum + "  |  Trigger Stay: " + other.name);
            if(!animator.GetBool($"Finger{fingerNum}Touch")){
                animator.SetBool($"Finger{fingerNum}Touch", true);
            }
        }
    }
}
