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
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class NetworkScript : NetworkBehaviour {
    ListOfSamples LOS;
    public bool isAtStartup = true;
    NetworkClient myClient;
    //public InputField addressText;
    //public Text serverAddress;
    private void Start()
    {
        GameObject LOS_object = GameObject.Find("ListOfSamples");
        LOS = LOS_object.GetComponent<ListOfSamples>();
        //addressText.text = "127.0.0.1";
    }

    [Command]
    public void CmdSendSamples(Samples.singleSampleObject [] incomingSamplesArray, int incomingSampleID)
    {
        if (!LOS.connections.Contains(connectionToClient)){
            LOS.connections.Add(connectionToClient);
        }

        foreach (NetworkConnection connection in LOS.connections){
            if (connection != connectionToClient){
                TargetRpcSendToOne(connection, incomingSamplesArray, incomingSampleID);
            }
        }
    }


    [TargetRpc]
    public void TargetRpcSendToOne(NetworkConnection target, Samples.singleSampleObject[] incomingSamplesArray, int incomingSampleID)
    {
        while (LOS.listOfLOS.Count + 1 > ListOfSamples.maxListCount)
        {
            LOS.listOfLOS.RemoveAt(0);
            LOS.deviceIDs.RemoveAt(0);
            LOS.timestamps.RemoveAt(0);
        }

        if (incomingSampleID != LOS.myID)
        {
            LOS.listOfLOS.Add(incomingSamplesArray);
            LOS.deviceIDs.Add(incomingSampleID);
            LOS.timestamps.Add(CurrentTimeMillis());
        }
    }

    [ClientRpc]
    public void RpcSendToAll(Samples.singleSampleObject[] incomingSamplesArray, int incomingSampleID) {
        while (LOS.listOfLOS.Count + 1 > ListOfSamples.maxListCount) {
            LOS.listOfLOS.RemoveAt(0);
            LOS.deviceIDs.RemoveAt(0);
            LOS.timestamps.RemoveAt(0);
        }

        if (incomingSampleID != LOS.myID) {
            LOS.listOfLOS.Add(incomingSamplesArray);
            LOS.deviceIDs.Add(incomingSampleID);
            LOS.timestamps.Add(CurrentTimeMillis());
        }
    }

    private static readonly DateTime Jan1st1970 = new DateTime
    (1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

    public static long CurrentTimeMillis()
    {
        return (long)(DateTime.UtcNow - Jan1st1970).TotalMilliseconds;
    }

    public void makeHost() {
        NetworkManager.singleton.StartHost();
    }

    public void makeServer() {
        NetworkManager.singleton.StartServer();
    }

    public void makeClient() {
        //NetworkManager.singleton.networkAddress = addressText.text;
        //NetworkManager.singleton.networkPort = int.Parse("8888");
        NetworkManager.singleton.StartClient();
    }

    public void stopHost() {
        NetworkManager.singleton.StopHost();
    }

    public void stopServer() {
        NetworkManager.singleton.StopServer();
    }

    public void stopClient() {
        NetworkManager.singleton.StopClient();
    }
}
