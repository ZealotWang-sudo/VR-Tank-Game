using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestroyTime : MonoBehaviour {

	public float timer;
	// Use this for initialization
	void Start () 
	{
		StartCoroutine(DestroyTimer(timer));
	}
	IEnumerator DestroyTimer(float seconds)
	{
		yield return new WaitForSeconds(seconds);

		Destroy(this.gameObject);
	}
}
