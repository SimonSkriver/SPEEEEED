// -----------------------------------------------------------------------------
// SplineArchitect
// Filename: NativeNormalFrame.cs
//
// Author: Mikael Danielsson
// Date Created: 10-10-2026
// (C) 2023 Mikael Danielsson. All rights reserved.
// -----------------------------------------------------------------------------

using System.Numerics;
using Unity.Mathematics;

namespace SplineArchitect
{
    public struct NativeNormalFrame
    {

        public float3 x;
        public float3 y;
        public float3 z;

        public NativeNormalFrame(float3 x, float3 y, float3 z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }
    }
}