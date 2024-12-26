
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
    // �滻Ϊ��� DeepSeek API key
    private string apiKey = "sk-9e3b19c2aa324db0b2d38d1e5d542ab2";
    private string apiUrl = "https://api.deepseek.com/chat/completions";

    // Unity UI Ԫ��
    public TMP_InputField userInputField;

    // UI���
    [SerializeField] private InputField _playerInputField;
    [SerializeField] private Text _outStuff;
    [SerializeField] GameManager _gameManager;
    [SerializeField]
    Text _inputFieldText;
    [SerializeField]
    Button _button;


    // ���ڴ洢�Ի���ʷ
    private List<Dictionary<string, string>> messages = new List<Dictionary<string, string>>();
    void Start()
    {
        // ��ʼ��ϵͳ��Ϣ
        messages.Add(new Dictionary<string, string> { { "role", "system" }, { "content", "����һ����Ϸ����Ϸ����һ���ƣ�" +
            "�������¿��ƣ���ɫ1~9����ɫ1~9����ɫ1~9������������ƣ���ʼ�������Ҫ������һ�Ŵ������������ͬ����ɫ��ͬ���ơ�������Ʋ��Ϸ�����ҽ��ܵ��ͷ�����ĳЩ����£���ҿ���Ϊ��Ϸ����Լ��Ĺ���" +
            "�Զ������Ӧ�����Ҳ�ƫ���κ�һ���������������ֹ��Ҵ�����ƣ�������ĳЩ�������������ɫ��ͬ���ƣ����߽�ֹ���һЩ�ơ�" +
            "���ڣ��û���Ҫ�ƶ�һ��������Ӧ��Ϊ�������ṩ�ʵ��Ĳ����������������˳����Ƿ�Ϸ���" +
            "����������Ҫ���²�����������,�Ƚ϶���1,�Ƚ϶���2,�ȽϷ�ʽ��������Ϊtrue��ʾ������Чʱ�����������Ʊ������Ϊfalse��ʾ������Чʱ����ֹ������Ʊ�������Ƚ϶�����һ��ö�٣���Ϊ{played card, last played card, hand card," +
            " anyCard}������Щֵ֮�⣬��������{��ɫ_����}�ĸ�ʽָ����ĳ���ض����ƱȽϡ���ɫ��{red, yellow, green},������{1~9, odd, even, prime, nonPrime}����ɫ��������Ǳ����ṩ��" +
            "������red_9��ʾ��9����yelllow��ʾ�ƿ����������ж���ɫ��prime��ʾ�����������жϵ����Ƿ����������ȽϷ�ʽҲ��һ��ö�٣���Ϊ" +
            "{equal, notEqual, bigger, smaller, biggerOrEqual, smallerOrEqual, sameParity, diffParity, samePrimality, diffPrimality, sum}" +
            "�������Ǹ�����ֵ��ֻ��played card, last played card, hand card���Գ�Ϊ�Ƚ϶���1�����������з�ʽ�ǣ��Ƚ϶���1��Ƚ϶���2�ԱȽϷ�ʽ���бȽϣ�" +
            "�����Ϊ�棬������Ч����֮������Ч��played card�ǵ�ǰ������ƣ�last played card����һ�Ŵ�����ƣ�hand card��ĳһ�����ƣ���ʹ����һ������Ҫ" +
            "ͨ���»���_�νӸ�����Ϣ�������ǡ��������򡰶Է�������ȷ����˭�����ƣ�Ȼ����һ��ö��{count, biggest, smallest}��count��һ������������ָ��ĳһ�����ƣ�" +
            "����n�����������n�ţ�����-n�����������n�ţ����û���ض�����Ϊ���������������ұߵĿ���ͨ�� ����_-1��ָ����" +
            "biggest��smallest�ֱ�ָ����������С�Ŀ��ƣ������ν����ݡ�" +
            "red, yellow, green���ж���ɫʱ��Ϊ�Ƚ϶���2ʹ�õģ�even, prime, nonPrime,�����жϵ���" +
            "ʱʹ�á�anycard�������⿨�ƣ�ʵ�ֶ����еĿ��������õĹ����������п��������" +
            "�ȽϷ�ʽ�У�equal��notEqualͬʱ���ԱȽ���ɫ�����������ֻ�ܱȽϵ�����equal��ʾ������Ȼ�����ɫ��ͬ��notEqual��ʾ������ͬ����ɫ��ͬ��" +
            "sum������߼���������_�ν�һ����������ʾ�Ƚ϶���1��Ƚ϶���2������ӵ���Aʱ������Ч�����ν�����ʱ����ʾ�ڱȽ϶���1�Ǹ�����ʱ����Щ��������ӵ�ֵ���ڱȽ϶���2��" +
            "��ʱ�Ƚ϶���2ͨ��Ϊodd, even, prime, nonPrime����������������true��false������һ��ð��:��" +
            "Ȼ���ǱȽ϶���1,�Ƚ϶���2,�ȽϷ�ʽ������������ͨ���������ӡ������Ҫ��αȽϣ����һ��ð��:��Ȼ�����{OR,AND,XOR}�е�" +
            "һ�������ж���Щ���������ϣ�������ORʹ�������ж��߼�ֻ��һ���ɹ���������Ч������true:played card,red_odd,equal:OR:played card,green_even,equal ��һ��ʽ��ʾ��ɫ����������ɫż������Զ�ܴ�" +
            "�Լ���AND��OR�����������鿴�����Ƿ����ĳЩ�����������Լ����ϵĿ�ʱֻ���ν�hand card_����_1~hand card_����_10����Ϊ���ֻ����ʮ�����ơ�" +
            "��ע������Ψһ�ļ���������Ƶ��ֶΣ�������Ҫ�������10�������{{{��ע���漰anycard�Ĺ��򲻻�����κ���ϣ�ֻ�ᵥ�����ڣ������������ƶ��ܴ�֮��Ĺ��򣬶���ĳЩ�����������ƶ��ܻ��ܴ���Ҫ��anycard���бȽϣ�ֻ�������������}}}" +
            "Ȼ�������һ��ð��:���������ظ���һ�αȽϵıȽ϶���1,�Ƚ϶���2,�ȽϷ�ʽ������������ֱ��������ɡ�" +
            "��ע���ϸ��ո�����ö�ٸ�ʽ���ɣ����б����Ŷ���Ӣ�ı�㡣���δָ���Ƚ϶���ʱ��ͨ���Ǵ����������һ�Ŵ�����ƽ��бȽϡ�" +
            "����ҵĹ����޷�����һ����ʵ�֣��������ƶ������޹�ʱ����Ҫ����������������һ��������������" } });
    }

    public void OnSendButtonClicked()
    {
        string playerInput = _playerInputField.text;
        if (playerInput == "") return;
        _playerInputField.text = "";
        _inputFieldText.text = "�ȵȣ�����˼��";
        _button.interactable = false;
        // ����û���Ϣ���Ի���ʷ
        messages.Add(new Dictionary<string, string> { { "role", "user" }, { "content", playerInput } });

        // ���� DeepSeek API
        StartCoroutine(CallDeepSeekAPI());
    }

    private IEnumerator CallDeepSeekAPI()
    {
        // ������������
        var requestData = new
        {
            model = "deepseek-chat",
            messages = messages,
            stream = false
        };

        string jsonData = JsonConvert.SerializeObject(requestData);

        // ���� UnityWebRequest
        UnityWebRequest request = new UnityWebRequest(apiUrl, "POST");
        byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonData);
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");
        request.SetRequestHeader("Authorization", "Bearer " + apiKey);

        // ��������
        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            // ������Ӧ
            var response = JsonConvert.DeserializeObject<DeepSeekResponse>(request.downloadHandler.text);
            string botMessage = response.choices[0].message.content;

            // ��ʾ��Ӧ
            _outStuff.text += "\nAI: " + botMessage;

            string[] parts = botMessage.Split(':');
            if (parts.Length > 0)
            {
                if (parts[0] == "true")
                {
                    _gameManager.AddRule(true, botMessage);
                }
                else if (parts[0] == "false")
                {
                    _gameManager.AddRule(false, botMessage);
                }
                else
                {
                    Debug.LogError("Wrong formatted line: " + botMessage);
                }
            }else Debug.LogError("Wrong formatted line: " + botMessage);

            // ��� AI ��Ϣ���Ի���ʷ
            messages.Add(new Dictionary<string, string> { { "role", "assistant" }, { "content", botMessage } });

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
