using System;
using Unity.Netcode;
using UnityEngine;

public class PlayerMovement : NetworkBehaviour
{
    [Header("Rwferences")]
    [SerializeField] private InputReader inputReader;
    [SerializeField] private Transform playerTransform;
    [SerializeField] private Rigidbody2D rb;
    
    [Header("Setting")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float turningRate = 30f;
    
    private Vector2 previousMovementInput;
    
    void Update()
    {
        if (!IsOwner) return;
        
        float zRotation = previousMovementInput.x * -turningRate * Time.deltaTime;
        playerTransform.Rotate(0, 0, zRotation);
    }

    private void FixedUpdate()
    {
        if (!IsOwner) return;
        
        rb.velocity = playerTransform.up * previousMovementInput.y * moveSpeed;
    }

    public override void OnNetworkSpawn()
    {
        if (!IsOwner) return;
        inputReader.MoveEvent += HandleMove;
       
    }
    
    public override void OnNetworkDespawn()
    {
        if (!IsOwner) return;
        inputReader.MoveEvent -= HandleMove;
        
    }

    private void HandleMove(Vector2 movementInput)
    {
        previousMovementInput = movementInput;
    }
}
