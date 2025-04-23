using System.Collections;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Unity.Netcode;
using UnityEngine.SceneManagement;

public class MazeExit : NetworkBehaviour
{
    [Header("UI")]
    [SerializeField] private TextMeshProUGUI hintText;
    [SerializeField] private Slider holdSlider;
    [SerializeField] private TextMeshProUGUI winnerText;

    [Header("Settings")]
    [SerializeField] private float holdTimeToExit = 2f;
    [SerializeField] private string menuSceneName = "Menu";

    private float holdTimer = 0f;
    private bool isLocalPlayerNear = false;

    private void Start()
    {
        if (hintText != null) hintText.enabled = false;
        if (holdSlider != null) holdSlider.gameObject.SetActive(false);
        if (winnerText != null) winnerText.enabled = false;
    }

    private void Update()
    {
        if (!isLocalPlayerNear) return;

        if (Input.GetKey(KeyCode.E))
        {
            holdTimer += Time.deltaTime;
            if (holdSlider != null) holdSlider.gameObject.SetActive(true);
            if (hintText != null) hintText.enabled = false;

            if (holdSlider != null)
                holdSlider.value = holdTimer / holdTimeToExit;

            if (holdTimer >= holdTimeToExit)
            {
                holdTimer = 0f;

                if (IsClient)
                {
                    RequestWinServerRpc(NetworkManager.Singleton.LocalClientId);
                }
            }
        }
        else
        {
            if (holdSlider != null) holdSlider.gameObject.SetActive(false);
            if (hintText != null) hintText.enabled = true;
            holdTimer = 0f;
            if (holdSlider != null)
                holdSlider.value = 0;
        }
    }

    [ServerRpc(RequireOwnership = false)]
    void RequestWinServerRpc(ulong winnerClientId)
    {
        string winnerName = $"Player {winnerClientId}";
        AnnounceWinnerClientRpc(winnerName);
        
        StartCoroutine(WaitAndReturnToMenu());
    }
    
    IEnumerator WaitAndReturnToMenu()
    {
        yield return new WaitForSeconds(3f);

        NetworkManager.Singleton.SceneManager.LoadScene(menuSceneName, LoadSceneMode.Single);
        
        StartCoroutine(ShutdownAfterSceneLoad());
    }
    
    IEnumerator ShutdownAfterSceneLoad()
    {
        yield return new WaitForSeconds(1f);
    
        if (IsServer)
        {
            NetworkManager.Singleton.Shutdown();
        }
    }

    [ClientRpc]
    void AnnounceWinnerClientRpc(string winnerName)
    {
        if (winnerText != null)
        {
            winnerText.enabled = true;
            winnerText.text = $"{winnerName} Wins!";
        }
        
        if (hintText != null) hintText.enabled = false;
        if (holdSlider != null) holdSlider.gameObject.SetActive(false);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("LocalPlayer"))
        {
            isLocalPlayerNear = true;
            if (hintText != null) hintText.enabled = true;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("LocalPlayer"))
        {
            isLocalPlayerNear = false;
            holdTimer = 0f;
            if (hintText != null) hintText.enabled = false;
            if (holdSlider != null)
            {
                holdSlider.value = 0;
                holdSlider.gameObject.SetActive(false);
            }
        }
    }
}