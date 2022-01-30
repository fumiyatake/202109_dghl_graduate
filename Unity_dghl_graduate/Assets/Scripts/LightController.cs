using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class LightController : MonoBehaviour
{
    /** @var string OSC_ADDRESS シリアル通信を仲介するOSC通信のアドレス */
    static private string OSC_ADDRESS_CONNECT   = "/serial/connect";
    static private string OSC_ADDRESS_WRITE     = "/serial/write";

    /** @var OSC osc OSC通信用のゲームオブジェクト*/
    public bool isVisible = false;
    public OSC osc;
    public string serialPort;
    public Color _color;

    private Material _mat;

    // Start is called before the first frame update
    void Start()
    {
        this._mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        this._color = new Color( 0, 0, 0 );
        this._mat.color = this._color;
        this.transform.Find("LightEntity").GetComponent<Renderer>().material = _mat;

        this.transform.Find("LightEntity").gameObject.SetActive(isVisible);


        // シリアル通信を確立
        OscMessage message = new OscMessage();
        message.address = OSC_ADDRESS_CONNECT;
        message.values.Add(this.serialPort);
        osc.Send(message);
    }

    // Update is called once per frame
    void Update()
    {
        this.transform.Find("LightEntity").gameObject.SetActive(isVisible);
        this._mat.color = this._color;

        // RGBの状態をデバイスに書き込み
        OscMessage message  = new OscMessage();
        message.address     = OSC_ADDRESS_WRITE;
        message.values.Add( this.serialPort );
        message.values.Add(string.Format(
          "{{ r:{0:0.00}, g:{1:0.00}, b:{2:0.00} }};",
            (this._color.r * 255).ToString(),
            (this._color.g * 255).ToString(),
            (this._color.b * 255).ToString()
        ));
        osc.Send(message);

    }

    public void setColor( Color color )
    {
        // TODO このままだとちかちかしすぎるので、もう少し滑らかに変化するようにする(直接Colorを受け取らない方がいいかも)
        this._color = new Color( color.r, color.g, color.b );
    }
}
