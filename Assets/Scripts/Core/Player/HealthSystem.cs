using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;

public class HealthSystem : MonoBehaviour
{
    [SerializeField] private int maxHealth = 3;
    private int currentHealth;

    [Header("UI Elements")]
    [SerializeField] private Image[] hearts;
    [SerializeField] private Sprite fullHeart;
    [SerializeField] private Sprite emptyHeart;

    [Header("Escape UI")]
    [SerializeField] private TextMeshProUGUI escapeText;
    [SerializeField] private Slider escapeProgressBar;

    private Vector3 initialPosition;

    void Start()
    {
        initialPosition = transform.position;
        currentHealth = maxHealth;
        UpdateHealthUI();
        HideEscapeUI();
        
        Debug.Log($"[HealthSystem] My Owner ID = {GetComponent<NetworkBehaviour>().OwnerClientId}, Local ID = {Unity.Netcode.NetworkManager.Singleton.LocalClientId}");
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            TakeDamage(1);
        }

        if (Input.GetKeyDown(KeyCode.H))
        {
            Heal(1);
        }
    }

    public void TakeDamage(int amount)
    {
        currentHealth -= amount;
        if (currentHealth < 0) currentHealth = 0;
        UpdateHealthUI();

        if (currentHealth == 0)
        {
            // ปิด UI หนี Trap ทันที (แม้ยังอยู่ใน Trap)
            HideEscapeUI();
            Respawn();
        }
    }

    public void Heal(int amount)
    {
        currentHealth += amount;
        if (currentHealth > maxHealth) currentHealth = maxHealth;
        UpdateHealthUI();
    }

    private void UpdateHealthUI()
    {
        for (int i = 0; i < hearts.Length; i++)
        {
            hearts[i].sprite = (i < currentHealth) ? fullHeart : emptyHeart;
        }
    }

    private void Respawn()
    {
        transform.position = initialPosition;
        currentHealth = maxHealth;
        UpdateHealthUI();

        // ปิด UI เผื่อยังค้างอยู่
        HideEscapeUI();

        // ปล่อยตัวเองจากกับดักทั้งหมดที่จำไว้
        BearTrap[] traps = FindObjectsOfType<BearTrap>();
        foreach (BearTrap trap in traps)
        {
            trap.ForceReleaseIfTrapped(this);
        }
    }

    public void ShowEscapeUI()
    {
        if (escapeText != null) escapeText.gameObject.SetActive(true);
        if (escapeProgressBar != null)
        {
            escapeProgressBar.value = 0f;
            escapeProgressBar.gameObject.SetActive(true);
        }
    }

    public void HideEscapeUI()
    {
        Debug.Log("[HealthSystem] HideEscapeUI CALLED");

        if (escapeText != null)
        {
            Debug.Log("[HealthSystem] escapeText FOUND");
            escapeText.gameObject.SetActive(false);
        }
        else
        {
            Debug.LogWarning("[HealthSystem] escapeText is NULL");
        }

        if (escapeProgressBar != null)
        {
            Debug.Log("[HealthSystem] escapeProgressBar FOUND");
            escapeProgressBar.gameObject.SetActive(false);
            escapeProgressBar.value = 0f;
        }
        else
        {
            Debug.LogWarning("[HealthSystem] escapeProgressBar is NULL");
        }
    }

    public void SetEscapeProgress(float value)
    {
        if (escapeProgressBar != null)
            escapeProgressBar.value = value;
    }
}
