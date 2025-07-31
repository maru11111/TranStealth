using UnityEngine;
using UnityEngine.InputSystem;

public class Transform : MonoBehaviour
{

    public GameObject tansu;

    [SerializeField]
    private TransformableObject nextObjectParam;

    [SerializeField]
    private TransformableObject myObjectParam;

    private GameInputs gameInputs;



    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        gameInputs = new GameInputs();

        gameInputs.Player.Transform.started += OnTransform;

        // Input Actionを有効化
        gameInputs.Enable();

        //DEBUG
        nextObjectParam = tansu.GetComponent<TransformableObject>();
    }

    private void OnTransform(InputAction.CallbackContext context)
    {
        //if(nextObjectParam == null) nextObjectParam = tansu.GetComponent<TransformableObject>();
        myObjectParam.meshFilter.mesh = nextObjectParam.meshFilter.sharedMesh;
        myObjectParam.meshRenderer.material = nextObjectParam.meshRenderer.sharedMaterial;
        myObjectParam.boxCollider.size = nextObjectParam.boxCollider.size;
        myObjectParam.boxCollider.center = nextObjectParam.boxCollider.center;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
