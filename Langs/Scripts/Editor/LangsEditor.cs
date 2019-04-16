using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace FMLHT.Langs {

[CustomEditor(typeof(LangManager))]
public class LangsEditor : Editor
{
    [MenuItem("FMLHT/Langs/Add to scene")]
    public static void AddPrefab() {
        if (Editor.FindObjectOfType<LangManager>() == null) {
            UnityEngine.Object prefab = Resources.Load("LangManager");
            var newObj = PrefabUtility.InstantiatePrefab(prefab);
            GameObject obj = (GameObject)newObj;
            obj.name = "LangManager";
            var core = GameObject.Find("Core");
            if (core == null) {
                core = new GameObject();
                core.name = "Core";
            }
            obj.transform.SetParent(core.transform);
        } else {
            Debug.Log("There is already one LangManager in this scene!");
        }
    }

    LangManager manager;

    public override void OnInspectorGUI() {
        manager = (LangManager)target;
        DrawDefaultInspector();
        if (manager.googleSheetID != "") {
            if (GUILayout.Button("Fetch translations")) {
                bool isOnline = manager.fetchOnline;
                manager.fetchOnline = true;
                manager.ProcessSheets();
                manager.afterFetch["All"].Add(() => {
                    Debug.Log("Dicts are successfully fetched!");
                    manager.fetchOnline = isOnline;
                });
                manager.FetchSheets();
            }
        }
    }
}

}