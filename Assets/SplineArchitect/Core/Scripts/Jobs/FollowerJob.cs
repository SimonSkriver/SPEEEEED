// -----------------------------------------------------------------------------
// SplineArchitect
// Filename: FollowerJob.cs
//
// Author: Mikael Danielsson
// Date Created: 06-04-2026
// (C) 2023 Mikael Danielsson. All rights reserved.
// -----------------------------------------------------------------------------

using UnityEngine;
using Unity.Burst;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Collections;

using SplineArchitect.Utility;

namespace SplineArchitect.Jobs
{
    [BurstCompile(FloatPrecision.Low, FloatMode.Default, OptimizeFor = OptimizeFor.Performance)]
    public struct FollowerJob : IJobParallelFor
    {
        public NativeArray<Vector3> newLocalPositions;
        public NativeArray<Quaternion> newLocalRotations;
        public NativeArray<Matrix4x4> newWorldMatrices;
        public NativeArray<float> closestLinkZPositions;
        public NativeArray<int> closestLinkIds;
        public NativeArray<Vector3> rightDirs;
        public NativeArray<Vector3> upDirs;
        public NativeArray<Vector3> forwardDirs;

        // Spline
        [ReadOnly] public float splineLength;
        [ReadOnly] public float splineResolution;
        [ReadOnly] public bool loop;
        [ReadOnly] public Matrix4x4 matrix;
        [ReadOnly] public NativeList<float> distanceMap;
        [ReadOnly] public NativeList<float3> positionMapLocal;
        [ReadOnly] public NativeList<SegmentSample> segmentSamples;
        [ReadOnly] public NativeList<NativeNormalFrame> normals;
        [ReadOnly] public NativeList<int> normalsMap;
        [ReadOnly] public NativeArray<NativeSegment> nativeSegments;
        [ReadOnly] public NativeArray<NoiseLayer> noises;

        // Follower
        [ReadOnly] public NativeArray<Vector3> localSplinePositions;
        [ReadOnly] public NativeArray<Quaternion> localSplineRotations;
        [ReadOnly] public NativeArray<Quaternion> combinedParentRotations;
        [ReadOnly] public NativeArray<RotationMode> rotationModeMap;
        [ReadOnly] public NativeHashMap<int, float4x4> localSpaces;
        [ReadOnly] public NativeArray<int> localSpaceMap;
        [ReadOnly] public NativeArray<bool> alignToEndMap;
        [ReadOnly] public NativeArray<float> lockPositionMap;

        public void Execute(int i)
        {
            float3 localSplinePosition = localSplinePositions[i];
            quaternion localSplineRotation = localSplineRotations[i];
            float4x4 localSpace = localSpaces[localSpaceMap[i]];
            float4x4 inverseLocalSpace = math.inverse(localSpace);
            bool alignToEnd = alignToEndMap[i];
            float lockAmount = lockPositionMap[i];
            bool lockPosition = lockAmount > 0.01f;
            RotationMode rotationMode = rotationModeMap[i];

            float3 splinePosition = math.transform(localSpace, localSplinePosition);
            float3 deformPoint = lockPosition ? localSpace.c3.xyz : splinePosition;

            (float3, NativeNormalFrame) tupleData = DeformPointGetNormals(deformPoint, inverseLocalSpace, alignToEnd, rotationMode);

            float3 newLocalPosition = tupleData.Item1;
            NativeNormalFrame normal = tupleData.Item2;

            if (rightDirs.Length > 0) rightDirs[i] = normal.x;
            if (upDirs.Length > 0) upDirs[i] = normal.y;
            if (forwardDirs.Length > 0) forwardDirs[i] = normal.z;

            quaternion newLocalRotation;

            // Set new rotation
            if (lockPosition)
            {
                float3 lockPoint = newLocalPosition;
                lockPoint += normal.z * localSplinePosition.z;
                lockPoint += normal.y * localSplinePosition.y;
                lockPoint += normal.x * localSplinePosition.x;

                float fixedTime2 = SplineUtilityNative.TimeToFixedTime(distanceMap, splineResolution, splinePosition.z / splineLength, loop);
                int segment = math.clamp(SplineUtility.GetSegmentIndex(nativeSegments.Length, fixedTime2), 1, nativeSegments.Length - 1);
                float segmentTime = SplineUtility.GetSegmentTime(segment, nativeSegments.Length, fixedTime2);

                NativeNormalFrame normals2 = SplineUtilityNative.GetNativeNormalFrame(segment, segmentTime, alignToEnd, nativeSegments, normals, normalsMap);
                newLocalRotation = math.mul(new quaternion(new float3x3(normals2.x, normals2.y, normals2.z)), localSplineRotation);

                if (!GeneralUtility.IsEqual(lockAmount, 1, 0.01f))
                {
                    (float3, NativeNormalFrame) tupleData2 = DeformPointGetNormals(splinePosition, inverseLocalSpace, alignToEnd, rotationMode);
                    lockPoint = math.lerp(tupleData2.Item1, lockPoint, lockAmount);
                }

                newLocalPosition = lockPoint;
            }
            else
            {
                quaternion combinedParentRotation = combinedParentRotations[i];
                quaternion rotation2 = new quaternion(new float3x3(normal.x, normal.y, normal.z));
                newLocalRotation = math.mul(math.mul(math.conjugate(combinedParentRotation), rotation2),
                                   math.mul(combinedParentRotation, localSplineRotation)
                );
            }

            newLocalRotations[i] = newLocalRotation;
            newLocalPositions[i] = newLocalPosition;

            if (closestLinkZPositions.Length > 0)
            {
                float crossingZ = alignToEnd ? splineLength - splinePosition.z : splinePosition.z;
                int closestId = GetClosestSegmentIndex(crossingZ);

                closestLinkZPositions[i] = nativeSegments[closestId].zPosition;
                closestLinkIds[i] = closestId;
            }
        }

        private int GetClosestSegmentIndex(float z)
        {
            int low = 0;
            int high = nativeSegments.Length - 1;

            while (low < high)
            {
                int mid = (low + high) >> 1;

                if (nativeSegments[mid].zPosition < z)
                    low = mid + 1;
                else
                    high = mid;
            }

            if (low == 0)
                return 0;

            int previous = low - 1;
            float distanceA = math.abs(nativeSegments[previous].zPosition - z);
            float distanceB = math.abs(nativeSegments[low].zPosition - z);

            return distanceA <= distanceB ? previous : low;
        }

        private (float3, NativeNormalFrame) DeformPointGetNormals(float3 deformPoint, float4x4 inverseLocalSpace, bool alignToEnd, RotationMode rotationMode)
        {
            if (alignToEnd) deformPoint.z = splineLength - deformPoint.z;
            float time = deformPoint.z / splineLength;
            float fixedTime = SplineUtilityNative.TimeToFixedTime(distanceMap, splineResolution, time, loop);

            SegmentSample segmentSample = SplineUtilityNative.GetSegmentSample(fixedTime, segmentSamples);

            int segment = math.clamp(SplineUtility.GetSegmentIndex(nativeSegments.Length, fixedTime), 1, nativeSegments.Length - 1);
            float segmentTime = SplineUtility.GetSegmentTime(segment, nativeSegments.Length, fixedTime);

            //Is exstension
            float3 splinePoint;
            if (!loop && (time >= 1 || time <= 0)) splinePoint = SplineUtilityNative.GetPositionExtended(nativeSegments, splineLength, time);
            else splinePoint = SplineUtilityNative.GetPositionFast(positionMapLocal, fixedTime);

            // Get normal
            NativeNormalFrame normal = SplineUtilityNative.GetNativeNormalFrame(segment, segmentTime, alignToEnd, nativeSegments, normals, normalsMap, rotationMode);

            //Saddle skew
            float2 saddleSkew = segmentSample.saddleSkew;
            deformPoint.y += saddleSkew.y * (deformPoint.x * deformPoint.x);
            deformPoint.x += saddleSkew.x * (deformPoint.y * deformPoint.y) * -math.sign(deformPoint.x);

            //Scale
            float2 scale = segmentSample.scale;
            deformPoint.x *= scale.x;
            deformPoint.y *= scale.y;

            //NoiseLayer
            float noiseModification = segmentSample.noise;
            if (noises.Length > 0)
                deformPoint.y += NoiseUtility.GetNoiseValue(noises, deformPoint, noiseModification);

            if (rotationMode != RotationMode.LOCK_UPWARDS && rotationMode != RotationMode.FOLLOW_PROJECTION)
            {
                //Rotation
                quaternion rotation = segmentSample.zRotation;
                normal.x = math.mul(rotation, normal.x);
                normal.y = math.mul(rotation, normal.y);
            }

            //Calculate the new world position
            deformPoint = splinePoint + normal.x * deformPoint.x + normal.y * deformPoint.y;

            // Set new position
            float3 newLocalPosition = math.transform(inverseLocalSpace, deformPoint);

            return (newLocalPosition, normal);
        }
    }
}
