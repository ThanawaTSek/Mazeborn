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

    private void Awake()
    {
        currentSpeed = normalSpeed;
    }

    public override void OnNetworkSpawn()
    {
        if (!IsOwner) return;

        // ตั้ง Tag สำหรับ Local Player
        gameObject.tag = "LocalPlayer";

        // เชื่อม Event Input เฉพาะ Local Player
        inputReader.MoveEvent += HandleMove;
        inputReader.SprintEvent += HandleSprint;
    }

    public override void OnNetworkDespawn()
    {
        if (!IsOwner) return;

        // ยกเลิก Event เมื่อ Player หายไป
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
        // เคลื่อนที่ในทิศทางที่กด
        rb.velocity = movementInput.normalized * currentSpeed;
    }

    private void RotatePlayer()
    {
        // เช็คว่ามี Input หรือไม่
        if (movementInput != Vector2.zero)
        {
            // หันหน้าไปในทิศทางที่รับ Input
            float angle = Mathf.Atan2(movementInput.y, movementInput.x) * Mathf.Rad2Deg;
            rb.rotation = angle;
        }
    }

    private void HandleMove(Vector2 input)
    {
        movementInput = input;
    }

    private void HandleSprint(bool isSprinting)
    {
        currentSpeed = isSprinting ? sprintSpeed : normalSpeed;
    }
}