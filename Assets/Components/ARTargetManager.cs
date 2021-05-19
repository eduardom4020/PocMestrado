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
        public List<MarkerTarget> MarkersTargets;

        private Text logger;
        private CombinedRepresentationManager combinedRepresentationManager;

        private GameObject BasicRepresentation;
        private GameObject CombinedRepresentation;

        // Start is called before the first frame update
        void Start()
        {
            logger = GameObject.Find("Logger").GetComponent<Text>();

            var trackedImage = gameObject.GetComponent<ARTrackedImage>();

            var target = MarkersTargets.FirstOrDefault(mt => mt.MarkerName == trackedImage.referenceImage.name);
            if(target.Prefab != null)
            {
                BasicRepresentation = Instantiate(target.Prefab, gameObject.transform);
            }

            combinedRepresentationManager = gameObject.GetComponent<CombinedRepresentationManager>();
        }

        private void HandleCollision(Transform other)
        {
            var direction = (GameObject.Find("AR Camera").transform.position - gameObject.transform.position).normalized;

            BasicRepresentation.SetActive(false);

            if (Vector3.Dot(gameObject.transform.forward, direction) < Vector3.Dot(other.forward, direction))
            {
                var combinedRepresentation = combinedRepresentationManager.GetCombinedRepresentation(gameObject, other.gameObject);
                CombinedRepresentation = Instantiate(combinedRepresentation, gameObject.transform);

                logger.text = $"{logger.text}\nGerou representacao combinada!";
                logger.SetAllDirty();
            }
        }

        // Update is called once per frame
        void OnTriggerEnter(Collider other) => HandleCollision(other.transform);
        void OnTriggerExit(Collider other)
        {
            if (!BasicRepresentation.activeSelf)
                BasicRepresentation.SetActive(true);

            if (CombinedRepresentation != null && CombinedRepresentation.activeSelf)
            {
                CombinedRepresentation.transform.parent = null;
                Destroy(CombinedRepresentation);
                CombinedRepresentation = null;
            }
        }
    }
}