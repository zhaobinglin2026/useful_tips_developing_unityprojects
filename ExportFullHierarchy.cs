using UnityEngine;
using UnityEditor;
using System.IO;
using System.Text;
using System.Reflection;
using System.Collections.Generic;

public static class ExportFullHierarchy
{
    [MenuItem("Tools/Export Full Hierarchy with Components")]
    private static void Export()
    {
        StringBuilder sb = new StringBuilder();
        // 遍历根物体
        foreach (GameObject rootObj in UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects())
        {
            WriteObject(rootObj, 0, sb);
        }

        string path = EditorUtility.SaveFilePanel("Save Hierarchy Details", "", "Hierarchy_Details.txt", "txt");
        if (!string.IsNullOrEmpty(path))
            File.WriteAllText(path, sb.ToString());

        Debug.Log("Full hierarchy exported to: " + path);
    }

    private static void WriteObject(GameObject obj, int indent, StringBuilder sb)
    {
        // 缩进
        string indentStr = new string(' ', indent * 2);
        string prefix = indent == 0 ? "├─ " : "├─ ";

        // 物体名称与是否激活
        sb.AppendLine($"{indentStr}{prefix}{obj.name} (Active: {obj.activeSelf})");

        // 输出所有组件及其字段
        Component[] components = obj.GetComponents<Component>();
        foreach (Component comp in components)
        {
            if (comp == null) continue;
            string compIndent = new string(' ', (indent + 1) * 2);
            sb.AppendLine($"{compIndent}[{comp.GetType().Name}]");

            // 反射读取可序列化的公有/私有字段（标记了SerializeField或public）
            SerializedObject so = new SerializedObject(comp);
            SerializedProperty prop = so.GetIterator();
            bool enterChildren = true;
            while (prop.NextVisible(enterChildren))
            {
                enterChildren = false;
                // 跳过一些通用无意义属性
                if (prop.name == "m_Script" || prop.name == "m_ObjectHideFlags") continue;

                string propIndent = new string(' ', (indent + 2) * 2);
                string valueStr = GetPropertyValueString(prop);
                sb.AppendLine($"{propIndent}{prop.displayName}: {valueStr}");
            }
        }

        // 递归子物体
        foreach (Transform child in obj.transform)
        {
            WriteObject(child.gameObject, indent + 1, sb);
        }
    }

    private static string GetPropertyValueString(SerializedProperty prop)
    {
        switch (prop.propertyType)
        {
            case SerializedPropertyType.Generic:
                return "(nested)";
            case SerializedPropertyType.ObjectReference:
                return prop.objectReferenceValue ? prop.objectReferenceValue.name : "None";
            case SerializedPropertyType.Vector2:
                return prop.vector2Value.ToString();
            case SerializedPropertyType.Vector3:
                return prop.vector3Value.ToString();
            case SerializedPropertyType.Vector4:
                return prop.vector4Value.ToString();
            case SerializedPropertyType.Quaternion:
                return prop.quaternionValue.eulerAngles.ToString();
            case SerializedPropertyType.Color:
                return prop.colorValue.ToString();
            case SerializedPropertyType.AnimationCurve:
                return "(curve)";
            case SerializedPropertyType.ArraySize:
                return prop.intValue.ToString();
            default:
                return prop.boxedValue?.ToString() ?? "null";
        }
    }
}