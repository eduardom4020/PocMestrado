using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.ARFoundation;

namespace POCMestrado.Assets.Components.ARManagers
{
    [Serializable]
    public struct MarkerTarget
    {
        public string MarkerName;
        public GameObject Prefab;
    }

    public class ARTargetManager : MonoBehaviour
    {
        public GameObject TestPrefab;
        public List<MarkerTarget> MarkersTargets;

        private Text logger;
        private CombinedRepresentationManager combinedRepresentationManager;

        // Start is called before the first frame update
        void Start()
        {
            logger = GameObject.Find("Logger").GetComponent<Text>();
            logger.text = $"{logger.text}\nInicializou ARTargetManager!";
            logger.SetAllDirty();

            var trackedImage = gameObject.GetComponent<ARTrackedImage>();

            var target = MarkersTargets.FirstOrDefault(mt => mt.MarkerName == trackedImage.referenceImage.name);
            if(target.Prefab != null)
            {
                Instantiate(target.Prefab, gameObject.transform);
            }

            combinedRepresentationManager = gameObject.GetComponent<CombinedRepresentationManager>();
        }

        private void HandleCollision(Transform other)
        {
            logger.text = $"{logger.text}\nOcorreu Colisão!";
            logger.SetAllDirty();

            gameObject.SetActive(false);

            var direction = (Camera.main.transform.position - gameObject.transform.position).normalized;
            if (Vector3.Dot(gameObject.transform.forward, direction) < Vector3.Dot(other.forward, direction))
            {
                var combinedRepresentation = combinedRepresentationManager.GetCombinedRepresentation(gameObject, other.gameObject);
                Instantiate(combinedRepresentation, gameObject.transform.parent.transform);
            }
        }

        // Update is called once per frame
        void OnTriggerEnter(Collider other) => HandleCollision(other.transform);
    }
}