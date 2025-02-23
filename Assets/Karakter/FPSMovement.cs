using UnityEngine;

public class FPSMovement : MonoBehaviour
{
    public CharacterController controller;
    public float speed = 5f;
    public float sprintSpeed = 8f;  // Koþma hýzý
    public float crouchSpeed = 2.5f; // Eðilme hýzý
    public float gravity = -9.81f;
    public float jumpHeight = 1.5f;
    public float crouchHeight = 1f; // Eðildiðinde karakterin yüksekliði
    public float normalHeight = 2f; // Normal karakter yüksekliði

    private Vector3 velocity;
    private bool isGrounded;

    void Update()
    {
        isGrounded = controller.isGrounded;
        if (isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;
        }

        float moveX = Input.GetAxis("Horizontal");
        float moveZ = Input.GetAxis("Vertical");

        bool isSprinting = Input.GetKey(KeyCode.LeftShift) && isGrounded;
        bool isCrouching = Input.GetKey(KeyCode.LeftControl);

        float currentSpeed = speed;
        if (isSprinting)
        {
            currentSpeed = sprintSpeed;
        }
        else if (isCrouching)
        {
            currentSpeed = crouchSpeed;
            controller.height = Mathf.Lerp(controller.height, crouchHeight, Time.deltaTime * 10);
        }
        else
        {
            controller.height = Mathf.Lerp(controller.height, normalHeight, Time.deltaTime * 10);
        }

        Vector3 move = transform.right * moveX + transform.forward * moveZ;
        controller.Move(move * currentSpeed * Time.deltaTime);

        if (Input.GetButtonDown("Jump") && isGrounded && !isCrouching)
        {
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
        }

        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }
}
