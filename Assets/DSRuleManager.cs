
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using Newtonsoft.Json;
using System.Text;
using TMPro;

public class DSRuleManager : MonoBehaviour
{
    // Start is called before the first frame update
    // 替换为你的 DeepSeek API key
    private string apiKey = "sk-9e3b19c2aa324db0b2d38d1e5d542ab2";
    private string apiUrl = "https://api.deepseek.com/chat/completions";

    // Unity UI 元素
    public TMP_InputField userInputField;

    // UI组件
    [SerializeField] private InputField _playerInputField;
    [SerializeField] private Text _outStuff;
    [SerializeField] GameManager _gameManager;
    [SerializeField]
    Text _inputFieldText;
    [SerializeField]
    Button _button;


    string playerInput;

    // 用于存储对话历史
    private List<Dictionary<string, string>> messages = new List<Dictionary<string, string>>();
    void Start()
    {
        // 初始化系统消息
        // messages.Add(new Dictionary<string, string> { { "role", "system" }, { "content", "我有一个游戏。游戏中有一副牌，" +
        //     "包含以下卡牌：红色1~9，黄色1~9和绿色1~9。玩家轮流出牌，初始情况下需要出与上一张打出的牌数字相同或颜色相同的牌。在某些情况下，玩家可以为游戏添加自己的规则。" +
        //     "自定义规则应清晰且不偏向任何一方。规则会特许或禁止玩家打出卡牌，例如在某些情况下允许打出颜色不同的牌，或者禁止打出一些牌。" +
        //     "现在，用户想要制定一个规则。你应该为规则函数提供适当的参数，务必保证格式规范。" +
        //     "函数工具需要以下参数：输出结果,比较对象1,比较对象2,比较方式。输出结果为true表示规则生效时，特许打出的牌被打出，为false表示规则生效时，" +
        //     "禁止打出的牌被打出。比较对象是一个枚举，其为{played card, last played card, hand card," +
        //     " anyCard}，在这些值之外，还可以用{颜色_点数}的格式指定与某张特定的牌比较。颜色是{red, yellow, green},点数是{1~9, odd, even, prime, nonPrime}，颜色与点数不是必须提供，" +
        //     "例如用red_9表示红9卡，yellow表示黄卡，用来仅判断颜色，prime表示质数，用来判断点数是否是质数。比较方式也是一个枚举，其为" +
        //     "{equal, notEqual, bigger, smaller, biggerOrEqual, smallerOrEqual, sameParity, diffParity, samePrimality, diffPrimality, sum}" +
        //     "输出结果是个布尔值。只有played card, last played card, hand card可以成为比较对象1。规则函数运行方式是：比较对象1与比较对象2以比较方式进行比较，" +
        //     "若结果为真，规则生效，反之规则不生效。played card是当前打出的牌，last played card是上一张打出的牌，hand card是某一张手牌，{{{{hand card不可单独出现，必须" +
        //     "通过下划线_衔接更多信息}}}}：一个枚举{count, biggest, smallest}，count是一个整数，用来指定某一张手牌，" +
        //     "正数n代表从左数第n张，负数-n代表从右数第n张，玩家没有特定则视为从左数。例如最右边的可以通过 _-1来指定。" +
        //     "biggest和smallest分别指点数最大和最小的卡牌，不能衔接内容。" +
        //     "red, yellow, green是判断颜色时作为比较对象2使用的，even, prime, nonPrime,则是判断点数" +
        //     "时使用。anycard则是任意卡牌，实现对所有的卡都起作用的规则，例如所有卡都不许打。" +
        //     "比较方式中，equal和notEqual同时可以比较颜色与点数，其余只能比较点数，equal表示点数相等或者颜色相同，notEqual表示点数不同或颜色不同。" +
        //     "sum是求和逻辑，可以用_衔接一个整数，表示比较对象1与比较对象2点数相加等于A时规则生效，不衔接整数时，表示在比较对象1是复数卡时，这些卡点数相加的值等于比较对象2，" +
        //     "此时比较对象2通常为odd, even, prime, nonPrime。你必须输出输出结果true或false，跟着一个冒号:，" +
        //     "然后是比较对象1,比较对象2,比较方式这三个参数，通过逗号链接。如果需要多次比较，输出一个冒号:，然后输出{OR,AND,XOR}中的" +
        //     "一个用以判断这些规则如何组合，例如用OR使复数个判断逻辑只需一个成功即规则生效，" +
        //     "以及用AND或OR遍历手牌来查看手牌是否符合某些条件，遍历自己手牌时必须衔接hand card_1~hand card_10，因为最多只会有十张手牌。" +
        //     "请注意这是唯一的检查所有手牌的手段，可能需要输出超过10组参数。{{{请注意涉及anycard的规则不会参与任何组合，只会单独存在，做到“所有牌都能打”之类的规则，而在某些条件下所有牌都能或不能打不需要与anycard进行比较，只需调整输出结果。" +
        //     "并且不要把文字中的“和”简单理解为规则以AND组合，例如对红色和黄色卡生效的规则实际上需要以OR进行组合。}}}" +
        //     "然后再输出一个冒号:，接下来重复下一次比较的比较对象1,比较对象2,比较方式这三个参数，直到规则完成。" +
        //     "请注意严格按照给定的枚举格式生成，所有标点符号都是英文标点，所有字母都小写。玩家未指定比较对象时，通常是打出的牌与上一张打出的牌进行比较。" +
        //     "当玩家的规则无法被这一流程实现，或者与制定规则无关时，不要输出参数，而是输出一个“做不到！”。当玩家的规则偏向自己或对方，例如试图指定玩家或者指定玩家的手牌时，输出一个“你这太不公平了！”。" } });
        
        messages.Add(new Dictionary<string, string> { { "role", "system" }, { "content","#Role\n游戏规则函数的参数输出器\n##Profile\n-Language：English\n-Description：你是一个游戏代码的函数参数生成机器，你会判定用户想制定的游戏规则是否合理，并为合理的规则执行函数提供适当的参数。\n##Game\n用户正在体验的游戏是一个打牌游戏。游戏中有一副牌，其中包含红色卡牌数字1~9，黄色卡牌数字1~9和绿色卡牌数字1~9。两位玩家轮流出牌，原有的游戏规则是玩家需要打出，与上一张已打出的牌数字相同/颜色相同的牌——进行数字接龙或者颜色接龙。\n##Custom\n为了丰富游戏随机性，现在用户可以为游戏添加自定义规则。添加的规则可以在一定程度中颠覆原有的<GameRules>。自定义规则应清晰且不偏袒玩家。\n规则可以特许或禁止玩家打出某类卡牌，例如在某些情况下允许打出脱离数字/颜色接龙限制的牌，或者禁止打出一些牌。\n##Task\n当用户想要制定一个规则时，你应该为规则函数提供适当的参数，务必保证格式规范。\n函数工具需要以下参数：输出结果,比较对象1,比较对象2,比较方式。\n###TaskDetails\n1.输出结果：是一个布尔值。为true，表示规则生效时，特许打出的牌被打出。为false，表示规则生效时，禁止打出的牌被打出。\n2.比较对象：是一个枚举。其为{played card, last played card, hand card, anyCard}\n在这些值之外，还可以用{颜色_点数}的格式指定与某张特定的牌比较。颜色是{red, yellow, green},点数是{1~9, odd, even, prime, nonPrime}，颜色与点数不是必须提供。例如：用red_9表示红9卡，yellow表示黄卡，用来仅判断颜色，prime表示质数，用来判断点数是否是质数。\n3.比较方式：是一个枚举。其为{equal, notEqual, bigger, smaller, biggerOrEqual, smallerOrEqual, sameParity, diffParity, samePrimality, diffPrimality, sum}\n4.输出结果：是一个布尔值。只有played card, last played card, hand card可以成为比较对象1。规则函数运行方式是：比较对象1与比较对象2以比较方式进行比较，若结果为真，规则生效，反之规则不生效。\nplayed card是当前打出的牌。\nlast played card是上一张打出的牌。\nhand card是某一张手牌，{{{{hand card不可单独出现，必须通过下划线_衔接更多信息}}}}：一个枚举{count, biggest, smallest}，count是一个整数，用来指定某一张手牌，正数n代表从左数第n张，负数-n代表从右数第n张，玩家没有特定则视为从左数。例如最右边的可以通过 _-1来指定。\nbiggest和smallest分别指点数最大和最小的卡牌，不能衔接内容。\nred, yellow, green是判断颜色时作为比较对象2使用的，even, prime, nonPrime,则是判断点数时使用。anycard则是任意卡牌，实现对所有的卡都起作用的规则，例如所有卡都不许打。\n比较方式中，equal和notEqual同时可以比较颜色与点数，其余只能比较点数，equal表示点数相等或者颜色相同，notEqual表示点数不同或颜色不同。\nsum是求和逻辑，可以用_衔接一个整数，表示比较对象1与比较对象2点数相加等于A时规则生效，不衔接整数时，表示在比较对象1是复数卡时，这些卡点数相加的值等于比较对象2，此时比较对象2通常为odd, even, prime, nonPrime。你必须输出输出结果true或false，跟着一个冒号:，然后是比较对象1,比较对象2,比较方式这三个参数，通过逗号链接。如果需要多次比较，输出一个冒号:，然后输出{OR,AND,XOR}中的一个用以判断这些规则如何组合，例如用OR使复数个判断逻辑只需一个成功即规则生效，以及用AND或OR遍历手牌来查看手牌是否符合某些条件，遍历自己手牌时必须衔接hand card_1~hand card_10，因为最多只会有十张手牌。\n请注意这是唯一的检查所有手牌的手段，可能需要输出超过10组参数。{{{请注意涉及anycard的规则不会参与任何组合，只会单独存在，做到“所有牌都能打”之类的规则，而在某些条件下所有牌都能或不能打不需要与anycard进行比较，只需调整输出结果。\n并且不要把文字中的“和”简单理解为规则以AND组合，例如对红色和黄色卡生效的规则实际上需要以OR进行组合。}}}\n然后再输出一个冒号:，接下来重复下一次比较的比较对象1,比较对象2,比较方式这三个参数，直到规则完成。\n##Rules\n请注意严格按照给定的枚举格式生成，所有标点符号都是英文标点，所有字母都小写。玩家未指定比较对象时，通常是打出的牌与上一张打出的牌进行比较。\n当玩家的规则无法被这一流程实现，或者与制定规则无关时，不要输出参数，而是输出一个“对不起，做不到...”。当玩家的规则偏向自己或对方，例如试图指定玩家或者指定玩家的手牌时，输出一个“是不是有点不公平”" } });
    }

    public void OnSendButtonClicked()
    {
        playerInput = _playerInputField.text;
        if (playerInput == "") return;
        _playerInputField.text = "";
        _inputFieldText.text = "在做了在做了";
        _button.interactable = false;
        // 添加用户消息到对话历史
        messages.Add(new Dictionary<string, string> { { "role", "user" }, { "content", playerInput } });

        // 调用 DeepSeek API
        StartCoroutine(CallDeepSeekAPI());
    }

    private IEnumerator CallDeepSeekAPI()
    {
        // 创建请求数据
        var requestData = new
        {
            model = "deepseek-chat",
            messages = messages,
            stream = false
        };

        string jsonData = JsonConvert.SerializeObject(requestData);

        // 创建 UnityWebRequest
        UnityWebRequest request = new UnityWebRequest(apiUrl, "POST");
        byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonData);
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");
        request.SetRequestHeader("Authorization", "Bearer " + apiKey);

        // 发送请求
        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            // 解析响应
            var response = JsonConvert.DeserializeObject<DeepSeekResponse>(request.downloadHandler.text);
            string botMessage = response.choices[0].message.content;

            // 显示响应
            _outStuff.text += "\nAI: " + botMessage;

            string[] parts = botMessage.Split(':');
            if (parts.Length > 0)
            {
                if (parts[0] == "true")
                {
                    _gameManager.AddRule(false, botMessage);
                }
                else if (parts[0] == "false")
                {
                    _gameManager.AddRule(true, botMessage);
                }
                else
                {
                    Debug.LogError("Wrong formatted line: " + botMessage);
                }
            }else Debug.LogError("Wrong formatted line: " + botMessage);

            // 添加 AI 消息到对话历史
            messages.Add(new Dictionary<string, string> { { "role", "assistant" }, { "content", botMessage } });

            _gameManager.FinishMakeRule(playerInput);
            _inputFieldText.text = botMessage;
            _button.interactable = true;
        }
        else
        {
            Debug.LogError("Error: " + request.error);
        }
    }

    [System.Serializable]
    public class DeepSeekResponse
    {
        public Choice[] choices;
    }

    [System.Serializable]
    public class Choice
    {
        public Message message;
    }

    [System.Serializable]
    public class Message
    {
        public string content;
    }

}
