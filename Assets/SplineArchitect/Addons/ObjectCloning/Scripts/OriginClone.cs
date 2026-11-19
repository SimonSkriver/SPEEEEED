// -----------------------------------------------------------------------------
// SplineArchitect
// Filename: OriginClone.cs
//
// Author: Mikael Danielsson
// Date Created: 20-09-2026
// (C) 2023 Mikael Danielsson. All rights reserved.
// -----------------------------------------------------------------------------

#if UNITY_EDITOR

using System;
using UnityEngine;

namespace SplineArchitect
{
    [Serializable]
    internal class OriginClone
    {
        public GameObject prefab;
        public bool deform = true;
        public bool incrementUvX;
        public bool incrementUvY;
        public bool incrementUv2X;
        public bool incrementUv2Y;
        public bool incrementUv3X;
        public bool incrementUv3Y;
        public bool incrementUv4X;
        public bool incrementUv4Y;
        public Vector3 positionOffset;
        public Vector3 rotation;
        public Vector3 scale = Vector3.one;
        public float probability = 1;
    }
}
#endif
