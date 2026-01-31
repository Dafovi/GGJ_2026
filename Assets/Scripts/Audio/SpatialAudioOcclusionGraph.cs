using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public sealed class SpatialAudioOcclusionGraph : MonoBehaviour
{
    [SerializeField]
    private AudioRoomTracker _listenerTracker;

    [SerializeField]
    private AudioRoomTracker _sourceTracker;

    [SerializeField]
    private LayerMask _occluderMask;

    [SerializeField]
    private float _updateRate = 0.08f;

    [SerializeField]
    private float _smooth = 10f;

    [SerializeField]
    private float _facingBoost = 0.25f;

    [SerializeField]
    private float _portalFacingRange = 10f;

    private AudioSource _source;
    private AudioLowPassFilter _lowPass;

    private float _baseVolume;

    private float _t;

    private float _targetVolMul = 1f;
    private float _targetCutoff = 22000f;

    private float _currentVolMul = 1f;
    private float _currentCutoff = 22000f;

    private AudioPortalLink[] _portals;

    [System.Obsolete]
    private void Awake()
    {
        _source = GetComponent<AudioSource>();
        _baseVolume = _source.volume;

        _lowPass = GetComponent<AudioLowPassFilter>();
        if (_lowPass == null) _lowPass = gameObject.AddComponent<AudioLowPassFilter>();

        _lowPass.enabled = true;
        _lowPass.cutoffFrequency = 22000f;

        _portals = FindObjectsOfType<AudioPortalLink>(true);
    }

    private void Update()
    {
        if (_listenerTracker == null || _sourceTracker == null) return;

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
        Transform listenerTf = _listenerTracker.transform;

        Vector3 lis = listenerTf.position;
        Vector3 src = transform.position;

        if (HasLineOfSight(lis, src))
        {
            ApplyTargets(OcclusionPresets.None, 0f);
            return;
        }

        AudioRoom listenerRoom = _listenerTracker.CurrentRoom;
        AudioRoom sourceRoom = _sourceTracker.CurrentRoom;

        if (listenerRoom == null || sourceRoom == null)
        {
            ApplyTargets(OcclusionPresets.Wall, 0f);
            return;
        }

        if (listenerRoom == sourceRoom)
        {
            ApplyTargets(OcclusionPresets.Wall, 0f);
            return;
        }

        if (TryFindBestPath(listenerRoom, sourceRoom, out PathResult path))
        {
            OcclusionSettings combined = CombinePath(path);
            float facing01 = FacingBoost01(listenerTf, path.firstPortalPosition);

            ApplyTargets(combined, facing01);
            return;
        }

        ApplyTargets(OcclusionPresets.Wall, 0f);
    }

    private bool HasLineOfSight(Vector3 from, Vector3 to)
    {
        Vector3 dir = to - from;
        float dist = dir.magnitude;
        if (dist <= 0.01f) return true;

        dir /= dist;

        return !Physics.Raycast(from, dir, dist, _occluderMask, QueryTriggerInteraction.Ignore);
    }

    private struct PathResult
    {
        public List<AudioPortalLink> portals;
        public Vector3 firstPortalPosition;
    }

    private bool TryFindBestPath(AudioRoom start, AudioRoom goal, out PathResult result)
    {
        result = default;

        Dictionary<AudioRoom, float> dist = new Dictionary<AudioRoom, float>();
        Dictionary<AudioRoom, AudioPortalLink> prevPortal = new Dictionary<AudioRoom, AudioPortalLink>();
        Dictionary<AudioRoom, AudioRoom> prevRoom = new Dictionary<AudioRoom, AudioRoom>();

        List<AudioRoom> open = new List<AudioRoom>();

        dist[start] = 0f;
        open.Add(start);

        while (open.Count > 0)
        {
            int bestIndex = 0;
            float bestDist = dist[open[0]];

            for (int i = 1; i < open.Count; i++)
            {
                float d = dist[open[i]];
                if (d < bestDist)
                {
                    bestDist = d;
                    bestIndex = i;
                }
            }

            AudioRoom current = open[bestIndex];
            open.RemoveAt(bestIndex);

            if (current == goal) break;

            for (int i = 0; i < _portals.Length; i++)
            {
                AudioPortalLink p = _portals[i];
                if (p == null || !p.Enabled) continue;

                AudioRoom other = p.GetOther(current);
                if (other == null) continue;

                float edgeCost = PortalCost(p);

                float newDist = dist[current] + edgeCost;

                if (!dist.ContainsKey(other) || newDist < dist[other])
                {
                    dist[other] = newDist;
                    prevPortal[other] = p;
                    prevRoom[other] = current;

                    if (!open.Contains(other))
                        open.Add(other);
                }
            }
        }

        if (!dist.ContainsKey(goal)) return false;

        List<AudioPortalLink> pathPortals = new List<AudioPortalLink>();

        AudioRoom r = goal;
        while (r != start)
        {
            if (!prevPortal.ContainsKey(r)) break;

            AudioPortalLink p = prevPortal[r];
            pathPortals.Add(p);
            r = prevRoom[r];
        }

        pathPortals.Reverse();

        if (pathPortals.Count == 0) return false;

        result = new PathResult
        {
            portals = pathPortals,
            firstPortalPosition = pathPortals[0].transform.position
        };

        return true;
    }

    private float PortalCost(AudioPortalLink portal)
    {
        switch (portal.Material)
        {
            case OcclusionMaterial.DoorOpen: return 1f;
            case OcclusionMaterial.Window: return 2f;
            case OcclusionMaterial.DoorClosed: return 4f;
            default: return 6f;
        }
    }

    private OcclusionSettings CombinePath(PathResult path)
    {
        float vol = 1f;
        float cutoff = 22000f;

        for (int i = 0; i < path.portals.Count; i++)
        {
            OcclusionSettings s = OcclusionPresets.From(path.portals[i].Material);

            vol *= s.volumeMultiplier;
            cutoff = Mathf.Min(cutoff, s.lowPassCutoffHz);
        }

        vol = Mathf.Clamp(vol, 0.05f, 1f);
        cutoff = Mathf.Clamp(cutoff, 400f, 22000f);

        return new OcclusionSettings { volumeMultiplier = vol, lowPassCutoffHz = cutoff };
    }

    private float FacingBoost01(Transform listener, Vector3 portalPos)
    {
        Vector3 toPortal = portalPos - listener.position;
        float dist = toPortal.magnitude;
        if (dist <= 0.01f) return 1f;
        if (dist > _portalFacingRange) return 0f;

        toPortal.y = 0f;
        toPortal.Normalize();

        Vector3 fwd = listener.forward;
        fwd.y = 0f;
        fwd.Normalize();

        float dot = Vector3.Dot(fwd, toPortal);
        float facing = Mathf.Clamp01((dot + 1f) * 0.5f);

        float distanceFactor = 1f - Mathf.Clamp01(dist / _portalFacingRange);

        return facing * distanceFactor;
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
}