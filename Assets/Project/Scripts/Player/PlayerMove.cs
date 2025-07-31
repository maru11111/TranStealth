using System.ComponentModel.Design.Serialization;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMove : MonoBehaviour
{
    [SerializeField] private Rigidbody rigidbody;
    // 接地判定レイヤー
    [SerializeField] private LayerMask groundLayer;
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
    //接地判定スフィアの半径 (変身ごとに取得し直す
    private float groundCheckRadius;
    // 接地判定スフィアの開始Y座標オフセット（スフィアが開始時点で床と重ならないようにする
    private float groundCheckStartOffsetY=0.1f;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        groundCheckRadius = transform.localScale.x / 2.0f;
        
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
        RaycastHit hit;
        // レイスフィアの発射距離. オブジェクトからわずかに下の部分だけ見る
        float groundCheckDistance = groundCheckStartOffsetY * 1.1f;
        //DEBUG
        Debug.DrawRay(transform.position + Vector3.up * groundCheckStartOffsetY, Vector3.down * groundCheckDistance, Color.red, 0.5f);
        return Physics.SphereCast(transform.position + Vector3.up * 0.1f, groundCheckRadius, Vector3.down, out hit, groundCheckDistance, groundLayer, QueryTriggerInteraction.Ignore);
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
