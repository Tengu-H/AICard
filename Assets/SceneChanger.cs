using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SceneChanger : MonoBehaviour
{
    [SerializeField]
    AnimationCurve m_xScaler;
    [SerializeField]
    AnimationCurve m_yScaler;
    [SerializeField]
    AnimationCurve m_xScalerCounter;
    [SerializeField]
    AnimationCurve m_yScalerCounter;
    [SerializeField]
    float m_totalTime;
    [SerializeField]
    float m_totalTimeCounter;
    [SerializeField]
    Color m_color;
    [SerializeField]
    TextMeshProUGUI m_max;
    [SerializeField]
    TextMeshProUGUI m_curr;
    [SerializeField]
    Image m_cover;


    bool m_endScene;
    float m_timer = 0;
    float m_timerCounter = 0;
    bool m_setuped = false;

    public Color Color => m_color;
    public bool EndScene => m_endScene;
    private void Awake()
    {
        if (FindObjectsOfType<SceneChanger>().Length > 1)
        {
            Destroy(gameObject);
            return;
        }
        DontDestroyOnLoad(this);
    }
    // Start is called before the first frame update
    void Start()
    {
        if (!m_setuped)
        {
            Setup(m_color, true, 0, 0);
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (m_endScene)
        {
            if(m_timer <= m_totalTime)
            {
                m_timer += Time.deltaTime;
                m_cover.transform.localScale = new Vector3(m_xScaler.Evaluate(m_timer/m_totalTime), m_yScaler.Evaluate(m_timer / m_totalTime), 1f);
            }
            if (m_timerCounter <= m_totalTimeCounter)
            {
                m_timerCounter += Time.deltaTime;
                m_max.transform.localScale = new Vector3(m_xScalerCounter.Evaluate(m_timerCounter / m_totalTimeCounter), m_yScalerCounter.Evaluate(m_timerCounter / m_totalTimeCounter), 1f);
                m_curr.transform.localScale = new Vector3(m_xScalerCounter.Evaluate(m_timerCounter / m_totalTimeCounter), m_yScalerCounter.Evaluate(m_timerCounter / m_totalTimeCounter), 1f);
            }
        }
        else
        {
            if(m_timer >= 0)
            {
                m_timer -= Time.deltaTime;
                m_cover.transform.localScale = new Vector3(m_xScaler.Evaluate(m_timer / m_totalTime), m_yScaler.Evaluate(m_timer / m_totalTime), 1f);
            }
            if (m_timerCounter >= 0)
            {
                m_timerCounter -= Time.deltaTime;
                m_max.transform.localScale = new Vector3(m_xScalerCounter.Evaluate(m_timerCounter / m_totalTimeCounter), m_yScalerCounter.Evaluate(m_timerCounter / m_totalTimeCounter), 1f);
                m_curr.transform.localScale = new Vector3(m_xScalerCounter.Evaluate(m_timerCounter / m_totalTimeCounter), m_yScalerCounter.Evaluate(m_timerCounter / m_totalTimeCounter), 1f);
            }
        }
    }
    public void Setup(Color color, bool endScene, int max = -1, int curr = -1)
    {
        m_color = color;
        m_endScene = endScene;
        m_max.text = max >= 0 ? "×î¸ß: " + max.ToString() : "";
        m_curr.text = curr >= 0 ? "Á¬Ê¤: " + curr.ToString() : "";
        m_cover.color = m_color;
        if (endScene)
        {
            m_timer = 0;
            m_timerCounter = 0;
        }
        else
        {
            m_timer = m_totalTime;
            m_timerCounter = m_totalTimeCounter;
        }
        m_cover.transform.localScale = new Vector3(m_xScaler.Evaluate(m_timer / m_totalTime), m_yScaler.Evaluate(m_timer / m_totalTime), 1f);
        m_max.transform.localScale = new Vector3(m_xScalerCounter.Evaluate(m_timerCounter / m_totalTimeCounter), m_yScalerCounter.Evaluate(m_timerCounter / m_totalTimeCounter), 1f);
        m_curr.transform.localScale = new Vector3(m_xScalerCounter.Evaluate(m_timerCounter / m_totalTimeCounter), m_yScalerCounter.Evaluate(m_timerCounter / m_totalTimeCounter), 1f);
        m_setuped = true;
    }
}
