using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using Unity.Netcode;

public class BearTrap : NetworkBehaviour
{
    [SerializeField] private int damage = 1;
    [SerializeField] private float escapeTime = 3f;
    [SerializeField] private Slider escapeProgressBar;
    private bool isPlayerTrapped = false;
    private float escapeTimer = 0f;
    private PlayerMovement playerMovement;
    private HealthSystem playerHealth;

    private void Start()
    {
        escapeProgressBar.gameObject.SetActive(false);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        Debug.Log($"[BearTrap] {collision.gameObject.name} entered the trap!");
        if (collision.CompareTag("LocalPlayer") && !isPlayerTrapped)
        {
            playerMovement = collision.GetComponent<PlayerMovement>();
            playerHealth = collision.GetComponent<HealthSystem>();
            Debug.Log($"[BearTrap] PlayerMovement: {playerMovement}, HealthSystem: {playerHealth}");

            if (playerMovement != null && playerHealth != null && playerMovement.IsOwner)
            {
                Debug.Log("[BearTrap] Player trapped!");
                isPlayerTrapped = true;
                playerMovement.SetMovementLocked(true); // ล็อกการเคลื่อนที่
                playerHealth.TakeDamage(damage);
                escapeProgressBar.gameObject.SetActive(true);
                StartCoroutine(EscapeRoutine());
            }
            else
            {
                Debug.Log("[BearTrap] PlayerMovement or HealthSystem is null");
            }
        }
    }

    private IEnumerator EscapeRoutine()
    {
        escapeTimer = 0f;
        while (escapeTimer < escapeTime)
        {
            if (Input.GetKey(KeyCode.E))
            {
                escapeTimer += Time.deltaTime;
                escapeProgressBar.value = escapeTimer / escapeTime;
            }
            else
            {
                escapeTimer = 0f;
                escapeProgressBar.value = 0f;
            }
            yield return null;
        }
        ReleasePlayer();
    }

    private void ReleasePlayer()
    {
        isPlayerTrapped = false;
        if (playerMovement != null)
        {
            playerMovement.SetMovementLocked(false); // ปลดล็อกการเคลื่อนที่
        }
        escapeProgressBar.gameObject.SetActive(false);
    }
}