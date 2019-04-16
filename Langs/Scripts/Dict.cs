using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FMLHT.Langs {

public class Dict
{
    private Dictionary<string, string[]> data;
    private Dictionary<string, int> dictIDs;

    public string this[string idV, string idH] {
        get {
            return data[idV][dictIDs[idH]];
        }
    }

    public Dict(Dictionary<string, string[]> data_, Dictionary<string, int> dictIDs_) {
        data = data_;
        dictIDs = dictIDs_;
    }

    public int GetValuesCount() {
        if (data == null || data.Values == null) return 0;
        return data.Values.Count;
    }

    public Serialized Serialize() {
        Serialized asset = new Serialized();
        asset.dataKeys = data.Keys.ToArray();
        asset.dataValues = data.Values.ToList().ToArray();
        asset.idsKeys = dictIDs.Keys.ToArray();
        asset.idsValues = dictIDs.Values.ToArray();
        return asset;
    }

    public string SerializeJSON() {
        return JsonUtility.ToJson(Serialize());
    }

    public static Dict DeserializeJSON(string json) {
        return new Dict(JsonUtility.FromJson<Serialized>(json));
    }

    public Dict(Serialized asset) {
        data = new Dictionary<string, string[]>();
        dictIDs = new Dictionary<string, int>();

        if (asset.dataKeys != null
            && asset.dataValues != null
            && asset.dataKeys.Length > 0
            && asset.dataValues.Length == asset.dataKeys.Length) {
            for (int i = 0; i < asset.dataKeys.Length; i++) {
                data[asset.dataKeys[i]] = asset.dataValues[i];
            }
        }

        if (asset.idsKeys != null
            && asset.idsValues != null
            && asset.idsKeys.Length > 0
            && asset.idsValues.Length == asset.idsKeys.Length) {
            for (int i = 0; i < asset.idsKeys.Length; i++) {
                dictIDs[asset.idsKeys[i]] = asset.idsValues[i];
            }
        }
    }

    [System.Serializable]
    public struct Serialized {
        public string[] dataKeys;
        public string[][] dataValues;
        public string[] idsKeys;
        public int[] idsValues;
    }

}

}