using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using GlmUnity;

public class GlmConversation : MonoBehaviour
{
    [SerializeField] private Text glmText;
    [SerializeField]
    private InputField playerInputField;

    [SerializeField, Tooltip("玩家和GLM的历史对话记录以List<SendDat>格式保存")]
    private List<SendData> chatHistory = new List<SendData>();

    public void Start()
    {
        // 创建system prompt，让GLM进行角色扮演。System prompt不是强制要求的
        SendData systemPrompt = new SendData()
        {
            role = "system",
            content = "请扮演一个武器商店的老板，玩家正在你的商店里购买物品。"
        };

        // 将system prompt作为第一条对话记录加入chatHistory
        chatHistory.Add(systemPrompt);
    }

    public async void SendPlayerResponse()
    {
        // 从InputField读取玩家的输入
        string playerInput = playerInputField.text;
        playerInputField.text = "";

        // 创建玩家SendData信息，并将其加入chatHistory
        SendData playerMessage = new SendData()
        {
            role = "user",
            content = playerInput
        };
        chatHistory.Add(playerMessage);

        // 使用GLMHandler.GenerateGLMResponse生成GLM回复，设置tmeperature=0.8
        // 注意需要使用await关键词
        SendData respone = await GlmHandler.GenerateGlmResponse(chatHistory, 0.8f);
        // 将GLM的回复加进chatHistory
        chatHistory.Add(respone);

        // 使用response.content获取GLM的回复
        glmText.text = respone.content;
    }

}
