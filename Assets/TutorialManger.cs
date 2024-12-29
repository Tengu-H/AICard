using UnityEngine;
using UnityEngine.UI;
using System.Collections;

using TMPro;

public class TutorialManger : MonoBehaviour
{
    public GameObject tutorialPanel; // Reference to the tutorial panel
    public TextMeshProUGUI tutorialText; // Reference to the UI Text component
    // public TextMeshProUGUI tutorialText; // Uncomment if using TextMeshPro
    public string[] tutorialMessages; // Array of tutorial messages
    public string[] tutorialRuleMessages; // Array of tutorial messages

    private int currentMessageIndex = 0;

    bool canClick = true;
    bool isFirst = true;
    IEnumerator ClickCD(float time)
    {
        canClick = false;
        yield return new WaitForSeconds(time);
        canClick = true;
    }

    void Start()
    {
        tutorialPanel.SetActive(false); // Hide the panel initially
        StartTutorial();
        StartCoroutine(ClickCD(.5f));
    }

    public void StartTutorial()
    {
        tutorialPanel.SetActive(true); // Show the panel
        currentMessageIndex = 0; // Reset the index
        ShowNextMessage(); // Show the first message
    }

    public void ShowNextMessage()
    {
        if (currentMessageIndex < tutorialMessages.Length)
        {
            tutorialText.text = tutorialMessages[currentMessageIndex]; // Update the text
            currentMessageIndex++; // Move to the next message

            StartCoroutine(ClickCD(.5f));
        }
        else
        {
            EndTutorial(); // End tutorial if no more messages
        }
    }

    void EndTutorial()
    {
        tutorialPanel.SetActive(false); // Hide the panel
        // Optionally, reset the tutorial or trigger other actions
    }
    void Update()
    {
        // Detect mouse clicks to progress the tutorial
        if (Input.GetMouseButtonDown(0) && canClick && tutorialPanel.activeInHierarchy) // Left mouse button
        {
            GetComponentInChildren<Animator>().SetTrigger("Say");
            if (isFirst) ShowNextMessage();
            else ShowNextRuleMessage();
        }
    }

    public void RuleTutorial()
    {
        isFirst = false;
        tutorialPanel.SetActive(true); // Show the panel
        currentMessageIndex = 0; // Reset the index
        ShowNextRuleMessage(); // Show the first message
    }
    public void ShowNextRuleMessage()
    {
        if (currentMessageIndex < tutorialRuleMessages.Length)
        {
            tutorialText.text = tutorialRuleMessages[currentMessageIndex]; // Update the text
            currentMessageIndex++; // Move to the next message

            StartCoroutine(ClickCD(.5f));
        }
        else
        {
            EndTutorial(); // End tutorial if no more messages
        }
    }
}
