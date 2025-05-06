using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;
using System.Collections;

public class HealthSystem : NetworkBehaviour
{
    [SerializeField] private int maxHealth = 3;
    private NetworkVariable<int> currentHealth = new NetworkVariable<int>(
        writePerm: NetworkVariableWritePermission.Server);

    public bool IsDead() => currentHealth.Value <= 0;

    [Header("UI Elements")]
    [SerializeField] private Image[] hearts;
    [SerializeField] private Sprite fullHeart;
    [SerializeField] private Sprite emptyHeart;

    [Header("Escape UI")]
    [SerializeField] private TextMeshProUGUI escapeText;
    [SerializeField] private Slider escapeProgressBar;

    private Vector3 initialPosition;

    public override void OnNetworkSpawn()
    {
        initialPosition = transform.position;

        if (IsServer)
            currentHealth.Value = maxHealth;

        currentHealth.OnValueChanged += OnHealthChanged;

        if (!IsOwner)
        {
            // ซ่อน UI ถ้าไม่ใช่ของ Local Player 
            if (escapeText != null) escapeText.gameObject.SetActive(false);
            if (escapeProgressBar != null) escapeProgressBar.gameObject.SetActive(false);
            foreach (var heart in hearts)
            {
                if (heart != null)
                    heart.gameObject.SetActive(false);
            }
        }

        if (IsOwner)
        {
            UpdateHealthUI();
            HideEscapeUI();
        }

        Debug.Log($"[HealthSystem] OnNetworkSpawn -> Owner: {OwnerClientId}, Local: {NetworkManager.Singleton.LocalClientId}, IsOwner = {IsOwner}");
    }

    private void OnDestroy()
    {
        if (IsSpawned)
            currentHealth.OnValueChanged -= OnHealthChanged;
    }

    private void OnHealthChanged(int oldValue, int newValue)
    {
        if (!IsOwner) return;
        UpdateHealthUI();
    }

    void Update()
    {
        if (!IsOwner) return;

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
        if (!IsServer)
        {
            Debug.LogWarning("TakeDamage called on client, skipping.");
            return;
        }

        currentHealth.Value -= amount;
        if (currentHealth.Value < 0) currentHealth.Value = 0;

        Debug.Log($"[HealthSystem] {OwnerClientId} took {amount} damage. Remaining: {currentHealth.Value}");

        if (currentHealth.Value == 0)
        {
            HideEscapeUI();
            StartCoroutine(DelayedRespawn());
        }
    }

    private IEnumerator DelayedRespawn()
    {
        yield return null;
        Respawn();
    }

    public void Heal(int amount)
    {
        if (!IsServer) return;

        currentHealth.Value += amount;
        if (currentHealth.Value > maxHealth) currentHealth.Value = maxHealth;
    }

    private void UpdateHealthUI()
    {
        if (!IsOwner) return;

        for (int i = 0; i < hearts.Length; i++)
        {
            if (hearts[i] != null)
                hearts[i].sprite = (i < currentHealth.Value) ? fullHeart : emptyHeart;
        }
    }

    private void Respawn()
    {
        transform.position = initialPosition;
        currentHealth.Value = maxHealth;

        UpdateRespawnClientRpc(initialPosition); //Sync ตำแหน่งกลับไปให้ Owner

        if (IsOwner)
        {
            UpdateHealthUI();
            HideEscapeUI();
        }

        BearTrap[] traps = FindObjectsOfType<BearTrap>();
        foreach (BearTrap trap in traps)
        {
            trap.ForceReleaseIfTrapped(this);
        }
    }

    [ClientRpc]
    private void UpdateRespawnClientRpc(Vector3 newPosition, ClientRpcParams rpcParams = default)
    {
        if (!IsOwner) return;
        transform.position = newPosition;
    }


    public void ShowEscapeUI()
    {
        if (!IsOwner) return;

        if (escapeText != null) escapeText.gameObject.SetActive(true);
        if (escapeProgressBar != null)
        {
            escapeProgressBar.value = 0f;
            escapeProgressBar.gameObject.SetActive(true);
        }
    }

    public void HideEscapeUI()
    {
        if (!IsOwner) return;

        if (escapeText != null)
            escapeText.gameObject.SetActive(false);

        if (escapeProgressBar != null)
        {
            escapeProgressBar.gameObject.SetActive(false);
            escapeProgressBar.value = 0f;
        }
    }

    public void SetEscapeProgress(float value)
    {
        if (!IsOwner) return;

        if (escapeProgressBar != null)
            escapeProgressBar.value = value;
    }
}
