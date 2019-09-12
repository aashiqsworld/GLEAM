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

using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System;


public class Samples : IComparable<Samples>
{
    public struct singleSampleObject
    {
        public float u, v;
        public CubemapFace face;
        public Color32 pix;
        //public short alpha;
    };

    public int CompareTo(Samples other)
    {
        if(other == null){
            return 0;
        }
        return 1;
    }
}