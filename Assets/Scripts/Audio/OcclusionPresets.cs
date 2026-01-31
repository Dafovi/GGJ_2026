public enum OcclusionMaterial
{
    None,
    Wall,
    DoorOpen,
    DoorClosed,
    Window
}

[System.Serializable]
public struct OcclusionSettings
{
    public float volumeMultiplier;
    public float lowPassCutoffHz;
}

public static class OcclusionPresets
{
    public static OcclusionSettings None => new OcclusionSettings { volumeMultiplier = 1f, lowPassCutoffHz = 22000f };
    public static OcclusionSettings Wall => new OcclusionSettings { volumeMultiplier = 0.30f, lowPassCutoffHz = 1000f };
    public static OcclusionSettings DoorClosed => new OcclusionSettings { volumeMultiplier = 0.55f, lowPassCutoffHz = 1800f };
    public static OcclusionSettings DoorOpen => new OcclusionSettings { volumeMultiplier = 0.85f, lowPassCutoffHz = 7000f };
    public static OcclusionSettings Window => new OcclusionSettings { volumeMultiplier = 0.70f, lowPassCutoffHz = 3500f };

    public static OcclusionSettings From(OcclusionMaterial m)
    {
        switch (m)
        {
            case OcclusionMaterial.DoorOpen: return DoorOpen;
            case OcclusionMaterial.DoorClosed: return DoorClosed;
            case OcclusionMaterial.Window: return Window;
            case OcclusionMaterial.Wall: return Wall;
            default: return None;
        }
    }
}