using System;
using TMPro;
using Unity.Netcode;
using UnityEngine;

public class PlayerMovement : NetworkBehaviour
{
    [Header("References")]
    [SerializeField] private InputReader inputReader;
    [SerializeField] private Transform playerTransform;
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private Animator animator;
    [SerializeField] private SpriteRenderer spriteRenderer;

    [Header("Setting")]
    [SerializeField] private float normalSpeed = 5f;
    [SerializeField] private float sprintSpeed = 10f;

    private Vector2 movementInput;
    private float currentSpeed;

    private NetworkVariable<bool> isMoving = new NetworkVariable<bool>(
        false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);

    private NetworkVariable<bool> isFacingLeft = new NetworkVariable<bool>(
        false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);

    private void Awake()
    {
        currentSpeed = normalSpeed;
    }

    public override void OnNetworkSpawn()
    {
        if (IsOwner)
        {
            gameObject.tag = "LocalPlayer";
            inputReader.MoveEvent += HandleMove;
            inputReader.SprintEvent += HandleSprint;
        }
    }

    public override void OnNetworkDespawn()
    {
        if (IsOwner)
        {
            inputReader.MoveEvent -= HandleMove;
            inputReader.SprintEvent -= HandleSprint;
        }
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
            bool facingLeft = movementInput.x < 0;
            isFacingLeft.Value = facingLeft;
        }
    }

    private void Update()
    {
        // Sync visual state for all players
        spriteRenderer.flipX = isFacingLeft.Value;
        animator.SetBool("IsMoving", isMoving.Value);

        // Local input controls
        if (IsOwner)
        {
            isMoving.Value = movementInput.magnitude == 1;
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

    public void SetMovementLocked(bool isLocked)
    {
        rb.velocity = Vector2.zero;
        enabled = !isLocked;
    }
}