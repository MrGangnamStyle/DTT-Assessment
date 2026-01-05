using UnityEngine;

public class ControllablePlayer : MonoBehaviour
{
    Rigidbody playerRB;

    [SerializeField] float walkSpeed;
    [SerializeField] float turnSpeed;

    float xMove;
    float yMove;

    Vector3 direction;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        playerRB = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        xMove = Input.GetAxis("Horizontal");
        yMove = Input.GetAxis("Vertical");

        direction = new Vector3(xMove, 0, yMove);

        playerRB.linearVelocity = direction * walkSpeed;

        if (direction != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            Quaternion newRotation = Quaternion.RotateTowards(
                playerRB.rotation,
                targetRotation,
                turnSpeed * Time.fixedDeltaTime
            );

            playerRB.MoveRotation(newRotation);
        }
    }
}
