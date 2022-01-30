using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class LightController : MonoBehaviour
{
    /** @var string OSC_ADDRESS �V���A���ʐM�𒇉��OSC�ʐM�̃A�h���X */
    static private string OSC_ADDRESS_CONNECT   = "/serial/connect";
    static private string OSC_ADDRESS_WRITE     = "/serial/write";

    /** @var OSC osc OSC�ʐM�p�̃Q�[���I�u�W�F�N�g*/
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


        // �V���A���ʐM���m��
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

        // RGB�̏�Ԃ��f�o�C�X�ɏ�������
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
        // TODO ���̂܂܂��Ƃ���������������̂ŁA�����������炩�ɕω�����悤�ɂ���(����Color���󂯎��Ȃ�������������)
        this._color = new Color( color.r, color.g, color.b );
    }
}
