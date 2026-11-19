// -----------------------------------------------------------------------------
// SplineArchitect
// Filename: DeformJob.cs
//
// Author: Mikael Danielsson
// Date Created: 12-02-2023
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
    public struct DeformJob : IJobParallelFor
    {
        public NativeArray<Vector3> vertices;
        public NativeArray<Vector3> meshNormals;
        public NativeArray<Vector4> meshTangents;
        public NativeArray<RaycastCommand> raycastCommands;

        [ReadOnly] public int splineObjectCount;
        [ReadOnly] public NativeArray<int> localSpaceMap;
        [ReadOnly] public NativeArray<MapsDataDeformation> mapsData;
        [ReadOnly] public NativeArray<NativeSegment> nativeSegments;
        [ReadOnly] public NativeArray<NoiseLayer> noises;
        [ReadOnly] public float4x4 splineLocalToWorldMatrix;
        [ReadOnly] public float splineLength;
        [ReadOnly] public NativeList<float> distanceMap;
        [ReadOnly] public NativeList<NativeNormalFrame> normals;
        [ReadOnly] public NativeList<int> normalsMap;
        [ReadOnly] public NativeList<SegmentSample> segmentSamples;
        [ReadOnly] public NativeList<float3> positionMapLocal;
        [ReadOnly] public float splineResolution;
        [ReadOnly] public bool loop;

        public void Execute(int i)
        {
            float3 vertice = vertices[i];

            float4x4 localSpace;
            bool alignToEnd = false;
            bool skipTangents = false;
            bool createRayCastCommand = false;
            LayerMask layerMask = ~(1 << 2);
            NormalType normalType = NormalType.SPLINE_SPACE;
            SnapData snapData = new SnapData();
            localSpace = float4x4.identity;

            for (int i2 = 0; i2 < splineObjectCount; i2++)
            {
                if (i < localSpaceMap[i2])
                {
                    if(mapsData[i2].mirrorDeformation)
                        vertice = -vertice;

                    alignToEnd = mapsData[i2].alignToEnd;
                    skipTangents = mapsData[i2].skipTangents;
                    layerMask = mapsData[i2].layerMask;
                    normalType = mapsData[i2].normalType;
                    snapData = mapsData[i2].snapData;
                    createRayCastCommand = mapsData[i2].createRayCastCommand;
                    localSpace = mapsData[i2].localSpace;

                    break;
                }
            }

            //Vertice to spline position
            vertice = math.transform(localSpace, vertice);

            //Snapping, needs to be before time.
            if (snapData.end || snapData.start)
            {
                if (snapData.end && snapData.start)
                {
                    float point = snapData.soStartPoint + (snapData.soEndPoint - snapData.soStartPoint) / 2;

                    float partLength = vertice.z - point;
                    float length = math.abs(snapData.soEndPoint - snapData.soStartPoint) / 2;

                    if(partLength > 0) vertice.z = math.lerp(point, snapData.snapEndPoint, math.abs(partLength) / length);
                    else vertice.z = math.lerp(point, snapData.snapStartPoint, math.abs(partLength) / length);
                }
                else if (snapData.start)
                {
                    float partLength = math.abs(vertice.z - snapData.soEndPoint);
                    float length = math.abs(snapData.soStartPoint - snapData.soEndPoint);
                    vertice.z = math.lerp(snapData.soEndPoint, snapData.snapStartPoint, partLength / length);
                }
                else
                {
                    float partLength = math.abs(vertice.z - snapData.soStartPoint);
                    float length = math.abs(snapData.soEndPoint - snapData.soStartPoint);
                    vertice.z = math.lerp(snapData.soStartPoint, snapData.snapEndPoint, partLength / length);
                }
            }

            if (alignToEnd) vertice.z = splineLength - vertice.z;
            float time = vertice.z / splineLength;
            float fixedTime = SplineUtilityNative.TimeToFixedTime(distanceMap, splineResolution, time, loop);

            int segment = SplineUtility.GetSegmentIndex(nativeSegments.Length, fixedTime);
            float segmentTime = SplineUtility.GetSegmentTime(segment, nativeSegments.Length, fixedTime);
            SegmentSample segmentSample = SplineUtilityNative.GetSegmentSample(fixedTime, segmentSamples);
            float3 splinePoint;

            //Is exstension
            if (!loop && (time >= 1 || time <= 0)) splinePoint = SplineUtilityNative.GetPositionExtended(nativeSegments, splineLength, time);
            else splinePoint = SplineUtilityNative.GetPositionFast(positionMapLocal, fixedTime);

            // Get normal
            NativeNormalFrame normal = SplineUtilityNative.GetNativeNormalFrame(segment, segmentTime, alignToEnd, nativeSegments, normals, normalsMap);
            float3 xDirection = normal.x;
            float3 yDirection = normal.y;
            float3 zDirection = normal.z;

            //Saddle skew
            float2 saddleSkew = segmentSample.saddleSkew;
            bool saddleSkewIsEnabled = !GeneralUtility.IsZeroFast(saddleSkew, 0.001f);
            if (saddleSkewIsEnabled)
            {
                vertice.y += saddleSkew.y * (vertice.x * vertice.x);
                vertice.x += saddleSkew.x * (vertice.y * vertice.y) * -math.sign(vertice.x);
            }

            //Scale
            float2 scale = segmentSample.scale;
            bool scaleIsEnabled = !GeneralUtility.IsEqualFast(scale, new float2(1f, 1f), 0.001f);

            vertice.x *= scale.x;
            vertice.y *= scale.y;

            //NoiseLayer
            float noiseModification = segmentSample.noise;
            bool noiseIsEnabled = noises.Length > 0;
            if (noiseIsEnabled)
            {
                vertice.y += NoiseUtility.GetNoiseValue(noises, vertice, noiseModification);
            }

            bool effectsEnabled = saddleSkewIsEnabled || scaleIsEnabled || noiseIsEnabled;

            float3x3 effectMatrix = float3x3.identity;
            if (effectsEnabled)
                effectMatrix = GeEffectsMatrix(saddleSkew, vertice, scale, noiseModification);

            //Rotation
            quaternion zRotation = segmentSample.zRotation;
            if (alignToEnd) zRotation = math.inverse(zRotation);
            xDirection = math.mul(zRotation, xDirection);
            yDirection = math.mul(zRotation, yDirection);

            if(time > 0.998f)
            {
                // Small adjdustment for end vertices.
                float thresholdTime = math.saturate(splineLength / 1000f);
                float endThreshold = math.lerp(0.999f, 0.99999f, thresholdTime);

                if(time > endThreshold)
                {
                    splinePoint += xDirection * 0.0000256f;
                    splinePoint += yDirection * 0.0000256f;
                    splinePoint += zDirection * 0.0008f;
                }
            }
            else if(time < 0.002f)
            {
                // Small adjdustment for start vertices.
                float thresholdTime = math.saturate(splineLength / 1000f);
                float endThreshold = math.lerp(0.001f, 0.00001f, thresholdTime);

                if(time < endThreshold)
                {
                    splinePoint -= xDirection * 0.0000256f;
                    splinePoint -= yDirection * 0.0000256f;
                    splinePoint -= zDirection * 0.0008f;
                }
            }

            //Calculate the new world position
            vertice = splinePoint + xDirection * vertice.x + yDirection * vertice.y;

            if(createRayCastCommand)
            {
                //Create raycast command
                Vector3 direction = math.rotate(splineLocalToWorldMatrix, new float3(0, -1, 0));
                raycastCommands[i] = new RaycastCommand(math.transform(splineLocalToWorldMatrix, vertice), direction, new QueryParameters(layerMask), float.MaxValue);
            }

            //To objects local space
            vertices[i] = math.transform(math.inverse(localSpace), vertice);

            // Normals
            if (meshNormals.Length > 0)
            {
                float3 meshNormal = meshNormals[i];

                if (normalType == NormalType.SPLINE_SPACE)
                {
                    float3x3 localSpaceMatrix = (float3x3)localSpace;
                    float3x3 worldToLocalMatrix = (float3x3)math.inverse(localSpace);

                    float3 worldNormal = math.mul(math.transpose(worldToLocalMatrix), meshNormal);
                    worldNormal = math.normalize(worldNormal);

                    if (effectsEnabled)
                    {
                        float3x3 em = math.transpose(math.inverse(effectMatrix));
                        worldNormal = math.mul(em, worldNormal);
                        worldNormal = math.normalize(worldNormal);
                    }

                    float3 newWorldNormal = xDirection * worldNormal.x +
                                            yDirection * worldNormal.y +
                                            zDirection * worldNormal.z;

                    float3 newLocalNormal = math.mul(math.transpose(localSpaceMatrix), newWorldNormal);
                    newLocalNormal = math.normalize(newLocalNormal);
                    meshNormals[i] = newLocalNormal;
                }
                else if (normalType == NormalType.CYLINDER_BASED)
                {
                    splinePoint = math.transform(math.inverse(localSpace), splinePoint);
                    float3 newLocalNormal = (float3)vertices[i] - splinePoint;
                    newLocalNormal = math.normalize(newLocalNormal);
                    meshNormals[i] = newLocalNormal;
                }

                // Tangent
                if (meshTangents.Length > 0 && !skipTangents)
                {
                    float3 tangent = new Vector3(meshTangents[i].x, meshTangents[i].y, meshTangents[i].z);
                    float3 worldTangent = math.mul((float3x3)localSpace, tangent);

                    if (effectsEnabled)
                        worldTangent = math.mul(effectMatrix, worldTangent);

                    float3 newWorldTangent = xDirection * worldTangent.x +
                                             yDirection * worldTangent.y +
                                             zDirection * worldTangent.z;

                    float3 newLocalTangent = math.mul((float3x3)math.inverse(localSpace), newWorldTangent);
                    newLocalTangent = newLocalTangent - (float3)meshNormals[i] * math.dot(newLocalTangent, meshNormals[i]);
                    newLocalTangent = math.normalize(newLocalTangent);

                    if (!math.all(math.isfinite(newLocalTangent)))
                        newLocalTangent = new float3(1, 0, 0);

                    meshTangents[i] = new Vector4(newLocalTangent.x, newLocalTangent.y, newLocalTangent.z, meshTangents[i].w);
                }
            }
        }

        private float3x3 GeEffectsMatrix(float2 saddleSkew, float3 vertice, float2 scale, float noiseModification)
        {
            float dyDx = 2f * saddleSkew.y * vertice.x;
            float dxDy = -2f * saddleSkew.x * math.sign(vertice.x) * (vertice.y / scale.y);

            float dxDx = 1f + dxDy * dyDx;

            float3x3 matrix = new float3x3(new float3(scale.x * dxDx, scale.y * dyDx, 0f), new float3(scale.x * dxDy, scale.y, 0f), new float3(0f, 0f, 1f));

            if(noises.Length > 0)
            {            
                float epsilon = 0.01f;

                float noiseXMinus = NoiseUtility.GetNoiseValue(noises, vertice - new float3(epsilon, 0f, 0f), noiseModification);
                float noiseXPlus = NoiseUtility.GetNoiseValue(noises, vertice + new float3(epsilon, 0f, 0f), noiseModification);

                float noiseDx = (noiseXPlus - noiseXMinus) / (2f * epsilon);

                float noiseZMinus = NoiseUtility.GetNoiseValue(noises, vertice - new float3(0f, 0f, epsilon), noiseModification);
                float noiseZPlus = NoiseUtility.GetNoiseValue(noises, vertice + new float3(0f, 0f, epsilon), noiseModification);
                float noiseDz = (noiseZPlus - noiseZMinus) / (2f * epsilon);

                float3x3 matrix2 = new float3x3(new float3(1f, noiseDx, 0f), new float3(0f, 1f, 0f), new float3(0f, noiseDz, 1f));
                matrix = math.mul(matrix2, matrix);
            }

            return matrix;
        }
    }
}
