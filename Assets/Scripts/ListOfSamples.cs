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

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
public class ListOfSamples : MonoBehaviour {
    public List<Samples.singleSampleObject []> listOfLOS;
    public List<NetworkConnection> connections = new List<NetworkConnection>();

    public List<int> deviceIDs;
    public List<long> timestamps;
    public int myID;
    
    public static int maxListCount = 10;

    int currentSampleListCount = 0;
    public float currentTime;
    //public float UPDATE_INTERVAL = 100;//throttles to collect new samples when this has passed
    //public int LIM_TOTAL = 80000;
    public int LIM_SAMPLES_PER_LOCAL_LIST = 4000; // Coverage
    //public int LIM_NUM_LISTS = 40; // Forces Update Rate
    //public int SKIPPING = 0; // Coverage
    public int AGE = 100; // Freshness in milliseconds
    public int RESOLUTION = 128; // RESOLUTION
    public bool HOLEFILLING = true;
    public bool THREADING = false;
    public int LIM_NUM_SAMPLES_TO_SEND;
    public int FRAME_INTERVAL_TO_SEND_SAMPLES = 0;
    //public float SMOOTHING = 16;

    public void initListOfSamples() {
        listOfLOS = new List<Samples.singleSampleObject[]>();
        deviceIDs = new List<int>();
        timestamps = new List<long>();
        LIM_NUM_SAMPLES_TO_SEND = LIM_SAMPLES_PER_LOCAL_LIST;
    }

	// Return number of sample list present
	public int returnNumberOfSampleList () {
        currentSampleListCount = listOfLOS.Count;
        return currentSampleListCount;
	}

}
