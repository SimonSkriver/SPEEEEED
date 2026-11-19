// -----------------------------------------------------------------------------
// SplineArchitect
// Filename: ConversionUtility.cs
//
// Author: Mikael Danielsson
// Date Created: 09-02-2024
// (C) 2023 Mikael Danielsson. All rights reserved.
// -----------------------------------------------------------------------------

using System;

using UnityEngine;

namespace SplineArchitect.Utility
{
    public class ConversionUtility
    {
        public static string CapitalizeString(string value)
        {
            string newS = "";

            for (int i = 0; i < value.Length; i++)
            {
                if (value[i] == '_')
                    newS += ' ';
                else if (i == 0)
                    newS += char.ToUpper(value[i]);
                else if (i != 0)
                    newS += char.ToLower(value[i]);
            }

            return newS;
        }

        public static float[] Convert2DArrayToFlat(float[,] array2D, int ySize, int xSize)
        {
            float[] arrayFlat = new float[ySize * xSize];

            //A float[,] is stored row by row, the same layout as the flat array (y * xSize + x), so it can be copied as one block.
            if (array2D.GetLength(1) == xSize)
            {
                Buffer.BlockCopy(array2D, 0, arrayFlat, 0, ySize * xSize * sizeof(float));
                return arrayFlat;
            }

            //Only part of each row is used.
            for (int y = 0; y < ySize; y++)
            {
                for (int x = 0; x < xSize; x++)
                {
                    arrayFlat[y * xSize + x] = array2D[y, x];
                }
            }

            return arrayFlat;
        }

        public static float[] ConvertAlphamapToFlat(float[,,] array3D, int part, int ySize, int xSize)
        {
            float[] arrayFlat = new float[ySize * xSize];

            for (int y = 0; y < ySize; y++)
            {
                for (int x = 0; x < xSize; x++)
                {
                    arrayFlat[y * xSize + x] = array3D[y, x, part];
                }
            }

            return arrayFlat;
        }

        public static float[,] ConvertFlatArrayTo2D(float[] flatArray, int ySize, int xSize)
        {
            float[,] array2D = new float[ySize, xSize];

            //A float[,] is stored row by row, the same layout as the flat array (y * xSize + x), so it can be copied as one block.
            Buffer.BlockCopy(flatArray, 0, array2D, 0, ySize * xSize * sizeof(float));

            return array2D;
        }

        public static Vector2Int FlatArrayIndexTo2DIndex(int index, int width)
        {
            Vector2Int index2D = new Vector2Int(-1, -1);
            index2D.x = index % width;
            index2D.y = index / width;
            return index2D;
        }
    }
}
