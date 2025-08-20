using UnityEngine;

public class ScoreKeeper : MonoBehaviour
{
    public static ScoreKeeper Instance;

    public int player1Score = 0;
    public int player2Score = 0;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
}