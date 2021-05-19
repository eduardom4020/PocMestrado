using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace POCMestrado.Assets.Components.ARManagers
{
    [Serializable]
    public struct CombinationTuple
    {
        public GameObject PrefabA;
        public GameObject PrefabB;
        public GameObject CombinedRepresentation;
    }

    public class CombinedRepresentationManager : MonoBehaviour
    {
        public List<CombinationTuple> CombinedRepresentations;
        private Text logger;

        void Start()
        {
            logger = GameObject.Find("Logger").GetComponent<Text>();
        }

        public GameObject GetCombinedRepresentation(GameObject objA, GameObject objB)
        {
            var prefabs = new List<GameObject>() { objA, objB };

            var match = CombinedRepresentations?.FirstOrDefault(t => prefabs.All(p => p.tag == t.PrefabA.tag || p.tag == t.PrefabB.tag));

            if (match != null && match.HasValue)
            {
                logger.text = $"{logger.text}\nPossui representação combinada!";
                logger.SetAllDirty();
                return match.Value.CombinedRepresentation;
            }

            return null;
        }
    }
}