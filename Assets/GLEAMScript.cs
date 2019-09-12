/**
 * GLEAM - Generating Light Estimation Across Mixed-reality Devices through Collaborative Visual Sensing
 * 
 * Copyright (c) 2018-present, Meteor Studio http://meteor.ame.asu.edu/.
 * 
 * For any questions contact:
 * Siddhant Prakash, sprakas9@asu.edu
 * Paul A. Nathan, panathan@asu.edu
 * Robert LiKamWa, likamwa@asu.edu
 * 
 */

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
//using Vuforia;
using System.IO;
using UnityEngine.Rendering;
using System.Threading;
using UnityEngine.SceneManagement;
using UnityEngine.XR.iOS;
using TMPro;

#if false
public class GLEAMScript : MonoBehaviour {

    public RenderTexture cameraFeedRT; //hosts camera pixels 
    
    #region DECLARE_GLOBAL_VARS
    // Global Variables
    // Vuforia parameters
    //private PIXEL_FORMAT mPixelFormat = PIXEL_FORMAT.UNKNOWN_FORMAT;
    private bool mAccessCameraImage = true;
    private bool mFormatRegistered = false;
    string text = "";
    
    Camera mainCam;         // Specify camera to use
    public Camera dummyCam, dummyCamHalf;
    // SampleImage & Cubemap faces
    public RawImage sampleImage;
    public Image sampleImageIndicator;
    public RawImage arkitCanvas;
    public RawImage imageFacePosX,
                    imageFacePosY,
                    imageFacePosZ,
                    imageFaceNegX,
                    imageFaceNegY,
                    imageFaceNegZ;
    Cubemap cubemap; // Texture that stores environment map
    CubemapFace face;
    ListOfSamples clientLOS;
    GameObject clientListOfSamples;

    public Material skyMat; // Material storing cubemap texture
    //public Slider intensity;
    public float intensity;// = 1.0f;
    public static int textureSize = 128; // Size of bounding box around reflective object
    public static int faceSideLength;

    public Collider reflectorCollider;
    public Collider SPHEREcollider;
    public InputField addressText;

    public List<int>[] neighborhood;
    public int[] sumsR;
    public int[] sumsG;
    public int[] sumsB;

    public Texture2D[] faces = new Texture2D[6];
    // Debug variables
    public bool debug = false;
    public Text debugText;

    //public Slider smoothingSlider;
    //public Slider samplesToSendSlider;
    private float smoothingFactor = 10.0f;
    Color[] pixels;

    Texture3D skyMatTex;

    GameObject localPlayer;
    //GameObject clientListOfSamples;
    //public Toggle sendSamplesToggle;
    //public Toggle holeFillToggle;
    //public Toggle threadToggle;
    Samples.singleSampleObject[] samplesToSend;
    private int sampleSize;
    private int maxSampleNetwork;
    int screenShotIdx = 0;

    //public Slider distanceSlider;

    //public Slider updateRateSlider;
    //public Text screenRecordText;
    //ListOfSamples clientLOS;
    private NetworkScript networkScript = null;
    long gameStart;

    CubemapFace[] faceArray = { CubemapFace.PositiveX, CubemapFace.NegativeX, CubemapFace.PositiveY, CubemapFace.NegativeY, CubemapFace.PositiveZ, CubemapFace.NegativeZ };
    List<Color[]> cubemapColors = new List<Color[]>();

    private static readonly DateTime Jan1st1970 = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
    public static long CurrentTimeMillis() {
        return (long)(DateTime.UtcNow - Jan1st1970).TotalMilliseconds;
    }

    private bool resultsReady = false;
    bool running = false;
    Thread thread;

    byte[] imagepixels;
    int imagewidth;
    int camwidth;
    int camheight;

    int wx, wy;
    float calcratio;

    int age;
    private float sampledist;
    private int mainCamPixWidth;
    private int mainCamPixHeight;
    private Texture2D snap;
    private Texture2D texture2D;

    string path1 = "Assets/updateVsSamples.txt";
    string path2 = "Assets/tailVsSamples.txt";
    string textUpdatevsSamples = "";
    string textTailsvsSamples = "";
    int runCount = 0;
    public int testRuns = 100;

    long runstarttime, runendtime, runTimeSum;
    long[] runArray;
    double runAverage;
    long imagingstarttime, imagingendtime, imagingTimeSum;
    long[] imagingArray;
    double imagingAverage;
    long samplingstarttime, samplingendtime, samplingTimeSum;
    long[] samplingArray;
    double samplingAverage;
    long networkstarttime, networkendtime, networkTimeSum;
    long[] networkArray;
    double networkAverage;
    long composestarttime, composeendtime, composeTimeSum;
    long[] composeArray;
    double composeAverage;

    long idwstarttime, idwendtime, idwTimeSum;
    long holefillstarttime, holefillendtime, holefillTimeSum;
    long avgstarttime, avgendtime, avgTimeSum;
    long holeFillSumTime;
    long texCapstartime, texCapendtime, texCapsumtime;

    long[] texCapArray;
    double texCapAverage;
    double sumOfSquaresOfDifferences, sd;

    int numberSampleGenerate = 0;
    int numberSampleCompose = 0;

    float dev;
    Vector3 colWebTexPos;
    private List<Samples.singleSampleObject[]> clientList;
    private List<long> clientTimestamps;
    private List<int> clientDevices;
    private RenderTexture renderTexture;

    float ageLimit;

    Color[] buff2;
    List<int> holeX = new List<int>();
    List<int> holeY = new List<int>();

    List<Color[]> sumsList = new List<Color[]>();
    List<float[]> weightsList = new List<float[]>();
    long idwStartTime, holeFillStartTime;
    int numSamplesToSend;

    // new variables
    public TextMeshProUGUI debugInfo;
    #endregion


    // new methods
    public void UpdateDebugInfo(string debugText)
    {
        debugInfo.text = debugText;
    }


    // For profiling purposes.
    void InitializeProfiler() {
        runArray = new long[testRuns];
        imagingArray = new long[testRuns];
        samplingArray = new long[testRuns];
        networkArray = new long[testRuns];
        composeArray = new long[testRuns];
        texCapArray = new long[testRuns];
    }

    /*
     * Initialize sample collections, cubemap visualizations, and referencing of data structures.
     * This class populates all sample collections for single device or multi-device sessions. 
     * It additionally works on extracting data from Vuforia Image Target & model.
     */
    void Start() {
        UnityARSessionNativeInterface.ARImageAnchorUpdatedEvent += UpdateImageAnchor;
        mainCam = gameObject.GetComponent<Camera>();
        //InvokeFillHoles.init();
        gameStart = CurrentTimeMillis();
        intensity = 1.0f;
        // Init List of Samples to begin populating with extracted data.
        clientListOfSamples = GameObject.Find("ListOfSamples");
        clientLOS = clientListOfSamples.GetComponent<ListOfSamples>();
        clientLOS.initListOfSamples();
        /* 
         * Assign resolution for each face of cubemap, 
         * this resolution will be equivalent to the samples stored.
         */
        faceSideLength = clientLOS.RESOLUTION;
        // Create neighborhood sample list for all faces of cube.
        // A cube is length^2, faceSideLength^2 will give us all faces as a list reference.
        neighborhood = new List<int>[faceSideLength * faceSideLength];
        // Assign color channel for each element of the cubemap surface area.
        sumsR = new int[faceSideLength * faceSideLength];
        sumsG = new int[faceSideLength * faceSideLength];
        sumsB = new int[faceSideLength * faceSideLength];
        //InitializeProfiler();

        /*
         * TODO: Revise data structures utilized for this.
         * Here we store each client's respective cubemap calculations
         * along with meta information (what device captured this collection, when).
         */
        clientList = clientLOS.listOfLOS;
        clientTimestamps = clientLOS.timestamps;
        clientDevices = clientLOS.deviceIDs;

        mainCam = Camera.main;

        cameraFeedRT = new RenderTexture(Screen.height, Screen.width, 24); // switched because TextureY is rotated 90 degrees for some reason

        mainCamPixHeight = mainCam.pixelHeight;
        mainCamPixWidth = mainCam.pixelWidth;
        // Texture2D used for storing samples and assigning them to cubemap generated for skybox.
        texture2D = new Texture2D(textureSize, textureSize, TextureFormat.RGBA32, false);
        // Snap used for assigning snapshot capture pixels (sized 128 for resolution).
        snap = new Texture2D(textureSize, textureSize);
//#if UNITY_EDITOR
//        mPixelFormat = PIXEL_FORMAT.RGBA8888; // Need Grayscale for Editor
//#else
//        mPixelFormat = PIXEL_FORMAT.RGB888; // Use RGB888 for mobile
//#endif

        // Initialize cubemap, devices, & webcamTexture
        /* 
         * Cubemap is initialized with 128 rows/columns per face, 
         * pure color channel no alpha,
         * TODO: Research tradeoffs in fidelity when not using mipmaps. Perhaps offer it as a option?
         */
        cubemap = new Cubemap(faceSideLength, TextureFormat.RGB24, false);
        for (int i = 0; i < 6; i++) {
            // Each face is stored as a texture for populating in GUI.
            faces[i] = new Texture2D(faceSideLength, faceSideLength);
        }
        localPlayer = NetworkManager.singleton.client.connection.playerControllers[0].gameObject;
        NetworkManager.singleton.StartHost();
        Debug.Log("RLEOS: End of Start");
        // Register Vuforia life-cycle callbacks:
        //VuforiaARController.Instance.RegisterVuforiaStartedCallback(OnVuforiaStarted);
        //VuforiaARController.Instance.RegisterTrackablesUpdatedCallback(OnTrackablesUpdated);
        //VuforiaARController.Instance.RegisterOnPauseCallback(OnPause);
    }

    void UpdateImageAnchor(ARImageAnchor arImageAnchor) {
        string debugString = "";
        Vector3 position = UnityARMatrixOps.GetPosition(arImageAnchor.transform);

        SPHEREcollider.transform.position = position + new Vector3(0.0f, 0.045f, 0.0f);

        debugString += string.Format("image anchor pos:{0}", position.ToString());
        // UpdateDebugInfo(debugString);

    }

    // Update is called once a registered frame is available
    void FixedUpdate() {

        string debugString = "";

        // TODO: Fix redundant calls.
        if (reflectorCollider == null) {
            reflectorCollider = GameObject.Find("GLEAMCollider").GetComponent<Collider>();
            if (reflectorCollider != null) {
                Debug.Log("RL: reflectorCollider found!");
            }
        }
        if (SPHEREcollider == null) {
            SPHEREcollider = GameObject.Find("GLEAMCollider").GetComponent<Collider>();
            if (SPHEREcollider != null) {
                Debug.Log("RL: reflectorCollider found!");
            }
        }
        // Once Vuforia camera instance is tracking pose and color formatting is successfully
        // set, it'll begin calculating cubemap for update events.
        if (true) {
            Texture2D backgroundtexture = null;

#if UNITY_EDITOR
            dummyCam.CopyFrom(Camera.main);
#else
            dummyCam.CopyFrom(Camera.main);
            //dummyCam.CopyFrom(dummyCamHalf);        // Move one camera feed to the other
            //dummyCamHalf.CopyFrom(Camera.main);     // Store new frame
#endif

            if (!running) {         // TODO: Running?
                Debug.Log("RL: running!");

                //updateTime = CurrentTimeMillis(); // Used to define update interval
                texCapendtime = CurrentTimeMillis();
                // Identify where on the screen the reflective collider is 
                // based on origin camera which copies frame texture from main camera via prior frame.
                // Full frame texture captured by Vuforia instance.
                //backgroundtexture = (Texture2D)VuforiaRenderer.Instance.VideoBackgroundTexture;

                if (true){//backgroundtexture != null) {
                    imagingstarttime = CurrentTimeMillis();
                    // Assign width & height props of camera frame/feed.
                    arkitCanvas.texture = mainCam.gameObject.GetComponent<UnityARVideo>()._videoTextureY;
                    //reflectorCollider.transform.position = SPHEREcollider.transform.position;
                    colWebTexPos = mainCam.WorldToScreenPoint(SPHEREcollider.transform.position);
                    camwidth = mainCam.gameObject.GetComponent<UnityARVideo>()._videoTextureY.width;
                    camheight = mainCam.gameObject.GetComponent<UnityARVideo>()._videoTextureY.height;
                    Graphics.Blit(mainCam.gameObject.GetComponent<UnityARVideo>()._videoTextureY, cameraFeedRT);

                    //camwidth = cameraFeedRT.width;
                    //camheight = cameraFeedRT.height;
                    /* 
                     * Vuforia texture sampling is less than received camera frame dimensions.
                     * So we check the new aspect ratio camera feed to sample image of specular object.
                     */
                    /*Debug.Log(string.Format("Dummy Cam: {0}, Cam: {1}, Ratio of Width: {2}, Ratio of Height: {3}", 
                                            dummyCam.pixelWidth, 
                                            camwidth,
                                            1.0f * camwidth / dummyCam.pixelWidth,
                                            1.0f * camheight / dummyCam.pixelHeight));*/
                    // Convert to floats for calculating ratio of width/height for contained vuforia frame in full frame.
                    calcratio = Mathf.Min(1.0f * camwidth / dummyCam.pixelWidth, 1.0f * camheight / dummyCam.pixelHeight);
                    //debugString += string.Format("Col X:{0}\nY:{1}\nunityar:{2}\nunityar:{3}\ncamW:{4}\ncamH:{5}\nWTS:{6}\nColl:{7}", colWebTexPos.x, colWebTexPos.y, 
                    //camwidth, camheight, mainCam.pixelWidth, mainCam.pixelHeight, mainCam.transform.position.ToString(), reflectorCollider.transform.position.ToString());

                    /* Converts 3D space coordinate to screen coordinates.
                     * By performing the worldPos - fullCamera feed * ratio, we get how far from center of camera feed object is based
                     * on shared canvas space of Vuforia tracking and Camera feed.
                     * We then add half the Vuforia Canvas to this to get the position in Vuforia's camera space.
                     * Lastly, we subtract half the texture size to know the location from which we sample the texture size. 
                     * Since we are workin on one side of the screen (bc adding halves gives full values for working in a defined range)
                     * we subtract the half of texture size because moving forward we sample from wx+textureSize.
                     */


                    /*
                     * wx & wy represent pixel location for probe canvas size with respect to camera canvas.
                     */
                    wy = (camwidth / 2 + Mathf.RoundToInt(calcratio * (colWebTexPos.x - dummyCam.pixelWidth / 2))) - textureSize / 2;
                    wx = (camheight / 2 - Mathf.RoundToInt(calcratio * (colWebTexPos.y - dummyCam.pixelHeight / 2))) - textureSize / 2;
                    // So long as we aren't outside of tracking area && full textureSampleSpace is collectible on x/y axis.
                    Debug.Log("RL: wx,wy,camwidth,camheight" + wx + "," + wy + "," + camwidth + "," + camheight);


                    if (wx > 0 && wx < camwidth - textureSize && wy > 0 && wy < camheight - textureSize) {
                        Debug.Log("RL: wx wy inside!");

#if UNITY_EDITOR
                        //                   pixels = backgroundtexture.GetPixels(wx, wy, textureSize, textureSize);
#else
                        //MOBILE
                        // Store Vuforia feed as Texture object.
                        //Texture mainTexture = (Texture)backgroundtexture;
                        // Render Texture is for implementing image-based rendering of captured canvas/texture camera feed.
                        //if (renderTexture == null)
                        //{
                        //    if(debug){
                        //        Debug.Log("renderTexture");
                        //    }
                        //    renderTexture = new RenderTexture(mainTexture.width, mainTexture.height, 32);
                        //}

                        // Temporarily store prior active rendertexture into currentRT
                        RenderTexture currentRT = RenderTexture.active; // Hold reference to current render texture.
                        Debug.Log("RL: currentRT = RenderTexture.active");
                        // Set active RenderTexture to be the camera feed
                        RenderTexture.active = cameraFeedRT;
                        Debug.Log("RL: RT.active = cameraFeedRT");

                        //texture2D.ReadPixels(new Rect(0, 0, renderTexture.width, renderTexture.height), 0, 0);
                        /* 
                         * Here we read in the sample space as calculated via wx from current activated RenderTexture.
                         * ReadPixels reads from RenderTexture.active
                         * Assign sampled space to projected Texture Display on screen. We take wx,wy as starting positions
                         * and project out by 128 pixels wide.
                         */
                        Debug.Log("RL: ReadPixels, wx,wy,camwidth,camheight" + wx + "," + wy + "," + camwidth + "," + camheight);
                        texture2D.ReadPixels(new Rect(wx, wy, textureSize, textureSize), 0, 0);
                        UpdateDebugInfo("wx: " + wx + "\nwy: " + wy);
                        sampleImageIndicator.transform.position = new Vector3((float)wy, (float)(Screen.height - wx), 0.0f);
                        Debug.Log("RL: ReadPixels");

                        texture2D.Apply();
                       Debug.Log("RL: texture2D.Apply()");

                        pixels = texture2D.GetPixels(); // Gather color pixel vlaues of region from assigned sample space.
                        Debug.Log("RL: pixels get pixels");

                        // Restores currentRT as active RenderTExture
                        RenderTexture.active = currentRT;
                        //END

#endif
                        //THIS IS FOR DISPLAYING SAMPLE IMAGE
                        //if (debug) {
                        Texture2D snap = new Texture2D(textureSize, textureSize); // Texture needs to be flipped in editor
                        snap.SetPixels(pixels);     // Assign sampled space into projected texture.
                        snap.Apply();               // Set pixels.
                        sampleImage.texture = snap; // Assign created texture to sample image object (Raw Image dislayed)
                        Debug.Log("RL:"+(pixels[0].r + " " + pixels[0].g + " " + pixels[0].b + " " + pixels[0].a + pixels[1].r + " " + pixels[1].g + " " + pixels[1].b + " " + pixels[1].a));

                        //}
                        //debugText.text += " painted " + (pixels[0].r + " " + pixels[0].g + " " + pixels[0].b + " " + pixels[0].a + pixels[1].r + " " + pixels[1].g + " " + pixels[1].b + " " + pixels[1].a);
                    }
                    imagingendtime = CurrentTimeMillis();
                }

                runendtime = CurrentTimeMillis();
                if (resultsReady) {
                    applyCubemap(); // Minus this function and displaycubemap(), everything else is for profiling.
                    if (debug) {    // Set the 6 rawImages to project the cubemap sampling values 
                        displayCubemap();       // Note, this is not the same cubemap as our processed cubemap.
                    }// TODO: Reduce quantity of cubemaps used.

                    #region PROFILING
                    runCount++;
                    if (runCount > 1 && runCount <= testRuns) {
                        runTimeSum = runTimeSum + (runendtime - runstarttime);
                        runArray[runCount - 1] = (runendtime - runstarttime);
                        texCapsumtime = texCapsumtime + (texCapendtime - texCapstartime);
                        texCapArray[runCount - 1] = (texCapendtime - texCapstartime);

                        imagingTimeSum = imagingTimeSum + (imagingendtime - imagingstarttime);
                        imagingArray[runCount - 1] = (imagingendtime - imagingstarttime);
                        samplingTimeSum = samplingTimeSum + (samplingendtime - samplingstarttime);
                        samplingArray[runCount - 1] = (samplingendtime - samplingstarttime);
                        networkTimeSum = networkTimeSum + (networkendtime - networkstarttime);
                        networkArray[runCount - 1] = (networkendtime - networkstarttime);
                        composeTimeSum = composeTimeSum + (composeendtime - composestarttime);
                        composeArray[runCount - 1] = (composeendtime - composestarttime);


                        idwTimeSum = idwTimeSum + ((idwendtime - idwstarttime));
                        holefillTimeSum = holefillTimeSum + holeFillSumTime;
                        avgTimeSum = avgTimeSum + (avgendtime - avgstarttime);
                        holefillTimeSum = holefillTimeSum + (holefillendtime - holefillstarttime);
                    } else {
                        runTimeSum = 0;
                        texCapsumtime = 0;
                    }
                    #endregion
                    #region DEBUG_PROFILE
                    if (debug && runCount >= testRuns) {
                        //debugText.text = "TOTAL RUNTIME: " + ((runTimeSum) * 1.0 / runCount).ToString();
                        //debugText.text = debugText.text + "\nTOTAL TEX UPDATE TIME: " + (texCapsumtime * 1.0 / runCount).ToString();
                        ////debugText.text = debugText.text + "\nTOTAL CAPTURE TIME: " + (imagingTimeSum * 1.0 / runCount).ToString();
                        //debugText.text = debugText.text + "\nTOTAL SAMPLING TIME: " + (samplingTimeSum * 1.0 / runCount).ToString();
                        //debugText.text = debugText.text + "\nTOTAL NETWORK TIME: " + (networkTimeSum * 1.0 / runCount).ToString();
                        //debugText.text = debugText.text + "\nTOTAL COMPOSE TIME: " + (composeTimeSum * 1.0 / runCount).ToString();

                        //debugText.text = debugText.text + "\n" + (idwTimeSum * 1.0 / runCount).ToString();
                        //debugText.text = debugText.text + "\n" + (holefillTimeSum * 1.0 / runCount).ToString();
                        //debugText.text = debugText.text + "\n" + (avgTimeSum * 1.0 / runCount).ToString();
                        runAverage = runArray.Average();
                        sumOfSquaresOfDifferences = runArray.Select(val => (val - runAverage) * (val - runAverage)).Sum();
                        sd = Math.Sqrt(sumOfSquaresOfDifferences / testRuns);
                        debugText.text = "RUNTIME Mean: " + runAverage.ToString() + ", Std: " + sd.ToString();

                        texCapAverage = texCapArray.Average();
                        sumOfSquaresOfDifferences = texCapArray.Select(val => (val - texCapAverage) * (val - texCapAverage)).Sum();
                        sd = Math.Sqrt(sumOfSquaresOfDifferences / testRuns);
                        debugText.text = debugText.text + "\nTEX UPDATE TIME Mean: " + texCapAverage.ToString() + ", Std:" + sd.ToString();

                        //sumOfSquaresOfDifferences = runArray.Select(val => (val - runAverage) * (val - runAverage)).Sum();
                        //sd = Math.Sqrt(sumOfSquaresOfDifferences / testRuns);
                        ////debugText.text = debugText.text + "\nTOTAL CAPTURE TIME: " + (imagingTimeSum * 1.0 / runCount).ToString();

                        samplingAverage = samplingArray.Average();
                        sumOfSquaresOfDifferences = samplingArray.Select(val => (val - samplingAverage) * (val - samplingAverage)).Sum();
                        sd = Math.Sqrt(sumOfSquaresOfDifferences / testRuns);
                        debugText.text = debugText.text + "\nSAMPLING TIME Mean: " + samplingAverage.ToString() + ", Std:" + sd.ToString();

                        networkAverage = networkArray.Average();
                        sumOfSquaresOfDifferences = networkArray.Select(val => (val - networkAverage) * (val - networkAverage)).Sum();
                        sd = Math.Sqrt(sumOfSquaresOfDifferences / testRuns);
                        debugText.text = debugText.text + "\nNETWORK TIME Mean: " + networkAverage.ToString() + ", Std:" + sd.ToString();

                        composeAverage = composeArray.Average();
                        sumOfSquaresOfDifferences = composeArray.Select(val => (val - composeAverage) * (val - composeAverage)).Sum();
                        sd = Math.Sqrt(sumOfSquaresOfDifferences / testRuns);
                        debugText.text = debugText.text + "\nCOMPOSE TIME Mean: " + composeAverage.ToString() + ", Std:" + sd.ToString();

                        debugText.text = debugText.text + "\n" + runCount.ToString();
                        //debugText.text = debugText.text + "\nNumber of Samples Generated: " + (numberSampleGenerate * 1.0 / runCount).ToString();
                        //debugText.text = debugText.text + "\nNumber of Samples Composed: " + (numberSampleCompose * 1.0 / runCount).ToString();

                        Debug.Log("Number of runs: " + runCount);
                        Debug.Log("Total run time: " + runTimeSum * 1.0 / runCount);
                        Debug.Log("Texture Capture run time: " + texCapsumtime * 1.0 / runCount);
                        Debug.Log("Sampling run time: " + samplingTimeSum * 1.0 / runCount);
                        Debug.Log("Network run time: " + networkTimeSum * 1.0 / runCount);
                        Debug.Log("Compose run time: " + composeTimeSum * 1.0 / runCount);
                    } else {
                        debugText.text = "Run " + runCount.ToString();
                    }
                    #endregion
                    resultsReady = false;
                }
                runstarttime = CurrentTimeMillis();
                onImageAcquired();
                texCapstartime = CurrentTimeMillis();
            }


            if (networkScript == null && !running && backgroundtexture != null && NetworkManager.singleton.client != null && NetworkManager.singleton.client.connection.isReady) {
                localPlayer = NetworkManager.singleton.client.connection.playerControllers[0].gameObject;

                if (localPlayer != null) {
                    networkScript = localPlayer.GetComponent<NetworkScript>();
                    clientLOS.myID = localPlayer.GetHashCode();
                }
            }

        }
    }

    Thread thread2;
    int updateCount = 0;

    void onImageAcquired() {
        // Sizeof(CubemapFace idx) + Sizeof(Pixel intensity) + Sizeof(spatial idx on each face) Bytes
        sampleSize = sizeof(CubemapFace) + 4 + 2 * sizeof(float);
        maxSampleNetwork = 65500 / sampleSize; // Maximum number of samples that can be sent over network [S]
        ageLimit = clientLOS.AGE; // Age in milliseconds

        // Generate samples only if at least one client exist on network
        if (NetworkManager.singleton.client != null && NetworkManager.singleton.client.connection.isReady) {
            localPlayer = NetworkManager.singleton.client.connection.playerControllers[0].gameObject;

            if (localPlayer != null && networkScript != null) {
                List<Samples.singleSampleObject> localSamples = new List<Samples.singleSampleObject>();


                // TO Perform Radiance Sampling
                samplingstarttime = CurrentTimeMillis();
                generateRadianceSamples(localSamples);
                samplingendtime = CurrentTimeMillis();

                if (localSamples.Count == 0) { return; }

                networkstarttime = CurrentTimeMillis();
                selectSamplesToBroadcast(localSamples, clientLOS.myID); // Find num sample sent [which should be <= S]


                // Call Network Transfer Module
                if (updateCount == clientLOS.FRAME_INTERVAL_TO_SEND_SAMPLES) // If sending samples is on
                {
                    networkScript.CmdSendSamples(samplesToSend, clientLOS.myID);
                    //debugText.text = debugText.text + "\nSample sent on " + updateCount;
                    updateCount = 0;
                } else {
                    /* 
                     Do not send Samples
                     */
                    //debugText.text = debugText.text + "\nNo Sample sent on " + updateCount;
                    updateCount++;

                }
                networkendtime = CurrentTimeMillis();
                sampledist = 0; // defines neighborhood region of the samples to interpolate
                holdCubemap();

                if (clientLOS.THREADING) {
                    running = true;
                    thread2 = new Thread(onImageAcquiredTailThread);
                    thread2.Start();
                } else {
                    running = true;
                    onImageAcquiredTailThread();
                }
                return;
            }

        }
    }

    /*
     * Input parameter includes the u,v texture samples, this method
     * will collect the radiance samples from the pixel values corresponding to 
     * cubemap face.   
     */
    void generateRadianceSamples(List<Samples.singleSampleObject> samples) {
        // Initialize variables
        Ray ray;
        RaycastHit hit;
        Vector3 outDirection;

        float hitLength = 5.0f;
        float u, v; // Sample generation variables from respective cubemap.
        int screen_i, screen_j;
        int numHitRays = 0;

        List<float> deviations = new List<float>();

        // Radiance Sampling
        //BEGIN SPIRAL COORDS
        int x, y, dx, dy;
        int X = textureSize;
        int Y = textureSize;
        x = y = dx = 0;
        // initialized to -1 because first pixel delta corresponds to -1 to 0
        dy = -1;
        int t = textureSize;
        int maxI = t * t;       // max # pixels.
        int iii = 5;
        int jjj = 5;
        //debugText.text = "";
        //Vector3 prevCoord = Vector3.zero;

        // USING ARRAY OF PIXELS FROM SAMPLE SPACE TEXTURE.
        Color32 pixeltemp = pixels[(iii + (textureSize - jjj - 1) * textureSize)];
        for (int ii = 0; ii < maxI && numHitRays < clientLOS.LIM_SAMPLES_PER_LOCAL_LIST; ii = ii + 1) {
            // Check corresponding locations for spiral effect of pixel center.
            if ((x == y) || ((x < 0) && (x == -y)) || ((x > 0) && (x == 1 - y))) {
                t = dx;     // Accumulated texture size.
                dx = -dy;   // Increment the delta x
                dy = t;     // Y is the texture size 'tracker'
            }
            // Assign new pixel index of x & y.
            x += dx;
            y += dy;
            // I and J assigned respective rows/columns of 'WIP' texture.
            int i = x + textureSize / 2;
            int j = y + textureSize / 2 - 1;
            //END SPIRAL COORDS
            float sx = ((i - camwidth / 2) / calcratio) + mainCamPixWidth / 2;
            float sy = ((j - camheight / 2) / calcratio) + mainCamPixHeight / 2;
            screen_i = Mathf.RoundToInt(sx);
            screen_j = Mathf.RoundToInt(sy);
            float spx = colWebTexPos.x + screen_i - (((textureSize / 2 - camwidth / 2) / calcratio) + mainCamPixWidth / 2);
            float spy = colWebTexPos.y + screen_j - (((textureSize / 2 - camheight / 2) / calcratio) + mainCamPixHeight / 2);
            // Sampling space with respect to collider position (depth z)
            Vector3 newCoord = new Vector3(spx, spy, colWebTexPos.z);

            // Raycasting from screen pixel
            Vector3 rayCoordinateOnScreen = newCoord;
            // Shoot ray from sample space with respect to collider position.
            ray = dummyCam.ScreenPointToRay(rayCoordinateOnScreen);
            // Check if screen space sample pixel collides with reflective probe.
            bool coll = reflectorCollider.Raycast(ray, out hit, hitLength);
            if (coll) // If raycast collides with the collider
            {
                if (!((i + (textureSize - 1 - j) * textureSize) >= pixels.Length || (i + (textureSize - 1 - j) * textureSize) < 0)) {
                    Color32 pixel = pixels[(i + (textureSize - 1 - j) * textureSize)]; // Generating samples
                    // We have angle of incidence, here we calculate the angle from environment.
                    outDirection = Vector3.Reflect(ray.direction, hit.normal);
                    //hit.point
                    //text = text + hit.barycentricCoordinate.x + " " + hit.barycentricCoordinate.y + " " + hit.barycentricCoordinate.z + " " + outDirection.x + " " + outDirection.y +" " + outDirection.z + "\n";
                    // Point on cube is calculated by taking respective vector on iteration to scale into set of 
                    // weighted distance.
                    Vector3 pointOnCube = new Vector3(outDirection.x / Mathf.Max(Mathf.Abs(outDirection.x), Mathf.Abs(outDirection.y), Mathf.Abs(outDirection.z)),
                                                        outDirection.y / Mathf.Max(Mathf.Abs(outDirection.x), Mathf.Abs(outDirection.y), Mathf.Abs(outDirection.z)),
                                                        outDirection.z / Mathf.Max(Mathf.Abs(outDirection.x), Mathf.Abs(outDirection.y), Mathf.Abs(outDirection.z)));
                    dev = calcDeviations(-outDirection, transform.position - dummyCam.transform.position);
                    //if (dev > 120.0f)
                    //continue;
                    if (pointOnCube.x >= .99f) {
                        face = CubemapFace.PositiveX;
                        u = (-(pointOnCube.z) + 1) / 2;
                        v = ((pointOnCube.y) + 1) / 2;
                    } else if (pointOnCube.x <= -.99f) {
                        face = CubemapFace.NegativeX;
                        u = ((pointOnCube.z) + 1) / 2;
                        v = ((pointOnCube.y) + 1) / 2;
                    } else if (pointOnCube.y >= .99f) {
                        face = CubemapFace.PositiveY;
                        u = ((pointOnCube.x) + 1) / 2;
                        v = (-(pointOnCube.z) + 1) / 2;
                    } else if (pointOnCube.y <= -.99f) {
                        face = CubemapFace.NegativeY;
                        u = ((pointOnCube.x) + 1) / 2;
                        v = ((pointOnCube.z) + 1) / 2;
                    } else if (pointOnCube.z >= .99f) {
                        face = CubemapFace.PositiveZ;
                        u = ((pointOnCube.x) + 1) / 2;
                        v = ((pointOnCube.y) + 1) / 2;
                    } else if (pointOnCube.z <= -.99f) {
                        face = CubemapFace.NegativeZ;
                        u = (-(pointOnCube.x) + 1) / 2;
                        v = ((pointOnCube.y) + 1) / 2;
                    } else {
                        face = CubemapFace.PositiveX;
                        u = 0;
                        v = 0;
                    }
                    Samples.singleSampleObject sample = new Samples.singleSampleObject();
                    //text = text + dev + "\n";
                    ////pixel = Color.Lerp(pixel, Color.black, dev/150.0f);
                    //pixel.r = (byte)((1 - (dev / 180.0f)) * pixel.r);
                    //pixel.g = (byte)((1 - (dev / 180.0f)) * pixel.g);
                    //pixel.b = (byte)((1 - (dev / 180.0f)) * pixel.b);


                    pixel.a = (byte)((1 - ((dev / 180.0f) * (dev / 180.0f))) * 255);
                    //sample.alpha = (short)((1 - (dev / 180.0f)) * 255);
                    sample.face = face;
                    sample.u = u;
                    sample.v = v;
                    sample.pix = pixel;
                    samples.Add(sample);
                    numHitRays++;
                }

            }
        }
        if (debug && runCount <= testRuns) {
            //debugText.text = debugText.text + "\nNumber of Samples Generated: " + (numHitRays-1).ToString();
            numberSampleGenerate = numberSampleGenerate + (numHitRays);
        }
        //Debug.Log(text);
        //writeToFile("./rawData.txt", text, false);
        return;
    }

    void selectSamplesToBroadcast(List<Samples.singleSampleObject> localSampleList, int deviceID) {
        //maxSampleNetwork: maximum number of samples the network CAN send.
        //numLocalSamples: number of samples in the frame.
        //numSamplesToSend: number of samples chosen by the user/app

        int numLocalSamples = localSampleList.Count;
        numSamplesToSend = clientLOS.LIM_NUM_SAMPLES_TO_SEND;

        //Assign the  numSamplesToSend = min(maxSampleCount, maxSamples, numSamplesToSend);
        numSamplesToSend = maxSampleNetwork < numSamplesToSend ? maxSampleNetwork : numSamplesToSend;
        numSamplesToSend = numLocalSamples < numSamplesToSend ? numLocalSamples : numSamplesToSend;

        samplesToSend = new Samples.singleSampleObject[numSamplesToSend];
        Samples.singleSampleObject[] localSampleArray = new Samples.singleSampleObject[localSampleList.Count];
        for (int i = 0; i < numSamplesToSend; i++) {
            samplesToSend[i] = localSampleList[i];
        }

        // Populating the local sample's list with samples generated
        for (int i = 0; (i < localSampleList.Count) && (i < clientLOS.LIM_SAMPLES_PER_LOCAL_LIST); i++) {
            localSampleArray[i] = localSampleList[i];
        }

        //Add to own list if not receiving it from server 
        clientList.Add(localSampleArray);
        clientLOS.timestamps.Add(CurrentTimeMillis());
        clientLOS.deviceIDs.Add(deviceID);
        return;
    }

    void onImageAcquiredTailThread() {
        composestarttime = CurrentTimeMillis();
        long currentTime = CurrentTimeMillis();
        int count = clientList.Count;

        while (clientList.Count > ListOfSamples.maxListCount) {

            clientList.RemoveAt(0);
            clientTimestamps.RemoveAt(0);
            clientDevices.RemoveAt(0);
        }

        // Call Compose Cubemap Module
        composeCubemap(sampledist);


        // Purge Samples based on age of samples
        for (int numS = clientList.Count - 1; numS >= 0; numS--) {
            long ageOfList = currentTime - clientTimestamps[numS];

            if (ageOfList > ageLimit) {
                clientList.RemoveAt(numS);
                clientTimestamps.RemoveAt(numS);
                clientDevices.RemoveAt(numS);
            }
        }

        resultsReady = true;
        running = false;
        composeendtime = CurrentTimeMillis();
    }

    long idw1starttime, idw1endtime;
    long idw2starttime, idw2endtime, idw2SumTime;
    private void  composeCubemap(float distanceBetweenSamples) {
        //runstarttime = CurrentTimeMillis();

        int totalSamples = 0;

        Color pixel;
        float s, t;
        sumsList = new List<Color[]>();
        weightsList = new List<float[]>();
        idwstarttime = CurrentTimeMillis();
        idw1starttime = CurrentTimeMillis();
        for (int a = 0; a < 6; a++) {
            Color[] sums = new Color[faceSideLength * faceSideLength];
            float[] weights = new float[faceSideLength * faceSideLength];
            sumsList.Add(sums);
            weightsList.Add(weights);
        }

        for (int client_i = 0; client_i < clientList.Count; client_i++) {
            Samples.singleSampleObject[] samples = clientList[client_i];

            // Loop to find the neigborhood samples
            for (int sIdx = 0; sIdx < samples.Length; sIdx++) {
                // Find nearest pixel on face corresponding to the current sample
                s = samples[sIdx].u * faceSideLength;
                t = samples[sIdx].v * faceSideLength;
                int faceIndex = (int)(samples[sIdx].face);
                // Find neighborhood region for current sample
                for (int jNbd = Math.Max((int)(s - distanceBetweenSamples), 0); jNbd <= (int)(s + distanceBetweenSamples) && jNbd < faceSideLength; jNbd++) {
                    for (int kNbd = Math.Max((int)(t - distanceBetweenSamples), 0); kNbd <= (int)(t + distanceBetweenSamples) && kNbd < faceSideLength; kNbd++) {

                        float weight = 0.01f / (Mathf.Abs(jNbd - s) + Mathf.Abs(kNbd - t) + 0.01f);
                        //if (samples[sIdx].alpha >= 128)
                        //{
                        //    sumsList[faceIndex][jNbd * faceSideLength + kNbd] += (Color)(samples[sIdx].pix) * weight;
                        //    weightsList[faceIndex][jNbd * faceSideLength + kNbd] += weight;
                        //}

                        pixel = (Color)(samples[sIdx].pix) * weight;
                        //pixel.a = samples[sIdx].alpha;
                        sumsList[faceIndex][jNbd * faceSideLength + kNbd] += pixel;
                        weightsList[faceIndex][jNbd * faceSideLength + kNbd] += weight;
                    }
                }
                totalSamples++;
            }
        }
        idw1endtime = CurrentTimeMillis();
        calcPixelVals();

        if (debug && runCount <= testRuns) {
            //debugText.text = debugText.text + "\nNumber of Samples Composed: " + totalSamples.ToString();
            //numberSampleCompose = numberSampleCompose + totalSamples;
            //text = totalSamples.ToString() + "\n";
            //writeToFile("./AGE_" + clientLOS.AGE.ToString() + ".txt", text, true);
        }

        //debugText.text = debugText.text + "\nIDW RUNTTIME: " + (idwendtime - idwstarttime).ToString();
        //debugText.text = debugText.text + "\nIDW1 RUNTIME: " + (idw1endtime - idw1starttime).ToString();
        //debugText.text = debugText.text + "\nIDW2 RUNTIME: " + idw2SumTime.ToString();
        //debugText.text = debugText.text + "\nNN RUNTIME: " + holeFillSumTime.ToString();
        //debugText.text = debugText.text + "\nAVG FACE RUNTIME: " + (avgendtime - avgstarttime).ToString();


    }

    // Iterate through Cubemap pixel and set the pixel value based on average of its neighborhood sample pixels
    void calcPixelVals() {
        Color[] faceColorAvgs = new Color[6];
        int faceColorCount = 0;
        Color avgFaceColor = new Color(0f, 0f, 0f, 0f);
        holeFillSumTime = 0;
        idw2SumTime = 0;
        for (int a = 0; a < 6; a++) {
            idw2starttime = CurrentTimeMillis();
            Color[] faceColors = cubemapColors[a];
            Color pix = new Color(0f, 0f, 0f, 0f); // Converts samples to cubemap variables
            Color[] sums = sumsList[a];
            Color[] debugColorFace = { Color.red, Color.green, Color.yellow, Color.blue, Color.magenta, Color.cyan };
            float[] weightTotal = weightsList[a];
            int nIdx, q;
            float scale = 1.0f;

            for (int i = 0; i < sums.Length; i++) {
                sums[i] = sums[i] / weightTotal[i];
            }

            for (int i = 0; i < faceSideLength; i++) {
                for (int j = 0; j < faceSideLength; j++) {
                    nIdx = i * faceSideLength + j; // Find index of current pixel in neighborhood list
                    q = (faceSideLength - j - 1); // Invert y-direction
                    //faceColors[i + q * faceSideLength] = sums[nIdx] * intensity.value; // Assign pixel intensity to face pixel
                    faceColors[i + q * faceSideLength] = sums[nIdx] * intensity; // Assign pixel intensity to face pixel

                    //if (!debug) {
                    //    faceColors[i + q * faceSideLength] = sums[nIdx]; // Assign pixel intensity to face pixel
                    //} else {
                    //    Color faceColorD = Color.Lerp(debugColorFace[a], Color.black, ((i+q)*1.0f/(faceSideLength+faceSideLength)));
                    //    faceColors[i + q * faceSideLength] = faceColorD; // Assign pixel intensity to face pixel
                    //}
                }
            }
            idw2endtime = CurrentTimeMillis();
            holefillstarttime = CurrentTimeMillis();
            if (clientLOS.HOLEFILLING) {
                faceColorAvgs[a] = FillHoles(faceColors);
                //faceColorAvgs[a] = FillHoles2(faceColors);

                if (faceColorAvgs[a].a > 0.0f) {
                    faceColorCount++;
                    avgFaceColor += faceColorAvgs[a];
                }
            }
            holefillendtime = CurrentTimeMillis();
            holeFillSumTime = holeFillSumTime + (holefillendtime - holefillstarttime);
            idw2SumTime = idw2SumTime + (idw2endtime - idw2starttime);
        }

        idwendtime = CurrentTimeMillis();


        //avgstarttime = CurrentTimeMillis();
        //if (clientLOS.HOLEFILLING) {
        //    avgFaceColor /= faceColorCount;
        //    for (int a = 0; a < 6; a++) {
        //        for (int i = 0; i < faceSideLength * faceSideLength; i++) {
        //            if (faceColorAvgs[a].a <= 0.0f)
        //                cubemapColors[a][i] = avgFaceColor;
        //        }
        //    }
        //}
        avgendtime = CurrentTimeMillis();
    }

    Color FillHoles2(Color[] faceColors) {
        long fholestime = CurrentTimeMillis();
        //InvokeFillHoles.FHoles(faceColors);
        Debug.Log("Fholes" + (CurrentTimeMillis() - fholestime));
        holeX.Clear();
        holeY.Clear();

        Color avgFaceColor = new Color(0f, 0f, 0f, 0f);

        int pixCount = 0;
        //Compute average and find holes
        for (int i = 0; i < faceSideLength; i++) {
            for (int j = 0; j < faceSideLength; j++) {
                if (faceColors[i + j * faceSideLength].a > 0.0f) {
                    avgFaceColor += faceColors[i + j * faceSideLength];
                    pixCount++;
                } else {
                    holeX.Add(i);
                    holeY.Add(j);
                }
            }
        }
        if (pixCount == 0) {
            return avgFaceColor;
        }

        avgFaceColor = avgFaceColor / pixCount;
        for (int ii = holeX.Count - 1; ii >= 0; ii--) //count backwards so removal doesn't affect indexing
       {
            int x = holeX[ii];
            int y = holeY[ii];
            faceColors[x + y * faceSideLength] = avgFaceColor;
        }


        return avgFaceColor;
    }

    Color FillHoles(Color[] faceColors) {

        if (buff2 == null || buff2.Length != faceColors.Length) {
            buff2 = new Color[faceColors.Length];
        }

        holeX.Clear();
        holeY.Clear();

        Color avgFaceColor = new Color(0f, 0f, 0f, 0f);

        int pixCount = 0;
        //Compute average and find holes
        for (int i = 0; i < faceSideLength; i++) {
            for (int j = 0; j < faceSideLength; j++) {
                if (faceColors[i + j * faceSideLength].a > 0.0f) {
                    avgFaceColor += faceColors[i + j * faceSideLength];
                    pixCount++;
                } else {
                    holeX.Add(i);
                    holeY.Add(j);
                }
            }
        }
        if (pixCount == 0) {
            return avgFaceColor;
        }

        avgFaceColor = avgFaceColor / pixCount;

        faceColors.CopyTo(buff2, 0);

        //holefilling pass
        for (int holeRun = 0; holeRun < 12 && holeX.Count > 0; holeRun++) {
            int[] xdiff = { -1, 0, 1, -1, 1, -1, 0, 1 };
            int[] ydiff = { -1, -1, -1, 0, 0, 1, 1, 1 };

            for (int ii = holeX.Count - 1; ii >= 0; ii--) //count backwards so removal doesn't affect indexing
            {
                int x = holeX[ii];
                int y = holeY[ii];

                //looks for all neighbors
                for (int diffindex = 0; diffindex < 8; diffindex++) {
                    int i = x + xdiff[diffindex];
                    int j = y + ydiff[diffindex];
                    i = i < faceSideLength ? i : faceSideLength - 1; //make sure it's in bounds < faceSideLength
                    j = j < faceSideLength ? j : faceSideLength - 1; //make sure it's in bounds < faceSideLength
                    i = i >= 0 ? i : 0; //make sure it's in bounds >= 0
                    j = j >= 0 ? j : 0; //make sure it's in bounds >= 0

                    if (faceColors[i + j * faceSideLength].a > 0.0f) {
                        buff2[x + y * faceSideLength] = faceColors[i + j * faceSideLength];
                        holeX.RemoveAt(ii);
                        holeY.RemoveAt(ii);
                        diffindex = 9;//forces inner for loop exit
                    }
                }
            }
            buff2.CopyTo(faceColors, 0);
        }
        //copy into faceColors if necessary
        for (int ii = holeX.Count - 1; ii >= 0; ii--) //count backwards so removal doesn't affect indexing
        {
            int x = holeX[ii];
            int y = holeY[ii];
            faceColors[x + y * faceSideLength] = avgFaceColor;
        }


        return avgFaceColor;
    }

    //#region POLICY_METHODS
    //    public void updateVars(int update_interval, int lim_samples_per_list,
    //        int skipping, int age, bool holefilling, bool threading, float smoothing) {
    //        clientLOS.UPDATE_INTERVAL = update_interval;
    //        clientLOS.LIM_SAMPLES_PER_LOCAL_LIST = lim_samples_per_list;
    //        clientLOS.LIM_NUM_LISTS = clientLOS.LIM_TOTAL / lim_samples_per_list;
    //        clientLOS.SKIPPING = skipping;
    //        clientLOS.AGE = age;
    //        clientLOS.HOLEFILLING = holefilling;
    //        clientLOS.THREADING = threading;
    //        clientLOS.SMOOTHING = smoothing;

    //        for (int numS = clientList.Count - 1; numS >= 0; numS--) {
    //            clientList.RemoveAt(numS);
    //            clientTimestamps.RemoveAt(numS);
    //            clientDevices.RemoveAt(numS);
    //        }
    //    }

    //    // BANDWIDTH : 4000 samples/sec. = 64 kB/sec.
    //    // Low Update Rate | Max Resolution - 1 FPS, 1000 msec. freshness, 4000 samples, 0 Skipping
    //    public void updateButton1() {
    //        updateVars(100, 4000, 0, 10, true, false, 32);
    //        //screenRecordText.text = "1";
    //    }
    //    // High Update Rate | Max Resolution - 2 FPS, 1000 msec. freshness, 2000 samples, 0 skipping
    //    public void updateButton2() {
    //        updateVars(500, 2000, 1, 10, true, false, 16);
    //        //screenRecordText.text = "2";
    //    }
    //    // Low Update Rate | Max Coverage - 1 FPS, 500 msec. freshness, 4000 samples, 1 skipping 
    //    public void updateButton3() {
    //        updateVars(1000, 1000, 0, 10, true, false, 16);
    //        //screenRecordText.text = "3";
    //    }
    //    // High Update Rate | Max Coverage - 5 FPS, 500 msec. freshness, 800 samples, 1 skipping
    //    public void updateButton4() {
    //        updateVars(200, 4000, 0, 10, false, false, 16);
    //        //screenRecordText.text = "4";
    //    }
    //    // Low Update Rate | Max Dynamicity - 5 FPS, 25 msec. freshness, 800 samples, 1 skipping
    //    public void updateButton5() {
    //        updateVars(200, 2000, 1, 10, false, false, 16);
    //        //screenRecordText.text = "5";
    //    }
    //    // High Update Rate | Max Dynamicity - 10 FPS, 25 msec. freshness, 400 samples, 1 skipping
    //    public void updateButton6() {
    //        updateVars(1000, 1000, 0, 10, false, false, 32);
    //        //screenRecordText.text = "6";
    //    }

    //#endregion

    #region PRIVATE_METHODS

    //void OnVuforiaStarted() {
    //    CameraDevice.Instance.SetFocusMode(CameraDevice.FocusMode.FOCUS_MODE_CONTINUOUSAUTO);
    //    mFormatRegistered = (CameraDevice.Instance.SetFrameFormat(mPixelFormat, true));
    //}

    void OnPause(bool paused) {
        //if (paused) {
        //    CameraDevice.Instance.SetFrameFormat(mPixelFormat, false);
        //    mFormatRegistered = false;
        //} else {
        //    CameraDevice.Instance.SetFocusMode(CameraDevice.FocusMode.FOCUS_MODE_CONTINUOUSAUTO);
        //    mFormatRegistered = (CameraDevice.Instance.SetFrameFormat(mPixelFormat, true));
        //}
    }

    #endregion //PRIVATE_METHODS

    #region CUSTOM_METHODS
    // Find distance between neighborhood samples
    //float getDistanceBetweenSamples() { 
    //    //Debug.Log("Distance: " + (transform.position - mainCam.transform.position).magnitude);
    //    return (transform.position - mainCam.transform.position).magnitude * clientLOS.SMOOTHING / faceSideLength;
    //}


    // Apply cubemap texture to skyMat material
    void applyCubemap() {
        setCubemap();
        cubemap.Apply();        // Save changes from Setting Cubemap faces to our stored/sampled cubemap.
        skyMat.SetTexture("_Tex", cubemap);     // Assign lighting cubemap to skybox so objects adhere to lighting.
    }

    // Clear cubemap texture to skyMat material
    public void clearCubemap() {
        setCubemap();
        cubemap.Apply();
        skyMat.SetTexture("_Tex", null);
    }

    //void OnGUI()
    //{
    //    //if (GUI.Button(new Rect(Camera.main.pixelWidth / 2 - Camera.main.pixelWidth / 20, Camera.main.pixelHeight - Camera.main.pixelHeight / 10, Camera.main.pixelWidth / 10, Camera.main.pixelHeight / 20), "Capture Screenshot")) {

    //    //    writeFaces();

    //    //    //negZFace = faces[5].EncodeToEXR();
    //    //    //File.WriteAllBytes(Application.persistentDataPath + "/negZFaceEXR_" + (screenShotIdx - 1) + ".exr", negZFace);
    //    //}
    //    //if (GUI.Button(new Rect(Camera.main.pixelWidth / 2 - Camera.main.pixelWidth / 20, Camera.main.pixelHeight / 10, Camera.main.pixelWidth / 10, Camera.main.pixelHeight / 20), "Reset runs")) {
    //    //    writeDebug = true;
    //    //    run1 = 0;
    //    //    run2 = 0;
    //    //    Debug.Log("WRITE_DEBUG : " + writeDebug);
    //    //}
    //}

    void writeFaces() {

        ScreenCapture.CaptureScreenshot("Screenshot_" + screenShotIdx + ".png");
        screenShotIdx++;
        //screenRecordText.text = screenShotIdx.ToString();


        faces[0].SetPixels(cubemapColors[0]);
        faces[0].Apply();
        faces[0] = FlipTexture(faces[0], true);
        byte[] posXFace = faces[0].EncodeToJPG(100);
        File.WriteAllBytes(Application.dataPath + "/../posXFace_" + (screenShotIdx - 1) + ".jpg", posXFace);


        faces[1].SetPixels(cubemapColors[1]);
        faces[1].Apply();
        faces[1] = FlipTexture(faces[1], true);
        byte[] negXFace = faces[1].EncodeToJPG(100);
        File.WriteAllBytes(Application.dataPath + "/../negXFace_" + (screenShotIdx - 1) + ".jpg", negXFace);


        faces[2].SetPixels(cubemapColors[2]);
        faces[2].Apply();
        faces[2] = FlipTexture(faces[2], true);
        byte[] posYFace = faces[2].EncodeToJPG(100);
        File.WriteAllBytes(Application.dataPath + "/../posYFace_" + (screenShotIdx - 1) + ".jpg", posYFace);


        faces[3].SetPixels(cubemapColors[3]);
        faces[3].Apply();
        faces[3] = FlipTexture(faces[3], true);
        byte[] negYFace = faces[3].EncodeToJPG(100);
        File.WriteAllBytes(Application.dataPath + "/../negYFace_" + (screenShotIdx - 1) + ".jpg", negYFace);


        faces[4].SetPixels(cubemapColors[4]);
        faces[4].Apply();
        faces[4] = FlipTexture(faces[4], true);
        byte[] posZFace = faces[4].EncodeToJPG(100);
        File.WriteAllBytes(Application.dataPath + "/../posZFace_" + (screenShotIdx - 1) + ".jpg", posZFace);


        faces[5].SetPixels(cubemapColors[5]);
        faces[5].Apply();
        faces[5] = FlipTexture(faces[5], true);
        byte[] negZFace = faces[5].EncodeToJPG(100);
        File.WriteAllBytes(Application.dataPath + "/../negZFace_" + (screenShotIdx - 1) + ".jpg", negZFace);
    }



    private void holdCubemap() {
        cubemapColors.Clear();
        for (int a = 0; a < 6; a++) {
            Color[] col = cubemap.GetPixels(faceArray[a], 0); // Get previous face colors
            cubemapColors.Add(col);
        }
    }
    private void setCubemap() {
        for (int a = 0; a < 6; a++) {
            cubemap.SetPixels(cubemapColors[a], faceArray[a]);
        }
    }

    // Function to display cubemap on the screen
    private void displayCubemap() {
        // Image Positive X
        faces[0].SetPixels(cubemapColors[0]);
        faces[0].Apply();
        imageFacePosX.texture = faces[0];

        // Image Negative X
        faces[1].SetPixels(cubemapColors[1]);
        faces[1].Apply();
        imageFaceNegX.texture = faces[1];

        // Image Positive Y
        faces[2].SetPixels(cubemapColors[2]);
        faces[2].Apply();
        imageFacePosY.texture = faces[2];

        // Image Negative Y
        faces[3].SetPixels(cubemapColors[3]);
        faces[3].Apply();
        imageFaceNegY.texture = faces[3];

        // Image Positive Z
        faces[4].SetPixels(cubemapColors[4]);
        faces[4].Apply();
        imageFacePosZ.texture = faces[4];

        // Image Negative Z
        faces[5].SetPixels(cubemapColors[5]);
        faces[5].Apply();
        imageFaceNegZ.texture = faces[5];

    }

    Texture2D FlipTexture(Texture2D original, bool upSideDown = true) {

        Texture2D flipped = new Texture2D(original.width, original.height);

        int xN = original.width;
        int yN = original.height;


        for (int i = 0; i < xN; i++) {
            for (int j = 0; j < yN; j++) {
                if (upSideDown) {
                    flipped.SetPixel(j, xN - i - 1, original.GetPixel(j, i));
                } else {
                    flipped.SetPixel(xN - i - 1, j, original.GetPixel(i, j));
                }
            }
        }
        flipped.Apply();

        return flipped;
    }

    float calcDeviations(Vector3 dir1, Vector3 dir2) {

        return Mathf.Rad2Deg * Mathf.Acos((dir1.x * dir2.x + dir1.y * dir2.y + dir1.z * dir2.z) / (dir1.magnitude * dir2.magnitude));

    }


    void writeToFile(string path, string text, bool app) {
        //Write some text to the test.txt file
        StreamWriter writer = new StreamWriter(path, append: app);
        writer.WriteLine(text);
        writer.Close();
    }

    #endregion // CUSTOM Methods

    public void killTailThreadIfRunning() {
        if (running) {
            thread2.Abort();
        }
    }

}
#endif