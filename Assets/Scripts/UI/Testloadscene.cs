using UnityEngine;
using UnityEngine.SceneManagement;

public class Testloadscene : MonoBehaviour
{
    public void LoadCreditScene()
    {
        SceneManager.LoadScene("Credit"); 
    }
    
    public void LoadMenuScene()
    {
        SceneManager.LoadScene("Menu"); 
    }
}
