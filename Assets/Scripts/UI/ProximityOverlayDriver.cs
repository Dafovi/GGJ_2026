using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public sealed class ProximityOverlayDriver : MonoBehaviour
{
    [SerializeField]
    private ProximitySense360 _sense;

    [SerializeField]
    private Image _image;

    [SerializeField]
    private float _smooth = 14f;

    [SerializeField]
    private float _baseDarkness = 0.92f;

    [SerializeField]
    private Color _glowColor = Color.white;

    [SerializeField]
    private float _tintFadeSpeed = 8f;

    [SerializeField]
    private float _deadzone = 0.04f;

    [SerializeField]
    private float _gamma = 1.6f;

    private Material _mat;

    private float _l;
    private float _r;
    private float _f;
    private float _b;

    private float _tint;

    private Coroutine _colorChangetine;

    private void Awake()
    {
        if (_image == null)
            _image = GetComponent<Image>();

        _mat = _image.material;
    }

    private void Update()
    {
        if (_sense == null || _mat == null) return;

        float rawL = Process(_sense.VizLeft01);
        float rawR = Process(_sense.VizRight01);
        float rawF = Process(_sense.VizFront01);
        float rawB = Process(_sense.VizBack01);

        float k = 1f - Mathf.Exp(-_smooth * Time.unscaledDeltaTime);

        _l = Mathf.Lerp(_l, rawL, k);
        _r = Mathf.Lerp(_r, rawR, k);
        _f = Mathf.Lerp(_f, rawF, k);
        _b = Mathf.Lerp(_b, rawB, k);

        _tint = Mathf.MoveTowards(_tint, 0f, _tintFadeSpeed * Time.unscaledDeltaTime);

        _mat.SetFloat("_Darkness", _baseDarkness);

        _mat.SetFloat("_Left", _l);
        _mat.SetFloat("_Right", _r);
        _mat.SetFloat("_Front", _f);
        _mat.SetFloat("_Back", _b);

        _mat.SetColor("_GlowColor", _glowColor);

        _mat.SetFloat("_Tint", _tint);
        //_mat.SetColor("_TintColor", _hitTintColor);
    }

    private float Process(float v)
    {
        if (v <= _deadzone) return 0f;

        float x = (v - _deadzone) / Mathf.Max(0.0001f, 1f - _deadzone);
        x = Mathf.Clamp01(x);

        return Mathf.Pow(x, _gamma);
    }

    public void SetHitTint(float amount01)
    {
        _tint = Mathf.Clamp01(Mathf.Max(_tint, amount01));
    }

    public void SetGlowColor(Color color)
    {
        if(_colorChangetine == null)
        {
            _colorChangetine = StartCoroutine(ChangeGlowColor(color));
        }
    }

    IEnumerator ChangeGlowColor(Color targetColor)
    {
        Color initialColor = _glowColor;
        yield return new WaitForEndOfFrame();
        _glowColor = targetColor;
        yield return new WaitForSeconds(0.1f);
        _glowColor = initialColor;

        _colorChangetine = null;
    }
}