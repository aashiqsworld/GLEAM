using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.iOS;
using System;

public class GenerateImageAnchor : MonoBehaviour {


	[SerializeField]
	private ARReferenceImage referenceImage;

	[SerializeField]
	private GameObject prefabToGenerate;

	private GameObject imageAnchorGO;

	// Use this for initialization
	void Start () {
		UnityARSessionNativeInterface.ARImageAnchorAddedEvent += AddImageAnchor;
		UnityARSessionNativeInterface.ARImageAnchorUpdatedEvent += UpdateImageAnchor;
		UnityARSessionNativeInterface.ARImageAnchorRemovedEvent += RemoveImageAnchor;

	}
    UnityAREnvironmentProbeAnchorData anchorData;
    void CreateEnvironmentProbe(Matrix4x4 worldTransform)
    {
        //note we have not converted to Unity coord system yet, so we can pass it in directly

        anchorData.ptrIdentifier = IntPtr.Zero;
        anchorData.probeExtent = Vector3.one;
        anchorData.transform = UnityARMatrixOps.GetMatrix(worldTransform); //this should be in ARKit coords
        anchorData.cubemapData.cubemapPtr = IntPtr.Zero;
        anchorData.cubemapData.textureFormat = UnityAREnvironmentTextureFormat.UnityAREnvironmentTextureFormatDefault;
        anchorData.cubemapData.width = 0;
        anchorData.cubemapData.height = 0;
        anchorData.cubemapData.mipmapCount = 0;
        anchorData = UnityARSessionNativeInterface.GetARSessionNativeInterface().AddEnvironmentProbeAnchor(anchorData);
    }

    void AddImageAnchor(ARImageAnchor arImageAnchor)
	{
		Debug.LogFormat("image anchor added[{0}] : tracked => {1}", arImageAnchor.identifier, arImageAnchor.isTracked);
		if (arImageAnchor.referenceImageName == referenceImage.imageName) {
			Vector3 position = UnityARMatrixOps.GetPosition (arImageAnchor.transform);
			Quaternion rotation = UnityARMatrixOps.GetRotation (arImageAnchor.transform);

			imageAnchorGO = Instantiate<GameObject> (prefabToGenerate, position, rotation);
            CreateEnvironmentProbe(arImageAnchor.transform);
		}
	}

	void UpdateImageAnchor(ARImageAnchor arImageAnchor)
	{
		Debug.LogFormat("image anchor updated[{0}] : tracked => {1}", arImageAnchor.identifier, arImageAnchor.isTracked);
		if (arImageAnchor.referenceImageName == referenceImage.imageName) {
            imageAnchorGO.SetActive(false);
            //if (arImageAnchor.isTracked)
            //{
            //    //if (!imageAnchorGO.activeSelf)
            //    //{
            //    //    imageAnchorGO.SetActive(false);
            //    //}
            //    //CreateEnvironmentProbe(arImageAnchor.transform);
            //    Debug.Log("IMAGEANCHORDEBUG: TRACKED");
            //    anchorData.transform = UnityARMatrixOps.GetMatrix(arImageAnchor.transform);
            //    //imageAnchorGO.transform.position = UnityARMatrixOps.GetPosition(arImageAnchor.transform);
            //    //imageAnchorGO.transform.rotation = UnityARMatrixOps.GetRotation(arImageAnchor.transform);
            //}
            //else if (imageAnchorGO.activeSelf)
            //{
            //    imageAnchorGO.SetActive(false);
            //}
        }

	}

	void RemoveImageAnchor(ARImageAnchor arImageAnchor)
	{
		Debug.LogFormat("image anchor removed[{0}] : tracked => {1}", arImageAnchor.identifier, arImageAnchor.isTracked);
		if (imageAnchorGO) {
			GameObject.Destroy (imageAnchorGO);
		}

	}

	void OnDestroy()
	{
		UnityARSessionNativeInterface.ARImageAnchorAddedEvent -= AddImageAnchor;
		UnityARSessionNativeInterface.ARImageAnchorUpdatedEvent -= UpdateImageAnchor;
		UnityARSessionNativeInterface.ARImageAnchorRemovedEvent -= RemoveImageAnchor;

	}

	// Update is called once per frame
	void Update () {
		
	}
}
