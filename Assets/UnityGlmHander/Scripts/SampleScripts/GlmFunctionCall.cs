using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using GlmUnity;

public class GlmFunctionCall : MonoBehaviour
{
    // UI组件
    [SerializeField] private InputField _playerInputField;
    [SerializeField] private Text _weaponNameText, _weaponDescriptionText, _weaponPriceText, _damageTypeText;

    public async void CreateAvatar()
    {
        // 读取玩家输入并创建用户信息
        string playerInput = _playerInputField.text;
        _playerInputField.text = "";
        List<SendData> chat = new List<SendData>()
    {
        new SendData("user", playerInput),
    };

        // 使用GlmFunctionTool类创建一个函数调用工具，并加入函数需要返回的参数
        GlmFunctionTool functionTool = new GlmFunctionTool("forge_weapon", "用户想打造一件武器。根据用户的描述，提取关于这把武器的信息");
        functionTool.AddProperty("weapon_name", "string", "武器名称");
        functionTool.AddProperty("weapon_description", "string", "武器简介");
        // 把武器价格输出的isRequired设置为false。如果玩家输入的文字未提供价格，GLM就不会输出价格
        functionTool.AddProperty("weapon_price", "int", "武器价格", false);
        // 设置属性的enum为"火", "雷", "冰"中的一种
        functionTool.AddProperty("damage_type", "string", "武器造成的伤害的属性", false, new List<string> { "火", "雷", "冰" });

        // 发送GLM函数请求，注意需要将functionTool放在一个List<GlmTool>工具列表里
        SendData response = await GlmHandler.GenerateGlmResponse(chat, 0.6f, new List<GlmTool> { functionTool });

        // GLM会根据user的输入内容和函数是否相关，决定是使用函数工具还是生成普通对话
        // 如果response.tool_calls != null，则代表GLM使用了函数工具
        if (response.tool_calls != null)
        {
            // 使用response.tool_calls[0].arguments_dict来获取输出参数
            Dictionary<string, string> functionOutput = response.tool_calls[0].arguments_dict;

            _weaponNameText.text = functionOutput["weapon_name"];
            _weaponDescriptionText.text = functionOutput["weapon_description"];
            // 因为GLM的输出可能不包含武器价格和属性，所以需要额外判断
            _weaponPriceText.text = functionOutput.ContainsKey("weapon_price") ? functionOutput["weapon_price"] : "";
            _damageTypeText.text = functionOutput.ContainsKey("damage_type") ? functionOutput["damage_type"] : "";
        }
        // 如果user的输入与函数不相关，GLM会执行普通的对话，不调用函数。
        else
        {
            _weaponNameText.text = _weaponDescriptionText.text = _weaponPriceText.text = _damageTypeText.text = "";
            Debug.Log(response.content);
        }
    }
}