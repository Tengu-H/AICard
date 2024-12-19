using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Card : MonoBehaviour
{
    Card.CardColor m_color;
    Image m_image;
    static readonly int HOVER_HASH = Animator.StringToHash("Hover");
    GameObject pile;
    [SerializeField]
    GameObject m_cardVisual;

    bool isHovered;
    public bool IsHovered => isHovered;
    public GameObject CardVisual => m_cardVisual;
    bool shaking;
    public enum CardColor
    {
        Red,
        Green,
        Yellow
    }
    int m_number = 1;
    CardsManager m_manager;
    Animator m_animator;
    GameManager m_gameManager;
    Transform toShake;
    public void Setup(CardColor color, int number)
    {
        shaking = false;
        this.m_color = color;
        this.m_number = number;
        int id = m_number + (int)m_color * 10 - 1;
        m_cardVisual = Instantiate(m_cardVisual, transform.position + 2000 * Random.onUnitSphere,Quaternion.identity);
        CardVisual.transform.SetParent(GameObject.Find("VisualHandler").transform);
        CardVisual.transform.localScale = Vector3.one;
        CardVisual.GetComponent<CardTilter>().card = this;
        m_animator = CardVisual.GetComponentInChildren<Animator>();
        m_image = CardVisual.GetComponentInChildren<Image>();
        CardVisual.transform.position = transform.position;
        CardVisual.transform.rotation = transform.rotation;
        CardVisual.GetComponent<CardTilter>().LerpMovement();
        if (id >= 0)
        {
            Sprite sprite = m_manager.CardSprites[id];
            m_image.sprite = sprite;
        }
        else
        {
            Debug.LogError("Wrong card id, Something wrong with setup.");
        }
        pile = GameObject.Find("GameManager").GetComponent<GameManager>().Pile;
        toShake = m_cardVisual.transform.GetChild(0);
    }

    public int Number => m_number;
    public CardColor Color => m_color;

    private void Awake()
    {
        m_manager = GameObject.Find("CardsManager").GetComponent<CardsManager>();
        m_gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
        isHovered = false;
    }

    public void OnHover()
    {
        m_animator.SetTrigger(HOVER_HASH);
        isHovered = true;
    }
    public void EndHover()
    {
        isHovered = false;
    }
    public void ClickPlay()
    {
        if (m_gameManager.m_playTimer < m_gameManager.m_playCD) return;
        Play(false);
    }
    public void Play(bool isFirst)
    {
        if (m_gameManager.isEnd) return;
        GetComponent<Button>().interactable = false;
        GameObject slot = new GameObject("Slot");
        slot.AddComponent<RectTransform>();
        slot.transform.SetParent(pile.transform);
        transform.SetParent(slot.transform);
        slot.transform.localPosition = Vector3.zero;
        transform.localPosition = Vector3.zero;
        if (!isFirst)
        {
            bool isValid = m_gameManager.SetPlayedCard(this);
            m_gameManager.ConcludePlay(isValid, this);

        }
        else
        {
            m_gameManager.PileCards.Insert(0, this);
        }
        CardVisual.transform.SetAsLastSibling();
        CardVisual.GetComponent<CardTilter>().LerpMovement();
        m_gameManager.MoveCards();
    }
    public void Shake()
    {
        shaking = true;
    }
    private void Update()
    {
        if (shaking)
        {
            toShake.localPosition = UnityEngine.Random.insideUnitSphere * 5f;
        }
    }
}
