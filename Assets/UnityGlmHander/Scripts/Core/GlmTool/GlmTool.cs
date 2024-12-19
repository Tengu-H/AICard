using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GlmUnity
{
    /// <summary>
    /// GLM工具基类
    /// GLM目前可以调用：function, retrieval, web_search
    /// </summary>
    public abstract class GlmTool
    {
        public virtual string type { get; }
    }
}