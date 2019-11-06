using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.iOS;


public class ARKitCameraWrapper : MonoBehaviour
{
    public NewGLEAMBehaviour mGLEAM;
    [HideInInspector]
    public RenderTexture cameraImage; // full color camera image
    public Camera sampleCamera; // refenence to the camera that
    public Camera sampleCamera2; // refenence to the camera that
    Vector3 screenProbePosition; // screen position of the probe
    Vector3 imageAnchorPosition; // position of the image target
    float secondCounter = 0.0f;
    public GameObject reflectiveObject;
    // Start is called before the first frame update
    void Awake()
    {
        if (sampleCamera == null)
            sampleCamera = gameObject.GetComponent<Camera>();
        // event to update image anchor position
        UnityARSessionNativeInterface.ARImageAnchorUpdatedEvent += UpdateImageAnchor;
        // Set the texture to sample from for the probe.
        cameraImage = new RenderTexture(Screen.width, Screen.height, 24);
        sampleCamera.targetTexture = cameraImage;
    }

    // Updates the position of the probe and image anchor. Called by event. Method can't be renamed because its called by an event.
    void UpdateImageAnchor(ARImageAnchor arImageAnchor) {
        imageAnchorPosition = UnityARMatrixOps.GetPosition(arImageAnchor.transform);
        mGLEAM.probe.transform.position = imageAnchorPosition + mGLEAM.probeOffset;
        reflectiveObject.transform.position = imageAnchorPosition + mGLEAM.probeOffset;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if(secondCounter >= 0.5f)
        {
            mGLEAM.GLEAM_Update(sampleCamera2, cameraImage);
            secondCounter = 0.0f;
        }
        secondCounter += Time.deltaTime;
    }
}
