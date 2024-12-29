using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    [SerializeField]
    SceneChanger m_sceneChangerPrefab;
    [SerializeField]
    Color m_startColor;
    [SerializeField]
    Color m_tutorialColor;
    IEnumerator StartLevel(SceneChanger changer)
    {
        yield return new WaitForSeconds(1.5f);
        SceneManager.LoadScene("DeepSeekGame");
        changer.Setup(changer.Color, !changer.EndScene);
    }
    IEnumerator StartTutorial(SceneChanger changer)
    {
        yield return new WaitForSeconds(1.5f);
        SceneManager.LoadScene("DeepSeekGameTutorial");
        changer.Setup(changer.Color, !changer.EndScene);
    }
    public void StartGame()
    {
        SceneChanger changer = FindObjectOfType<SceneChanger>();
        if (!changer)
        {
            changer = Instantiate(m_sceneChangerPrefab);
        }
        changer.Setup(m_startColor, true);
        StartCoroutine(StartLevel(changer));
    }
    public void StartTutorial()
    {
        SceneChanger changer = FindObjectOfType<SceneChanger>();
        if (!changer)
        {
            changer = Instantiate(m_sceneChangerPrefab);
        }
        changer.Setup(m_tutorialColor, true);
        StartCoroutine(StartTutorial(changer));
    }
}
