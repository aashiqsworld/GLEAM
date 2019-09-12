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

public class Rotation : MonoBehaviour {

	
	// Update is called once per frame
	void Update () {
		this.transform.Rotate(Vector3.up);
    }
}
