using System;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEditor;
using UnityEngine;

namespace Blendity
{
  public class AddSnow : Editor
  {
    [MenuItem("Assets/Blendity/Modify/Add Snow", true)]
    public static bool CreateFracValid()
    {
      return Utils.IsValidImports();
    }

    [MenuItem("Assets/Blendity/Modify/Add Snow")]
    public static void CreateFrac()
    {
      ParamsModal modal = ScriptableObject.CreateInstance<ParamsModal>();
      string[,] defaultVariables = {
        { "Coverage %", "60", "int:0,100" },
        { "Height", "0.2", "float:0,1" },
        { "Mesh Reduction", "0.8", "float:0,1" },
        { "Rotate X", "0", "float:0,360" },
        { "Rotate Y", "0", "float:0,360" },
        { "Rotate Z", "0", "float:0,360" }
        };
      modal.defaultVariables = defaultVariables;
      modal.OnStart = (List<KeyValueConfig> variables) =>
      {
        EditorUtility.DisplayProgressBar("Snowing on Your Mesh !", "Generating Snow", .2f);

        Func<string, int, Dictionary<string, string>> EnvCreator = (string fileName, int threadSeed) =>
        {
          int seed = (int)Stopwatch.GetTimestamp() + threadSeed;
          string output = Utils.GetWindowsPath(fileName, "-with snow");
          Dictionary<string, string> envVars = new Dictionary<string, string>{
          {"input",$"{fileName}"},
          {"output",$"{output}"},
          };
          variables.ForEach((variable) => envVars.Add(variable.key, variable.value));
          return envVars;
        };

        List<CommandOutput> procOutputs = Core.RunCommandOnSelected(
          $@"-b -P py_scripts~\add_snow.py",
          EnvCreator
         );
        procOutputs.ForEach(output => output.Print());

        EditorUtility.DisplayProgressBar("Snowing on Your Mesh !", "Importing Models", .8f);
        AssetDatabase.Refresh();
        EditorUtility.ClearProgressBar();
      };
      modal.titleContent = new GUIContent("Snow Parameters");
      modal.ShowModalUtility();
    }
  }
}