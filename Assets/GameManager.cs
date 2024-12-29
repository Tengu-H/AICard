using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using UnityEngine.SceneManagement;
using System.IO;

public struct RuleSet
{
    public RuleType type;
    public Target target;
    public CompMethod CompMethod;
    public int depth;
    public int result;
    public bool condition;
    public bool outcome;
    public int number;
}
public enum RuleType
{
    Color,
    Number
}
public enum Target
{
    lastPlayedCard,
    playedCards,
    redCards,
    yellowCards,
    greenCards,
    oddCards,
    evenCards,
    primeCards,
    nonPrimeCards,
    numberCards,
    anyCard
}
public enum CompMethod
{
    equal,
    notEqual,
    bigger,
    smaller,
    biggerOrEqual,
    smallerOrEqual,
    sameParity,
    diffParity,
    samePrimality,
    diffPrimality,
    sum
}

public enum CompObj
{
    playedCard,
    lastPlayedCard,
    handCard,
    anyCard
}
public enum CompMeth
{
    equal, notEqual, bigger, smaller, biggerOrEqual, smallerOrEqual, sameParity, diffParity, samePrimality, diffPrimality, sum
}
public class GameManager : MonoBehaviour
{
    [SerializeField]
    int m_startHandCount = 5;
    [SerializeField]
    GameObject m_hand;
    [SerializeField]
    GameObject m_OpponentHand;
    [SerializeField]
    OpponentManager m_opponentManager;
    [SerializeField]
    GameObject m_pile;
    [SerializeField]
    Card m_cardPrefab;
    [SerializeField]
    GameObject m_makeRuleUI;
    [SerializeField]
    GameObject m_rules;
    [SerializeField]
    GameObject m_rulePrefab;
    [SerializeField]
    int m_maxPlay = 10;
    [SerializeField]
    SceneChanger m_sceneChangerPrefab;
    [SerializeField]
    Color m_winColor;
    [SerializeField]
    Color m_loseColor;
    [SerializeField]
    int m_ruleCD = 3;
    [SerializeField]
    Button m_ruleButton;
    [SerializeField]
    ParticleSystem m_opponentParticle;
    [SerializeField]
    ParticleSystem m_playerParticle;
    [SerializeField]
    AudioSource punishSFX;

    public float m_playCD;

    public float m_playTimer;

    int ruleCDCount = 0;

    List<Card> m_pileCards;
    List<RuleSet> m_rulesForbidden;
    List<RuleSet> m_rulesPriviledged;
    List<List<RuleSet>> m_rulesForbiddenSets;
    List<List<RuleSet>> m_rulesPriviledgedSets;
    List<bool> m_rulesForbiddenSets_isAnd;
    List<bool> m_rulesPriviledgedSets_isAnd;

    List<string> m_rulesPrivs;
    List<string> m_rulesForbs;

    public event Action OnPlayerPlayed;
    public GameObject Pile => m_pile;
    public List<Card> PileCards => m_pileCards;
    Card playedCard;
    public bool isPlayer;
    public bool isEnd = false;
    Vector3 pilePos;

    int currWinStreak = 0;
    int maxWinStreak = 0;
    string filePath;
    bool isTutorial;
    bool tutorialShowed = false;
    // Start is called before the first frame update
    void Awake()
    {
        m_rulesForbidden = new List<RuleSet>();
        m_rulesPriviledged = new List<RuleSet>();
        m_rulesForbiddenSets = new List<List<RuleSet>>();
        m_rulesPriviledgedSets = new List<List<RuleSet>>();
        m_rulesForbiddenSets_isAnd = new List<bool>();
        m_rulesPriviledgedSets_isAnd = new List<bool>();

        m_rulesPrivs = new List<string>();
        m_rulesForbs = new List<string>();
        m_pileCards = new List<Card>();
        pilePos = m_pile.transform.position;
        for (int i = 0; i < m_startHandCount; i++)
        {
            Card card = Instantiate(m_cardPrefab, m_hand.transform);
            card.Setup((Card.CardColor)UnityEngine.Random.Range(0, 3), UnityEngine.Random.Range(1, 10));
        }
        Card startCard = Instantiate(m_cardPrefab, m_hand.transform);
        startCard.Setup((Card.CardColor)UnityEngine.Random.Range(0, 3), UnityEngine.Random.Range(1, 10));
        m_pileCards.Add(startCard);
        startCard.Play(true);
        isPlayer = true;
        m_ruleButton.interactable = false;
        filePath = Path.Combine(Application.persistentDataPath, "saveData.txt");
        LoadData();
        isTutorial = FindObjectOfType<TutorialManger>();
    }
    private void Update()
    {
        if (m_maxPlay == 1)
        {
            foreach (Card card in m_pileCards)
            {
                card.Shake();
            }
        }

        m_playTimer += Time.deltaTime;


    }
    public bool SetPlayedCard(Card card)
    {
        playedCard = card;
        bool base_1 = CheckCard(RuleType.Color, Target.lastPlayedCard);
        bool base_2 = CheckCard(RuleType.Number, Target.lastPlayedCard);
        bool finalResult = true;
        if (!base_1 && !base_2)
        {
            finalResult = false;
        }
        //foreach (List<RuleSet> ruleList in m_rulesForbiddenSets)
        //{
        //    bool isAnd = m_rulesForbiddenSets_isAnd[index];
        //    foreach (RuleSet rule in ruleList)
        //    {
        //        if (isAnd)
        //        {
        //            if (!CheckCard(rule.type, rule.target, rule.depth, rule.result, rule.condition, rule.CompMethod, rule.number))
        //            {
        //                if (rule.outcome == false)
        //                {
        //                    break;
        //                }
        //            }
        //            finalResult = false;
        //        }
        //        else
        //        {
        //            if (CheckCard(rule.type, rule.target, rule.depth, rule.result, rule.condition, rule.CompMethod, rule.number))
        //            {
        //                if (rule.outcome == false)
        //                {
        //                    finalResult = false;
        //                    break;
        //                }
        //            }
        //        }
        //    }
        //    index++;
        //}
        foreach (string rule in m_rulesForbs)
        {
            if (CheckCard(rule))
            {
                finalResult = false;
                break;
            }
        }
        foreach (string rule in m_rulesPrivs)
        {
            if (CheckCard(rule))
            {
                finalResult = true;
                break;
            }
        }
        //foreach (List<RuleSet> ruleList in m_rulesPriviledgedSets)
        //{
        //    bool isAnd = m_rulesPriviledgedSets_isAnd[index];
        //    foreach (RuleSet rule in ruleList)
        //    {
        //        if (isAnd)
        //        {
        //            if (!CheckCard(rule.type, rule.target, rule.depth, rule.result, rule.condition, rule.CompMethod, rule.number))
        //            {
        //                if (rule.outcome == true)
        //                {
        //                    break;
        //                }
        //            }
        //            finalResult = true;
        //        }
        //        else
        //        {
        //            if (CheckCard(rule.type, rule.target, rule.depth, rule.result, rule.condition, rule.CompMethod, rule.number))
        //            {
        //                if (rule.outcome == true)
        //                {
        //                    finalResult = true;
        //                    break;
        //                }
        //            }
        //        }
        //    }
        //    index++;
        //}
        return finalResult;
    }
    public void ConcludePlay(bool result, Card card)
    {
        if (!result)
        {
            Debug.Log("Invalid Play!");
            Punish();
        }
        m_pileCards.Insert(0, card);
        if (m_hand.transform.childCount == 0)
        {
            EndGame(true);
        }
        if (m_OpponentHand.transform.childCount == 0)
        {
            EndGame(false);
        }
        if (m_maxPlay == 0)
        {
            EndGame(false);
        }
        if (isEnd) return;
        if (isPlayer)
        {
            m_playTimer = 0;
            OnPlayerPlayed?.Invoke();
            m_maxPlay--;
            ruleCDCount++;
            if (ruleCDCount == m_ruleCD)
            {
                ruleCDCount = 0;
                m_ruleButton.interactable = true;
                if(isTutorial && !tutorialShowed)
                {
                    FindObjectOfType<TutorialManger>().RuleTutorial();
                    tutorialShowed = true;
                }
            }
        }
        isPlayer = !isPlayer;
    }
    public void MoveCards()
    {
        Card[] cards = m_hand.transform.GetComponentsInChildren<Card>();
        foreach (Card card in cards)
        {
            card.CardVisual.GetComponent<CardTilter>().LerpMovement();
        }
    }
    bool CheckCard(string rule)
    {
        string[] parts = rule.Split(':');
        int count = parts.Length;
        if (count % 2 != 0 || count == 0) FormatError(rule);
        int i = 2;
        bool bActive = RulePart(parts[1]); ;
        while (i < count)
        {
            if (parts[i] == "AND")
            {
                bActive = bActive & RulePart(parts[i + 1]);
            }
            else if (parts[i] == "OR")
            {
                bActive = bActive | RulePart(parts[i + 1]);
            }
            else if (parts[i] == "XOR")
            {
                bActive = bActive != RulePart(parts[i + 1]);
            }
            else
            {
                FormatError(parts[i]);
            }

            i += 2;
        }
        return bActive;
    }
    bool RulePart(string part)
    {
        string[] parts = part.Split(',');
        int count = parts.Length;
        if (count != 3) FormatError(part);

        var obj1 = parts[0].Trim();
        var obj2 = parts[1].Trim();
        var comparison = parts[2].Trim();

        Card[] handCards = m_hand.GetComponentsInChildren<Card>();
        Card[] oppHandCards = m_OpponentHand.GetComponentsInChildren<Card>();

        Card card1 = GetCard(obj1, playedCard, m_pileCards[0], handCards, oppHandCards);
        Card card2 = GetCard(obj2, playedCard, m_pileCards[0], handCards, oppHandCards);

        return CompareCards(card1, card2, comparison);
    }

    private Card GetCard(string obj, Card playedCard, Card lastPlayedCard, Card[] handCards, Card[] oppHandCards)
    {
        if (obj == "played card") return playedCard;
        if (obj == "last played card") return lastPlayedCard;
        if (obj.StartsWith("hand card"))
        {
            var parts = obj.Split('_');
            var specifier = parts[1];

            Card[] cards = isPlayer ? handCards : oppHandCards;

            if (specifier == "biggest") return cards.OrderByDescending(c => c.Number).FirstOrDefault();
            if (specifier == "smallest") return cards.OrderBy(c => c.Number).FirstOrDefault();
            if (int.TryParse(specifier, out int index))
            {
                return index > 0 ? cards[Math.Clamp(index - 1,0,9)] : cards[Math.Clamp(cards.Length + index,0,9)];
            }
        }

        var colorNumber = obj.Split('_');
        var color = colorNumber[0];
        var number = colorNumber.Length > 1 ? colorNumber[1] : null;

        Card.CardColor cardColor = Card.CardColor.Green;
        switch (color)
        {
            case "green":
                cardColor = Card.CardColor.Green;
                break;
            case "red":
                cardColor = Card.CardColor.Red;
                break;
            case "yellow":
                cardColor = Card.CardColor.Yellow;
                break;
            case "odd":
                number = "5";
                break;
            case "even":
                number = "4";
                break;
            case "prime":
                number = "5";
                break;
            case "nonprime":
                number = "4";
                break;
            default:
                if (int.TryParse(color, out int num))
                {
                    number = color;
                }
                else
                {
                    throw new ArgumentException("Invalid color or number format");
                }
                break;
        }

        int cardNumber = number != null ? int.Parse(number) : 11;
        Card toReturn = Instantiate(m_cardPrefab, 10000f * Vector3.up, Quaternion.identity);
        toReturn.Setup(cardColor, cardNumber);
        return toReturn;
    }

    private bool CompareCards(Card card1, Card card2, string comparison)
    {
        switch (comparison)
        {
            case "equal":
                return card1.Number == card2.Number || card1.Color == card2.Color;
            case "notEqual":
                return card1.Number != card2.Number || card1.Color != card2.Color;
            case "bigger":
                return card1.Number > card2.Number;
            case "smaller":
                return card1.Number < card2.Number;
            case "biggerOrEqual":
                return card1.Number >= card2.Number;
            case "smallerOrEqual":
                return card1.Number <= card2.Number;
            case "sameParity":
                return (card1.Number % 2) == (card2.Number % 2);
            case "diffParity":
                return (card1.Number % 2) != (card2.Number % 2);
            case "samePrimality":
                return IsPrime(card1.Number) == IsPrime(card2.Number);
            case "diffPrimality":
                return IsPrime(card1.Number) != IsPrime(card2.Number);
            case "sum":
                return card1.Number + card2.Number == int.Parse(comparison.Split('_')[1]);
            default:
                throw new ArgumentException("Invalid comparison type");
        }
    }

    private static bool IsPrime(int number)
    {
        if (number <= 1) return false;
        if (number == 2) return true;
        if (number % 2 == 0) return false;
        for (int i = 3; i <= Math.Sqrt(number); i += 2)
        {
            if (number % i == 0) return false;
        }
        return true;
    }
    void FormatError(string line)
    {
        Debug.LogError("Wrong formatted line: " + line);
    }
    bool CheckCard(RuleType type, Target target, int depth = 1, int result = 10, bool condition = true, CompMethod compMethod = CompMethod.equal, int number = 5)
    {
        switch (type)
        {
            case RuleType.Color:
                switch (target)
                {
                    case Target.lastPlayedCard:
                        return (playedCard.Color == m_pileCards[0].Color) == condition;
                    case Target.playedCards:
                        for (int i = 0; i < depth; i++)
                        {
                            if (m_pileCards.Capacity >= i)
                            {
                                if ((m_pileCards[i].Color == playedCard.Color) != condition)
                                {
                                    return false;
                                }
                            }
                            else
                            {
                                return false;
                            }
                        }
                        return true;
                    case Target.greenCards:
                        return (playedCard.Color == Card.CardColor.Green) == condition;
                    case Target.redCards:
                        return (playedCard.Color == Card.CardColor.Red) == condition;
                    case Target.yellowCards:
                        return (playedCard.Color == Card.CardColor.Yellow) == condition;
                    case Target.anyCard:
                        return true;
                    default:
                        return false;
                }
            case RuleType.Number:
                Card compTarget = null;
                switch (target)
                {
                    case Target.playedCards:
                        int resultSum = 0;
                        for (int i = 0; i < depth; i++)
                        {
                            if (m_pileCards.Count >= i)
                            {
                                switch (compMethod)
                                {
                                    case CompMethod.equal:
                                        if (playedCard.Number != m_pileCards[i].Number)
                                        {
                                            return false;
                                        }
                                        break;
                                    case CompMethod.notEqual:
                                        if (playedCard.Number == m_pileCards[i].Number)
                                        {
                                            return false;
                                        }
                                        break;
                                    case CompMethod.bigger:
                                        if (playedCard.Number <= m_pileCards[i].Number)
                                        {
                                            return false;
                                        }
                                        break;
                                    case CompMethod.biggerOrEqual:
                                        if (playedCard.Number < m_pileCards[i].Number)
                                        {
                                            return false;
                                        }
                                        break;
                                    case CompMethod.smaller:
                                        if (playedCard.Number >= m_pileCards[i].Number)
                                        {
                                            return false;
                                        }
                                        break;
                                    case CompMethod.smallerOrEqual:
                                        if (playedCard.Number > m_pileCards[i].Number)
                                        {
                                            return false;
                                        }
                                        break;
                                    case CompMethod.sameParity:
                                        if (playedCard.Number % 2 != m_pileCards[i].Number % 2)
                                        {
                                            return false;
                                        }
                                        break;
                                    case CompMethod.diffParity:
                                        if (playedCard.Number % 2 == m_pileCards[i].Number % 2)
                                        {
                                            return false;
                                        }
                                        break;
                                    case CompMethod.samePrimality:
                                        if (isPrime(playedCard.Number) != isPrime(m_pileCards[i].Number))
                                        {
                                            return false;
                                        }
                                        break;
                                    case CompMethod.diffPrimality:
                                        if (isPrime(playedCard.Number) == isPrime(m_pileCards[i].Number))
                                        {
                                            return false;
                                        }
                                        break;
                                    case CompMethod.sum:
                                        if (resultSum < playedCard.Number)
                                        {
                                            resultSum = playedCard.Number;
                                        }
                                        resultSum += m_pileCards[i].Number;
                                        break;
                                    default:
                                        return false;
                                }
                            }
                            else
                            {
                                return false;
                            }
                        }
                        if (compMethod == CompMethod.sum)
                        {
                            return result == resultSum;
                        }
                        return true;
                    case Target.lastPlayedCard:
                        compTarget = m_pileCards[0];
                        switch (compMethod)
                        {
                            case CompMethod.equal:
                                return playedCard.Number == compTarget.Number;
                            case CompMethod.notEqual:
                                return playedCard.Number != compTarget.Number;
                            case CompMethod.bigger:
                                return playedCard.Number > compTarget.Number;
                            case CompMethod.biggerOrEqual:
                                return playedCard.Number >= compTarget.Number;
                            case CompMethod.smaller:
                                return playedCard.Number < compTarget.Number;
                            case CompMethod.smallerOrEqual:
                                return playedCard.Number <= compTarget.Number;
                            case CompMethod.sameParity:
                                return playedCard.Number % 2 == compTarget.Number % 2;
                            case CompMethod.diffParity:
                                return playedCard.Number % 2 != compTarget.Number % 2;
                            case CompMethod.samePrimality:
                                return isPrime(playedCard.Number) == isPrime(compTarget.Number);
                            case CompMethod.diffPrimality:
                                return isPrime(playedCard.Number) != isPrime(compTarget.Number);
                            case CompMethod.sum:
                                return playedCard.Number + compTarget.Number == result;
                            default:
                                return false;

                        }
                    case Target.oddCards:
                        switch (compMethod)
                        {
                            case CompMethod.sameParity:
                                return playedCard.Number % 2 == 1;
                            case CompMethod.diffParity:
                                return playedCard.Number % 2 == 0;
                            default:
                                return false;
                        }
                    case Target.evenCards:
                        switch (compMethod)
                        {
                            case CompMethod.sameParity:
                                return playedCard.Number % 2 != 1;
                            case CompMethod.diffParity:
                                return playedCard.Number % 2 != 0;
                            default:
                                return false;
                        }
                    case Target.primeCards:
                        switch (compMethod)
                        {
                            case CompMethod.samePrimality:
                                return isPrime(playedCard.Number);
                            case CompMethod.diffPrimality:
                                return !isPrime(playedCard.Number);
                            default:
                                return false;
                        }
                    case Target.nonPrimeCards:
                        switch (compMethod)
                        {
                            case CompMethod.samePrimality:
                                return !isPrime(playedCard.Number);
                            case CompMethod.diffPrimality:
                                return isPrime(playedCard.Number);
                            default:
                                return false;
                        }
                    case Target.numberCards:
                        switch (compMethod)
                        {
                            case CompMethod.equal:
                                return playedCard.Number == number;
                            case CompMethod.notEqual:
                                return playedCard.Number != number;
                            case CompMethod.bigger:
                                return playedCard.Number > number;
                            case CompMethod.biggerOrEqual:
                                return playedCard.Number >= number;
                            case CompMethod.smaller:
                                return playedCard.Number < number;
                            case CompMethod.smallerOrEqual:
                                return playedCard.Number <= number;
                            case CompMethod.sameParity:
                                return playedCard.Number % 2 == number % 2;
                            case CompMethod.diffParity:
                                return playedCard.Number % 2 != number % 2;
                            case CompMethod.samePrimality:
                                return isPrime(playedCard.Number) == isPrime(number);
                            case CompMethod.diffPrimality:
                                return isPrime(playedCard.Number) != isPrime(number);
                            case CompMethod.sum:
                                return playedCard.Number + number == result;
                            default:
                                return false;
                        }
                    case Target.anyCard:
                        return true;
                    default:
                        return false;

                }
            default:
                return false;
        }
    }
    bool isPrime(int number)
    {
        if (number == 1) return false;
        if (number == 2) return true;

        var limit = Math.Ceiling(Math.Sqrt(number)); //hoisting the loop limit

        for (int i = 2; i <= limit; ++i)
            if (number % i == 0)
                return false;
        return true;

    }
    void Punish()
    {
        punishSFX.Play();
        if (isPlayer)
        {
            Card card = Instantiate(m_cardPrefab, m_hand.transform);
            card.Setup((Card.CardColor)UnityEngine.Random.Range(0, 3), UnityEngine.Random.Range(1, 10));
            m_playerParticle.Emit(100);
        }
        else
        {
            Card card = Instantiate(m_cardPrefab, m_OpponentHand.transform);
            card.Setup((Card.CardColor)UnityEngine.Random.Range(0, 3), UnityEngine.Random.Range(1, 10));
            card.transform.Rotate(Vector3.back, 180);
            m_opponentManager.cards.Add(card);
            m_opponentParticle.Emit(100);
        }
    }
    public void AddRule(bool isForbid, RuleSet rule)
    {
        if (isForbid)
        {
            m_rulesForbidden.Add(rule);
        }
        else
        {
            m_rulesPriviledged.Add(rule);
        }
    }
    public void AddRule(bool isForbid, string rule)
    {
        if (isForbid)
        {
            m_rulesForbs.Add(rule);
        }
        else
        {
            m_rulesPrivs.Add(rule);
        }

    }
    public void FinishMakeRule(string description)
    {
        GameObject rule = Instantiate(m_rulePrefab, m_rules.transform);
        rule.GetComponentInChildren<Text>().text = description;
        m_ruleButton.interactable = false;
        m_makeRuleUI.SetActive(false);
    }
    public void ShowMakeRuleUI()
    {
        m_makeRuleUI.SetActive(!m_makeRuleUI.activeInHierarchy);
    }
    public void EndGame(bool win)
    {
        if (isEnd) return;
        Debug.Log("Game Ended!");
        isEnd = true;
        Color color = win ? m_winColor : m_loseColor;
        SceneChanger changer = FindObjectOfType<SceneChanger>();
        if (!changer)
        {

            changer = Instantiate(m_sceneChangerPrefab);
        }
        if (win)
        {
            currWinStreak++;
            maxWinStreak = Math.Max(currWinStreak, maxWinStreak);
        }
        else
        {
            currWinStreak = 0;
        }
        changer.Setup(color, true, maxWinStreak, currWinStreak);
        SaveData();
        if (isTutorial)
        {
            StartCoroutine(ReturnToMenu(changer));
            return;
        }
        StartCoroutine(RestartLevel(changer));
    }
    IEnumerator RestartLevel(SceneChanger changer)
    {
        yield return new WaitForSeconds(1.5f);
        Debug.Log(SceneManager.GetActiveScene().ToString());
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        changer.Setup(changer.Color, !changer.EndScene, maxWinStreak, currWinStreak);
    }
    IEnumerator ReturnToMenu(SceneChanger changer)
    {
        yield return new WaitForSeconds(1.5f);
        Debug.Log(SceneManager.GetActiveScene().ToString());
        SceneManager.LoadScene("MainMenu");
        changer.Setup(changer.Color, !changer.EndScene, maxWinStreak, currWinStreak);
    }

    void LoadData()
    {
        if (File.Exists(filePath))
        {
            using (StreamReader reader = new StreamReader(filePath))
            {
                maxWinStreak = int.Parse(reader.ReadLine());
                currWinStreak = int.Parse(reader.ReadLine());
            }
        }
        else
        {
            maxWinStreak = 0;
            currWinStreak = 0;
        }
    }
    void SaveData()
    {
        using (StreamWriter writer = new StreamWriter(filePath))
        {
            writer.WriteLine(maxWinStreak);
            writer.WriteLine(currWinStreak);
            writer.WriteLine("第一行是最高连胜，第二行是目前的连胜。想改就改吧！");
        }
    }
}
