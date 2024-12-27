using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using GlmUnity;
using System;

public class GlmRuleManager : MonoBehaviour
{
    // UI组件
    [SerializeField] private InputField _playerInputField;
    [SerializeField] private Text _outStuff;
    [SerializeField] GameManager _gameManager;
    [SerializeField]
    Text _inputFieldText;
    [SerializeField]
    Button _button;

    RuleSet ruleSet;
    string _originalInputText;
    bool isFirst;
    int count;
    bool legal;
    private void Awake()
    {
        _originalInputText = _inputFieldText.text;
    }
    public async void CreateAvatar()
    {
        // 读取玩家输入并创建用户信息
        string playerInput = _playerInputField.text;
        if (playerInput == "") return;
        _playerInputField.text = "";
        _inputFieldText.text = "等等，我在思考";
        _button.interactable = false;
        isFirst = true;
        legal = false;
        List<SendData> chat = new List<SendData>()
    {
        new SendData("user", playerInput),
    };

        // 使用GlmFunctionTool类创建一个函数调用工具，并加入函数需要返回的参数
        GlmFunctionTool functionTool = new GlmFunctionTool("makeRule", "我有一个游戏。游戏中有一副牌，" +
            "包含以下卡牌：红色1~9，黄色1~9和绿色1~9。玩家轮流出牌，如果出牌不合法，玩家将受到惩罚。在某些情况下，玩家可以为游戏添加自己的规则。" +
            "自定义规则应清晰、可编程且不偏向任何一方。" +
            "{{{{{ 现在，用户想要制定一个规则。你应该为规则函数提供适当的参数。规则函数定义了出牌是否合法。在玩家确实输入了规则后，使用函数工具。}}}}}"
            );
        functionTool.AddProperty("Can", "bool", "玩家输入的规则能否被函数实现");
        functionTool.AddProperty("Count", "int", "Count.需要多少次输出");
        functionTool.AddProperty("isColor", "bool", "isColor");
        functionTool.AddProperty("target", "string", "target", true, new List<string> { "lastPlayedCard","playedCards",
            "redCards","yellowCards","greenCards","oddCards", "evenCards", "primeCards", "nonPrimeCards" , "numberCards", "anyCard","other"});
        functionTool.AddProperty("condition", "bool", "condition");
        functionTool.AddProperty("compMethod", "string", "compMethod", true, new List<string> { "equal",
            "notEqual", "bigger", "smaller", "biggerOrEqual", "smallerOrEqual", "sameParity", "diffParity", "samePrimality", "diffPrimality", "sum","other" });
        functionTool.AddProperty("result", "int", "result", true);
        functionTool.AddProperty("depth", "int", "depth", true);
        functionTool.AddProperty("outcome", "bool", "outcome");
        functionTool.AddProperty("number", "int", "number");
        //functionTool.AddProperty("finished", "bool", "finished. {{{{{和某一张牌对比时，这个一定首先是false！}}}}}");
        functionTool.AddProperty("isAnd", "bool", "isAnd");
        SendData rulePrompt = new SendData()
        {
            role = "system",
            content = "指定一张卡牌总是需要两组参数。如果玩家省略了颜色或数字，则表示任何颜色或任何数字。" + "Count 表示实现这个规则需要几组参数，例如一定能打黄3的规则，" +
            "需要两组，因此count为2.如果已经提供过一次参数，Count减去1" + "规则函数并非能实现所有规则，它只能进行有限种判断" +
            "，规则函数如下所示：" + "Bool CheckCard(bool isColor, enum target, int depth = 1, bool condition, enum CompMethod, int result, bool outcome, int number); "
            + "bool isColor 定义了规则的类型，如果为 true，则规则与卡牌的颜色有关，如果为 false，则与卡牌的数字有关。" +
            "target{ lastPlayedCard, playedCards, redCards, yellowCards, greenCards, oddCards, evenCards, primeCards, nonPrimeCards, numberCards, anyCard, other }。"
            + "当检查所打出的卡牌的颜色是否为指定颜色时，将 target 设置为 redCards / yellowCards / greenCards。请注意规则内不能存在别的颜色" +
            "当检查卡牌的数字是奇数、偶数、质数或非质数时，将 target 设置为 oddCards / evenCards / primeCards / nonPrimeCards。当 target 是 playedCards 时，需要提供 depth。"
            + "只有当玩家说堆中有多张卡牌应包括在内时，才应选择 playedCards。" + "当玩家希望所打出的卡牌与特定数字进行比较时，应使用 numberCards，在这种情况下应提供一个 int 数字。" +
            "当玩家希望有对所有牌生效的效果时，应使用 anyCard，例如所有卡牌都是合法的或不合法的。{{{请注意玩家输入无法实现的游戏规则时，不要使用anyCard！！！}}}应当使用other或者新生成一个参数名。" +
            "Color 是一个枚举类型 { red, green, yellow }。Target 是与所打出的卡牌进行比较的对象。" +
            "当 target 是 playedCards 时，应提供 depth，表示应比较堆顶的几张卡牌。depth = 1 表示只应比较堆顶的卡牌与所打出的卡牌。depth = 2 表示应比较堆顶的两张卡牌与所打出的卡牌。" +
            "其他target参数不适用时，选取other。" +
            "condition 是一个布尔值，仅在这是一个关于颜色的规则时需要，表示规则何时生效：" + "true 表示所有比较的卡牌颜色相同，false 表示有比较的卡牌颜色不同。" +
            "CompMethod 是在这是一个关于数字的规则时需要的，表示如何比较卡牌。它是：" +
            "comMethod{equal, notEqual, bigger, smaller, biggerOrEqual, smallerOrEqual, sameParity, diffParity, samePrimality, diffPrimality, sum, other} " +
            "sameParity 表示规则在比较的卡牌数字全为偶数或奇数时生效，diffParity 表示规则在比较的卡牌数字不全为偶数或奇数时生效。" +
            "samePrimality 表示规则在比较的卡牌数字全为质数或非质数时生效，diffPrimality 表示规则在比较的卡牌数字不全为质数或非质数时生效。" +
            "当 compMethod 是 sum 时，应提供 result，表示如果比较的卡牌数字之和等于 result，规则生效。" +
            "请注意，当玩家说 sum 而未指定数字时，表示所打出的卡牌数字与 lastPlayedCard 的数字之和，因此 target 应为 lastPlayedCard。要进行加法以外的计算再比较结果时，选取other，或者新生成一个参数名" +
            "outcome 表示规则生效时，所打出的卡牌是否合法：true 表示合法，false 表示不合法。” + “当 target 是 numberCards 时，应提供 number，表示要比较的数字。" +
            "isAnd 表示多个函数如何形成单个规则，定义参数之间的或 / 与关系。" + "如果 isAnd 为 true，则所有条件必须为真才能使规则生效，如果 isAnd 为 false，" + "单个条件为真将导致规则生效。" 
            //"当玩家说只有某些卡牌是合法的时，任何其他相关卡牌将是不合法的。如果规则需要通过上述参数以外的参数实现，就把can设为false，例如玩家想要判断点数的相乘结果，或者是否为某一数的倍数时，暂时无法实现。"
        };
        chat.Add(rulePrompt);
        bool outputIsAnd = true;
        bool outputFinished = false;
        do
        {
            SendData response = await GlmHandler.GenerateGlmResponse(chat, 0f, new List<GlmTool> { functionTool });

            // GLM会根据user的输入内容和函数是否相关，决定是使用函数工具还是生成普通对话
            // 如果response.tool_calls != null，则代表GLM使用了函数工具
            legal = response.tool_calls != null && response.tool_calls[0].arguments_dict["Can"] == "true";
            if (response.tool_calls == null)legal = false;
            else if (response.tool_calls[0].arguments_dict["target"] == "other" || response.tool_calls[0].arguments_dict["compMethod"] == "other") legal = false;
            if (response.tool_calls != null && response.tool_calls[0].arguments_dict["Can"] == "true")
            {
                // 使用response.tool_calls[0].arguments_dict来获取输出参数
                Dictionary<string, string> functionOutput = response.tool_calls[0].arguments_dict;
                if (isFirst)
                {
                    count = int.Parse(functionOutput["Count"]);
                    isFirst = false;
                }
                else
                {
                    count--;
                }
                outputFinished = count <= 1;
                ruleSet = new RuleSet
                {
                    type = RuleType.Color,
                    target = Target.lastPlayedCard,
                    CompMethod = CompMethod.equal,
                    depth = 1,
                    result = 10,
                    condition = true,
                    outcome = true
                };
                ruleSet.type = functionOutput["isColor"] == "true" ? RuleType.Color : RuleType.Number;
                switch (functionOutput["target"])
                {
                    case "lastPlayedCard":
                        ruleSet.target = Target.lastPlayedCard;
                        break;
                    case "playedCards":
                        ruleSet.target = Target.playedCards;
                        break;
                    case "redCards":
                        ruleSet.target = Target.redCards;
                        break;
                    case "yellowCards":
                        ruleSet.target = Target.yellowCards;
                        break;
                    case "greenCards":
                        ruleSet.target = Target.greenCards;
                        break;
                    case "oddCards":
                        ruleSet.target = Target.oddCards;
                        break;
                    case "evenCards":
                        ruleSet.target = Target.evenCards;
                        break;
                    case "primeCards":
                        ruleSet.target = Target.primeCards;
                        break;
                    case "nonPrimeCards":
                        ruleSet.target = Target.nonPrimeCards;
                        break;
                    case "numberCards":
                        ruleSet.target = Target.numberCards;
                        break;
                    case "anyCard":
                        ruleSet.target = Target.anyCard;
                        break;
                    default:
                        legal = false;
                        break;
                }
                if (functionOutput.ContainsKey("compMethod"))
                {
                    switch (functionOutput["compMethod"])
                    {
                        case "equal":
                            ruleSet.CompMethod = CompMethod.equal;
                            break;
                        case "notEqual":
                            ruleSet.CompMethod = CompMethod.notEqual;
                            break;
                        case "bigger":
                            ruleSet.CompMethod = CompMethod.bigger;
                            break;
                        case "smaller":
                            ruleSet.CompMethod = CompMethod.smaller;
                            break;
                        case "biggerOrEqual":
                            ruleSet.CompMethod = CompMethod.biggerOrEqual;
                            break;
                        case "smallerOrEqual":
                            ruleSet.CompMethod = CompMethod.smallerOrEqual;
                            break;
                        case "sameParity":
                            ruleSet.CompMethod = CompMethod.sameParity;
                            break;
                        case "diffParity":
                            ruleSet.CompMethod = CompMethod.diffParity;
                            break;
                        case "samePrimality":
                            ruleSet.CompMethod = CompMethod.samePrimality;
                            break;
                        case "diffPrimality":
                            ruleSet.CompMethod = CompMethod.diffPrimality;
                            break;
                        case "sum":
                            ruleSet.CompMethod = CompMethod.sum;
                            break;
                        default:
                            legal = false;
                            break;

                    }
                }
                ruleSet.depth = functionOutput.ContainsKey("depth") ? int.Parse(functionOutput["depth"]) : 1;
                ruleSet.result = functionOutput.ContainsKey("result") ? int.Parse(functionOutput["result"]) : 10;
                ruleSet.condition = functionOutput.ContainsKey("condition") ? functionOutput["condition"] == "true" : true;
                ruleSet.outcome = functionOutput.ContainsKey("outcome") ? functionOutput["outcome"] == "true" : true;
                ruleSet.number = functionOutput.ContainsKey("number") ? int.Parse(functionOutput["number"]) : 5;
                _gameManager.AddRule(!ruleSet.outcome, ruleSet);
                outputIsAnd = functionOutput["isAnd"] == "true";
                Debug.Log(response.content);
                //_weaponDescriptionText.text = functionOutput["weapon_description"];
                // 因为GLM的输出可能不包含武器价格和属性，所以需要额外判断
                //_weaponPriceText.text = functionOutput.ContainsKey("weapon_price") ? functionOutput["weapon_price"] : "";
                //_damageTypeText.text = functionOutput.ContainsKey("damage_type") ? functionOutput["damage_type"] : "";
                SendData lastPrompt = new SendData()
                {
                    role = "system",
                    content = "你刚才已经提供了一组参数 " +
                    "你应该再提供一组参数来试图完善这一规则。你上次提供的参数是： "
                    + string.Join(Environment.NewLine, functionOutput)
                };
                //chat.Add(lastPrompt);
            }
            // 如果user的输入与函数不相关，GLM会执行普通的对话，不调用函数。
            else
            {
                _outStuff.text = "";
                _inputFieldText.text = "对不起！做不到！";
                _button.interactable = true;
                Debug.Log(response.content);
                return;
            }

        }
        // 发送GLM函数请求，注意需要将functionTool放在一个List<GlmTool>工具列表里
        while (!outputFinished);
        _inputFieldText.text = _originalInputText;
        _button.interactable = true;

    }
}

