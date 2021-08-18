using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;

public class LightSceneController : MonoBehaviour
{
    public OSC oscIn;
    public GameObject heartBeatVFX;

    private float _lifePower = 0f;
    private float _targetPower = 0f;

    void Start()
    {
        // OSC イベントの受付設定
        oscIn.SetAddressHandler( "/heartbeat", OnReceiveHeartbeat);
        oscIn.SetAddressHandler( "/body_tracking", OnReceiveBodyTrackingt);

    }

    // Update is called once per frame
    void Update()
    {
        _lifePower = _lifePower + (_targetPower - _lifePower) * 0.1f;
        heartBeatVFX.GetComponent<VisualEffect>().SetFloat("LifePower", _lifePower);
    }

    void OnReceiveHeartbeat( OscMessage msg )
    {
        float val = float.Parse( msg.values[0].ToString() );
        float lifePower = _map(val, 2800f, 3200f, 0f, 1f);
        if (lifePower < 0f ) lifePower = 0f;
        if (lifePower > 1f ) lifePower = 1f;
        lifePower = Mathf.Sin(lifePower * Mathf.PI / 2f);
        _targetPower = lifePower;
    }

    void OnReceiveBodyTrackingt( OscMessage msg )
    {
        Debug.Log(msg.values[0]);
    }

    private static float _map(float value, float fromLow, float fromHigh, float toLow, float toHigh)
    {
        return (value - fromLow) * (toHigh - toLow) / (fromHigh - fromLow) + toLow;
    }
}
