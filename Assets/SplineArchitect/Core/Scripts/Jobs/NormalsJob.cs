// -----------------------------------------------------------------------------
// SplineArchitect
// Filename: NormalsJob.cs
//
// Author: Mikael Danielsson
// Date Created: 05-11-2023
// (C) 2023 Mikael Danielsson. All rights reserved.
// -----------------------------------------------------------------------------

using Unity.Burst;
using Unity.Collections;
using Unity.Mathematics;
using Unity.Jobs;

using SplineArchitect.Utility;

namespace SplineArchitect.Jobs
{
    [BurstCompile(FloatPrecision.Low, FloatMode.Default, OptimizeFor = OptimizeFor.Performance)]
    public struct NormalsJob : IJob
    {
        public NativeList<NativeNormalFrame> normals;
        public NativeList<int> normalMap;
        [ReadOnly] public NativeArray<NativeSegment> nativeSegments;
        [ReadOnly] public float resolution;

        public void Execute()
        {
            normalMap.Clear();
            normals.Clear();

            normalMap.Add(0);
            for (int i2 = 0; i2 < nativeSegments.Length - 1; i2++)
            {
                NativeSegment ns = nativeSegments[i2];
                NativeSegment ns2 = nativeSegments[i2 + 1];

                if (!ns.isStatic)
                {
                    // Start normals
                    float3 zNormal = BezierUtility.GetDirection(ns.anchor, ns.tangentA, ns2.tangentB, ns2.anchor, 0);
                    if (i2 == 0) zNormal = math.normalizesafe(ns.anchor - ns.tangentB);
                    float3 xNormal = math.cross(zNormal, math.mul(ns.rotation, -math.up()));
                    xNormal = math.normalizesafe(xNormal);
                    float3 yNormal = math.cross(zNormal, xNormal);
                    yNormal = math.normalizesafe(yNormal);

                    int nIndex = normals.Length;
                    normals.Add(new NativeNormalFrame(xNormal, yNormal, zNormal));

                    // Calculate steps
                    float step = 1 / (resolution * (ns.length / 100));
                    step = math.clamp(step, 0.00001f, 0.25f);

                    for (float time = step; time < 1.0f; time += step)
                    {
                        bool isLastSample = i2 == nativeSegments.Length - 2 && (time + step) >= 1.0f;

                        float3 nextZNormal = BezierUtility.GetDirection(ns.anchor, ns.tangentA, ns2.tangentB, ns2.anchor, time);
                        if (isLastSample) nextZNormal = math.normalizesafe(ns2.tangentA - ns2.anchor);

                        float3 v = zNormal + nextZNormal;
                        v = math.normalizesafe(v);

                        float3 nextXNormal = xNormal - 2f * math.dot(xNormal, v) * v;
                        float3 nextYNormal = math.cross(nextZNormal, nextXNormal);
                        nextYNormal = math.normalizesafe(nextYNormal);

                        normals.Add(new NativeNormalFrame(nextXNormal, nextYNormal, nextZNormal));

                        xNormal = nextXNormal;
                        yNormal = nextYNormal;
                        zNormal = nextZNormal;
                    }

                    float3 zNormalEnd = BezierUtility.GetDirection(ns.anchor, ns.tangentA, ns2.tangentB, ns2.anchor, 1);
                    float3 xNormalEnd = math.cross(zNormalEnd, math.mul(ns2.rotation, -math.up()));
                    xNormalEnd = math.normalizesafe(xNormalEnd);
                    float3 yNormalEnd = math.cross(zNormalEnd, xNormalEnd);
                    yNormalEnd = math.normalizesafe(yNormalEnd);

                    NativeNormalFrame lastNormalFrame = normals[normals.Length - 1];
                    float degreeDif = GeneralUtility.SignedAngle(lastNormalFrame.y, yNormalEnd, zNormalEnd);
                    int frameCount = normals.Length - nIndex;

                    if (frameCount > 1 && math.abs(degreeDif) > 0.0001f)
                    {
                        float totalAngle = math.radians(degreeDif);

                        for (int i = 0; i < frameCount; i++)
                        {
                            int index = nIndex + i;

                            NativeNormalFrame normalFrame = normals[index];

                            float3 xFrame = normalFrame.x;
                            float3 yFrame = normalFrame.y;

                            float t = (float)i / (frameCount - 1);
                            float contrastedT = SplineUtilityNative.GetSegmentContrastedTime(nativeSegments, i2 + 1, t);

                            float angle = totalAngle * contrastedT;
                            float sin = math.sin(angle);
                            float cos = math.cos(angle);

                            normalFrame.x = xFrame * cos + yFrame * sin;
                            normalFrame.y = yFrame * cos - xFrame * sin;

                            normals[index] = normalFrame;
                        }
                    }

                    // Explicitly set the final cached normal to the exact end rotation.
                    // Floating-point accumulation during normal generation can otherwise leave
                    // the last cached frame slightly misaligned with the spline endpoint.
                    if (i2 == nativeSegments.Length - 2)
                    {
                        normals[normals.Length - 1] = new NativeNormalFrame(xNormalEnd, yNormalEnd, zNormalEnd);
                    }
                }

                normalMap.Add(normals.Length);
            }
        }
    }
}
