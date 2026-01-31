[System.Serializable]
public struct OcclusionSettings
{
    public float volumeMultiplier;
    public float lowPassCutoffHz;
}

public static class AudioOcclusionPresets
{
    public static OcclusionSettings None => new OcclusionSettings
    {
        volumeMultiplier = 1f,
        lowPassCutoffHz = 22000f
    };

    public static OcclusionSettings Wall => new OcclusionSettings
    {
        volumeMultiplier = 0.35f,
        lowPassCutoffHz = 1200f
    };

    public static OcclusionSettings ClosedDoor => new OcclusionSettings
    {
        volumeMultiplier = 0.55f,
        lowPassCutoffHz = 1800f
    };

    public static OcclusionSettings OpenDoor => new OcclusionSettings
    {
        volumeMultiplier = 0.85f,
        lowPassCutoffHz = 7000f
    };

    public static OcclusionSettings Window => new OcclusionSettings
    {
        volumeMultiplier = 0.70f,
        lowPassCutoffHz = 3500f
    };
}