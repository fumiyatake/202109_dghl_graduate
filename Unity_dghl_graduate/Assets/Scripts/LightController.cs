using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class LightController : MonoBehaviour
{
    /** @var string OSC_ADDRESS シリアル通信を仲介するOSC通信のアドレス */
    static private string OSC_ADDRESS_CONNECT   = "/serial/connect";
    static private string OSC_ADDRESS_WRITE     = "/serial/write";

    /** @var OSC osc OSC通信用のゲームオブジェクト*/
    public OSC osc;
    public string serialPort;

    private Material _mat;
    public Color _color;

    // Start is called before the first frame update
    void Start()
    {
        this._mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
 // TODO 0,0,0に変える
        this._color = new Color( Random.Range(0f,.1f), Random.Range(0f, .1f), Random.Range(0f, .1f));
        this._mat.color = this._color;
        this.transform.Find("LightEntity").GetComponent<Renderer>().material = _mat;

        // シリアル通信を確立
        OscMessage message = new OscMessage();
        message.address = OSC_ADDRESS_CONNECT;
        message.values.Add(this.serialPort);
        osc.Send(message);
    }

    // Update is called once per frame
    void Update()
    {   
        this._color.r += Random.Range(-0.01f, 0.01f);
        this._color.g += Random.Range(-0.01f, 0.01f);
        this._color.b += Random.Range(-0.01f, 0.01f);
        if (this._color.r < 0) this._color.r = 0f;
        if (this._color.g < 0) this._color.g = 0f;
        if (this._color.b < 0) this._color.b = 0f;
        if (this._color.r > 1) this._color.r = 1f;
        if (this._color.g > 1) this._color.g = 1f;
        if (this._color.b > 1) this._color.b = 1f;

        this._mat.color = this._color;
        // RGBの状態をデバイスに書き込み
        OscMessage message  = new OscMessage();
        message.address     = OSC_ADDRESS_WRITE;
        message.values.Add( this.serialPort );
        message.values.Add( string.Format(
            "{{ r:{0:0.00}, g:{1:0.00}, b:{2:0.00} }};",
            (this._color.r * 255).ToString(),
            (this._color.g * 255).ToString(),
            (this._color.b * 255).ToString()
        ) );
        Debug.Log(message.values[1]);
        osc.Send( message );
    }
 
    public void setColor( Color color )
    {
        this._color = new Color( color.r, color.g, color.b );
    }
}
