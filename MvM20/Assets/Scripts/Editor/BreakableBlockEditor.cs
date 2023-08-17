using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
#if UNITY_EDITOR
namespace MvM20.Editor
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(BreakableBlock))]
    public class BreakableBlockEditor : UnityEditor.Editor
    {
        BreakableBlock m_BreakableBlock;

        private SerializedProperty m_breakSize;

        private void OnEnable()
        {
            m_BreakableBlock = (BreakableBlock)target;
            m_breakSize = serializedObject.FindProperty("breakSize");
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            serializedObject.Update();
            EditorGUI.BeginChangeCheck();

            m_breakSize.intValue = EditorGUILayout.IntField("Breakable Size", m_breakSize.intValue);
            m_BreakableBlock.transform.localScale = new Vector3(m_breakSize.intValue, m_breakSize.intValue, 1);
            m_BreakableBlock.RenderCounters();

            EditorGUI.EndChangeCheck();
            serializedObject.ApplyModifiedProperties();
        }
    }
}
#endif