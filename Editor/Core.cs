using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

namespace Blendity
{
  public class CommandOutput
  {
    public string result, outputFile, error = "";
    public override string ToString() => result;
    public void Print()
    {
      Debug.Log(result);
      if (error.Length > 0)
      {
        Debug.LogError(error);
      }
    }
  }

  public class Core : Editor
  {
    public static string GetBlenderPath()
    {
      string blenderPath = EditorPrefs.GetString("blenderInstallationPath");
      if (blenderPath.Length == 0)
      {
        Utils.SetBlenderPath();
        blenderPath = EditorPrefs.GetString("blenderInstallationPath");
      }
      return blenderPath;
    }

    public static CommandOutput RunCommand(
      string command,
      Dictionary<string, string> env = null,
      string appName = null,
      bool isThreaded = false
    )
    {
      if (appName == null)
      {
        appName = GetBlenderPath();
      }

      if (!isThreaded)
        EditorUtility.DisplayProgressBar("Executing Command", command, .25f);

      System.Diagnostics.ProcessStartInfo procStartInfo =
          new System.Diagnostics.ProcessStartInfo(appName, command)
          {
            WorkingDirectory = Utils.GetPackagePath(),
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true,
          };

      procStartInfo.EnvironmentVariables["BLENDER_USER_CONFIG"] = $@"{Utils.GetPackagePath()}\blender~\2.92\Config\";
      procStartInfo.EnvironmentVariables["BLENDER_USER_SCRIPTS"] = $@"{Utils.GetPackagePath()}\blender~\2.92\scripts";
      if (env != null)
        foreach (var variable in env)
          procStartInfo.EnvironmentVariables[variable.Key] = variable.Value;

      System.Diagnostics.Process proc = new System.Diagnostics.Process
      {
        StartInfo = procStartInfo
      };
      try
      {
        proc.Start();
        proc.WaitForExit();
      }
      catch (Exception e)
      {
        Debug.LogException(e);
      }
      if (!isThreaded)
        EditorUtility.ClearProgressBar();

      string result = proc.StandardOutput.ReadToEnd();
      string error = proc.StandardError.ReadToEnd();

      CommandOutput output = new CommandOutput
      {
        outputFile = procStartInfo.EnvironmentVariables["output"],
        result = result,
        error = error
      };
      return output;
    }

    public static List<CommandOutput> RunCommandOnSelected(
      string command,
      Func<string, int, Dictionary<string, string>> envCreator = null,
      string appName = null
    )
    {
      List<CommandOutput> output = new List<CommandOutput>();
      List<string> selectedFileNames = Utils.GetValidImports();
      List<Task<CommandOutput>> tasks = new List<Task<CommandOutput>>();

      if (appName == null)
        appName = GetBlenderPath();
      for (int i = 0; i < selectedFileNames.Count; i++)
      {
        string fileName = selectedFileNames[i];
        fileName = Utils.GetWindowsPath(fileName);

        tasks.Add(Task.Run(() => RunCommand(command, envCreator == null ? null : envCreator(fileName, i * 10), appName, true)));
      }

      Task.WaitAll(tasks.ToArray());
      tasks.ForEach(t => output.Add(t.Result));
      return output;
    }

    public static List<CommandOutput> RunCommandTimesN(
      string command,
      int n,
      Func<string, int, Dictionary<string, string>> envCreator = null,
      string appName = null
    )
    {
      List<CommandOutput> output = new List<CommandOutput>();
      List<Task<CommandOutput>> tasks = new List<Task<CommandOutput>>();
      string path = Utils.GetActiveFileName();
      path = Utils.GetWindowsPath(path);

      if (appName == null)
        appName = GetBlenderPath();
      for (int i = 0; i < n; i++)
      {
        tasks.Add(Task.Run(
          () => RunCommand(command, envCreator == null ? null : envCreator(path, i * 10), appName, true)
          ));
      }

      Task.WaitAll(tasks.ToArray());
      tasks.ForEach(t => output.Add(t.Result));
      return output;
    }
  }
}
