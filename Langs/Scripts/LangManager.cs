/* LANGUAGES SYSTEM
 * TRANSLATION USING GOOGLE SHEETS AND LOCAL FILES
 * V0.2
 * FMLHT, 04.2019
 */

using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.Networking;
using System.IO;

namespace FMLHT.Langs
{
    [ExecuteInEditMode]
    public class LangManager : MonoBehaviour
    {
        public string googleSheetID;
        public string googleSheetPageID = "0";
        public string defaultLanguage = "en";
        public GoogleSheet[] additionalSheets;
        public bool fetchOnAwake = false;
        public bool fetchOnline = true;
        public bool usePacked = false;
        public bool saveAssets = true;
        public string dictsFolder = "Dicts";

        [System.Serializable]
        public struct GoogleSheet
        {
            public string name;
            public string sheetID;
            public string pageID;
        }

        static string SPLIT_RE = @",(?=(?:[^""]*""[^""]*"")*(?![^""]*""))";
        //static string LINE_SPLIT_RE = @"\r\n|\n\r|\n|\r";
        static string LINE_SPLIT_RE = @"\r\n|\n\r";
        static char[] TRIM_CHARS = { '\"' };

        public Dictionary<string, Dict> dicts;
        private List<GoogleSheet> sheetsProcessed;
        private int readyQ = 999;
        public Dictionary<string, List<Translatable>> translatables;
        public Dictionary<string, List<System.Action>> afterFetch;

        void Awake()
        {
            ProcessSheets();
            L.Init(this);
            if (fetchOnAwake) FetchSheets();
        }

        void Update()
        {

        }

        public void ProcessSheets() {
            afterFetch = new Dictionary<string, List<System.Action>>();
            afterFetch["All"] = new List<System.Action>();
            sheetsProcessed = new List<GoogleSheet>();
            if (translatables == null) translatables = new Dictionary<string, List<Translatable>>();
            sheetsProcessed.Add(new GoogleSheet()
            {
                name = "Default",
                sheetID = googleSheetID,
                pageID = googleSheetPageID == "" ? "0" : googleSheetPageID
            });
            translatables["Default"] = new List<Translatable>();
            afterFetch["Default"] = new List<System.Action>();
            if (additionalSheets != null && additionalSheets.Length > 0) {
                additionalSheets.Foreach<GoogleSheet>(s =>
                {
                    if (s.sheetID == "") s.sheetID = googleSheetID;
                    if (s.pageID == "") s.pageID = "0";
                    if (s.pageID == sheetsProcessed[0].pageID && s.sheetID == sheetsProcessed[0].sheetID)
                    {
                        Debug.Log("Sheet is completely identical to default");
                    } else {
                        sheetsProcessed.Add(s);
                        if (!translatables.ContainsKey(s.name) || translatables[s.name] == null)
                            translatables[s.name] = new List<Translatable>();
                        if (!afterFetch.ContainsKey(s.name) || afterFetch[s.name] == null)
                            afterFetch[s.name] = new List<System.Action>();
                    }
                });
            }
            readyQ = sheetsProcessed.Count;
        }

        public void FetchSheets() {
            dicts = new Dictionary<string, Dict>();
            sheetsProcessed.Foreach(s => {
                if (fetchOnline) {
                    GetGoogleSheet(s);
                } else {
                    if (!usePacked) {
                        dicts[s.name] = LoadDictFromFile(s.name);
                    } else {
                        dicts[s.name] = LoadDictFromResource(s.name);
                    }
                    DictIsReady(s.name);
                }
            });
        }

        void SaveDictToFile(string name, string data) {
            var file = FilePathFromName(name);
            File.WriteAllText(file, data);
        }

        Dict LoadDictFromFile(string name) {
            var file = FilePathFromName(name);
            if (File.Exists(file)) {
                string raw = File.ReadAllText(file);
                return ParseCSV(raw);
            }
            Debug.Log("Dict file " + name + " not found!");
            return null;
        }

        Dict LoadDictFromResource(string name) {
            var asset = (TextAsset)Resources.Load<TextAsset>("Dicts/" + name);
            if (asset != null) {
                return ParseCSV(asset.text);
            }
            Debug.Log("Asset dict file " + name + ".csv not found!");
            return null;
        }

        string FilePathFromName(string name) {
            return DictsDir() + name + ".csv";
        }

        string DictsDir() {
            var dir = UnityEngine.Application.dataPath + "/Resources/";
            if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
            dir += dictsFolder + "/";
            if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
            return dir;
        }

        public string LangsList() {
            var res = new List<string>();
            foreach(var d in dicts) {
                res.Add(d.Key + ":" + d.Value.GetValuesCount());
            }
            return string.Join(", ", res);
        }

        void GetGoogleSheet(GoogleSheet sheet) {
            GetGoogleSheet(sheet.name, sheet.sheetID, sheet.pageID);
        }

        void GetGoogleSheet(string name, string sheetID, string pageID)
        {
            string url = "https://docs.google.com/spreadsheets/u/0/d/" + sheetID + "/export?format=csv&id=" + sheetID + "&gid=" + pageID;
            StartCoroutine(GetterGoogleSheet(url, name));
        }

        IEnumerator GetterGoogleSheet(string url, string name)
        {
            var www = UnityWebRequest.Get(url);
            yield return www.SendWebRequest();
            if (www.isNetworkError || www.isHttpError)
            {
                Debug.Log(www.error);
            }
            else
            {
                dicts[name] = ParseCSV(www.downloadHandler.text);
                if (saveAssets) SaveDictToFile(name, www.downloadHandler.text);
                DictIsReady(name);
            }
        }

        private void DictIsReady(string dict) {
            readyQ--;
            if (translatables[dict].Count > 0) {
                translatables[dict].Foreach(t => {
                    t.Translate();
                });
            }
            if (afterFetch[dict].Count > 0) {
                afterFetch[dict].Foreach(t => {
                    t.Invoke();
                });
            }
            if (readyQ == 0) {
                if (afterFetch["All"].Count > 0) {
                    afterFetch["All"].Foreach(t => {
                        t.Invoke();
                    });
                }
            }
        }

        public Dict ParseCSV(string raw)
        {
            //var lines = Regex.Split (raw, System.Environment.NewLine);
            var lines = Regex.Split(raw, LINE_SPLIT_RE);
            var header = Regex.Split(lines[0], SPLIT_RE);
            
            var aDict = new Dictionary<string, string[]>();
            var aDictIDs = new Dictionary<string, int>();

            for (var i = 1; i < header.Length; i++)
            {
                string value = header[i];
                value = value.TrimStart(TRIM_CHARS).TrimEnd(TRIM_CHARS).Replace("\\", "");
                aDictIDs[value] = i - 1;
            }

            for (var i = 1; i < lines.Length; i++)
            {
                var values = Regex.Split(lines[i], SPLIT_RE);
                if (values.Length == 0 || values[0] == "") continue;

                var entry = new string[values.Length - 1];
                var entryName = "";
                for (var j = 0; j < header.Length && j < values.Length; j++)
                {
                    string value = values[j];
                    value = value.TrimStart(TRIM_CHARS).TrimEnd(TRIM_CHARS).Replace("\\", "").Replace("\"\"", "\"");
                    if (j == 0)
                    {
                        entryName = value;
                    }
                    else
                    {
                        entry[j - 1] = value;
                    }
                }
                aDict[entryName] = entry;
            }

            return new Dict(aDict, aDictIDs);
        }
    }

    public static class L {
        public static LangManager Manager;

        public static void Init(LangManager manager_) {
            Manager = manager_;
        }

        public static string Get(string ID, string dict = "Default", string lang = "") {
            if (lang == "") lang = Manager.defaultLanguage;
            return Manager.dicts[dict][ID, lang];
        }

        public static void SetLang(string lang) {
            Manager.defaultLanguage = lang;
        }

        public static void AddTranslatable(Translatable obj) {
            if (Manager.translatables == null) Manager.translatables = new Dictionary<string, List<Translatable>>();
            if (!Manager.translatables.ContainsKey(obj.dict) || Manager.translatables[obj.dict] == null)
                Manager.translatables[obj.dict] = new List<Translatable>();
            Manager.translatables[obj.dict].Add(obj);
        }

        public static void DoAfterFetch(System.Action action, string dict = "All") {
            if (Manager.afterFetch == null) Manager.afterFetch = new Dictionary<string, List<System.Action>>();
            if (!Manager.afterFetch.ContainsKey(dict) || Manager.afterFetch[dict] == null)
                Manager.afterFetch[dict] = new List<System.Action>();
            Manager.afterFetch[dict].Add(action);
        }

        public static bool IsDictReady(string dict = "Default") {
            if (Manager == null) return false;
            if (Manager.dicts == null) return false;
            if (!Manager.dicts.ContainsKey(dict)) return false;
            if (Manager.dicts[dict] == null) return false;
            return true;
        }

        public static lline LL {
            get {
                return new lline();
            }
        }

        public static lstring S(string ID, string dict = "Default") {
            return new lstring(ID, dict);
        }
    }

}