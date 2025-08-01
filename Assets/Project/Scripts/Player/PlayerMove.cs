using System.ComponentModel.Design.Serialization;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMove : MonoBehaviour
{
    [SerializeField] private Rigidbody rigidbody;
    //カメラ
    [SerializeField]Camera camera;

    //InputSystem
    private GameInputs gameInputs;
    // 移動で加える力
    private float moveForce = 20;
    // ジャンプで加える力
    private float jumpForce = 5;
    // 摩擦力（着地後に摩擦で速度が落ちるのが気になったのでスクリプトで制御
    private float friction = 10;
    // 移動の最大速度
    private const float maxSpeed = 3;
    // 移動のキー入力値
    private Vector2 moveDir;
    // BoxCastの発射距離. オブジェクトからわずかに下の部分だけ見る
    float groundCheckDistance = 0.1f;

    [SerializeField] private PlayerCommonParam playerCommonParam;

    [SerializeField] private BoxCollider playerBoxCollider;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {   // 接地判定ボックスの一辺の長さ. オブジェクトのx幅, z幅のうち小さい方に合わせる. 
        playerCommonParam.groundCheckBoxSize = new Vector3(playerBoxCollider.size.x, 0.01f, playerBoxCollider.size.z);
        gameInputs = new GameInputs();

        /* GameInputsで Player の Move で指定したキーが押されたとき, 
           started と performed と canceled のときに OnMove関数を呼ぶ */
        gameInputs.Player.Move.started += OnMove;
        gameInputs.Player.Move.performed += OnMove;
        gameInputs.Player.Move.canceled += OnMove;
        // ジャンプの入力を受け付けるように
        gameInputs.Player.Jump.performed += OnJump;

        // Input Actionを有効化
        gameInputs.Enable();
    }

    private void OnDestroy()
    {
        // 自身でインスタンス化したActionクラスはIDisposableを実装しているので、
        // 必ずDisposeする必要がある
        gameInputs?.Dispose();
    }

    // 接地判定
    private bool isGrounded()
    {
        return Physics.BoxCast(
            // 物体の底面 - boxの射出距離/2.0 をすることで, boxCast が初めから床に当たるのを防ぐ
            transform.position + Vector3.down * (playerBoxCollider.size.y / 2.0f - playerCommonParam.groundCheckBoxSize.y / 2.0f) + Vector3.up*groundCheckDistance*0.5f
            , playerCommonParam.groundCheckBoxSize * 0.5f // BoxCastの判定ボックスの半分の大きさ
            , Vector3.down // 発射方向
            , out RaycastHit hit
            , Quaternion.identity  
            , groundCheckDistance // 射出距離
            );
    }

    // InputActionでMoveの時の動き（移動メソッド
    private void OnMove(InputAction.CallbackContext context)
    {
        // カメラの向きに移動
        // ADとWSの入力
        float inputAD = context.ReadValue<Vector2>().x;
        float inputWS = context.ReadValue<Vector2>().y;

        // カメラの正面方向とその右方向のXZ成分の単位ベクトルを取得
        Vector2 forward = new Vector2(camera.transform.forward.x, camera.transform.forward.z).normalized;
        Vector2 right = new Vector2(camera.transform.right.x, camera.transform.right.z).normalized;

        // 正面の進行方向は forward, 左右の進行方向は right にして,
        // WS, ADの値は大きさとして使う。
        moveDir = inputWS * forward + inputAD * right;
    }

    // InputActionでJumpのときの動き（ジャンプメソッド
    private void OnJump(InputAction.CallbackContext context)
    {
        if (isGrounded()) rigidbody.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
    }

    private void OnDrawGizmos()
    {
        // 接地判定デバッグ出力
        Gizmos.color = isGrounded() ? Color.green : Color.red;
        Gizmos.DrawWireCube(transform.position + Vector3.down * (playerBoxCollider.size.y / 2.0f - playerCommonParam.groundCheckBoxSize.y / 2.0f) + Vector3.up * groundCheckDistance * 0.5f, new Vector3(playerCommonParam.groundCheckBoxSize.x, playerCommonParam.groundCheckBoxSize.y + groundCheckDistance, playerCommonParam.groundCheckBoxSize.z));
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        // 移動方向の力を与える
        rigidbody.AddForce(new Vector3(
            moveDir.x,
            0,
            moveDir.y
        ) * moveForce);

        // 最高速度以下にする
        // 水平方向の速度
        Vector2 xzVel = new Vector2(rigidbody.linearVelocity.x, rigidbody.linearVelocity.z);
        if (maxSpeed < xzVel.magnitude)
        {
            xzVel = xzVel.normalized * maxSpeed;

            rigidbody.linearVelocity = new Vector3(xzVel.x, rigidbody.linearVelocity.y, xzVel.y);
        }

        // 摩擦（x, yに分けないと、両方入力→片方入力なし のときに慣性がかからない
        // x方向
        if (Mathf.Abs(moveDir.x) < 0.1f)
        {
            // 速度がある程度遅くなったら, 速度をゼロにする
            if (Mathf.Abs(xzVel.x) < 0.1f)
            {
                rigidbody.linearVelocity = new Vector3(0, rigidbody.linearVelocity.y, rigidbody.linearVelocity.z);
            }
            // 摩擦適用
            else
            {
                Vector3 frictionForce = -1 * new Vector3(xzVel.x, 0, 0).normalized * friction;
                rigidbody.AddForce(frictionForce, ForceMode.Acceleration);
            }
        }
        // y方向
        if (Mathf.Abs(moveDir.y) < 0.1f)
        {
            // 速度がある程度遅くなったら, 速度をゼロにする
            if (Mathf.Abs(xzVel.y )< 0.1f)
            {
                rigidbody.linearVelocity = new Vector3(rigidbody.linearVelocity.x, rigidbody.linearVelocity.y, 0);
            }
            // 摩擦適用
            else
            {
                Vector3 frictionForce = -1 * new Vector3(0, 0, xzVel.y).normalized * friction;
                rigidbody.AddForce(frictionForce, ForceMode.Acceleration);
            }
        }

    }

}
