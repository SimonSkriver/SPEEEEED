// -----------------------------------------------------------------------------
// SplineArchitect
// Filename: SegmentSample.cs
//
// Author: Mikael Danielsson
// Date Created: 10-10-2026
// (C) 2023 Mikael Danielsson. All rights reserved.
// -----------------------------------------------------------------------------

using Unity.Mathematics;

namespace SplineArchitect
{
    public struct SegmentSample
    {
        public float2 scale;
        public quaternion zRotation;
        public float2 saddleSkew;
        public float noise;

        public SegmentSample(float2 scale, quaternion zRotation, float2 saddleSkew, float noise)
        {
            this.zRotation = zRotation;
            this.scale = scale;
            this.saddleSkew = saddleSkew;
            this.noise = noise;
        }
    }
}