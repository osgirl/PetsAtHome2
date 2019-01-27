using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GlobalSettings : MonoBehaviour
{
	public static float worldColourScale, worldSize;
	[SerializeField]
	private float whiteRatio;
	public Material worldHSV;
	public Transform land;
	void Start(){
		
		worldColourScale = land.localScale.x * whiteRatio / 2f;
		worldSize = land.localScale.x;
		worldHSV.SetFloat("_ScaleFloat", worldColourScale);
	}
}
