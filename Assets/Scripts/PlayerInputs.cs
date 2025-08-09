using System;
using UnityEditor.Timeline;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInputs : MonoBehaviour
{
    [SerializeField] private float _moveSpeed;
    private Vector2 _moveDirection;
    [SerializeField] private float jumpHeight;
    [SerializeField] private float gravity;
    [SerializeField] private Rigidbody rb;
    private bool _isSprinting;

    public bool jumpPressed;
    public bool jumpJustPressed;

    public GameObject facingDirection;

    // Update is called once per frame
    void Update()
    {
        Vector3 move = new Vector3(_moveDirection.x, 0, _moveDirection.y) *
                              (Time.deltaTime * _moveSpeed * (_isSprinting ? 1.5f : 1));

        move = facingDirection.transform.TransformDirection(move);

        rb.linearVelocity += move;

        


        if (jumpJustPressed)
        {
            rb.linearVelocity = new Vector3(rb.linearVelocity.x, jumpHeight, rb.linearVelocity.z);
            jumpJustPressed = false;
        }

        if (transform.position.y > 1)
        {
            jumpHeight = rb.linearVelocity.y - gravity * Time.deltaTime;
        }
        else if(transform.position.y == 1)
        {
            jumpHeight = 5;
        }

    }
    

    // OnMove New Input // From SendMessages
    public void OnMove(InputValue input)
    {
        // Get the input value out of my input action
        _moveDirection = input.Get<Vector2>().normalized;
    }

    public void OnJump(InputValue input)
    {
        bool isPressedNow = input.isPressed;


        // jumpJustPressed is true exactly on the frame the button transitions from up to down
        jumpJustPressed = isPressedNow && !jumpPressed;
        jumpPressed = isPressedNow;
    }

    public void OnSprint(InputValue input)
    {
        print("Your Sprinting");
        _isSprinting = input.isPressed;
    }

    public void OnShoot(InputValue input)
    {
        print("BANG BANG! you ded");
    }

    #region Inspect
    //public void OnInspect(InputValue input)
    //{
    //    _rotateDirection = input.Get<Vector2>();

    //    Vector2 xAndY = new Vector2(0, 0);

    //    if (_rotateDirection.x >= 1 || _rotateDirection.x <= -1)
    //    {
    //        Debug.Log("moving one direction");
    //        xAndY.x = _rotateDirection.x;

    //    }

    //    if (_rotateDirection.y >= 1 || _rotateDirection.y <= -1)
    //    {
    //        xAndY.y = _rotateDirection.y;
    //    }
    //    transform.Rotate(-xAndY.x, 0, -xAndY.y, Space.World);
    //}
    #endregion
    #region extra
    // SendMessages Inputs will send with an InputValue object representing the player's input



    // Invoke UnityEvent version:
    public void MoveEvent(InputAction.CallbackContext context)
    {
        _moveDirection = context.ReadValue<Vector2>().normalized;
    }

    public void SprintEvent(InputAction.CallbackContext context)
    {
        _isSprinting = context.performed;
    }
    // UnityEvent will provide an InputAction.CallbackContext object to inspect for the players input
    
    
    
    // Invoke C# Events version
    private PlayerInput _playerInputComponent;

    private void Start()
    {
        _playerInputComponent = GetComponent<PlayerInput>();
        _playerInputComponent.onActionTriggered += DistributeInput;
    }

    void DistributeInput(InputAction.CallbackContext context)
    {
        // switch - like an 'if' statement, but more options than just 1 or 2
        switch(context.action.name)
        {
            case "Move":
                MoveEvent(context);
                break;
            case "Sprint":
                SprintEvent(context);
                break;
            // default // if the value doesn't match
            default:
                Debug.Log("Cant determine behaviour of " + context.action.name);
                break;
        }
    }
    #endregion
}
