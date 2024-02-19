using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoorRotation : MonoBehaviour
{
	public int Speed;
	void Start()
	{

	}

	void Update()
	{
		transform.Rotate(0, Speed * Time.deltaTime, 0);
	}
}