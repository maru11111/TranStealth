using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMove : MonoBehaviour
{
    [SerializeField] private Rigidbody rigidbody;
    
    private GameInputs gameInputs;
    
    private float moveForce = 5;
    private float jumpForce = 5;

    private Vector2 moveInputValue;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        gameInputs = new GameInputs();

        /* GameInputs�� Player �� Move �Ŏw�肵���L�[�������ꂽ�Ƃ�, 
           started �� performed �� canceled �̂Ƃ��� OnMove�֐����Ă� */
        gameInputs.Player.Move.started += OnMove;
        gameInputs.Player.Move.performed += OnMove;
        gameInputs.Player.Move.canceled += OnMove;
        // �W�����v�̓��͂��󂯕t����悤��
        gameInputs.Player.Jump.performed += OnJump;

        // Input Action��L����
        gameInputs.Enable();
    }

    private void OnMove(InputAction.CallbackContext context)
    {
        moveInputValue = context.ReadValue<Vector2>();
    }

    private void OnJump(InputAction.CallbackContext context)
    {
        rigidbody.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
    }

    // Update is called once per frame
    void Update()
    {
        // �ړ������̗͂�^����
        rigidbody.AddForce(new Vector3(
            moveInputValue.x,
            0,
            moveInputValue.y
        ) * moveForce);
    }
}
