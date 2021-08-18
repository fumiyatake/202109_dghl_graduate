using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoxTrailTest : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 pos = this.gameObject.transform.position;
        pos.x += 0.1f + Random.Range(-.1f, .1f);
        pos.y += Random.Range(-.1f, .1f);
        pos.z += Random.Range(-.1f, .1f);
        this.gameObject.transform.position = pos;
    }
}
