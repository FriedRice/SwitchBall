﻿using UnityEngine;
using System.Collections;

public class Score : MonoBehaviour {

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
        this.GetComponent<TextMesh>().text = Mathf.RoundToInt(this.GetComponentInParent<PlayerManager>().Score).ToString();
	}
}
