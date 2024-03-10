﻿using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace Blendity
{
  public class KeyValueConfig
  {
    public string key, value, config;
    public bool userCreated = false;
  }

  public class ParamsModal : EditorWindow
  {
    private string newVariableKey = "";
    public string[,] defaultVariables;
    public bool canAddCustomVariables = false;

    public delegate void OtherUIElements();
    public OtherUIElements otherUIElements;
    private List<KeyValueConfig> variables;
    private ReorderableList list;

    private string[] GetMinMax(string config)
    {
      return config.Split(':')[1].Split(',');
    }

    private void InitializeList()
    {
      variables = new List<KeyValueConfig>();
      for (int i = 0; i < defaultVariables.GetLength(0); i++)
      {
        variables.Add(new KeyValueConfig { key = defaultVariables[i, 0], value = defaultVariables[i, 1], config = defaultVariables[i, 2] });
      }

      list = new ReorderableList(variables, typeof(Dictionary<string, string>), false, false, true, true);
      list.drawElementCallback =
        (Rect rect, int index, bool isActive, bool isFocused) =>
        {
          EditorGUIUtility.labelWidth = 200f;
          KeyValueConfig item = variables[index];

          if (item.config.StartsWith("range"))
          {
            string[] minMax = GetMinMax(item.config);
            float minValue = int.Parse(minMax[0]);
            float maxValue = int.Parse(minMax[1]);
            float currentValueMin = float.Parse(item.value.Split(',')[0]);
            float currentValueMax = float.Parse(item.value.Split(',')[1]);
            EditorGUI.BeginChangeCheck();
            EditorGUI.MinMaxSlider(
                new Rect(rect.x, rect.y, rect.width - 60, EditorGUIUtility.singleLineHeight),
                item.key,
                ref currentValueMin,
                ref currentValueMax,
                minValue,
                maxValue
            );
            GUIStyle rightAlignedStyle = new GUIStyle(GUI.skin.label)
            {
              alignment = TextAnchor.MiddleCenter
            };

            EditorGUI.LabelField(new Rect(rect.x + rect.width - 55, rect.y, 60, EditorGUIUtility.singleLineHeight),
                    $"{currentValueMin}-{currentValueMax}", rightAlignedStyle);

            if (EditorGUI.EndChangeCheck())
            {
              if (item.config.StartsWith("rangeInt"))
                item.value = $"{(int)currentValueMin},{(int)currentValueMax}";
              else if (item.config.StartsWith("rangeFloat"))
              {
                int decimalPoints = 1;
                item.value = $"{currentValueMin.ToString("F" + decimalPoints)},{currentValueMax.ToString("F" + decimalPoints)}";
              }
            }
          }
          else if (item.config.StartsWith("float"))
          {
            string[] minMax = GetMinMax(item.config);
            item.value = "" + EditorGUI.Slider(
              new Rect(rect.x, rect.y, rect.width, EditorGUIUtility.singleLineHeight)
              , item.key, float.Parse(item.value), float.Parse(minMax[0]), float.Parse(minMax[1])
            );
          }
          else if (item.config.StartsWith("int"))
          {
            string[] minMax = GetMinMax(item.config);
            item.value = "" + EditorGUI.IntSlider(
              new Rect(rect.x, rect.y, rect.width, EditorGUIUtility.singleLineHeight)
              , item.key, int.Parse(item.value), int.Parse(minMax[0]), int.Parse(minMax[1])
            );
          }
          else if (item.config.StartsWith("bool"))
          {
            item.value = EditorGUI.Toggle(
              new Rect(rect.x, rect.y, rect.width, EditorGUIUtility.singleLineHeight)
              , item.key, item.value == "True" || item.value == "true"
            ) ? "True" : "False";
          }
          else if (item.config.StartsWith("dropdown"))
          {
            string[] options = item.config.Split(':')[1].Split(',');
            item.value = options[EditorGUI.Popup(
              new Rect(rect.x, rect.y, rect.width, EditorGUIUtility.singleLineHeight)
              , item.key, Array.IndexOf(options, item.value), options
            )];
          }
        };

      list.onCanRemoveCallback = (ReorderableList mlist) =>
      {
        return variables[mlist.index].userCreated;
      };

      list.onCanAddCallback = (ReorderableList mlist) =>
      {
        return canAddCustomVariables && newVariableKey.Length > 0 && variables.All((variable) => variable.key != newVariableKey);
      };

      list.onAddCallback = (ReorderableList l) =>
      {
        variables.Add(new KeyValueConfig { key = newVariableKey, userCreated = true });
        newVariableKey = "";
        focusControl = () =>
        {
          GUI.FocusControl("New Variable Key");
        };
      };
    }

    Action focusControl;

    private void OnGUI()
    {
      if (list == null)
      {
        InitializeList();
        position = new Rect(Event.current.mousePosition - new Vector2(200, 200), new Vector2(400, position.height));
      }
      GUILayout.BeginVertical(GUILayout.MinHeight(position.height - EditorGUIUtility.singleLineHeight * 3));
      if (otherUIElements != null)
        otherUIElements();
      list.DoLayoutList();
      GUILayout.EndVertical();

      if (canAddCustomVariables)
      {
        GUI.SetNextControlName("New Variable Key");
        newVariableKey = EditorGUILayout.TextField("New Variable Key", newVariableKey);
      }

      EditorGUILayout.BeginHorizontal();
      if (GUILayout.Button("Cancel"))
        Close();
      if (GUILayout.Button("Start"))
      {
        Close();
        OnStart(variables);
      }
      EditorGUILayout.EndHorizontal();

      if (focusControl != null)
      {
        focusControl();
        focusControl = null;
      }
    }

    public Action<List<KeyValueConfig>> OnStart;
    private void OnInspectorUpdate()
    {
      Repaint();
    }
  }
}