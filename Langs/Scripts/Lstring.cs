using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FMLHT.Langs {
    public class lstring {
        public string ID;
        public string dict;
        public lstring(string ID_, string dict_ = "Default") {
            ID = ID_;
            dict = dict_;
        }
        public string t {
            get {
                return this.ToString();
            }
        }
        public override string ToString() {
            if (dict == "") {
                return ID;
            } else {
                return L.Get(ID, dict);
            }
        }

        public static lline operator +(lstring obj1, lstring obj2) {
            var line = new lline();
            return line + obj1 + obj2;
        }

        public static lline operator +(lstring obj1, string obj2) {
            var line = new lline();
            return line + obj1 + obj2;
        }

        public static lline operator +(string obj1, lstring obj2) {
            var line = new lline();
            return line + obj1 + obj2;
        }
    }
}