using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;

public class CameraControll : MonoBehaviour
{
    [SerializeField]CinemachineInputAxisController controller;

    [SerializeField]GameInputs gameInputs;

    private void Start()
    {
        gameInputs = new GameInputs();
        gameInputs.Player.StartLook.started += OnLookStarted;
        gameInputs.Player.StartLook.canceled += OnLookEnded;
        gameInputs.Enable();
    }

    void OnLookStarted(InputAction.CallbackContext contex)
    {
        controller.enabled = true;
    }
    void OnLookEnded(InputAction.CallbackContext contex)
    {
        controller.enabled = false;
    }

    private void OnDestroy()
    {
        // 自身でインスタンス化したActionクラスはIDisposableを実装しているので、
        // 必ずDisposeする必要がある
        gameInputs?.Dispose();
    }

    void Update()
    {
        
    }
}