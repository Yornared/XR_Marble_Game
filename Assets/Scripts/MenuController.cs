using UnityEngine;

public class MenuController : MonoBehaviour
{
    public GameObject MainPanel;
    public GameObject HostPanel;
    public GameObject JoinPanel;

    public void ShowHostPanel()
    {
        MainPanel.SetActive(false);
        HostPanel.SetActive(true);
        JoinPanel.SetActive(false);
    }

    public void ShowJoinPanel()
    {
        MainPanel.SetActive(false);
        HostPanel.SetActive(false);
        JoinPanel.SetActive(true);
    }

    public void ShowMainPanel()
    {
        MainPanel.SetActive(true);
        HostPanel.SetActive(false);
        JoinPanel.SetActive(false);
    }
}
