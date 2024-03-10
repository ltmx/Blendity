﻿using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Blendity
{
  public class Utils : Editor
  {
    static readonly string[] validExtensions = new string[] { ".fbx", ".obj", ".x3d", "gltf" };
    static readonly Func<string, bool> IsValidExtension = fileName => validExtensions.Any((extension) => fileName.EndsWith(extension));
    public static string GetPackagePath() => System.IO.Path.GetFullPath("Packages/com.ae.blendity");
    public static string GetActiveFileName() => System.IO.Path.GetFullPath(AssetDatabase.GetAssetPath(Selection.activeInstanceID));
    private static string[] GetSelectedFileNames() => Array.ConvertAll(Selection.objects, obj => System.IO.Path.GetFullPath(AssetDatabase.GetAssetPath(obj.GetInstanceID())));
    public static bool IsValidImports()
    {
      string[] fileNames = GetSelectedFileNames();
      return fileNames.Any(IsValidExtension);
    }

    public static List<string> GetValidImports()
    {
      string[] fileNames = GetSelectedFileNames();
      return fileNames.Aggregate(new List<string>(), (a, b) => { if (IsValidExtension(b)) a.Add(b); return a; });
    }

    public static string GetWindowsPath(string path, string suffix = "")
    {
      string[] pathPieces = path.Split('/');

      string[] fileNamePieces = pathPieces.Last().Split('.');
      string extension = "";

      if (fileNamePieces.Length > 1)
      {
        extension = fileNamePieces.Last();
        pathPieces[pathPieces.Length - 1] = string.Join(".", fileNamePieces.Take(fileNamePieces.Length - 1));
      }

      string output = $@"{string.Join(@"\", pathPieces)}{suffix}" + (extension.Length > 0 ? $".{extension}" : "");
      return output;
    }

    // https://github.com/Unity-Technologies/UnityCsReference/blob/61f92bd79ae862c4465d35270f9d1d57befd1761/Editor/Mono/Prefabs/PrefabUtility.cs#L131
    public static void ExtractMaterialsFromAsset(AssetImporter importer, string destinationPath)
    {
      var assetsToReload = new HashSet<string>();
      var materials = AssetDatabase.LoadAllAssetsAtPath(importer.assetPath).Where(x => x.GetType() == typeof(Material)).ToArray();

      foreach (var material in materials)
      {
        var newAssetPath = destinationPath + "/" + material.name + ".mat";
        newAssetPath = AssetDatabase.GenerateUniqueAssetPath(newAssetPath);

        var error = AssetDatabase.ExtractAsset(material, newAssetPath);
        if (string.IsNullOrEmpty(error))
        {
          assetsToReload.Add(importer.assetPath);
        }
      }

      foreach (var path in assetsToReload)
      {
        AssetDatabase.WriteImportSettingsIfDirty(path);
        AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
      }
    }

    public static string GetImporterPath(string assetPath)
    {
      assetPath = assetPath.Replace("\\", "/");
      if (assetPath.StartsWith(Application.dataPath))
      {
        assetPath = "Assets" + assetPath.Substring(Application.dataPath.Length);
      }
      return assetPath;
    }

    public static void ExtractTexturesAndMaterials(string assetPath, bool searchAndRemap = false)
    {
      assetPath = GetImporterPath(assetPath);

      AssetImporter assetImporter = AssetImporter.GetAtPath(assetPath);
      ModelImporter modelImporter = assetImporter as ModelImporter;
      string[] outputFilePieces = assetPath.Split('/');
      string outputDir = string.Join("/", outputFilePieces.Take(outputFilePieces.Length - 1));

      try
      {
        if (searchAndRemap)
        {
          // modelImporter.materialLocation = ModelImporterMaterialLocation.External;
          modelImporter.SearchAndRemapMaterials(ModelImporterMaterialName.BasedOnTextureName, ModelImporterMaterialSearch.Everywhere);
          AssetDatabase.WriteImportSettingsIfDirty(assetImporter.assetPath);
          AssetDatabase.ImportAsset(assetImporter.assetPath, ImportAssetOptions.ForceUpdate);
        }
        else
        {
          modelImporter.ExtractTextures(outputDir);
          ExtractMaterialsFromAsset(assetImporter, outputDir);
        }
        AssetDatabase.Refresh();
      }
      catch (Exception e)
      {
        Debug.LogError($"An error when trying to extract textures and materials of the following asset: {outputDir}");
        Debug.LogError(e);
        EditorUtility.ClearProgressBar();
      }
    }

    public static void SearchAndRemapMaterials(string assetPath)
    {
      assetPath = GetImporterPath(assetPath);

      ModelImporter modelImporter = AssetImporter.GetAtPath(assetPath) as ModelImporter;
      modelImporter.SearchAndRemapMaterials(ModelImporterMaterialName.BasedOnMaterialName, ModelImporterMaterialSearch.Local);
      modelImporter.SaveAndReimport();
    }

    public static void CreateMaterial(string path)
    {
      string relativePath = Utils.GetImporterPath(path);
      if (AssetDatabase.LoadAssetAtPath(relativePath, typeof(Material)) == null)
      {
        Material mat = new Material(Shader.Find("Standard"));

        AssetDatabase.CreateAsset(mat, relativePath);
      }
    }

    [MenuItem("Assets/Blendity/Set Blender Path")]
    public static void SetBlenderPath()
    {
      string folderPath = EditorUtility.OpenFilePanelWithFilters("Blender Executable", @"C:\Program Files\Blender Foundation",
       new string[] { "Executable", "exe" });
      EditorPrefs.SetString("blenderInstallationPath", folderPath);
    }
  }
}
