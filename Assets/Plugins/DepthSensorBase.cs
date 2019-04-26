using UnityEngine;

public abstract class DepthSensorBase : MonoBehaviour
{
    public abstract int Width { get; }

    public abstract int Height { get; }

    /// <summary>
    /// Get the latest depth frame.
    /// </summary>
    public abstract ushort[] depthImage { get; }

    /// <summary>
    /// Polls the underlying device for a new depth frame.
    /// </summary>
    /// <returns>Whether a new frame has been acquired</returns>
    public abstract bool pollDepth();
}
