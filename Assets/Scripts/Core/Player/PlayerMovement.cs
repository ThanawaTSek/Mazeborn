using System;
using Unity.Netcode;
using UnityEngine;

public class PlayerMovement : NetworkBehaviour
{
    [Header("References")]
    [SerializeField] private InputReader inputReader;
    [SerializeField] private Transform playerTransform;
    [SerializeField] private Rigidbody2D rb;
    
    [Header("Setting")]
    [SerializeField] private float normalSpeed = 5f;
    [SerializeField] private float sprintSpeed = 10f;

    private Vector2 movementInput;
    private float currentSpeed;
    
    private NetworkVariable<float> scaleX = new NetworkVariable<float>(
        1f, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    private void Awake()
    {
        currentSpeed = normalSpeed;
    }

    public override void OnNetworkSpawn()
    {
        if (!IsOwner) return;
        
        gameObject.tag = "LocalPlayer";
        
        inputReader.MoveEvent += HandleMove;
        inputReader.SprintEvent += HandleSprint;
    }

    public override void OnNetworkDespawn()
    {
        if (!IsOwner) return;
        
        inputReader.MoveEvent -= HandleMove;
        inputReader.SprintEvent -= HandleSprint;
    }

    private void FixedUpdate()
    {
        if (!IsOwner) return;

        MovePlayer();
        RotatePlayer();
    }

    private void MovePlayer()
    {
        rb.velocity = movementInput.normalized * currentSpeed;
    }

    private void RotatePlayer()
    {
        if (movementInput.x != 0)
        {
            float newScaleX = movementInput.x > 0 ? Mathf.Abs(playerTransform.localScale.x) : -Mathf.Abs(playerTransform.localScale.x);
            
            if (IsOwner) 
            {
                UpdateScaleXServerRpc(newScaleX);
            }
        }
    }

    [ServerRpc]
    private void UpdateScaleXServerRpc(float newScaleX)
    {
        scaleX.Value = newScaleX;
    }

    private void HandleMove(Vector2 input)
    {
        movementInput = input;
    }

    private void HandleSprint(bool isSprinting)
    {
        currentSpeed = isSprinting ? sprintSpeed : normalSpeed;
    }

    private void Update()
    {
        playerTransform.localScale = new Vector3(scaleX.Value, 1f, 1f);
    }
}
