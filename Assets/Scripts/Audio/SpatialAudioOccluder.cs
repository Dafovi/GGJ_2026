using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public sealed class SpatialAudioOccluder : MonoBehaviour
{
    [SerializeField]
    private Transform _listener;

    [SerializeField]
    private LayerMask _occluderMask;

    [SerializeField]
    private LayerMask _portalMask;

    [SerializeField]
    private float _updateRate = 0.05f;

    [SerializeField]
    private float _smooth = 10f;

    [SerializeField]
    private float _facingBoost = 0.25f;

    private AudioSource _source;
    private AudioLowPassFilter _lowPass;

    private float _targetVolMul = 1f;
    private float _targetCutoff = 22000f;

    private float _currentVolMul = 1f;
    private float _currentCutoff = 22000f;

    private float _baseVolume;

    private float _t;

    private void Awake()
    {
        _source = GetComponent<AudioSource>();
        _baseVolume = _source.volume;

        _lowPass = GetComponent<AudioLowPassFilter>();
        if (_lowPass == null)
            _lowPass = gameObject.AddComponent<AudioLowPassFilter>();

        if (_listener == null && Camera.main != null)
            _listener = Camera.main.transform;

        _lowPass.enabled = true;
        _lowPass.cutoffFrequency = 22000f;
    }

    private void Update()
    {
        if (_listener == null) return;

        _t += Time.deltaTime;
        if (_t >= _updateRate)
        {
            _t = 0f;
            RecalculateTargets();
        }

        float k = 1f - Mathf.Exp(-_smooth * Time.deltaTime);

        _currentVolMul = Mathf.Lerp(_currentVolMul, _targetVolMul, k);
        _currentCutoff = Mathf.Lerp(_currentCutoff, _targetCutoff, k);

        _source.volume = _baseVolume * _currentVolMul;
        _lowPass.cutoffFrequency = _currentCutoff;
    }

    private void RecalculateTargets()
    {
        Vector3 src = transform.position;
        Vector3 lis = _listener.position;

        if (HasLineOfSight(lis, src))
        {
            ApplyTargets(AudioOcclusionPresets.None, 0f);
            return;
        }

        if (TryBestPortalPath(lis, src, out AudioPortal bestPortal, out OcclusionSettings settings))
        {
            float facing = FacingFactor(bestPortal.transform.position);
            ApplyTargets(settings, facing);
            return;
        }

        ApplyTargets(AudioOcclusionPresets.Wall, 0f);
    }

    private void ApplyTargets(OcclusionSettings baseSettings, float facing01)
    {
        float vol = baseSettings.volumeMultiplier;
        float cutoff = baseSettings.lowPassCutoffHz;

        if (facing01 > 0f)
        {
            vol = Mathf.Lerp(vol, 1f, _facingBoost * facing01);
            cutoff = Mathf.Lerp(cutoff, 22000f, _facingBoost * facing01);
        }

        _targetVolMul = Mathf.Clamp01(vol);
        _targetCutoff = Mathf.Clamp(cutoff, 400f, 22000f);
    }

    private float FacingFactor(Vector3 portalPos)
    {
        Vector3 toPortal = (portalPos - _listener.position);
        toPortal.y = 0f;

        if (toPortal.sqrMagnitude < 0.0001f) return 1f;

        toPortal.Normalize();

        Vector3 fwd = _listener.forward;
        fwd.y = 0f;
        fwd.Normalize();

        float dot = Vector3.Dot(fwd, toPortal);
        return Mathf.Clamp01((dot + 1f) * 0.5f);
    }

    private bool HasLineOfSight(Vector3 from, Vector3 to)
    {
        Vector3 dir = to - from;
        float dist = dir.magnitude;
        if (dist <= 0.01f) return true;

        dir /= dist;

        return !Physics.Raycast(from, dir, dist, _occluderMask, QueryTriggerInteraction.Ignore);
    }

    private bool TryBestPortalPath(Vector3 listenerPos, Vector3 sourcePos, out AudioPortal bestPortal, out OcclusionSettings bestSettings)
    {
        bestPortal = null;
        bestSettings = AudioOcclusionPresets.Wall;

        Collider[] hits = Physics.OverlapSphere(listenerPos, 12f, _portalMask, QueryTriggerInteraction.Collide);
        if (hits == null || hits.Length == 0) return false;

        float bestScore = float.PositiveInfinity;

        for (int i = 0; i < hits.Length; i++)
        {
            AudioPortal portal = hits[i].GetComponent<AudioPortal>();
            if (portal == null) continue;

            Vector3 p = portal.transform.position;

            if (!HasLineOfSight(listenerPos, p)) continue;
            if (!HasLineOfSight(sourcePos, p)) continue;

            float score = (listenerPos - p).sqrMagnitude + (sourcePos - p).sqrMagnitude;

            if (score < bestScore)
            {
                bestScore = score;
                bestPortal = portal;
                bestSettings = PortalSettings(portal.Type);
            }
        }

        return bestPortal != null;
    }

    private OcclusionSettings PortalSettings(PortalType type)
    {
        switch (type)
        {
            case PortalType.OpenDoor: return AudioOcclusionPresets.OpenDoor;
            case PortalType.ClosedDoor: return AudioOcclusionPresets.ClosedDoor;
            case PortalType.Window: return AudioOcclusionPresets.Window;
            default: return AudioOcclusionPresets.Wall;
        }
    }
}