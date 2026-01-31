using UnityEngine;
using UnityEngine.Audio;

public enum ProximityType
{
    None,
    Wall,
    Door,
    Window,
    Object
}

public sealed class ProximitySense360 : MonoBehaviour
{
    [SerializeField]
    private Transform _origin;

    [SerializeField]
    private LayerMask _detectMask = ~0;

    [SerializeField]
    private float _maxDistance = 2.5f;

    [SerializeField]
    private float _sphereRadius = 0.35f;

    [SerializeField]
    private float _updateRate = 0.06f;

    [SerializeField]
    private float _smooth = 12f;

    [SerializeField]
    private AudioSource _leftSource;

    [SerializeField]
    private AudioSource _rightSource;

    [SerializeField]
    private float _minVol = 0f;

    [SerializeField]
    private float _maxVol = 0.35f;

    [SerializeField]
    private float _centerBleed = 0.55f;

    [SerializeField]
    private float _frontWeight = 1f;

    [SerializeField]
    private float _sideWeight = 0.65f;

    [SerializeField]
    private float _backWeight = 0.35f;

    [SerializeField]
    private AudioMixer _mixer;

    [SerializeField]
    private string _cutoffParam = "Sense_LP_Cutoff";

    [SerializeField]
    private string _volumeParam = "Sense_Volume";

    [SerializeField]
    private bool _useMixerVolume = false;

    [SerializeField]
    private float _offDb = -80f;

    [SerializeField]
    private float _onDb = -10f;

    [SerializeField]
    private float _cutoffFar = 20000f;

    [SerializeField]
    private float _cutoffNearWall = 650f;

    [SerializeField]
    private float _cutoffNearDoor = 1200f;

    [SerializeField]
    private float _cutoffNearWindow = 900f;

    [SerializeField]
    private float _cutoffNearObject = 800f;

    private float _nextUpdate;

    private float _leftVol;
    private float _rightVol;

    private float _targetLeftVol;
    private float _targetRightVol;

    private float _currentCutoff;
    private float _targetCutoff;

    private float _currentDb;
    private float _targetDb;

    private float _vizLeft01;
    private float _vizRight01;
    private float _vizFront01;
    private float _vizBack01;

    public float VizLeft01 => _vizLeft01;
    public float VizRight01 => _vizRight01;
    public float VizFront01 => _vizFront01;
    public float VizBack01 => _vizBack01;


    private void Awake()
    {
        if (_origin == null && Camera.main != null)
            _origin = Camera.main.transform;

        SetupSource(_leftSource, -1f);
        SetupSource(_rightSource, 1f);

        _leftVol = 0f;
        _rightVol = 0f;

        _currentCutoff = _cutoffFar;
        _targetCutoff = _cutoffFar;

        _currentDb = _offDb;
        _targetDb = _offDb;

        ApplyMixer();
    }

    private void Update()
    {
        if (_origin == null || _leftSource == null || _rightSource == null) return;

        if (Time.unscaledTime >= _nextUpdate)
        {
            _nextUpdate = Time.unscaledTime + _updateRate;
            RecalculateTargets();
        }

        float k = 1f - Mathf.Exp(-_smooth * Time.unscaledDeltaTime);

        _leftVol = Mathf.Lerp(_leftVol, _targetLeftVol, k);
        _rightVol = Mathf.Lerp(_rightVol, _targetRightVol, k);

        _currentCutoff = Mathf.Lerp(_currentCutoff, _targetCutoff, k);
        _currentDb = Mathf.Lerp(_currentDb, _targetDb, k);

        _leftSource.volume = _leftVol;
        _rightSource.volume = _rightVol;

        ApplyMixer();
    }

    private void RecalculateTargets()
    {
        CastResult front = CastDir(_origin.forward);
        CastResult back = CastDir(-_origin.forward);
        CastResult left = CastDir(-_origin.right);
        CastResult right = CastDir(_origin.right);

        float f = front.near01 * _frontWeight;
        float b = back.near01 * _backWeight;
        float l = left.near01 * _sideWeight;
        float r = right.near01 * _sideWeight;

        float frontToBoth = f * _centerBleed;
        float backToBoth = b * _centerBleed;

        float left01 = Mathf.Clamp01(l + frontToBoth + backToBoth);
        float right01 = Mathf.Clamp01(r + frontToBoth + backToBoth);

        _vizLeft01 = Mathf.Clamp01(left01);
        _vizRight01 = Mathf.Clamp01(right01);
        _vizFront01 = Mathf.Clamp01(f);
        _vizBack01 = Mathf.Clamp01(b);

        _targetLeftVol = Mathf.Lerp(_minVol, _maxVol, left01);
        _targetRightVol = Mathf.Lerp(_minVol, _maxVol, right01);

        CastResult best = BestByWeighted(front, back, left, right);

        if (best.type == ProximityType.None || best.near01 <= 0f)
        {
            _targetCutoff = _cutoffFar;
            _targetDb = _offDb;
            return;
        }

        float bestWeight = WeightByDirection(best.direction);
        float intensity = Mathf.Clamp01(best.near01 * bestWeight);

        _targetCutoff = Mathf.Lerp(_cutoffFar, CutoffNearByType(best.type), intensity);

        if (_useMixerVolume)
            _targetDb = Mathf.Lerp(_offDb, _onDb, intensity);
    }

    private float WeightByDirection(Direction dir)
    {
        switch (dir)
        {
            case Direction.Front: return _frontWeight;
            case Direction.Side: return _sideWeight;
            case Direction.Back: return _backWeight;
            default: return 1f;
        }
    }

    private void ApplyMixer()
    {
        if (_mixer == null) return;

        _mixer.SetFloat(_cutoffParam, Mathf.Clamp(_currentCutoff, 10f, 22000f));

        if (_useMixerVolume)
            _mixer.SetFloat(_volumeParam, _currentDb);
    }

    private void SetupSource(AudioSource src, float pan)
    {
        if (src == null) return;

        src.playOnAwake = false;
        src.loop = true;
        src.spatialBlend = 0f;
        src.panStereo = pan;

        if (!src.isPlaying)
            src.Play();
    }

    private enum Direction
    {
        Front,
        Back,
        Side
    }

    private struct CastResult
    {
        public ProximityType type;
        public float near01;
        public Direction direction;
    }

    private CastResult CastDir(Vector3 dir)
    {
        Vector3 origin = _origin.position;

        if (!Physics.SphereCast(origin, _sphereRadius, dir, out RaycastHit hit, _maxDistance, _detectMask, QueryTriggerInteraction.Ignore))
            return new CastResult { type = ProximityType.None, near01 = 0f, direction = Direction.Front };

        float d = Mathf.Clamp(hit.distance, 0.01f, _maxDistance);
        float near01 = 1f - Mathf.Clamp01(d / _maxDistance);

        Direction direction = Direction.Side;

        float frontDot = Vector3.Dot(_origin.forward, dir.normalized);
        if (frontDot > 0.6f) direction = Direction.Front;
        else if (frontDot < -0.6f) direction = Direction.Back;

        return new CastResult { type = Classify(hit.collider), near01 = near01, direction = direction };
    }

    private CastResult BestByWeighted(CastResult front, CastResult back, CastResult left, CastResult right)
    {
        float f = front.near01 * _frontWeight;
        float b = back.near01 * _backWeight;
        float l = left.near01 * _sideWeight;
        float r = right.near01 * _sideWeight;

        CastResult best = front;
        float bestScore = f;

        if (b > bestScore) { best = back; bestScore = b; }
        if (l > bestScore) { best = left; bestScore = l; }
        if (r > bestScore) { best = right; bestScore = r; }

        return best;
    }

    private ProximityType Classify(Collider col)
    {
        if (col.CompareTag("Wall")) return ProximityType.Wall;
        if (col.CompareTag("Door")) return ProximityType.Door;
        if (col.CompareTag("Window")) return ProximityType.Window;
        if (col.CompareTag("Object")) return ProximityType.Object;

        return ProximityType.Object;
    }

    private float CutoffNearByType(ProximityType type)
    {
        switch (type)
        {
            case ProximityType.Door: return _cutoffNearDoor;
            case ProximityType.Window: return _cutoffNearWindow;
            case ProximityType.Object: return _cutoffNearObject;
            case ProximityType.Wall: return _cutoffNearWall;
            default: return _cutoffFar;
        }
    }
}