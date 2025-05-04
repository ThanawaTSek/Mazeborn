using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using TMPro;
using Unity.Netcode;

public class BearTrap : NetworkBehaviour
{
    [SerializeField] private int damage = 1;
    [SerializeField] private float escapeTime = 3f;
    [SerializeField] private float escapeForce = 5f;

    private bool isPlayerTrapped = false;
    private float escapeTimer = 0f;

    private NetworkObject trappedPlayer;
    private PlayerMovement playerMovement;
    private HealthSystem playerHealth;
    private Rigidbody2D playerRb;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!IsServer || isPlayerTrapped) return;

        if (collision.TryGetComponent<NetworkObject>(out NetworkObject netObj))
        {
            playerMovement = netObj.GetComponent<PlayerMovement>();
            playerHealth = netObj.GetComponent<HealthSystem>();
            playerRb = netObj.GetComponent<Rigidbody2D>();

            if (playerMovement != null && playerHealth != null)
            {
                trappedPlayer = netObj;
                isPlayerTrapped = true;

                Vector3 playerPosition = collision.transform.position;
                float footY = collision.bounds.min.y;
                float trapY = transform.position.y;
                float yOffset = playerPosition.y - footY;

                collision.transform.position = new Vector3(transform.position.x, trapY + yOffset, playerPosition.z);
                playerRb.velocity = Vector2.zero;

                playerMovement.SetMovementLocked(true);
                playerHealth.TakeDamage(damage);

                ShowUIClientRpc(netObj.OwnerClientId);
            }
        }
    }

    [ClientRpc]
    private void ShowUIClientRpc(ulong targetClientId)
    {
        if (NetworkManager.Singleton.LocalClientId != targetClientId) return;

        var health = NetworkManager.Singleton.SpawnManager.GetLocalPlayerObject()?.GetComponent<HealthSystem>();
        health?.ShowEscapeUI();
        StartCoroutine(EscapeRoutine(health));
    }

    private IEnumerator EscapeRoutine(HealthSystem health)
    {
        escapeTimer = 0f;
        while (isPlayerTrapped)
        {
            if (Input.GetKey(KeyCode.E))
            {
                escapeTimer += Time.deltaTime;
                health?.SetEscapeProgress(escapeTimer / escapeTime);

                if (escapeTimer >= escapeTime)
                {
                    EscapeAttemptServerRpc();
                    yield break;
                }
            }
            else
            {
                escapeTimer = 0f;
                health?.SetEscapeProgress(0f);
            }

            yield return null;
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void EscapeAttemptServerRpc(ServerRpcParams rpcParams = default)
    {
        if (!isPlayerTrapped || trappedPlayer == null) return;

        ulong targetClientId = trappedPlayer.OwnerClientId;
        ReleasePlayer();
        HideUIClientRpc(targetClientId);
    }

    [ClientRpc]
    private void HideUIClientRpc(ulong targetClientId)
    {
        if (NetworkManager.Singleton.LocalClientId != targetClientId) return;

        Debug.Log($"[BearTrap] Hiding UI on Client: {targetClientId}");
        
        var localPlayerObj = NetworkManager.Singleton.SpawnManager.GetLocalPlayerObject();
        var health = localPlayerObj?.GetComponent<HealthSystem>();
        health?.HideEscapeUI();
    }


    private void ReleasePlayer()
    {
        StopAllCoroutines();

        if (playerMovement != null)
            playerMovement.SetMovementLocked(false);

        if (playerRb != null)
        {
            Vector2 escapeDirection = (playerRb.transform.position - transform.position).normalized;
            playerRb.AddForce(escapeDirection * escapeForce, ForceMode2D.Impulse);
        }

        isPlayerTrapped = false;
        trappedPlayer = null;
    }

    public void ForceReleaseIfTrapped(HealthSystem player)
    {
        if (!isPlayerTrapped || trappedPlayer == null)
        {
            isPlayerTrapped = false;
            trappedPlayer = null;
            return;
        }

        var trappedHealth = trappedPlayer.GetComponent<HealthSystem>();
        if (trappedHealth == player)
        {
            ulong targetClientId = trappedPlayer.OwnerClientId;
            ReleasePlayer();
            HideUIClientRpc(targetClientId);
        }
    }
}
