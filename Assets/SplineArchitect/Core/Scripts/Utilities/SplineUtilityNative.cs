// -----------------------------------------------------------------------------
// SplineArchitect
// Filename: SplineUtilityNative.cs
//
// Author: Mikael Danielsson
// Date Created: 11-03-2025
// (C) 2023 Mikael Danielsson. All rights reserved.
// -----------------------------------------------------------------------------

using System;
using System.Collections.Generic;

using UnityEngine;
using Unity.Collections;
using Unity.Mathematics;

namespace SplineArchitect.Utility
{
    public static class SplineUtilityNative
    {
        public static float TimeToFixedTime(NativeList<float> distanceMap, float splineResolution, float time, bool loop)
        {
            time = SplineUtility.GetValidatedTime(time, loop);

            int lastIndex = distanceMap.Length - 1;

            float rawIndex = math.clamp(time / splineResolution, 0f, lastIndex);
            int i0 = math.min((int)rawIndex, lastIndex - 1);
            float frac = rawIndex - i0;

            return math.lerp(distanceMap[i0], distanceMap[i0 + 1], frac);
        }

        public static float FixedTimeToTime(NativeList<float> distanceMap, float splineResolution, float fixedTime, bool loop)
        {
            return TimeToFixedTime(distanceMap, splineResolution, fixedTime, loop);
        }

        public static float3 GetPosition(NativeArray<NativeSegment> nativeSegments, float time)
        {
            float anchorsCount = nativeSegments.Length;
            int segment = SplineUtility.GetSegmentIndex(nativeSegments.Length, time);
            return GetSegmentPosition(nativeSegments, segment - 1, SplineUtility.GetSegmentTime(segment, anchorsCount, time));
        }

        public static float3 GetSegmentPosition(NativeArray<NativeSegment> nativeSegments, int segment, float time)
        {
            segment = math.clamp(segment, 0, nativeSegments.Length - 2);

            float3 a = nativeSegments[segment].anchor;
            float3 ata = nativeSegments[segment].tangentA;
            float3 b = nativeSegments[segment + 1].anchor;
            float3 btb = nativeSegments[segment + 1].tangentB;

            return BezierUtility.Cubic(a, ata, btb, b, time);
        }

        public static float3 GetPositionFast(NativeList<float3> positionMap, float time)
        {
            int lastIndex = positionMap.Length - 1;
            float rawIndex = math.saturate(time) * lastIndex;
            int i0 = math.min((int)rawIndex, lastIndex - 1);
            float frac = rawIndex - i0;

            return math.lerp(positionMap[i0], positionMap[i0 + 1], frac);
        }

        public static float3 GetPositionExtended(NativeArray<NativeSegment> nativeSegments, float splineLength, float time)
        {
            int segementIndex = 0;
            float3 position = nativeSegments[0].anchor;
            if (time >= 1)
            {
                segementIndex = nativeSegments.Length - 1;
                position = nativeSegments[segementIndex].anchor;
            }

            float3 direction = -GetSegmentDirection(nativeSegments, segementIndex);

            if (time < 0)
            {
                direction = -direction;
                time = math.abs(time);
            }
            else if (time > 1)
                time -= 1;
            else
                return position;

            return position + direction * (time * splineLength);
        }

        public static float3 GetSegmentDirection(NativeArray<NativeSegment> nativeSegments, int segment)
        {
            if (segment == 0)
            {
                return math.normalize(nativeSegments[segment].tangentB - nativeSegments[segment].anchor);
            }

            return math.normalize(nativeSegments[segment].anchor - nativeSegments[segment].tangentA);
        }

        public static float3 GetDirection(NativeArray<NativeSegment> nativeSegments, float time)
        {
            if (time <= 0.00001f)
                return math.normalize(nativeSegments[0].anchor - nativeSegments[0].tangentB);
            else if (time >= 0.99999f)
                return math.normalize(nativeSegments[nativeSegments.Length - 1].tangentA - nativeSegments[nativeSegments.Length - 1].anchor);

            time = math.clamp(time, 0, 1);

            int segement = SplineUtility.GetSegmentIndex(nativeSegments.Length, time);
            if (segement < 1) segement = 1;

            float segementTime = SplineUtility.GetSegmentTime(segement, nativeSegments.Length, time);
            return BezierUtility.GetDirection(nativeSegments[segement - 1].anchor, nativeSegments[segement - 1].tangentA, 
                                              nativeSegments[segement].tangentB, nativeSegments[segement].anchor, segementTime);
        }

        public static float GetSegmentContrastedTime(NativeArray<NativeSegment> nativeSegments, int segment, float segmentTime)
        {
            float contrast = math.lerp(nativeSegments[segment - 1].contrast, nativeSegments[segment].contrast, segmentTime);
            float numerator = math.pow(segmentTime, contrast);
            float denominator = numerator + math.pow(1f - segmentTime, contrast);
            float contrastedTime = numerator / denominator;
            if (float.IsNaN(contrastedTime)) contrastedTime = 1f;

            return contrastedTime;
        }

        public static float2 GetSegmentSampleScale(float time, NativeList<SegmentSample> segmentSamples)
        {
            int lastIndex = segmentSamples.Length - 1;
            float rawIndex = math.saturate(time) * lastIndex;
            int i0 = math.min((int)rawIndex, lastIndex - 1);
            float fraction = rawIndex - i0;

            return math.lerp(segmentSamples[i0].scale, segmentSamples[i0 + 1].scale, fraction);
        }

        public static quaternion GetSegmentSampleZRotation(float time, NativeList<SegmentSample> segmentSamples)
        {
            int lastIndex = segmentSamples.Length - 1;
            float rawIndex = math.saturate(time) * lastIndex;
            int i0 = math.min((int)rawIndex, lastIndex - 1);
            float fraction = rawIndex - i0;

            return math.nlerp(segmentSamples[i0].zRotation, segmentSamples[i0 + 1].zRotation, fraction);
        }

        public static float2 GetSegmentSampleSaddleSkew(float time, NativeList<SegmentSample> segmentSamples)
        {
            int lastIndex = segmentSamples.Length - 1;
            float rawIndex = math.saturate(time) * lastIndex;
            int i0 = math.min((int)rawIndex, lastIndex - 1);
            float fraction = rawIndex - i0;

            return math.lerp(segmentSamples[i0].saddleSkew, segmentSamples[i0 + 1].saddleSkew, fraction);
        }

        public static float GetSegmentSampleNoise(float time, NativeList<SegmentSample> segmentSamples)
        {
            int lastIndex = segmentSamples.Length - 1;
            float rawIndex = math.saturate(time) * lastIndex;
            int i0 = math.min((int)rawIndex, lastIndex - 1);
            float fraction = rawIndex - i0;

            return math.lerp(segmentSamples[i0].noise, segmentSamples[i0 + 1].noise, fraction);
        }

        public static SegmentSample GetSegmentSample(float time, NativeList<SegmentSample> segmentSamples)
        {
            int lastIndex = segmentSamples.Length - 1;
            float rawIndex = math.saturate(time) * lastIndex;
            int i0 = math.min((int)rawIndex, lastIndex - 1);
            float fraction = rawIndex - i0;

            SegmentSample sample0 = segmentSamples[i0];
            SegmentSample sample1 = segmentSamples[i0 + 1];

            return new SegmentSample(
                math.lerp(sample0.scale, sample1.scale, fraction),
                math.nlerp(sample0.zRotation, sample1.zRotation, fraction),
                math.lerp(sample0.saddleSkew, sample1.saddleSkew, fraction),
                math.lerp(sample0.noise, sample1.noise, fraction));
        }

        public static NativeNormalFrame GetNativeNormalFrame(int segment, float segmentTime, bool alignToEnd, 
                                                        NativeArray<NativeSegment> nativeSegments, 
                                                        NativeList<NativeNormalFrame> normals,
                                                        NativeList<int> normalsMap,
                                                        RotationMode rotationMode = RotationMode.FOLLOW_SPLINE) 
        {
            NativeNormalFrame normalFrame = new NativeNormalFrame(math.right(), math.up(), math.forward());

            bool isStatic = nativeSegments.Length > 1 && nativeSegments[segment - 1].isStatic;

            if (!isStatic && rotationMode == RotationMode.FOLLOW_SPLINE)
            {
            #if UNITY_EDITOR
                // Normals map can be dirty when changing spline type.
                if (segment >= normalsMap.Length)
                    return normalFrame;
            #endif

                int frameStart = normalsMap[segment - 1];
                int frameCount = normalsMap[segment] - frameStart;

                if (frameCount <= 0)
                    return normalFrame;

                int normalIndex = frameStart + (int)(math.saturate(segmentTime) * (frameCount - 1));

                // Handle the final spline endpoint.
                if (segment == nativeSegments.Length - 1 && segmentTime > 0.99f)
                {
                    float length = nativeSegments[segment - 1].length;
                    float endThreshold = math.lerp(0.99f, 0.9999f, math.saturate(length * 0.001f));

                    if (segmentTime > endThreshold)
                        normalIndex = normals.Length - 1;
                }

                normalFrame = normals[normalIndex];

                if (alignToEnd)
                {
                    normalFrame.x = -normalFrame.x;
                    normalFrame.z = -normalFrame.z;
                }
            }
            else
            {
#if UNITY_EDITOR
                // Can only happen when trying to draw the splines triangles and the normalsMap is still dirty when changing spline type.
                if (segment >= nativeSegments.Length)
                    return normalFrame;
#endif
                if (segment < 1) 
                    segment = 1;

                if (segment == 1 && segmentTime <= 0.00001f)
                    normalFrame.z = math.normalizesafe(nativeSegments[0].anchor - nativeSegments[0].tangentB);
                else if (segment == nativeSegments.Length - 1 && segmentTime >= 0.99999f)
                    normalFrame.z = math.normalizesafe(nativeSegments[nativeSegments.Length - 1].tangentA - nativeSegments[nativeSegments.Length - 1].anchor);
                else
                {
                    normalFrame.z = BezierUtility.GetDirection(nativeSegments[segment - 1].anchor, nativeSegments[segment - 1].tangentA,
                                                            nativeSegments[segment].tangentB, nativeSegments[segment].anchor, segmentTime);
                }

                if (rotationMode == RotationMode.LOCK_UPWARDS || rotationMode == RotationMode.LOCK_UPWARDS_KEEP_Z_ROTATION)
                {
                    normalFrame.z = new float3(normalFrame.z.x, 0, normalFrame.z.z);
                    normalFrame.z = math.normalizesafe(normalFrame.z);
                }

                if (alignToEnd)
                {
                    normalFrame.z = -normalFrame.z;
                }

                //Calculate directions
                normalFrame.x = math.cross(normalFrame.z, -math.up());
                normalFrame.x = math.normalizesafe(normalFrame.x);
                normalFrame.y = math.cross(normalFrame.z, normalFrame.x);
                normalFrame.y = math.normalizesafe(normalFrame.y);
            }

            return normalFrame;
        }

        public static float GetNearestTimeRough(NativeList<float3> positionMap,
                                                Vector3 point,
                                                float fixedStep,
                                                bool ignoreYAxel = false)
        {
            float timeValue = -1;
            float distance = 999999;

            for (float t = 0; t < 1; t += fixedStep)
            {
                float3 bezierPoint = GetPositionFast(positionMap, t);
                float d2 = ignoreYAxel ? Vector2.Distance(new Vector2(bezierPoint.x, bezierPoint.z), new Vector2(point.x, point.z)) : Vector3.Distance(bezierPoint, point);

                if (d2 < distance)
                {
                    timeValue = t;
                    distance = d2;
                }
            }

            return timeValue;
        }

        public static float GetNearestTime(NativeList<float3> positionMap,
                                            float splineLength,
                                            bool loop,
                                            Vector3 point,
                                            int precision,
                                            float steps = 5,
                                            bool ignoreYAxel = false)
        {
            float fixedStep = 100f / splineLength / steps;
            if (fixedStep > 0.2f) fixedStep = 0.2f;
            if (fixedStep < 0.0001f) fixedStep = 0.0001f;

            float timeValue = GetNearestTimeRough(positionMap, point, fixedStep, ignoreYAxel);

            for (int i = precision; i > 0; i--)
            {
                //Needs to be lower then 1.999f here.
                fixedStep = fixedStep / 1.66f;
                float timeForwards = timeValue + fixedStep;
                float timeBackwards = timeValue - fixedStep;
                timeForwards = SplineUtility.GetValidatedTime(timeForwards, loop);
                timeBackwards = SplineUtility.GetValidatedTime(timeBackwards, loop);

                Vector3 pForward = GetPositionFast(positionMap, timeForwards);
                float dForward = ignoreYAxel ? Vector2.Distance(new Vector2(pForward.x, pForward.z), new Vector2(point.x, point.z)) : Vector3.Distance(pForward, point);

                Vector3 pBackwards = GetPositionFast(positionMap, timeBackwards);
                float dBackwards = ignoreYAxel ? Vector2.Distance(new Vector2(pBackwards.x, pBackwards.z), new Vector2(point.x, point.z)) : Vector3.Distance(pBackwards, point);

                if (dForward > dBackwards)
                {
                    timeValue = timeBackwards;
                }
                else
                {
                    timeValue = timeForwards;
                }
            }

            return timeValue;
        }

        public static NativeSegment ToNative(Vector3 anchor,
                                              Vector3 tangentA,
                                              Vector3 tangentB,
                                              Quaternion localRotation,
                                              bool isStatic,
                                              float length,
                                              float zPosition,
                                              float zRotation,
                                              float contrast,
                                              float noise,
                                              Vector2 sadleSkew,
                                              Vector2 scale)
        {
            return new NativeSegment(anchor,
                                     tangentA,
                                     tangentB,
                                     localRotation,
                                     isStatic,
                                     length,
                                     zPosition,
                                     zRotation,
                                     contrast,
                                     noise,
                                     sadleSkew,
                                     scale);
        }

        public static void CopyToNativeArray(List<Segment> segments, NativeArray<NativeSegment> nativeSegments, Space space)
        {
            int count = segments.Count;
            if (nativeSegments.Length < count)
                throw new ArgumentException($"nativeSegments too small: {nativeSegments.Length} < {count}");

            for (int i = 0; i < count; i++)
            {
                Segment s = segments[i];

                Vector3 a = s.GetPosition(ControlHandle.ANCHOR, space);
                Vector3 ta = s.GetPosition(ControlHandle.TANGENT_A, space);
                Vector3 tb = s.GetPosition(ControlHandle.TANGENT_B, space);

                nativeSegments[i] = ToNative(a, ta, tb, s.LocalRotation, s.IsStatic, s.length, s.zPosition, s.ZRotation, s.Contrast, s.Noise, s.SaddleSkew, s.Scale);
            }
        }

        public static NativeArray<NativeSegment> CreateNativeArray(List<Segment> segments, Space space, Allocator allocator)
        {
            NativeArray<NativeSegment> nativeSegments = new NativeArray<NativeSegment>(segments.Count, allocator);

            for (int i = 0; i < segments.Count; i++)
            {
                Segment s = segments[i];

                Vector3 a = s.GetPosition(ControlHandle.ANCHOR, space);
                Vector3 ta = s.GetPosition(ControlHandle.TANGENT_A, space);
                Vector3 tb = s.GetPosition(ControlHandle.TANGENT_B, space);

                nativeSegments[i] = ToNative(a, ta, tb, s.LocalRotation, s.IsStatic, s.length, s.ZPosition, s.ZRotation, s.Contrast, s.Noise, s.SaddleSkew, s.Scale);
            }

            return nativeSegments;
        }
    }
}
