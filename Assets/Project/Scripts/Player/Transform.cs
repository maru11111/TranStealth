using UnityEngine;
using UnityEngine.InputSystem;

public class Transform : MonoBehaviour
{
    [SerializeField]
    private TransformableObject nextObjectParam;

    [SerializeField]
    private TransformableObject myObjectParam;

    private GameInputs gameInputs;

    [SerializeField] private LayerMask transformableLayer;

    private Vector2 mousePos;

    [SerializeField] private PlayerCommonParam playerCommonParam;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        gameInputs = new GameInputs();

        gameInputs.Player.Transform.started += OnTransform;

        gameInputs.Player.MousePos.performed += OnGetMousePos;

        // Input Actionを有効化
        gameInputs.Enable();

        //DEBUG
        //nextObjectParam = tansu.GetComponent<TransformableObject>();
    }

    private void OnTransform(InputAction.CallbackContext context)
    {
        //DEBUG
        Debug.DrawRay(Camera.main.ScreenPointToRay(mousePos).origin, Camera.main.ScreenPointToRay(mousePos).direction*100, Color.red, 10f);

        Vector3 targetPos;

        // カメラからrayを発射（マウスの先に選択可能オブジェクトがあるか
        if (Physics.Raycast(Camera.main.ScreenPointToRay(mousePos), out RaycastHit targetHit, 100, transformableLayer))
        {
            targetPos = targetHit.point;
            Debug.Log(targetPos);

        }
        else
        {
            return;
        }

        //プレイヤーからターゲットまで遠すぎたら 失敗
        if (100 < (targetPos - transform.position).magnitude)
        {
            return;
        }

        //DEBUG
        Debug.DrawRay(transform.position, (targetPos - transform.position).normalized * 1000.0f, Color.green, 10f);

        // プレイヤーからターゲットに向かって ray を発射（障害物がないか確認
        Physics.Raycast(transform.position, targetPos - transform.position, out RaycastHit obstaclesHit);

        // プレイヤーからオブジェクトまでに障害物があったら 失敗
        // 比較方法は https://watablog.tech/2021/10/23/post-2243/ 参照
        // この方法だとレイヤーマスクが Defaut の時に困るが, 今回は違うので無視
        if ( ((1 << obstaclesHit.collider.gameObject.layer) & transformableLayer) == 0)
        {
            Debug.Log("a："+ obstaclesHit.collider.gameObject.layer);
            return;
        }

        // ターゲットの変身パラメータを取得
        nextObjectParam = targetHit.collider.GetComponent<TransformableObject>();
        // スクリプトがあるか
        if (nextObjectParam != null)
        {
            // 変身
            myObjectParam.meshFilter.mesh = nextObjectParam.meshFilter.sharedMesh;
            myObjectParam.meshRenderer.material = nextObjectParam.meshRenderer.sharedMaterial;
            myObjectParam.boxCollider.size = nextObjectParam.boxCollider.size;
            myObjectParam.boxCollider.center = nextObjectParam.boxCollider.center;
            // 接地判定ボックスの一辺の長さ. オブジェクトのx幅, z幅のうち小さい方に合わせる. 
            playerCommonParam.groundCheckBoxSize = new Vector3(myObjectParam.boxCollider.size.x, 0.1f, myObjectParam.boxCollider.size.z);
        }
        else
        {
            Debug.Log("変身可能ObjにTransformableObjectがアタッチされていません");
        }
    }

    private void OnGetMousePos(InputAction.CallbackContext context)
    {
        mousePos = context.ReadValue<Vector2>();
    }

    // Update is called once per frame
    void Update()
    {
    }
}
