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
    [SerializeField] private float turningRate = 30f;

    private Vector2 previousMovementInput;
    private float currentSpeed;

    private void Awake()
    {
        currentSpeed = normalSpeed;
    }

    public override void OnNetworkSpawn()
    {
        if (!IsOwner) return;
        
        inputReader.MoveEvent += HandleMove;
        inputReader.SprintEvent += HandleSprint; // ✅ เชื่อม Sprint Event
    }

    public override void OnNetworkDespawn()
    {
        if (!IsOwner) return;
        
        inputReader.MoveEvent -= HandleMove;
        inputReader.SprintEvent -= HandleSprint; // ✅ ลบ Event เมื่อ Destroy
    }

    void Update()
    {
        if (!IsOwner) return;
        
        float zRotation = previousMovementInput.x * -turningRate * Time.deltaTime;
        playerTransform.Rotate(0, 0, zRotation);
    }

    private void FixedUpdate()
    {
        if (!IsOwner) return;
        
        rb.velocity = playerTransform.up * previousMovementInput.y * currentSpeed;
    }

    private void HandleMove(Vector2 movementInput)
    {
        previousMovementInput = movementInput;
    }

    private void HandleSprint(bool isSprinting)
    {
        currentSpeed = isSprinting ? sprintSpeed : normalSpeed;
    }
}