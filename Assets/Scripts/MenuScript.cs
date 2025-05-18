using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuScript : MonoBehaviour
{
    public void StartGame()
    {
        // Load the main game scene
        SceneManager.LoadScene("SampleScene");
    }

    public void HostGame()
    {
        // Load the host game scene
        //SceneManager.LoadScene("HostGame");
        Debug.Log("Host Game button clicked");
    }

    public void JoinGame()
    {
        // Load the join game scene
        //SceneManager.LoadScene("JoinGame");
        Debug.Log("Join Game button clicked");
    }
}
