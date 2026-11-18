// -----------------------------------------------------------------------------
// SplineArchitect
// Filename: NormalFrame.cs
//
// Author: Mikael Danielsson
// Date Created: 10-10-2026
// (C) 2023 Mikael Danielsson. All rights reserved.
// -----------------------------------------------------------------------------

using UnityEngine;

namespace SplineArchitect
{
    public struct NormalFrame
    {

        public Vector3 x;
        public Vector3 y;
        public Vector3 z;

        public NormalFrame(Vector3 x, Vector3 y, Vector3 z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }
    }
}