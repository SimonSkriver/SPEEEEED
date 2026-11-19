// -----------------------------------------------------------------------------
// SplineArchitect
// Filename: InstanceJob.cs
//
// Author: Mikael Danielsson
// Date Created: 16-06-2026
// (C) 2023 Mikael Danielsson. All rights reserved.
// -----------------------------------------------------------------------------

using SplineArchitect.Utility;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

namespace SplineArchitect.Jobs
{
    [BurstCompile(FloatPrecision.Low, FloatMode.Fast, OptimizeFor = OptimizeFor.Performance)]
    public struct InstanceJob : IJobParallelFor
    {
        public NativeArray<NativeSplineInstance> nativeSplineInstance;
        public NativeArray<Matrix4x4> matrices;

        // Spline
        [ReadOnly] public float splineLength;
        [ReadOnly] public float splineResolution;
        [ReadOnly] public int splineFrame;
        [ReadOnly] public int frameSpreading;
        [ReadOnly] public bool isSplineLooping;
        [ReadOnly] public Matrix4x4 splinMatrix;
        [ReadOnly] public NativeList<float> distanceMap;
        [ReadOnly] public NativeList<float3> positionMapLocal;
        [ReadOnly] public NativeList<SegmentSample> segmentSamples;
        [ReadOnly] public NativeList<NativeNormalFrame> normals;
        [ReadOnly] public NativeList<int> normalsMap;
        [ReadOnly] public NativeArray<NativeSegment> nativeSegments;
        [ReadOnly] public NativeArray<NoiseLayer> noises;

        public void Execute(int i)
        {
            int version = nativeSplineInstance[i].version;
            int processedVersion = nativeSplineInstance[i].processedVersion;

            if (version == processedVersion)
                return;

            if ((i % frameSpreading) != splineFrame)
                return;

            // General data
            float3 splinePosition = nativeSplineInstance[i].SplinePosition;
            quaternion splineRotation = nativeSplineInstance[i].SplineRotation;
            bool alignToEnd = nativeSplineInstance[i].AlignToEnd;
            RotationMode rotationMode = nativeSplineInstance[i].RotationMode;

            // Get world data
            (float3, float3, float3, float3) tupleData = DeformPointGetNormals(splinePosition, alignToEnd, rotationMode);

            // Directions
            float3 localPosition = tupleData.Item1;
            float3 yDirection = math.normalize(tupleData.Item3);
            float3 zDirection = math.normalize(tupleData.Item4);

            quaternion splineFrameRotation = quaternion.LookRotation(zDirection, yDirection);
            quaternion finalLocalRotation = math.mul(splineFrameRotation, splineRotation);

            float4x4 localMatrix = float4x4.TRS(localPosition, finalLocalRotation, nativeSplineInstance[i].Scale);
            float4x4 worldMatrix = math.mul((float4x4)splinMatrix, localMatrix);

            matrices[i] = (Matrix4x4)worldMatrix;

            // Link corssing
            float3 crossingPoint = splinePosition;
            if (alignToEnd) crossingPoint.z = splineLength - crossingPoint.z;
            float closestDis = float.MaxValue;
            int closestId = -1;
            float closestZ = 0f;
            for (int i2 = 0; i2 < nativeSegments.Length; i2++)
            {
                float z = nativeSegments[i2].zPosition;
                float dis = math.abs(z - crossingPoint.z);

                if (dis < closestDis)
                {
                    closestDis = dis;
                    closestId = i2;
                    closestZ = z;
                }
            }
            float closestLinkZPosition = closestZ;
            float closestLinkId = closestId;

            // Set link data
            NativeSplineInstance newNsi = nativeSplineInstance[i];
            newNsi.closestSegmentZPosition = closestLinkZPosition;
            newNsi.closestSegmentIndex = closestId;
            newNsi.processedVersion = version;
            nativeSplineInstance[i] = newNsi;
        }

        private (float3, float3, float3, float3) DeformPointGetNormals(float3 point, bool alignToEnd, RotationMode rotationMode)
        {
            if (alignToEnd) point.z = splineLength - point.z;
            float time = point.z / splineLength;
            float fixedTime = SplineUtilityNative.TimeToFixedTime(distanceMap, splineResolution, time, isSplineLooping);
            SegmentSample segmentSample = SplineUtilityNative.GetSegmentSample(fixedTime, segmentSamples);

            int segment = math.clamp(SplineUtility.GetSegmentIndex(nativeSegments.Length, fixedTime), 1, nativeSegments.Length - 1);
            float segmentTime = SplineUtility.GetSegmentTime(segment, nativeSegments.Length, fixedTime);

            //Is exstension
            float3 splinePoint;
            if (!isSplineLooping && (time >= 1 || time <= 0)) splinePoint = SplineUtilityNative.GetPositionExtended(nativeSegments, splineLength, time);
            else splinePoint = SplineUtilityNative.GetPositionFast(positionMapLocal, fixedTime);

            // Get normal
            NativeNormalFrame normal = SplineUtilityNative.GetNativeNormalFrame(segment, segmentTime, alignToEnd, nativeSegments, normals, normalsMap, rotationMode);
            float3 xDirection = normal.x;
            float3 yDirection = normal.y;
            float3 zDirection = normal.z;

            //Saddle skew
            float2 saddleSkew = segmentSample.saddleSkew;
            point.y += saddleSkew.y * (point.x * point.x);
            point.x += saddleSkew.x * (point.y * point.y) * -math.sign(point.x);

            //Scale
            float2 scale = segmentSample.scale;
            point.x *= scale.x;
            point.y *= scale.y;

            //NoiseLayer
            float noiseModification = segmentSample.noise;
            if (noises.Length > 0)
                point.y += NoiseUtility.GetNoiseValue(noises, point, noiseModification);

            if (rotationMode != RotationMode.LOCK_UPWARDS && rotationMode != RotationMode.FOLLOW_PROJECTION)
            {
                //Rotation
                quaternion rotation = segmentSample.zRotation;
                if (rotationMode == RotationMode.LOCK_UPWARDS_KEEP_Z_ROTATION)
                {
                    float3 axis = alignToEnd ? -zDirection : zDirection;
                    float sinHalfAngle = math.length(rotation.value.xyz) * math.sign(math.dot(rotation.value.xyz, axis));
                    rotation = new quaternion(new float4(axis * sinHalfAngle, rotation.value.w));
                }
                xDirection = math.mul(rotation, xDirection);
                yDirection = math.mul(rotation, yDirection);
            }

            //Calculate the new world position
            point = splinePoint + xDirection * point.x + yDirection * point.y;

            return (point, xDirection, yDirection, zDirection);
        }
    }
}
