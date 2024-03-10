using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Blendity
{
  public class Building : Editor
  {
    [MenuItem("Assets/Blendity/Generate/Buildings (Blender ≤ V2.9)", true)]
    public static bool GenerateBuildingsValid()
    {
      string activeName = Utils.GetActiveFileName();
      return !activeName.EndsWith("Assets") && Directory.Exists(activeName);
    }

    [MenuItem("Assets/Blendity/Generate/Buildings (Blender ≤ V2.9)")]
    public static void GenerateBuildings()
    {
      ParamsModal modal = ScriptableObject.CreateInstance<ParamsModal>();
      string[,] defaultVariables = {
        { "Number of Buildings", "1","int:1,20" },
        { "Width", "3,10","rangeInt:2,100"  },
        { "Length", "3,10","rangeInt:2,100"  },
        { "Stories", "3,10","rangeInt:2,100"  },
        { "Entrance Every x Column", "3,5","rangeInt:1,50"  },
        { "Entrance Position Offset", "0,2","rangeInt:1,50"  },
        { "With Balcony", "True","bool"  },
        { "Balcony Every x Column", "2,5","rangeInt:1,50"  },
        { "Balcony Position Offset", "0,2","rangeInt:1,50"  },
        { "Wide Window Every x Column", "2,5","rangeInt:1,50"  },
        { "Wide Window Position Offset", "0,2","rangeInt:1,50"  },
        { "Roof Extra Every x Column", "2,5","rangeInt:2,50"  },
        { "Add Roof Top", "False","bool"  },
        { "Roof Top Height", "5,10","rangeFloat:0,50"  },
        { "Antennas", "10,50","rangeInt:0,200"  },
        { "Water Pipe Every x Column", "3,5","rangeInt:1,50"  },
        { "Extras on Balcony", "5,20","rangeInt:1,100"  },
        { "Material Option", "Extract Materials","dropdown:Extract Materials,Search and Remap"  },
      };
      modal.defaultVariables = defaultVariables;
      string materialOption = "";
      modal.OnStart = (List<KeyValueConfig> variables) =>
      {
        materialOption = variables.Last().value;
        EditorUtility.DisplayProgressBar("Creating Buildings !", "Generating Buildings", .1f);

        int numOfBuildings = int.Parse(variables[0].value);
        variables.RemoveAt(0);

        Func<string, int, Dictionary<string, string>> EnvCreator = (string path, int threadSeed) =>
        {
          int seed = (int)Stopwatch.GetTimestamp() + threadSeed;
          string buildingName = $"DIMENSIONS-building-{seed}";
          string output = $@"{path}\{buildingName}\{buildingName}.fbx";
          Dictionary<string, string> envVars = new Dictionary<string, string>{
            {"output",$"{output}"},
            {"seed",$"{seed}"}
          };
          variables.ForEach((variable) => envVars.Add(variable.key, variable.value));
          return envVars;
        };

        List<CommandOutput> procOutputs = Core.RunCommandTimesN(
          $@"-b blender~\PostUSSRBuilder.blend -P py_scripts~\generate_buildings.py",
          numOfBuildings,
          EnvCreator
        );
        EditorUtility.DisplayProgressBar("Creating Buildings !", "Importing Models", .5f);

        float progressPerLoop = 0.4f / procOutputs.Count;
        float progress = 0.55f;
        int i = 1;

        AssetDatabase.Refresh();
        procOutputs.ForEach(procOutput =>
        {
          progress += progressPerLoop;
          EditorUtility.DisplayProgressBar("Creating Buildings !", "Importing Materials and Textures #" + i++, progress);

          string[] pathParts = procOutput.outputFile.Split("\\");
          string searchPattern = $"{pathParts[pathParts.Length - 1].Replace("DIMENSIONS", "*")}";

          string[] newArray = new string[pathParts.Length - 2];
          Array.Copy(pathParts, newArray, newArray.Length);

          string directoryPath = string.Join("\\", newArray);
          string[] matchingFiles = Directory.GetFiles(directoryPath, searchPattern, SearchOption.AllDirectories);

          if (matchingFiles.Length > 0)
          {
            UnityEngine.Debug.Log(materialOption);
            Utils.ExtractTexturesAndMaterials(matchingFiles[0], materialOption == "Search and Remap");
          }
        });
        procOutputs.ForEach(output => output.Print());

        AssetDatabase.Refresh();
        EditorUtility.ClearProgressBar();
      };
      modal.position = new Rect(modal.position.x, modal.position.y, modal.position.width, 600);
      modal.titleContent = new GUIContent("Building Parameters (Tested on Blender V2.9)");
      modal.otherUIElements = () =>
      {
        GUILayout.Label("Credit goes to Alexey Yakovlev");

        if (GUILayout.Button("Check blend file and support"))
        {
          Application.OpenURL("https://kypcaht.gumroad.com/l/PmCLJ");
        }
      };
      modal.ShowModalUtility();
    }
  }
}