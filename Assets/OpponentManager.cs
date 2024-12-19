using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class OpponentManager : MonoBehaviour
{
    [SerializeField]
    GameManager m_gameManager;
    [SerializeField]
    GameObject m_hand;
    [SerializeField]
    int m_startHandCount = 4;
    [SerializeField]
    Card m_cardPrefab;

    public event Action OnOpponentPlayed;
    public List<Card> cards = new List<Card>();

    private void Awake()
    {
        for (int i = 0; i < m_startHandCount; i++)
        {
            Card card = Instantiate(m_cardPrefab, m_hand.transform);
            card.Setup((Card.CardColor)UnityEngine.Random.Range(0, 3), UnityEngine.Random.Range(1, 10));
            card.transform.Rotate(Vector3.back, 180);
            cards.Add(card);
        }
        m_gameManager.OnPlayerPlayed += PlayCard;
    }
    void PlayCard()
    {
        StartCoroutine(PlayCorountine());
    }
    IEnumerator PlayCorountine()
    {
        yield return new WaitForSeconds(.5f);
        bool isValid = false;
        if (cards.Count == 0)
        {
            StopAllCoroutines();
        }
        foreach (Card card in cards)
        {
            if (m_gameManager.SetPlayedCard(card))
            {
                isValid = true;
                card.Play(false);
                card.transform.rotation = Quaternion.identity;
                card.CardVisual.GetComponent<CardTilter>().LerpMovement();
                //m_gameManager.ConcludePlay(true, card);
                cards.Remove(card);
                OnOpponentPlayed?.Invoke();
                break;
            }
        }
        if (!isValid)
        {
            if (cards.Count > 0)
            {
                //m_gameManager.ConcludePlay(false, cards[0]);
                cards[0].Play(false);
                cards[0].transform.rotation = Quaternion.identity;
                cards[0].CardVisual.GetComponent<CardTilter>().LerpMovement();
                cards.Remove(cards[0]);
                OnOpponentPlayed?.Invoke();

            }
        }
        foreach (Card card in cards)
        {
            card.CardVisual.GetComponent<CardTilter>().LerpMovement();
        }
    }
}
