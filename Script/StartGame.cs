using UnityEngine;

public class StartGame : MonoBehaviour
{
    public GameObject startUI;
    private void Start()
    {
        startUI.SetActive(true);
    }
    public void StartTheGame()
    {
        Debug.Log("Game Started!");
        Movement.gameHasStarted = true;
        startUI.SetActive(false);
    }
}
