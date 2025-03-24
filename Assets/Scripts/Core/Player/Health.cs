using UnityEngine;

public class Health : MonoBehaviour
{
    [SerializeField] private int maxHert = 3;
    private int currentHert;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        currentHert = maxHert;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
