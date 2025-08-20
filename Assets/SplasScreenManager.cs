using UnityEngine;
using TMPro;
using UnityEngine.UI; // Add this line for Image
using UnityEngine.SceneManagement;
using System.Collections;
using System.Linq;

public class SplashScreenManager : MonoBehaviour
{
    public Image posterImage;
    public TextMeshProUGUI loadingText;
    public TextMeshProUGUI pressAnyKeyText;
    public AudioClip ambientMusic;

    [Header("Loading Settings")]
    public float loadingDuration = 5f;
    public float pulseSpeed = 1f;
    public float minScale = 0.8f;
    public float maxScale = 1.2f;

    [Header("Typewriter Settings")]
    public float typewriterSpeed = 0.1f;

    private AudioSource audioSource;
    private bool isLoading = true;

    void Start()
    {
        Debug.Log("Active Scene: " + SceneManager.GetActiveScene().name);
        Debug.Log("Loaded Scenes: " + string.Join(", ", Enumerable.Range(0, SceneManager.sceneCount).Select(i => SceneManager.GetSceneAt(i).name)));
        
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.clip = ambientMusic;
        audioSource.loop = true;
        audioSource.Play();

        pressAnyKeyText.gameObject.SetActive(false);
        StartCoroutine(LoadGame());
    }

    IEnumerator LoadGame()
    {
        float elapsedTime = 0f;
        while (elapsedTime < loadingDuration)
        {
            // Pulsing effect
            float scale = Mathf.Lerp(minScale, maxScale, (Mathf.Sin(elapsedTime * pulseSpeed) + 1) / 2);
            loadingText.transform.localScale = Vector3.one * scale;

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        loadingText.gameObject.SetActive(false);
        yield return StartCoroutine(TypewriterEffect("Press any key to start"));
        isLoading = false;
    }

    IEnumerator TypewriterEffect(string message)
    {
        pressAnyKeyText.gameObject.SetActive(true);
        pressAnyKeyText.text = "";
        foreach (char c in message)
        {
            pressAnyKeyText.text += c;
            yield return new WaitForSeconds(typewriterSpeed);
        }
    }

    void Update()
    {
        if (!isLoading && Input.anyKeyDown)
        {
            SceneManager.LoadScene("MainMenu");
        }
    }
}