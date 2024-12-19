using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GlmUnity
{
    [System.Serializable]
    public class SendData
    {
        [SerializeField] public string role;
        [SerializeField] public string content;
        public List<GlmFunctionTool> tool_calls;
        public SendData() { }
        public SendData(string _role, string _content)
        {
            role = _role;
            content = _content;
        }
    }
}