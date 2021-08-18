using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;

struct lightParam
{
    public string serialPort;
    public OSC osc;

    public lightParam(string serialPort, OSC osc)
    {
        this.serialPort = serialPort;
        this.osc = osc;
    }
}

public class LightSceneController : MonoBehaviour
{
    const bool IS_LIGHT_VISIBLE = true;

    public OSC oscLocal;
    public OSC oscRemote;
    public GameObject heartBeatVFX;
    public GameObject LightPrefab;
    public int radius = 5;

    private float _lifePower = 0f;
    private float _targetPower = 0f;
    private GameObject[] _lightList;

    void Start()
    {
        // OSC イベントの受付設定
        oscLocal.SetAddressHandler( "/heartbeat", OnReceiveHeartbeat);
        oscLocal.SetAddressHandler( "/body_tracking", OnReceiveBodyTrackingt);

        // ライトの設定
        lightParam[] lightParamList = new lightParam[]{
            new lightParam( "/COM9", oscLocal ),
        };
        _lightList = new GameObject[lightParamList.Length];

        for ( int i = 0; i < lightParamList.Length; i ++)
        {
            lightParam param    = lightParamList[i];
            
            // 数に応じて自動で円状に等間隔に配置
            // 正面にKinectを置いたときに邪魔にならないように、正面やや左から反時計回りに配置
            float radian        = Mathf.Deg2Rad * ( 360 / lightParamList.Length * ( i + 0.5f ) + 90 );
            Vector3 position    = new Vector3(Mathf.Cos(radian) * radius, 0, Mathf.Sin(radian) * radius);
            GameObject light    = Instantiate(LightPrefab, position, Quaternion.identity,gameObject.transform);
            _lightList[i] = light;

            LightController controller = light.GetComponent<LightController>();
            controller.osc = param.osc;
            controller.serialPort = param.serialPort;
            controller.isVisible = IS_LIGHT_VISIBLE;
        }

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
