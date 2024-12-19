using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CardTilter : MonoBehaviour
{
    public Card card;
    [SerializeField]
    float tiltStrength;
    [SerializeField]
    float m_lerpTime;

    public float m_timer = 0;
    public bool isMoving = false;
    Vector3 startPos;
    Quaternion startRot;

    Transform m_visual;
    // Start is called before the first frame update
    void Awake()
    {
        m_visual = transform.GetChild(0);
        m_timer = 0;
    }

    // Update is called once per frame
    void Update()
    {
        if (card.IsHovered)
        {
            Vector3 mousePos = Input.mousePosition;
            mousePos = Camera.main.ScreenToWorldPoint(mousePos);
            Vector3 offset = transform.position - mousePos;
            transform.rotation = new Quaternion(offset.y * -1 * tiltStrength, offset.x * tiltStrength, 0,1) ;
        }
        else
        {
            transform.rotation = Quaternion.identity;
        }
        if (isMoving)
        {
            m_timer += Time.deltaTime;
            if (m_timer < m_lerpTime)
            {
                float t = m_timer / m_lerpTime;
                t = t * t * (3f - 2f * t);
                transform.position = Vector3.Lerp(startPos, card.transform.position, t);
                m_visual.rotation = Quaternion.Lerp(startRot, card.transform.rotation, t);
            }
            else
            {
                transform.position = card.transform.position;
                m_visual.rotation = card.transform.rotation;
                isMoving = false;
            }
        }
    }
    public void LerpMovement()
    {
        startPos = transform.position;
        startRot = m_visual.rotation;
        m_timer = 0;
        isMoving = true;
    }
    private void LateUpdate()
    {
        
    }
}
