using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
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
                BasicRepresentation.transform.localPosition = new Vector3(0, 0.02f, 0);
                BasicRepresentation.transform.localEulerAngles = new Vector3(90.0f, 0, 0);
            }

            combinedRepresentationManager = gameObject.GetComponent<CombinedRepresentationManager>();
        }

        private void HandleCollision(Transform other)
        {
            var direction = (GameObject.Find("AR Camera").transform.position - gameObject.transform.position).normalized;

            if (CombinedRepresentation == null && Vector3.Dot(gameObject.transform.right, direction) > Vector3.Dot(other.right, direction))
            {
                var combinedRepresentation = combinedRepresentationManager.GetCombinedRepresentation(gameObject, other.gameObject);

                if(combinedRepresentation != null)
                {
                    CombinedRepresentation = Instantiate(combinedRepresentation, gameObject.transform);
                    CombinedRepresentation.transform.localPosition = new Vector3(-0.25f, 0.02f, 0);
                    CombinedRepresentation.transform.localEulerAngles = new Vector3(90.0f, 0, 0);

                    BasicRepresentation.SetActive(false);
                    other.gameObject.GetComponent<ARTargetManager>()?.BasicRepresentation?.SetActive(false);
                    other.gameObject.GetComponent<ARTargetManager>()?.CombinedRepresentation?.SetActive(false);

                    logger.text = $"{logger.text}\nGerou representacao combinada! {combinedRepresentation.name}";
                    logger.SetAllDirty();
                }
            }
        }

        // Update is called once per frame
        void OnTriggerEnter(Collider other) => HandleCollision(other.transform);
        //void OnTriggerExit(Collider other)
        //{
        //    if (!BasicRepresentation.activeSelf)
        //        BasicRepresentation.SetActive(true);

        //    if (CombinedRepresentation != null && CombinedRepresentation.activeSelf)
        //    {
        //        CombinedRepresentation.transform.parent = null;
        //        Destroy(CombinedRepresentation);
        //        CombinedRepresentation = null;
        //    }
        //}
    }
}