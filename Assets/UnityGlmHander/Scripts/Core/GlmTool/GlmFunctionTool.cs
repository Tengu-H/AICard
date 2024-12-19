using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GlmUnity
{
    public class GlmFunctionTool : GlmTool
    {
        public override string type { get => "function"; }
        public GlmFunction function;
        /// <summary>
        /// 转化为Dictionary的函数返回值
        /// </summary>
        public Dictionary<string, string> arguments_dict
        {
            get
            {
                if (function.arguments == null)
                    return null;
                return JsonConvert.DeserializeObject<Dictionary<string, string>>(function.arguments);
            }
        }

        /// <summary>
        /// 创建一个GLM函数调用工具
        /// </summary>
        /// <param name="functionName">函数的名称</param>
        /// <param name="functionDescription">函数功能的描述</param>
        public GlmFunctionTool(string functionName, string functionDescription)
        {
            function = new GlmFunction(functionName, functionDescription);
        }

        /// <summary>
        /// 给一个GLM函数工具添加输出参数
        /// </summary>
        /// <param name="propertyName">输出参数的英文名称</param>
        /// <param name="dataType">输出参数的数据类型</param>
        /// <param name="propertyDescription">输出参数的中文描述</param>
        /// <param name="isRequired">是否一定要输出该参数。如果用户没有提到该信息并且不一定需要输出该参数，则GLM会不输出该参数</param>
        public void AddProperty(string propertyName, string dataType, string propertyDescription, bool isRequired = true, List<string> Enum = null)
        {
            function.parameters.properties[propertyName] = new GlmFunction.GlmFunctionParameters.GlmFunctionProperty(dataType, propertyDescription, Enum);
            if (isRequired)
                function.parameters.required.Add(propertyName);
            
        }

        public class GlmFunction
        {
            /// <summary>
            /// 函数名称
            /// </summary>
            public string name;

            /// <summary>
            /// 函数描述
            /// </summary>
            public string description;

            /// <summary>
            /// 函数的输入参数
            /// </summary>
            public GlmFunctionParameters parameters;

            /// <summary>
            /// 函数的返回值，string格式
            /// </summary>
            public string arguments;

            public GlmFunction(string name, string description)
            {
                this.name = name;
                this.description = description;
                parameters = new GlmFunctionParameters();
            }

            public class GlmFunctionParameters
            {
                public string type => "object";
                public Dictionary<string, GlmFunctionProperty> properties;

                /// <summary>
                /// 哪些Property必须被包含
                /// </summary>
                public List<string> required;

                public GlmFunctionParameters()
                {
                    properties = new Dictionary<string, GlmFunctionProperty>();
                    required = new List<string>();
                }

                public class GlmFunctionProperty
                {
                    public string type;
                    public string description;
                    [JsonProperty("enum")]
                    public List<string> Enum;

                    public GlmFunctionProperty(string type, string description, List<string> Enum)
                    {
                        this.type = type;
                        this.description = description;
                        this.Enum = Enum;
                    }
                }
            }
        }

    }

}