using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using TMPro;
using Unity.Netcode;

public class BearTrap : NetworkBehaviour
{
    [SerializeField] private int damage = 1;
    [SerializeField] private float escapeTime = 3f;
    [SerializeField] private TextMeshProUGUI escapeText;
    [SerializeField] private Slider escapeProgressBar; // ✅ เพิ่ม Slider Progress Bar

    private bool isPlayerTrapped = false;
    private float escapeTimer = 0f;
    private PlayerMovement playerMovement;
    private HealthSystem playerHealth;

    private void Start()
    {
        escapeText.gameObject.SetActive(false);
        escapeProgressBar.gameObject.SetActive(false); // ✅ ซ่อน Progress Bar ตอนเริ่ม
        escapeProgressBar.value = 0f;
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

                escapeText.gameObject.SetActive(true);
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
        while (isPlayerTrapped)
        {
            if (Input.GetKey(KeyCode.E))
            {
                if (!escapeProgressBar.gameObject.activeSelf)
                {
                    escapeProgressBar.gameObject.SetActive(true); // ✅ แสดง Progress Bar ตอนกด E
                }

                escapeTimer += Time.deltaTime;
                escapeProgressBar.value = escapeTimer / escapeTime;

                if (escapeTimer >= escapeTime)
                {
                    ReleasePlayer();
                    yield break;
                }
            }
            else
            {
                escapeProgressBar.gameObject.SetActive(false); // ✅ ซ่อน Progress Bar ตอนปล่อย E
                escapeTimer = 0f;
            }
            yield return null;
        }
    }

    private void ReleasePlayer()
    {
        isPlayerTrapped = false;
        if (playerMovement != null)
        {
            playerMovement.SetMovementLocked(false); // ปลดล็อกการเคลื่อนที่
        }
        escapeText.gameObject.SetActive(false);
        escapeProgressBar.gameObject.SetActive(false); // ✅ ซ่อน Progress Bar ตอนปล่อยตัว
    }
}
