//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;

//public class PlayerMovementScript : MonoBehaviour
//{
//    public float speed = 6.0f;
//    public float gravity = -9.8f;

//    private Vector3 velocity;
//    private CharacterController controller;

//    // Start is called before the first frame update
//    void Start()
//    {
//        controller = GetComponent<CharacterController>();
//    }

//    // Update is called once per frame
//    void Update()
//    {
//        float moveX = Input.GetAxis("Horizontal");
//        float moveZ = Input.GetAxis("Vertical");

//        Vector3 move = transform.right * moveX + transform.forward * moveZ;

//        controller.Move(move * speed * Time.deltaTime);

//        if (controller.isGrounded && velocity.y < 0)
//        {
//            velocity.y = -2f; // Keeps the player grounded
//        }

//        velocity.y += gravity * Time.deltaTime;
//        controller.Move(velocity * Time.deltaTime);
//    }
//}
