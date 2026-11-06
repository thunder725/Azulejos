using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Random = UnityEngine.Random;

public class Azulejos : MonoBehaviour {

	[SerializeField] Texture2D[] masksArray;
	[SerializeField] GameObject[] tiles;


	// Use this for initialization
	void Start () {
		
		foreach (GameObject tile in tiles)
		{
			MeshRenderer renderer = tile.GetComponent<MeshRenderer>();
			Material _m = Instantiate(renderer.material);
			_m.SetTexture("_Mask", masksArray.PickRandom());
			_m.SetFloat("_Invert", Random.value);
			renderer.material = _m;
		}
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
