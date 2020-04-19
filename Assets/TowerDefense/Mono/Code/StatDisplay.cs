using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BE
{
    public class StatDisplay : Singleton<StatDisplay>
    {
        public int AgentCount { get; set; }

        private void OnGUI()
        {
            GUILayout.Label(string.Format("Agent count: {0}", AgentCount));
            GUILayout.Label(string.Format("FPS: {0}", (int)(1f / Time.unscaledDeltaTime)));
            GUILayout.Label(string.Format("Update time: {0:0.000}ms", Time.unscaledDeltaTime * 1000));
        }
    }
}
