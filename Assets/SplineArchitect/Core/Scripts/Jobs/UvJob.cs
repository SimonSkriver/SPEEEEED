// -----------------------------------------------------------------------------
// SplineArchitect
// Filename: NormalsJob.cs
//
// Author: Mikael Danielsson
// Date Created: 05-11-2023
// (C) 2023 Mikael Danielsson. All rights reserved.
// -----------------------------------------------------------------------------

using UnityEngine;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;

using SplineArchitect.Utility;

namespace SplineArchitect.Jobs
{
    [BurstCompile]
    public struct UvJob : IJobParallelFor
    {
        public NativeArray<Vector2> uv;
        public NativeArray<Vector2> uv2;
        public NativeArray<Vector2> uv3;
        public NativeArray<Vector2> uv4;

        [ReadOnly] public int meshContainersCount;
        [ReadOnly] public NativeArray<int> uvMap;
        [ReadOnly] public NativeArray<UvSettings> uvSettingsMap;

        public void Execute(int i)
        {
            UvSettings uvSettings = new UvSettings();

            for (int i2 = 0; i2 < meshContainersCount; i2++)
            {
                if (i < uvMap[i2])
                {
                    uvSettings = uvSettingsMap[i2];
                    break;
                }
            }

            uv[i] += uvSettings.uvOffset;
            uv2[i] += uvSettings.uv2Offset;
            uv3[i] += uvSettings.uv3Offset;
            uv4[i] += uvSettings.uv4Offset;
        }
    }
}
