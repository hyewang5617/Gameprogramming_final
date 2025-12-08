using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class TutorialManager : MonoBehaviour
{
    [Header("UI References")]
    public GameObject tutorialPanel;
    public Text tutorialText;

    [Header("Tutorial Settings")]
    public float displayDuration = 2f;
    public string tutorialSceneName = "Stage1";

    string[] tutorialMessages = new string[]
    {
        "WASD TO MOVE",
        "SPACE TO JUMP",
        "MOUSE TO LOOK AROUND",
        "SHIFT TO SPRINT"
    };

    int currentIndex = 0;
    bool isTutorialActive = false;

    void Start()
    {
        if (tutorialPanel != null)
            tutorialPanel.SetActive(false);
        
        if (tutorialText != null)
            tutorialText.text = "";
    }

    public void StartTutorial()
    {
        string currentSceneName = SceneManager.GetActiveScene().name;
        string currentSceneLower = currentSceneName.ToLower().Trim();
        string tutorialSceneLower = tutorialSceneName.ToLower().Trim();
        
        if (string.IsNullOrEmpty(tutorialSceneLower) || !currentSceneLower.Contains(tutorialSceneLower))
            return;
        
        if (isTutorialActive) return;
        
        isTutorialActive = true;
        StartCoroutine(ShowTutorialSequence());
    }

    IEnumerator ShowTutorialSequence()
    {
        if (tutorialPanel == null || tutorialText == null)
        {
            isTutorialActive = false;
            yield break;
        }

        tutorialPanel.SetActive(true);
        tutorialText.gameObject.SetActive(true);

        for (int i = 0; i < tutorialMessages.Length; i++)
        {
            currentIndex = i;
            tutorialText.text = tutorialMessages[i];
            tutorialText.enabled = true;
            
            yield return new WaitForSeconds(displayDuration);
            
            tutorialText.text = "";
            
            if (i < tutorialMessages.Length - 1)
                yield return new WaitForSeconds(0.3f);
        }

        tutorialText.gameObject.SetActive(false);
        tutorialPanel.SetActive(false);
        isTutorialActive = false;
    }
}

