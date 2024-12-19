using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

namespace GlmUnity
{
    public static class GlmHandler
    {
        private static string _url = "https://open.bigmodel.cn/api/paas/v4/chat/completions";

        // ***请在这里输入你的智谱AI密钥***
        // 请检查你的GLM API密钥格式为：{id}.{secret}
        private static string _key = "1d1d69eaf9f7a7968b3818672ee6b01c.3zgeo2d7JxcFen6P";

        private static string _apiKey => _key.Split('.')[0];
        private static string _secretKey => _key.Split(".")[1];

        /// <summary>
        /// 获取GLM回复
        /// </summary>
        /// <param name="chatHistory">历史对话记录</param>
        /// <param name="temperature">模型的temperature</param>
        /// <param name="toolList">如果使用GLM的模型工具，则这里为需要使用的工具列表</param>
        /// <returns></returns>
        public static async Task<SendData> GenerateGlmResponse(List<SendData> chatHistory, float temperature = 0.6f, List<GlmTool> toolList = null)
        {
        GenerateResponse:
            // 将请求转化为JSON
            RequestData requestObject = new RequestData
            {
                model = "glm-4",  // 使用的GLM模型类型，可视情况更换
                messages = chatHistory,
                temperature = Mathf.Clamp(temperature, 0f, 1f),
                tools = toolList,
            };
            string jsonPayload = JsonConvert.SerializeObject(requestObject, Formatting.Indented);

            // 发送请求
            using (UnityWebRequest request = new UnityWebRequest(_url, "POST"))
            {
                byte[] data = System.Text.Encoding.UTF8.GetBytes(jsonPayload);
                request.uploadHandler = (UploadHandler)new UploadHandlerRaw(data);
                request.downloadHandler = (DownloadHandler)new DownloadHandlerBuffer();

                request.SetRequestHeader("Content-Type", "application/json");
                request.SetRequestHeader("Authorization", GetToken());

                await request.SendWebRequest();

                // 如果代码=200，则请求成功，开始读取内容
                if (request.responseCode == 200)
                {
                    string msg = request.downloadHandler.text;
                    ResponseData response = JsonConvert.DeserializeObject<ResponseData>(msg);

                    // 确保GLM生成至少一个可用的回复
                    if (response.choices.Count > 0)
                    {
                        return response.choices[0].message;
                    }
                    else
                    {
                        Debug.LogWarning("GLM回复为空！\n" + msg);
                    }
                }
                else if (request.responseCode == 1301)
                {
                    Debug.LogWarning("GLM返回代码1301，代表生成了不良信息，重新生成中……");
                    goto GenerateResponse;
                }
                else
                {
                    Debug.LogWarning($"GLM请求出错，三秒后重新尝试请求。\n\r{request.error}");
                    await Task.Delay(3000);
                    if (Application.isPlaying)
                        goto GenerateResponse;
                }
                return null;
            }
        }

        /// <summary>
        /// 根据GLM文档生成一个函数工具
        /// </summary>
        /// <returns></returns>
        public static GlmFunctionTool GetDummyFunctionTool()
        {
            GlmFunctionTool tool = new GlmFunctionTool("get_flight_number", "根据始发地、目的地和日期，查询对应日期的航班号");
            tool.AddProperty("departure", "string", "出发地", true);
            tool.AddProperty("destination", "string", "目的地", true);
            tool.AddProperty("date", "string", "日期", true);
            return tool;
        }

        #region Token处理
        private static string GetToken()
        {
            long expirationMilliseconds = DateTimeOffset.Now.AddHours(1).ToUnixTimeMilliseconds();
            long timestampMilliseconds = DateTimeOffset.Now.ToUnixTimeMilliseconds();
            string jwtToken = GenerateJwtToken(_apiKey, expirationMilliseconds, timestampMilliseconds);
            return jwtToken;
        }

        private static string GenerateJwtToken(string apiKeyId, long expirationMilliseconds, long timestampMilliseconds)
        {
            // 构建Header
            string _headerJson = "{\"alg\":\"HS256\",\"sign_type\":\"SIGN\"}";

            string encodedHeader = Base64UrlEncode(_headerJson);

            // 构建Payload
            string _playLoadJson = string.Format("{{\"api_key\":\"{0}\",\"exp\":{1}, \"timestamp\":{2}}}", apiKeyId, expirationMilliseconds, timestampMilliseconds);

            string encodedPayload = Base64UrlEncode(_playLoadJson);

            // 构建签名
            string signature = HMACsha256(_secretKey, $"{encodedHeader}.{encodedPayload}");
            // 组合Header、Payload和Signature生成JWT令牌
            string jwtToken = $"{encodedHeader}.{encodedPayload}.{signature}";

            return jwtToken;
        }

        // Base64 URL编码
        private static string Base64UrlEncode(string input)
        {
            byte[] inputBytes = Encoding.UTF8.GetBytes(input);
            string base64 = Convert.ToBase64String(inputBytes);
            return base64.Replace('+', '-').Replace('/', '_').TrimEnd('=');
        }
        // 使用HMAC SHA256生成签名
        private static string HMACsha256(string apiSecretIsKey, string buider)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(apiSecretIsKey);
            HMACSHA256 hMACSHA256 = new System.Security.Cryptography.HMACSHA256(bytes);
            byte[] date = Encoding.UTF8.GetBytes(buider);
            date = hMACSHA256.ComputeHash(date);
            hMACSHA256.Clear();

            return Convert.ToBase64String(date);

        }
        #endregion

        #region 自定义类
        private class RequestData
        {
            [SerializeField] public string model;
            [SerializeField] public List<SendData> messages;
            [SerializeField] public float temperature;
            public List<GlmTool> tools;
        }

        private class ResponseData
        {
            public string id;
            public string created;
            public string model;
            public List<ResponseChoice> choices = new List<ResponseChoice>();
        }

        private class ResponseChoice
        {
            public int index;
            public string finish_reason;
            public SendData message;
        }
        #endregion
    }

    /// <summary>
    /// 原生UnityWebRequest.SendWebRequest()不支持被await，
    /// ExtensionMethods类主要实现await UnityWebRequest.SendWebRequest()，
    /// 参考自GitHub:https://gist.github.com/mattyellen/d63f1f557d08f7254345bff77bfdc8b3
    /// </summary>
    public static class ExtensionMethods
    {
        public static TaskAwaiter GetAwaiter(this AsyncOperation asyncOp)
        {
            var tcs = new TaskCompletionSource<object>();
            asyncOp.completed += obj => { tcs.SetResult(null); };
            return ((Task)tcs.Task).GetAwaiter();
        }
    }
}