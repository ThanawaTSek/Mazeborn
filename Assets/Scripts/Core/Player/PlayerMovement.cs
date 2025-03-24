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
        rb.velocity = movementInput.normalized * currentSpeed;
    }

    private void RotatePlayer()
    {
        if (movementInput.x != 0)
        {
            Vector3 scale = playerTransform.localScale;
            scale.x = movementInput.x > 0 ? Mathf.Abs(scale.x) : -Mathf.Abs(scale.x);
            playerTransform.localScale = scale;
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