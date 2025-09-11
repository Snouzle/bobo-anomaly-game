using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    public float speed = 5f;
    private CharacterController controller;
    private InputAction moveAction;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        var inputActions = new InputSystem_Actions();
        inputActions.Player.Enable();
        moveAction = inputActions.Player.Move;
    }

    void Update()
    {
        Vector2 moveInput = moveAction.ReadValue<Vector2>();
        float moveX = moveInput.x;
        float moveZ = moveInput.y;

        Vector3 move = transform.right * moveX + transform.forward * moveZ;
        controller.Move(move * speed * Time.deltaTime);
    }
}