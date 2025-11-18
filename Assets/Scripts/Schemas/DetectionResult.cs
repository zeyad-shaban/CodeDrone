using System;
using System.Collections.Generic;

[Serializable]
public class DetectionResult
{
    public float[] boxes; // xyxy format
    public float[] conf;
}