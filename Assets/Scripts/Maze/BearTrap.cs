using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;

public class BearTrap : NetworkBehaviour
{
    [SerializeField] private int damage = 1;
    [SerializeField] private float escapeTime = 3f;
    [SerializeField] private float escapeForce = 5f;
    [SerializeField] private float quarantineDuration = 1.5f;

    private NetworkVariable<bool> isPlayerTrapped = new NetworkVariable<bool>(
        false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    private float escapeTimer = 0f;

    private NetworkObject trappedPlayer;
    private PlayerMovement playerMovement;
    private HealthSystem playerHealth;
    private Rigidbody2D playerRb;

    private Dictionary<ulong, float> quarantineTimers = new Dictionary<ulong, float>();

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!IsServer || isPlayerTrapped.Value) return;

        if (collision.TryGetComponent<NetworkObject>(out NetworkObject netObj))
        {
            ulong clientId = netObj.OwnerClientId;

            if (quarantineTimers.ContainsKey(clientId) && Time.time < quarantineTimers[clientId])
                return; // ยังอยู่ในช่วง Quarantine

            playerMovement = netObj.GetComponent<PlayerMovement>();
            playerHealth = netObj.GetComponent<HealthSystem>();
            playerRb = netObj.GetComponent<Rigidbody2D>();

            if (playerMovement != null && playerHealth != null)
            {
                trappedPlayer = netObj;
                isPlayerTrapped.Value = true;

                Vector3 playerPosition = collision.transform.position;
                float footY = collision.bounds.min.y;
                float trapY = transform.position.y;
                float yOffset = playerPosition.y - footY;

                collision.transform.position = new Vector3(transform.position.x, trapY + yOffset, playerPosition.z);
                playerRb.velocity = Vector2.zero;

                playerMovement.SetMovementLocked(true);
                playerHealth.TakeDamage(damage);

                ShowUIClientRpc(clientId);
                SnapToTrapClientRpc(transform.position, yOffset, clientId);
            }
        }
    }

    [ClientRpc]
    private void SnapToTrapClientRpc(Vector3 trapPosition, float yOffset, ulong targetClientId)
    {
        if (NetworkManager.Singleton.LocalClientId != targetClientId) return;

        var player = NetworkManager.Singleton.SpawnManager.GetLocalPlayerObject();
        if (player == null) return;

        player.transform.position = new Vector3(trapPosition.x, trapPosition.y + yOffset, player.transform.position.z);

        var movement = player.GetComponent<PlayerMovement>();
        movement?.SetMovementLocked(true);
    }

    [ClientRpc]
    private void UnlockMovementClientRpc(ulong targetClientId)
    {
        if (NetworkManager.Singleton.LocalClientId != targetClientId) return;

        var player = NetworkManager.Singleton.SpawnManager.GetLocalPlayerObject();
        var movement = player?.GetComponent<PlayerMovement>();
        movement?.SetMovementLocked(false);
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
        while (true)
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
        if (!isPlayerTrapped.Value || trappedPlayer == null) return;

        if (rpcParams.Receive.SenderClientId != trappedPlayer.OwnerClientId) return;

        var health = trappedPlayer.GetComponent<HealthSystem>();
        if (health == null || health.IsDead()) return;

        ulong targetClientId = trappedPlayer.OwnerClientId;
        ReleasePlayer();
        HideUIClientRpc(targetClientId);
    }

    [ClientRpc]
    private void HideUIClientRpc(ulong targetClientId)
    {
        if (NetworkManager.Singleton.LocalClientId != targetClientId) return;

        var localPlayerObj = NetworkManager.Singleton.SpawnManager.GetLocalPlayerObject();
        var health = localPlayerObj?.GetComponent<HealthSystem>();
        health?.HideEscapeUI();
    }

    [ClientRpc]
    public void StopEscapeRoutineClientRpc(ulong targetClientId)
    {
        if (NetworkManager.Singleton.LocalClientId != targetClientId) return;

        StopAllCoroutines();

        var player = NetworkManager.Singleton.SpawnManager.GetLocalPlayerObject();
        var movement = player?.GetComponent<PlayerMovement>();
        var health = player?.GetComponent<HealthSystem>();

        movement?.SetMovementLocked(false);
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

        if (trappedPlayer != null)
        {
            UnlockMovementClientRpc(trappedPlayer.OwnerClientId);
            quarantineTimers[trappedPlayer.OwnerClientId] = Time.time + quarantineDuration;
        }

        isPlayerTrapped.Value = false;
        trappedPlayer = null;
    }

    public void ForceReleaseIfTrapped(HealthSystem player)
    {
        if (!isPlayerTrapped.Value || trappedPlayer == null)
        {
            isPlayerTrapped.Value = false;
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

    public void ClearTrapReferenceIfMatched(HealthSystem player)
    {
        if (trappedPlayer == null || !isPlayerTrapped.Value) return;

        var health = trappedPlayer.GetComponent<HealthSystem>();
        if (health == player)
        {
            StopAllCoroutines();
            quarantineTimers.Remove(player.OwnerClientId);
            trappedPlayer = null;
            isPlayerTrapped.Value = false;
        }
    }
}
