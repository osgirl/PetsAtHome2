using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GlobalSettings : MonoBehaviour
{
	public static float worldColourScale = 512f;
	public Material worldHSV;
	void Start(){
		worldHSV.SetFloat("_ScaleFloat", worldColourScale);
	}
}
