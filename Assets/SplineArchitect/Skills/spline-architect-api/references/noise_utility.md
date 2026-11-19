# `NoiseUtility.cs`
Namespace: `SplineArchitect.Utility`  

### `Overview`
Utility class for generating procedural noise functions such as Perlin, Billow, Voronoi, and fractal noise.

### `Static Functions`
```csharp
public static float PerlinNoise(float xPos, 
								float zPos, 
								float scaleX, 
								float scaleZ)

public static float BillowNoise(float xPos, 
								float zPos, 
								float scaleX, 
								float scaleZ)

public static float TerraceNoise(float xPos, 
								 float zPos, 
								 float scaleX, 
								 float scaleZ, 
								 int steps)		

public static float VoronoiNoise(float xPos, 
								 float zPos, 
								 float scaleX, 
								 float scaleZ)

public static float DomainWarpedNoise(float xPos, 
								  float zPos, 
								  float scaleX, 
								  float scaleZ, 
								  float amplitude)		

public static float RidgedPerlinNoise(float xPos, 
									  float zPos, 
									  float scaleX, 
									  float scaleZ, 
									  int octaves, 
									  float frequency, 
									  float amplitude)		

public static float FBMNoise(float xPos, float zPos, float scaleX, 
													 float scaleZ, 
													 int octaves, 
													 float frequency, 
													 float amplitude)		

public static float HybridMultifractal(float xPos, float zPos, 
												   float scaleX, 
												   float scaleZ, 
												   int octaves, 
												   float frequency, 
												   float amplitude)													 
```
<hr class="custom-line1">
