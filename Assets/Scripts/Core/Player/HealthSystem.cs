using UnityEngine;
using UnityEngine.UI;

public class HealthSystem : MonoBehaviour
{
    [SerializeField] private int maxHealth = 3;
    private int currentHealth;

    [Header("UI Elements")]
    [SerializeField] private Image[] hearts; 
    [SerializeField] private Sprite fullHeart;
    [SerializeField] private Sprite emptyHeart;

    void Start()
    {
        currentHealth = maxHealth;
        UpdateHealthUI();
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
            if (i < currentHealth)
            {
                hearts[i].sprite = fullHeart;
            }
            else
            {
                hearts[i].sprite = emptyHeart;
            }
        }
    }
}