using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using TMPro;
using Unity.Netcode;

public class BearTrap : NetworkBehaviour
{
    [SerializeField] private int damage = 1;
    [SerializeField] private float escapeTime = 3f;
    [SerializeField] private float escapeForce = 5f; // ✅ แรงดีดตัวเมื่อหลุดจากกับดัก
    [SerializeField] private TextMeshProUGUI escapeText;
    [SerializeField] private Slider escapeProgressBar;

    private bool isPlayerTrapped = false;
    private float escapeTimer = 0f;
    private PlayerMovement playerMovement;
    private HealthSystem playerHealth;
    private Rigidbody2D playerRb;
    private Collider2D playerCollider; // ✅ เพิ่ม Collider2D ของ Player เพื่อใช้หาตำแหน่งเท้า

    private void Start()
    {
        escapeText.gameObject.SetActive(false);
        escapeProgressBar.gameObject.SetActive(false);
        escapeProgressBar.value = 0f;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        Debug.Log($"[BearTrap] {collision.gameObject.name} entered the trap!");

        if (collision.CompareTag("LocalPlayer") && !isPlayerTrapped)
        {
            playerMovement = collision.GetComponent<PlayerMovement>();
            playerHealth = collision.GetComponent<HealthSystem>();
            playerRb = collision.GetComponent<Rigidbody2D>();
            playerCollider = collision.GetComponent<Collider2D>();
            
            if (playerMovement != null && playerHealth != null && playerMovement.IsOwner)
            {
                Debug.Log("[BearTrap] Player trapped!");
                isPlayerTrapped = true;
                
                Vector3 playerPosition = collision.transform.position;
                float playerFootY = playerCollider.bounds.min.y;
                float trapY = transform.position.y;
                float yOffset = playerPosition.y - playerFootY;
                
                collision.transform.position = new Vector3(transform.position.x, trapY + yOffset, playerPosition.z);
                playerRb.velocity = Vector2.zero;
                
                playerMovement.SetMovementLocked(true);
                playerHealth.TakeDamage(damage);
                
                escapeText.gameObject.SetActive(true);
                escapeProgressBar.value = 0f;

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
                escapeProgressBar.gameObject.SetActive(true);
                escapeTimer += Time.deltaTime;
                escapeProgressBar.value = escapeTimer / escapeTime;
            }
            else
            {
                escapeProgressBar.gameObject.SetActive(false);
                escapeTimer = 0f;
                escapeProgressBar.value = 0f;
            }

            if (escapeTimer >= escapeTime)
            {
                ReleasePlayer();
                yield break;
            }

            yield return null;
        }
    }

    private void ReleasePlayer()
    {
        isPlayerTrapped = false;

        if (playerMovement != null)
        {
            playerMovement.SetMovementLocked(false);
        }
        
        escapeText.gameObject.SetActive(false);
        escapeProgressBar.gameObject.SetActive(false);
        
        if (playerRb != null)
        {
            Vector2 escapeDirection = (playerRb.transform.position - transform.position).normalized;
            playerRb.AddForce(escapeDirection * escapeForce, ForceMode2D.Impulse);
        }

        Debug.Log("[BearTrap] Player escaped!");
    }
}
