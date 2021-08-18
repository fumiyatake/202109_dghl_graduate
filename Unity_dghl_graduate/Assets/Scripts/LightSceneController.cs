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

    private float _hue = 0f;
    private float _saturation = 0f;
    private Gradient _vfxGradient;
    private Vector3 _center;

    void Start()
    {
        // OSC イベントの受付設定
        oscLocal.SetAddressHandler( "/heartbeat", OnReceiveHeartbeat);
        oscLocal.SetAddressHandler( "/body_tracking", OnReceiveBodyTrackingt);

        // ライトの設定
        lightParam[] lightParamList = new lightParam[]{
            new lightParam( "/COM9", oscLocal ),
            new lightParam( "/COM9", oscLocal ),
            new lightParam( "/COM9", oscLocal ),
            new lightParam( "/COM9", oscLocal ),
            new lightParam( "/COM9", oscLocal ),
            new lightParam( "/COM9", oscLocal ),
            new lightParam( "/COM9", oscLocal ),
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

        // TODO 削除
        _hue = Random.Range( 0f, 1f );
        _saturation = Random.Range(0.8f, 1f);
        _setVfxGradient();

        _center = new Vector3( 2f, 0f, -1f );
    }

    // Update is called once per frame
    void Update()
    {
        // TODO 削除
        _hue += Random.Range(-0.01f, 0.01f);
        _saturation += Random.Range(-0.01f, 0.01f);

        VisualEffect vfx = heartBeatVFX.GetComponent<VisualEffect>();
        _lifePower = _lifePower + (_targetPower - _lifePower) * 0.1f;
        vfx.SetFloat("LifePower", _lifePower);    // TODO 本当は文字列ベースで当たらない方がパフォーマンスいいらしい
        vfx.SetVector3("Center", _center);    

        // 色を変更
        _setVfxGradient();


        for (int i = 0; i < _lightList.Length; i++)
        {
            Vector3 position = _lightList[i].transform.position;
            float distance = Vector3.Distance(_center, position );
            Debug.Log(radius * _lifePower / distance);
            Debug.Log(radius * _lifePower / distance);
            _lightList[i].GetComponent<LightController>().setColor(Color.HSVToRGB(_hue, radius * _lifePower / distance, radius * _lifePower / distance));
        }
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

    private void _setVfxGradient()
    {
        // vfxのグラデーションを設定
        Color color = Color.HSVToRGB(_hue, _saturation, 1);
        GradientColorKey[] colorKeys = new GradientColorKey[] { new GradientColorKey(new Color(0, 0, 0), 0f), new GradientColorKey(color, 1f) };
        GradientAlphaKey[] alphaKeys = new GradientAlphaKey[] { new GradientAlphaKey(0f, 0f), new GradientAlphaKey(1f, 1f) };
        _vfxGradient = new Gradient();
        _vfxGradient.SetKeys(colorKeys, alphaKeys);
        heartBeatVFX.GetComponent<VisualEffect>().SetGradient("LifeGradient", _vfxGradient);
    }

    private static float _map(float value, float fromLow, float fromHigh, float toLow, float toHigh)
    {
        return (value - fromLow) * (toHigh - toLow) / (fromHigh - fromLow) + toLow;
    }
}
