using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using System.IO;
using UnityEngine.Rendering;
using System.Threading;
using UnityEngine.SceneManagement;
using UnityEngine.XR.iOS;
using TMPro;



public class GLEAMBehaviour : MonoBehaviour {
    public RawImage sampleViewer; // displays the sample on the canvas
    public RawImage sampleIndicator; // red UI image that indicates where the sample is coming from
    protected Camera sampleCamera;
    public TextMeshProUGUI debugDisplay; // debug display

    //public Camera dummyCam, dummyCamHalf;

    public GameObject probe; // reference to the reflector probe
    private Collider probeCollider;
    public Vector3 probeOffset; // offset of the probe relative to the image anchor
    Vector3 probeScreenPosition; // screen position of the probe
    Vector3 imageAnchorPosition; // position of the image target

    public int probeSampleSize; // size of the square region to sample (pixels)
    private Texture2D sampleTexture;

    public Material skyMaterial; // Material storing cubemap texture

    public Texture2D[] faces = new Texture2D[6];
    public int LIM_SAMPLES_PER_LOCAL_LIST = 4000;
    protected float deviation;
    CubemapFace[] faceArray = { CubemapFace.PositiveX, CubemapFace.NegativeX, CubemapFace.PositiveY, CubemapFace.NegativeY, CubemapFace.PositiveZ, CubemapFace.NegativeZ };
    List<Color[]> cubemapColors = new List<Color[]>();
    Color[] buff2;
    List<int> holeX = new List<int>();
    List<int> holeY = new List<int>();
    public List<int>[] neighborhood;
    public RawImage imageFacePosX,
                    imageFacePosY,
                    imageFacePosZ,
                    imageFaceNegX,
                    imageFaceNegY,
                    imageFaceNegZ;

    int samplePosX, samplePosY;

    public List<Samples.singleSampleObject[]> clientList = new List<Samples.singleSampleObject[]>();
    List<Color[]> sumsList = new List<Color[]>();
    List<float[]> weightsList = new List<float[]>();

    Color[] pixels;
    Cubemap cubemap;
    CubemapFace face;
    Thread thread;
    public static int faceSideLength;

    private int maxSampleNetwork;
    float ageLimit;

    public ListOfSamples clientLOS;
    GameObject clientListOfSamples;
    GameObject localPlayer;

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

    private static readonly DateTime Jan1st1970 = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

    private NetworkScript networkScript = null;
    long gameStart;

    public int[] sumsR;
    public int[] sumsG;
    public int[] sumsB;

    private float smoothingFactor = 10.0f;

    Samples.singleSampleObject[] samplesToSend;
    private int sampleSize;
    int screenShotIdx = 0;

    private bool resultsReady = false;
    bool running = false;

    byte[] imagepixels;
    int imagewidth;
    int camwidth;
    int camheight;

    int age;
    private float sampledist;
    private int mainCamPixWidth;
    private int mainCamPixHeight;
    private Texture2D snap;
    private Texture2D texture2D;

    string textUpdatevsSamples = "";
    string textTailsvsSamples = "";
    int runCount = 0;
    public int testRuns = 100;

    long[] texCapArray;
    double texCapAverage;
    double sumOfSquaresOfDifferences, sd;

    int numberSampleGenerate = 0;
    int numberSampleCompose = 0;

    public bool debug = false;

    float dev;
    Vector3 colWebTexPos;
    private List<long> clientTimestamps;
    private List<int> clientDevices;
    private RenderTexture renderTexture;

    long idwStartTime, holeFillStartTime;
    int numSamplesToSend;
    float calcratio;

    public float intensity;// = 1.0f;

    bool generateOnce;

    List<Samples.singleSampleObject> localSamples;

    public static long CurrentTimeMillis() {
        return (long)(DateTime.UtcNow - Jan1st1970).TotalMilliseconds;
    }

    void Start() {
        // Assign reference to self.
        if (probe == null)
            probe = gameObject;
        if (probeCollider == null)
            probeCollider = probe.GetComponent<Collider>();
        // size of the sampled square of pixels
        probeSampleSize = 128;
        running = resultsReady = false;
        // Initialize the camera
        sampleTexture = new Texture2D(probeSampleSize, probeSampleSize, TextureFormat.RGBA32, false);
        clientLOS.initListOfSamples();    
        /* 
         * Assign resolution for each face of cubemap, 
         * this resolution will be equivalent to the samples stored.
         */
        faceSideLength = probeSampleSize;
        // Create neighborhood sample list for all faces of cube.
        // A cube is length^2, faceSideLength^2 will give us all faces as a list reference.
        neighborhood = new List<int>[faceSideLength * faceSideLength];
        clientList = clientLOS.listOfLOS;
        clientTimestamps = clientLOS.timestamps;
        clientDevices = clientLOS.deviceIDs;
        cubemap = new Cubemap(faceSideLength, TextureFormat.RGB24, false);
        for (int i = 0; i < 6; i++) {
            // Each face is stored as a texture for populating in GUI.
            faces[i] = new Texture2D(faceSideLength, faceSideLength);
        }
        localSamples = new List<Samples.singleSampleObject>();
    }

    float CalcDeviations(Vector3 dir1, Vector3 dir2) {
        return Mathf.Rad2Deg * Mathf.Acos((dir1.x * dir2.x + dir1.y * dir2.y + dir1.z * dir2.z) / (dir1.magnitude * dir2.magnitude));
    }

    // Update the debug display
    public void UpdateDebugDisplay(string debugString) {
        debugDisplay.text = debugString;
    }

    public void GLEAM_Update(Camera sampleCam, RenderTexture cameraImage) {
        if (running)
            return;
        sampleCamera = sampleCam;
        probeScreenPosition = sampleCamera.WorldToScreenPoint(probe.transform.position);
        string debugString = "";
        debugString += "\nProbe Screen X: " + probeScreenPosition.x + "\nProbe Screen Y: " + probeScreenPosition.y;

        // store the active render texture and replace it with the Y component
        RenderTexture activeRT = RenderTexture.active;
        RenderTexture.active = cameraImage;

        // calculates the position to do the ReadPixels call from
        samplePosX = ((int)probeScreenPosition.x - probeSampleSize / 2);
        samplePosY = ((int)probeScreenPosition.y - probeSampleSize / 2);
        sampleTexture.ReadPixels(new Rect(samplePosX, samplePosY, probeSampleSize, probeSampleSize), 0, 0);

        // calculates the position to place the sample indicator on the canvas
        float sampleIndicatorPosX = probeScreenPosition.x - probeSampleSize / 2;
        float sampleIndicatorPosY = probeScreenPosition.y + probeSampleSize / 2;
        sampleIndicator.transform.position = new Vector3(sampleIndicatorPosX, sampleIndicatorPosY, 0.0f);

        // applies the ReadPixels call to the sampleTexture, and displays it in the sampleViewer canvas image
        sampleTexture.Apply();

        if(sampleViewer != null)
            sampleViewer.texture = sampleTexture;

        if(sampleIndicator != null)
            sampleIndicator.texture = sampleTexture;

        pixels = sampleTexture.GetPixels();
        // revert the active render texture to its original value
        RenderTexture.active = activeRT;
        //UpdateDebugDisplay(debugString);

        if (resultsReady) {
            applyCubemap();

            if(imageFacePosX != null)
                displayCubemap();

            resultsReady = false;
        }
        onImageAcquired();

        if (networkScript == null && !running && NetworkManager.singleton.client != null && NetworkManager.singleton.client.connection.isReady) {
                localPlayer = NetworkManager.singleton.client.connection.playerControllers[0].gameObject;

                if (localPlayer != null) {
                    networkScript = localPlayer.GetComponent<NetworkScript>();
                    clientLOS.myID = localPlayer.GetHashCode();
                }
            }
        // TODO: Check for thread execution to prevent read/write on data structures.
        //running = true;
        //OnImageAcquiredTailThread();
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
                localSamples.Clear();

                // TO Perform Radiance Sampling
                samplingstarttime = CurrentTimeMillis();
                // if(!generateOnce)
                // {
                //     generateRadianceSamples(localSamples);
                //     generateOnce = true;
                // }

                generateRadianceSamples(localSamples);

                if (localSamples.Count == 0) { return; }

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
        int X = probeSampleSize;
        int Y = probeSampleSize;
        x = y = dx = 0;
        // initialized to -1 because first pixel delta corresponds to -1 to 0
        dy = -1;
        int t = probeSampleSize;
        int maxI = t * t;       // max # pixels.

        int counter = 0;
        //Color32 pixeltemp = pixels[(iii + (probeSampleSize - jjj - 1) * probeSampleSize)];
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
            int i = x + probeSampleSize / 2;
            int j = y + probeSampleSize / 2 - 1;
            //END SPIRAL COORDS
            // float spx = colWebTexPos.x + screen_i - (((probeSampleSize / 2 - camwidth / 2) / calcratio) + mainCamPixWidth / 2);
            // float spy = colWebTexPos.y + screen_j - (((probeSampleSize / 2 - camheight / 2) / calcratio) + mainCamPixHeight / 2);
            float spx = x + samplePosX + probeSampleSize / 2;
            float spy = samplePosY + probeSampleSize / 2 - y;
            // Sampling space with respect to collider position (depth z)
            Vector3 newCoord = new Vector3(spx, spy, colWebTexPos.z);

            // Raycasting from screen pixel
            Vector3 rayCoordinateOnScreen = newCoord;
            // Shoot ray from sample space with respect to collider position.
            ray = sampleCamera.ScreenPointToRay(rayCoordinateOnScreen);
            // Check if screen space sample pixel collides with reflective probe.
            bool coll = probeCollider.Raycast(ray, out hit, hitLength);
            if (coll) // If raycast collides with the collider
            {
                if ((i + (probeSampleSize - 1 - j) * probeSampleSize) >= pixels.Length || (i + (probeSampleSize - 1 - j) * probeSampleSize) < 0) {

                } else {
                    //Color32 pixel = new Color32((byte)(i * 255.0f / probeSampleSize), (byte)(j * 255.0f / probeSampleSize), 0, 255);//
                    Color32 pixel = pixels[(i + (probeSampleSize - 1 - j) * probeSampleSize)]; // Generating samples
                    // We have angle of incidence, here we calculate the angle from environment.
                    outDirection = Vector3.Reflect(ray.direction, hit.normal);
                   
                    // Point on cube is calculated by taking respective vector on iteration to scale into set of 
                    // weighted distance.
                    Vector3 pointOnCube = new Vector3(outDirection.x / Mathf.Max(Mathf.Abs(outDirection.x), Mathf.Abs(outDirection.y), Mathf.Abs(outDirection.z)),
                    outDirection.y / Mathf.Max(Mathf.Abs(outDirection.x), Mathf.Abs(outDirection.y), Mathf.Abs(outDirection.z)),
                    outDirection.z / Mathf.Max(Mathf.Abs(outDirection.x), Mathf.Abs(outDirection.y), Mathf.Abs(outDirection.z)));
                    dev = calcDeviations(-outDirection, transform.position - sampleCamera.transform.position);
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

                    //pixel = new Color32();
                    pixel.a = (byte)((1 - ((dev / 180.0f) * (dev / 180.0f))) * 255);
                    //pixel.r = (byte)(i * 255.0f / probeSampleSize);
                    //pixel.g = (byte)(j * 255.0f / probeSampleSize);
                    //pixel.b = 0;
                    //sample.alpha = (short)((1 - (dev / 180.0f)) * 255);
                    sample.face = face;
                    sample.u = u;
                    sample.v = v;
                    sample.pix = pixel;
                    samples.Add(sample);
                    numHitRays++;
                    counter++;
                }
            }
        }
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
    private void composeCubemap(float distanceBetweenSamples) {

        int totalSamples = 0;

        Color pixel;
        float s, t;
        sumsList.Clear();
        weightsList.Clear();
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
    }

    // Iterate through Cubemap pixel and set the pixel value based on average of its neighborhood sample pixels
    void calcPixelVals() {

        Color[] faceColorAvgs = new Color[6];
        int faceColorCount = 0;
        Color avgFaceColor = new Color(0f, 0f, 0f, 0f);
        for (int a = 0; a < 6; a++) {
            Color[] faceColors = cubemapColors[a];
            Color pix = new Color(0f, 0f, 0f, 0f); // Converts samples to cubemap variables
            Color[] sums = sumsList[a];
            Color[] debugColorFace = { Color.red, Color.green, Color.yellow, Color.blue, Color.magenta, Color.cyan };
            float[] weightTotal = weightsList[a];
            int nIdx, q;

            for (int i = 0; i < sums.Length; i++) {
                sums[i] = sums[i] / weightTotal[i];
            }
            for (int i = 0; i < faceSideLength; i++) {
                for (int j = 0; j < faceSideLength; j++) {
                    nIdx = i * faceSideLength + j; // Find index of current pixel in neighborhood list
                    q = (faceSideLength - j - 1); // Invert y-direction
                    //faceColors[i + q * faceSideLength] = sums[nIdx] * intensity.value; // Assign pixel intensity to face pixel
                    faceColors[i + q * faceSideLength] = sums[nIdx] * intensity; // Assign pixel intensity to face pixel

                }
            }
            if (clientLOS.HOLEFILLING) {
                faceColorAvgs[a] = FillHoles(faceColors);
                //faceColorAvgs[a] = FillHoles2(faceColors);

                if (faceColorAvgs[a].a > 0.0f) {
                    faceColorCount++;
                    avgFaceColor += faceColorAvgs[a];
                }
            }
        }
    }

    Color FillHoles2(Color[] faceColors) {
        long fholestime = CurrentTimeMillis();
        //InvokeFillHoles.FHoles(faceColors);
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

    void OnPause(bool paused) {
        // if (paused) {
        //     CameraDevice.Instance.SetFrameFormat(mPixelFormat, false);
        //     mFormatRegistered = false;
        // } else {
        //     CameraDevice.Instance.SetFocusMode(CameraDevice.FocusMode.FOCUS_MODE_CONTINUOUSAUTO);
        //     mFormatRegistered = (CameraDevice.Instance.SetFrameFormat(mPixelFormat, true));
        // }
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
        cubemap.Apply(); // Save changes from Setting Cubemap faces to our stored/sampled cubemap.
        skyMaterial.SetTexture("_Tex", cubemap); // Assign lighting cubemap to skybox so objects adhere to lighting.
    }

    // Clear cubemap texture to skyMat material
    public void clearCubemap() {
        setCubemap();
        cubemap.Apply();
        skyMaterial.SetTexture("_Tex", null);
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
