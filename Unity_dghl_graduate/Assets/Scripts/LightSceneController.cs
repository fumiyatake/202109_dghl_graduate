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
    const float FUSION_STEP = 0.0002f;

    public bool IS_LIGHT_VISIBLE = false;

    public OSC oscLocal;
    public OSC oscRemote;
    public GameObject heartBeatVFX;
    public GameObject LightPrefab;
    public GameObject NoiseAudio;
    public GameObject HeartAudio;
    public GameObject FireAudio;
    public int radius = 5;

    private bool _isFusionStart = false;
    private float _beatRate = 0f;
    private float _fusionRate = 0f;
    private GameObject[] _lightList;

    private Color _color = new Color();
    private Gradient _vfxGradient;
    private Vector3 _center;

    private AudioSource _noiseAudioComp;
    private AudioSource _heartAudioComp;
    private AudioSource _fireAudioComp;

    void Start()
    {
        // OSC イベントの受付設定
        oscLocal.SetAddressHandler( "/heartbeat", OnReceiveHeartbeat);
        oscLocal.SetAddressHandler( "/body_tracking", OnReceiveBodyTrackingt);

        // ライトの設定(正面やや左から反時計回りに配置)
        // windowsのbluetooth不安定なので全部リモートのmacにやらせる
        lightParam[] lightParamList = new lightParam[]{
            new lightParam( "/dev/tty.ESP32-ESP32SPP", oscRemote ),
            new lightParam( "/dev/tty.ESP32-2-ESP32SPP", oscRemote ),
            new lightParam( "/dev/tty.ESP32-4-ESP32SPP", oscRemote ),
            new lightParam( "/dev/tty.ESP32-5-ESP32SPP", oscRemote ),
            new lightParam( "/dev/tty.ESP32-9-ESP32SPP", oscRemote ),
            new lightParam( "/dev/tty.ESP32-8-ESP32SPP", oscRemote ),
        };
        _lightList = new GameObject[lightParamList.Length];

        for ( int i = 0; i < lightParamList.Length; i ++)
        {
            lightParam param    = lightParamList[i];
            
            // 数に応じて自動で円状に等間隔に配置
            // 正面が邪魔にならないように、正面やや左から反時計回りに配置
            float radian        = Mathf.Deg2Rad * ( 360 / lightParamList.Length * ( i + 0.5f ) + 90 );
            Vector3 position    = new Vector3(Mathf.Cos(radian) * radius, 0, Mathf.Sin(radian) * radius);
            GameObject light    = Instantiate(LightPrefab, position, Quaternion.identity,gameObject.transform);
            _lightList[i] = light;

            LightController controller = light.GetComponent<LightController>();
            controller.osc = param.osc;
            controller.serialPort = param.serialPort;
            controller.isVisible = IS_LIGHT_VISIBLE;
        }

        // vfxの色をセット
        _setVfxGradient();

        // 中央を設定（現状では固定）
        _center = new Vector3( 0f, 0f, 0f );

        // 音源のコンポーネントを取得
        _noiseAudioComp = NoiseAudio.GetComponent<AudioSource>();
        _heartAudioComp = HeartAudio.GetComponent<AudioSource>();
        _fireAudioComp  = FireAudio.GetComponent<AudioSource>();
        _heartAudioComp.Stop();
        _heartAudioComp.timeSamples = 0;
    }

    // Update is called once per frame
    void Update()
    {
        // 人が離れた時
        if ( _isFusionStart && _beatRate <= 0f )
        {
            _isFusionStart = false;
            _fusionRate = 0f;
            _heartAudioComp.Stop();
            _heartAudioComp.timeSamples = 0;
            _fireAudioComp.timeSamples = 0;
            _fireAudioComp.Play();
        }
        // 人が来た時
        else if ( !_isFusionStart && _beatRate > 0f )
        {
            _isFusionStart  = true;
            _fusionRate     = FUSION_STEP;
            _heartAudioComp.Play();
        }
        // 継続しているとき
        else if (_isFusionStart && _fusionRate < 1f)
        {
            _fusionRate += FUSION_STEP;
        }

        // 音周りの調整
        _heartAudioComp.volume  = 0.2f + _fusionRate * 2;
        _noiseAudioComp.volume  = 0.15f + ( _isFusionStart ? (1 - _fusionRate) * 0.4f : 0f );

        // 色周りの調整
        float noiseValG = Mathf.PerlinNoise(Time.time * 0.001f, Time.frameCount * 0.001f);
        float noiseValB = Mathf.PerlinNoise(Time.frameCount * 0.001f, Time.time * 0.001f);
        float r = _isFusionStart ? Mathf.Cos(Mathf.Deg2Rad * 90 * _fusionRate) : 0f;
        float g = ( _isFusionStart ? 1f - Mathf.Cos(Mathf.Deg2Rad * 90 * _fusionRate) : 1f ) * noiseValG;
        float b = ( _isFusionStart ? 1f - Mathf.Cos(Mathf.Deg2Rad * 90 * _fusionRate) : 1f) * noiseValB;
        _color = new Color(r, g, b, 1f);

        // 色を変更
        _setVfxGradient();

        float brightRate = _fusionRate * 0.8f + 0.2f;
        for (int i = 0; i < _lightList.Length; i++)
        {
            Vector3 position = _lightList[i].transform.position;
            float distance = Vector3.Distance(_center, position );
            float colorRate = 1f;
            if( _isFusionStart && _fusionRate < 1f)
            {
                if( Random.Range( 0f, 1f ) > ( 0.5f + 0.5f * _fusionRate ) )
                {
                    colorRate = 0f;
                }
            }
            _lightList[i].GetComponent<LightController>().setColor( _color * colorRate );
        }

        // vfxに反映
        VisualEffect vfx = heartBeatVFX.GetComponent<VisualEffect>();
        vfx.SetFloat("FusionRate", _fusionRate);    // TODO 本当は文字列ベースで当たらない方がパフォーマンスいいらしい
        vfx.SetVector3("Center", _center);
        vfx.SetFloat("BeatRate", _beatRate);

    }

    void OnReceiveHeartbeat( OscMessage msg )
    {
        _beatRate = float.Parse( msg.values[0].ToString() );
        _heartAudioComp.pitch = 1f / _beatRate * 0.95f;
    }

    void OnReceiveBodyTrackingt( OscMessage msg )
    {
        Debug.Log(msg.values[0]);
    }

    private void _setVfxGradient()
    {
        // vfxのグラデーションを設定
        GradientColorKey[] colorKeys = new GradientColorKey[] { new GradientColorKey(new Color(1f, 1f, 1f), 0f), new GradientColorKey(_color, 1f) };
        GradientAlphaKey[] alphaKeys = new GradientAlphaKey[] { new GradientAlphaKey(0.9f, 0.9f), new GradientAlphaKey(1f, 1f) };
        _vfxGradient = new Gradient();
        _vfxGradient.SetKeys(colorKeys, alphaKeys);
        heartBeatVFX.GetComponent<VisualEffect>().SetGradient("LifeGradient", _vfxGradient);
    }

    private static float _map(float value, float fromLow, float fromHigh, float toLow, float toHigh)
    {
        return (value - fromLow) * (toHigh - toLow) / (fromHigh - fromLow) + toLow;
    }
}
