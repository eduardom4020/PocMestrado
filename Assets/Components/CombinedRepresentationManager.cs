using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace POCMestrado.Assets.Components.ARManagers
{
    public static class GameObjectExtensions
    {
        public static GameObject GetFirstActiveChild(this GameObject gameObject)
        {
            for (int i = 0; i < gameObject.transform.childCount; i++)
            {
                var child = gameObject.transform.GetChild(i)?.gameObject;
                if (child.activeSelf == true)
                    return child;
            }

            return null;
        }
    }


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

            logger.text = $"{logger.text}\nConferindo objeto A {objA.GetFirstActiveChild()?.name}\nConferindo objeto B {objB.GetFirstActiveChild()?.name}";
            logger.text = $"{logger.text}\nTestando combined representation Obj A {CombinedRepresentations?.FirstOrDefault().PrefabA?.name}\nTestando combined representation Obj B {CombinedRepresentations?.FirstOrDefault().PrefabB?.name}";
            logger.SetAllDirty();

            var match = CombinedRepresentations?.FirstOrDefault
            (
                t => prefabs.Any
                    (
                        x => (x.GetFirstActiveChild()?.name?.Contains(t.PrefabA.name) ?? false) ||
                            (x.GetFirstActiveChild()?.name?.Contains(t.PrefabB.name) ?? false)
                    )
            );

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