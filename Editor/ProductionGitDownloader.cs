using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEditor;
using UnityEditor.PackageManager;
using UnityEditor.PackageManager.Requests;
using UnityEditor.Compilation;

using System.IO;




public class ProductionGitDownloader : EditorWindow
{

    struct PackInfo
    {
        public string packName;
        public string packPath;
        public string packDesc;
    }

    static AddRequest Request = null;
    static RemoveRequest rRequest = null;

    static List<string> InstalledPacks = new List<string>();

    static Dictionary<string, PackInfo> GitPacks = new Dictionary<string, PackInfo>();

    static ProductionGitDownloader win;

    static Vector2 scrollPosition = Vector2.zero;



    static List<string> GetInstalledPackList()
    {

        ListRequest currentListRequest = Client.List();

        InstalledPacks.Clear();

        while (!currentListRequest.IsCompleted)
        {
            if (currentListRequest.Status == StatusCode.Failure || currentListRequest.Error != null)
            {
                Debug.LogError(currentListRequest.Error.message);
                break;
            }
        }

        foreach (var package in currentListRequest.Result)
        {
            string packName = package.name;

            if (!InstalledPacks.Contains(packName))
                InstalledPacks.Add(packName);
        }

        return InstalledPacks;
    }


    [MenuItem("Production/Run \"Tool Downloader\"", priority = 1)]
    static void RunGitDownloader()
    {

        InstalledPacks = GetInstalledPackList();

        GitPacks = LoadGithubToolList();

        win = EditorWindow.GetWindow<ProductionGitDownloader>();
        win.maxSize = new Vector2(450, 300);
        win.minSize = win.maxSize;

        win.titleContent = new GUIContent("Tool Downloader");

        win.Show();

    }

    
    static Dictionary<string,PackInfo> LoadGithubToolList()
    {

        GitPacks = new Dictionary<string, PackInfo>();

        // Assets/Scripts/ProductionToolAllInOne/Editor/ProductionToolList.txt
        string[] listFileGuid = AssetDatabase.FindAssets("ProductionToolList");

        string assetPath = AssetDatabase.GUIDToAssetPath(listFileGuid[0]);

        string assetFullPath = Path.GetFullPath(assetPath);
        
        if (File.Exists(assetFullPath))
        {
            string[] lines = File.ReadAllLines(assetFullPath);

            foreach(string currentLine in lines)
            {
                string[] tokens = currentLine.Split(';');

                if (tokens.Length != 3)
                    continue;

                /*
                string packageName = tokens[0];
                string packageGitPath = tokens[1];
                string packageDesc = tokens[2];
                */

                PackInfo pInfo = new PackInfo();
                pInfo.packName = tokens[0];
                pInfo.packPath = tokens[1];
                pInfo.packDesc = tokens[2];

                GitPacks.Add(pInfo.packName, pInfo);

                //Debug.Log(pInfo.packName);

            }
        }
        


        return GitPacks;
    }
    



    static void Progress()
    {

        if (Request.IsCompleted)
        {
            if (Request.Status == StatusCode.Success)
            {
                //Debug.Log("Installed: " + Request.Result.packageId);
                SceneView.lastActiveSceneView.ShowNotification(new GUIContent("Installed : " + Request.Result.packageId), 5f);

                //RunGitDownloader();
            }
            else if (Request.Status >= StatusCode.Failure)
                Debug.Log(Request.Error.message);

            EditorApplication.update -= Progress;
            
        }
    }

    static void rProgress()
    {

        if (rRequest.IsCompleted)
        {
            if (rRequest.Status == StatusCode.Success)
            {
                SceneView.lastActiveSceneView.ShowNotification(new GUIContent("Uninstalled : " + rRequest.PackageIdOrName), 5f);
            }
            else if (rRequest.Status >= StatusCode.Failure)
                Debug.Log(rRequest.Error.message);

            EditorApplication.update -= rProgress;

        }
    }



    void DisplayUIs()
    {
        // pInfo
        GUILayout.BeginVertical();

        scrollPosition = GUILayout.BeginScrollView(scrollPosition, GUILayout.Width(win.maxSize.x), GUILayout.Height(win.maxSize.y));

        GUILayout.BeginVertical();

        

        foreach(KeyValuePair<string,PackInfo> currentPack in GitPacks)
        {
            GUILayout.BeginVertical();
            GUILayout.BeginHorizontal("Box");

            //if (GUILayout.Button(new GUIContent(currentPack.Value.packName, currentPack.Value.packDesc), GUILayout.Width(300)))
            GUILayout.Label(new GUIContent(currentPack.Value.packName, currentPack.Value.packDesc), GUILayout.Width(300));

            if (InstalledPacks.Contains(currentPack.Value.packName) == false)
            {
                if (GUILayout.Button("Install", GUILayout.Width(110)))
                {
                    Request = Client.Add(currentPack.Value.packPath);
                    EditorApplication.update += Progress;

                    win.Close();
                }
            }
            else
            {
                if (GUILayout.Button("Installed", GUILayout.Width(110)))
                {
                    rRequest = Client.Remove(currentPack.Value.packName);
                    EditorApplication.update += rProgress;

                    win.Close();
                }
            }

            GUILayout.Button("?", GUILayout.Width(20));

            GUILayout.EndHorizontal();
            GUILayout.EndVertical();
        }

        

        GUILayout.EndVertical();

        GUILayout.EndScrollView();

        GUILayout.EndVertical();
    }

    void OnGUI()
    {
        if (win == null)
            win = EditorWindow.GetWindow<ProductionGitDownloader>();

        if (GitPacks.Count == 0)
            GitPacks = LoadGithubToolList();

        /*
        GUILayout.BeginVertical("Box");

        GUILayout.Label("AAAAAA\nBBBBB");

        GUILayout.EndVertical();
        */

        DisplayUIs();
        
    }


}
