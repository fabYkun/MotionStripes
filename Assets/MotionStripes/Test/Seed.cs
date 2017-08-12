using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Seed : MonoBehaviour
{
    public int seed = 0;
	void Awake () {
        Random.InitState(seed);
    }
}
