using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FMLHT.Langs {
    public class lline {
        List<lstring> line;

        public lline() {
            line = new List<lstring>();
        }

        public lline AddTo(lstring item) {
            line.Add(item);
            return this;
        }

        public void Add(lstring item) {
            line.Add(item);
        }

        public static lline operator +(lline obj, lstring item) {
            return obj.AddTo(item);
        }

        public static lline operator +(lline obj, string item) {
            return obj.AddTo(new lstring(item, ""));
        }

        public static lline operator +(lline obj, lline obj2) {
            foreach (var i in obj2.line) {
                obj += i;
            }
            return obj;
        }

        public override string ToString() {
            var arr = new List<string>();
            foreach (var i in line) {
                arr.Add(i.ToString());
            }
            return string.Join("", arr);
        }
    }
}