using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using FMLHT.Langs;

public class Translatable : MonoBehaviour
{
    public string dict = "Default";
    public string ID = "";

    private Text textUI;
    private TextMesh textMesh;
    private TextMeshPro textTMP;

    public enum Type {
        None, UI, Mesh, TMP
    }
    public Type type = Type.None;

    void Awake() {
        L.AddTranslatable(this);
        CheckType();
        if (L.IsDictReady(dict)) Translate();
    }

    public void Translate() {
        SetText(L.Get(ID, dict));
    }

    void SetText(string text) {
        if (ID != "") {
            switch (type) {
                case Type.None:
                    break;
                case Type.UI:
                    textUI.text = text;
                    break;
                case Type.Mesh:
                    textMesh.text = text;
                    break;
                case Type.TMP:
                    textTMP.text = text;
                    break;
            }
        }
    }

    bool CheckType() {
        switch (type) {
            case Type.None:
                if (CheckUI()) {
                    type = Type.UI;
                    break;
                }
                if (CheckMesh()) {
                    type = Type.Mesh;
                    break;
                }
                if (CheckTMP()) {
                    type = Type.TMP;
                    break;
                }
                break;
            case Type.UI:
                if (!CheckUI()) type = Type.None;
                break;
            case Type.Mesh:
                if (!CheckMesh()) type = Type.None;
                break;
            case Type.TMP:
                if (!CheckTMP()) type = Type.None;
                break;
        }
        return type != Type.None;
    }

    bool CheckUI() {
        textUI = GetComponent<Text>();
        return textUI != null;
    }

    bool CheckMesh() {
        textMesh = GetComponent<TextMesh>();
        return textMesh != null;
    }

    bool CheckTMP() {
        textTMP = GetComponent<TextMeshPro>();
        return textTMP != null;
    }
}