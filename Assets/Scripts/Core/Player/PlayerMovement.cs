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

    [Header("Audio")]
    [SerializeField] private AudioClip footstepClip;

    private AudioSource audioSource;
    private Vector2 movementInput;
    private float currentSpeed;

    private NetworkVariable<bool> isMoving = new NetworkVariable<bool>(
        false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);

    private NetworkVariable<bool> isFacingLeft = new NetworkVariable<bool>(
        false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);

    private void Awake()
    {
        currentSpeed = normalSpeed;
        audioSource = GetComponent<AudioSource>();

        if (audioSource != null)
        {
            audioSource.loop = true;
            audioSource.playOnAwake = false;
            audioSource.clip = footstepClip;
        }
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

        if (!IsOwner) return;

        isMoving.Value = movementInput.magnitude == 1;

        if (movementInput.magnitude > 0.1f)
        {
            StartFootstepSound();
        }
        else
        {
            StopFootstepSound();
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

    private void StartFootstepSound()
    {
        if (audioSource != null && !audioSource.isPlaying && footstepClip != null)
        {
            audioSource.Play();
        }
    }

    public void StopFootstepSound()
    {
        if (audioSource != null && audioSource.isPlaying)
        {
            audioSource.Stop();
        }
    }

    public void SetMovementLocked(bool isLocked)
    {
        rb.velocity = Vector2.zero;
        enabled = !isLocked;
        StopFootstepSound();
    }
}