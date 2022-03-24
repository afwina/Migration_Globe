// World Political Map - Globe Edition for Unity - Main Script
// Copyright 2015 - 2017 by Ramiro Oliva
// Don't modify this script - changes could be lost if you upgrade to a more recent version of WPM

//#define PAINT_MODE
//#define TRACE_CTL

using UnityEngine;
using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using Poly2Tri;

namespace WPM {

	public partial class WorldMapGlobe : MonoBehaviour {

		#region Internal variables

		// resources
		Material frontiersMat;
		Material hudMatCountry;

		// gameObjects
		GameObject countryRegionHighlightedObj;
		GameObject frontiersLayer;

		// caché and gameObject lifetime control
		Vector3[][] frontiers;
		int[][] frontiersIndices;
		List<Vector3> frontiersPoints;
		Dictionary<Vector3,Region> frontiersCacheHit;
		struct CountryRegionRef { public int countryIndex; public int regionIndex; }
		CountryRegionRef[] candidateRegions;
		Country[] sortedCountries;

		/// <summary>
		/// Country look up dictionary. Used internally for fast searching of country names.
		/// </summary>
		Dictionary<string, int>_countryLookup;
		int lastCountryLookupCount = -1;
		
		Dictionary<string, int>countryLookup {
			get {
				if (_countryLookup != null && countries.Length == lastCountryLookupCount)
					return _countryLookup;
				if (_countryLookup == null) {
					_countryLookup = new Dictionary<string,int> ();
				} else {
					_countryLookup.Clear ();
				}
				for (int k=0; k<countries.Length; k++)
					_countryLookup.Add (countries [k].name, k);
				lastCountryLookupCount = _countryLookup.Count;
				return _countryLookup;
			}
		}

		#endregion



		#region System initialization
		void ReadCountriesPackedString () {
			lastCountryLookupCount = -1;

			string frontiersFileName = "Geodata/countries110";
			TextAsset ta = Resources.Load<TextAsset> (frontiersFileName);
			string s = ta.text;
		
			string[] countryList = s.Split (new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries);
			int countryCount = countryList.Length;
			List<Country>newCountries = new List<Country> (countryCount);
			List<Vector3> regionPoints = new List<Vector3>(10000);
			List<PolygonPoint> latlons = new List<PolygonPoint>(10000);
			for (int k=0; k<countryCount; k++) {
				string[] countryInfo = countryList [k].Split (new char[] { '$' }, StringSplitOptions.RemoveEmptyEntries);
				string name = countryInfo [0];
				string continent = countryInfo [1];
				Country country = new Country (name, continent);
				string[] regions;
				if (countryInfo.Length<3) {
					regions = new string[0];
				} else {
					regions = countryInfo [2].Split (new char[] {'*'}, StringSplitOptions.RemoveEmptyEntries);
				}
				int regionCount = regions.Length;
				country.regions = new List<Region> (regionCount);

				float maxArea = 0;
				for (int r=0; r<regionCount; r++) {
					string[] coordinates = regions [r].Split (new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
					int coorCount = coordinates.Length;
					Vector2 minMaxLat = new Vector2(float.MaxValue, float.MinValue);
					Vector2 minMaxLon = new Vector2(float.MaxValue, float.MinValue);
					Region countryRegion = new Region (country, country.regions.Count);
					regionPoints.Clear();
					latlons.Clear();
					int regionPointsIndex = -1;
					for (int c=0; c<coorCount; c++) {
						float lat, lon;
						GetLatLonFromPackedString(coordinates[c], out lat, out lon);

						// Convert to sphere coordinates
						Vector3 point = GetSpherePointFromLatLon2(lat, lon);
						if (regionPointsIndex>-1 && regionPoints[regionPointsIndex]== point) {
							continue; // In fact, points sometimes repeat so this check is neccesary
						}

						PolygonPoint latlon = new PolygonPoint(lat, lon);
						latlons.Add (latlon);

						if (lat<minMaxLat.x) minMaxLat.x = lat;
						if (lat>minMaxLat.y) minMaxLat.y = lat;
						if (lon<minMaxLon.x) minMaxLon.x = lon;
						if (lon>minMaxLon.y) minMaxLon.y = lon;

						regionPointsIndex++;
						regionPoints.Add (point);
					}
					countryRegion.latlon = latlons.ToArray();
					countryRegion.points = regionPoints.ToArray();
					countryRegion.minMaxLat = minMaxLat;
					countryRegion.minMaxLon = minMaxLon;
					countryRegion.rect2D = GetRect2DFromMinMaxLatLon(minMaxLat, minMaxLon);
					Vector2 midLatLon = new Vector2( (minMaxLat.x + minMaxLat.y)/2, (minMaxLon.x + minMaxLon.y)/2);
					Vector3 normRegionCenter = GetSpherePointFromLatLon2(midLatLon.x, midLatLon.y);
					countryRegion.center = normRegionCenter;

					float area = countryRegion.rect2D.width * countryRegion.rect2D.height;
					countryRegion.rect2DArea = area;

					if (area > maxArea) {
						maxArea = area;
						country.mainRegionIndex = country.regions.Count;
						country.center = countryRegion.center;
					}
					country.regions.Add (countryRegion);
				}
				newCountries.Add (country);
			}

			countries = newCountries.ToArray();
			newCountries.Sort (regionSizeComparer);
			sortedCountries = newCountries.ToArray();
			OptimizeFrontiers ();
		}

		int regionSizeComparer(Country country1, Country country2) {
			if (country1==country2) return 0;
			if (country1.regions==null) return -1;
			if (country2.regions==null) return 1;
			Region region1 = country1.regions[country1.mainRegionIndex];
			Region region2 = country2.regions[country2.mainRegionIndex];
			if (region1.rect2DArea<region2.rect2DArea) {
				return -1;
			} else if (region1.rect2DArea> region2.rect2DArea) {
				return 1;
			} else {
				return 0;
			}
		}

		/// <summary>
		/// Used internally by the Map Editor. It will recalculate de boundaries and optimize frontiers based on new data of countries array
		/// </summary>
		public void RefreshCountryDefinition (int countryIndex, List<Region>filterRegions)
		{
			lastCountryLookupCount = -1;
			if (countryIndex >= 0 && countryIndex < countries.Length) {
				float maxArea = 0;
				Country country = countries [countryIndex];
				int regionCount = country.regions.Count;
				for (int r=0; r<regionCount; r++) {
					Vector2 minMaxLat = new Vector2(float.MaxValue, float.MinValue);
					Vector2 minMaxLon = new Vector2(float.MaxValue, float.MinValue);
					Region countryRegion = country.regions[r];
					int coorCount = countryRegion.latlon.Length;
					for (int c=0; c<coorCount; c++) {
						PolygonPoint latlon = countryRegion.latlon[c];
						latlon.Reset();
						float lat = latlon.Xf;
						float lon = latlon.Yf;
						if (lat<minMaxLat.x) minMaxLat.x = lat;
						if (lat>minMaxLat.y) minMaxLat.y = lat;
						if (lon<minMaxLon.x) minMaxLon.x = lon;
						if (lon>minMaxLon.y) minMaxLon.y = lon;
					}
					countryRegion.minMaxLat = minMaxLat;
					countryRegion.minMaxLon = minMaxLon;
					countryRegion.rect2D = GetRect2DFromMinMaxLatLon(minMaxLat, minMaxLon);
					Vector2 midLatLon = new Vector2( (minMaxLat.x + minMaxLat.y)/2, (minMaxLon.x + minMaxLon.y)/2);
					Vector3 normRegionCenter = GetSpherePointFromLatLon2(midLatLon.x, midLatLon.y);
					countryRegion.center = normRegionCenter;

					float area = countryRegion.rect2D.width * countryRegion.rect2D.height;
					countryRegion.rect2DArea = area;
					if (area > maxArea) {
						maxArea = area;
						country.mainRegionIndex = r;
						country.center = countryRegion.center;
					}
				}
			}
			// Refresh latlongs
			if (filterRegions!=null) {
				for (int k=0;k<filterRegions.Count;k++) {
					Region region = filterRegions[k];
					for (int p=0;p<region.latlon.Length;p++)
						region.latlon[p].Reset();
				}
			}
			OptimizeFrontiers (filterRegions);
			DrawFrontiers ();
		}

		/// <summary>
		/// Regenerates frontiers mesh for all countries
		/// </summary>
		void OptimizeFrontiers () {
			OptimizeFrontiers(null);
		}


		/// <summary>
		/// Generates frontiers mesh for specific regions.
		/// </summary>
		void OptimizeFrontiers (List<Region>filterRegions)
		{
			if (frontiersPoints == null) {
				frontiersPoints = new List<Vector3> (200000);
			} else {
				frontiersPoints.Clear ();
			}
			if (frontiersCacheHit == null) {
				frontiersCacheHit = new Dictionary<Vector3, Region> (200000);
			} else {
				frontiersCacheHit.Clear ();
			}
			
			for (int k=0; k<countries.Length; k++) {
				Country country = countries [k];
				for (int r=0; r<country.regions.Count; r++) {
					Region region = country.regions [r];
					if (filterRegions == null || filterRegions.Contains (region)) {
						region.entity = country;
						region.regionIndex = r;
						region.neighbours.Clear ();
					}
				}
			}

			for (int k=0; k<countries.Length; k++) {
				Country country = countries [k];
				for (int r=0; r<country.regions.Count; r++) {
					Region region = country.regions [r];
					if (filterRegions == null || filterRegions.Contains (region)) {
						int max = region.points.Length - 1;
						for (int i = 0; i<max; i++) {
							Vector3 p0 = region.points [i];
							Vector3 p1 = region.points [i + 1];
							Vector3 hc = p0 + p1; // v = p0.x + p1.x + p0.y + p1.y + p0.z + p1.z;
							if (frontiersCacheHit.ContainsKey (hc)) { // add neighbour references
								Region neighbour = frontiersCacheHit [hc];
								if (neighbour != region) {
									if (!region.neighbours.Contains (neighbour)) {
										region.neighbours.Add (neighbour);
										neighbour.neighbours.Add (region);
									}
								}
							} else {
								frontiersCacheHit.Add (hc, region);
								frontiersPoints.Add (region.points [i]);
								frontiersPoints.Add (region.points [i + 1]);
							}
						}
						// Close the polygon
						frontiersPoints.Add (region.points [max]);
						frontiersPoints.Add (region.points [0]);
					}
				}
			}

			int meshGroups = (frontiersPoints.Count / 65000) + 1;
			int meshIndex = -1;
			frontiersIndices = new int[meshGroups][];
			frontiers = new Vector3[meshGroups][];
			for (int k=0; k<frontiersPoints.Count; k+=65000) {
				int max = Mathf.Min (frontiersPoints.Count - k, 65000); 
				frontiers [++meshIndex] = new Vector3[max];
				frontiersIndices [meshIndex] = new int[max];
				for (int j=k; j<k+max; j++) {
					frontiers [meshIndex] [j - k] = frontiersPoints [j];
					frontiersIndices [meshIndex] [j - k] = j - k;
				}
			}
		}
	
	#endregion

	#region Drawing stuff

		
		int GetCacheIndexForCountryRegion (int countryIndex, int regionIndex) {
			return countryIndex * 1000 + regionIndex;
		}
	
		void DrawFrontiers () {
			if (!gameObject.activeInHierarchy || frontiers==null)
				return;
			
			// Create frontiers layer
			Transform t = transform.Find ("Frontiers");
			if (t != null)
				DestroyImmediate (t.gameObject);
			frontiersLayer = new GameObject ("Frontiers");
			frontiersLayer.layer = gameObject.layer;
			frontiersLayer.transform.SetParent (transform, false);
			frontiersLayer.transform.localPosition = MiscVector.Vector3zero;
			frontiersLayer.transform.localRotation = Quaternion.Euler (MiscVector.Vector3zero);
			frontiersLayer.transform.localScale = MiscVector.Vector3one;

			for (int k=0; k<frontiers.Length; k++) {
				GameObject flayer = new GameObject ("flayer");
				flayer.layer = gameObject.layer;
				flayer.transform.SetParent (frontiersLayer.transform, false);
				flayer.transform.localPosition = MiscVector.Vector3zero;
				flayer.transform.localRotation = Quaternion.Euler (MiscVector.Vector3zero);

				Mesh mesh = new Mesh ();
				mesh.vertices = frontiers [k];
				mesh.SetIndices (frontiersIndices [k], MeshTopology.Lines, 0);
				mesh.RecalculateBounds ();
				mesh.hideFlags = HideFlags.DontSave;

				MeshFilter mf = flayer.AddComponent<MeshFilter> ();
				mf.sharedMesh = mesh;

				MeshRenderer mr = flayer.AddComponent<MeshRenderer> ();
				mr.receiveShadows = false;
				mr.reflectionProbeUsage = UnityEngine.Rendering.ReflectionProbeUsage.Off;
				mr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
				mr.sharedMaterial = frontiersMat;
			}

			// Toggle frontiers visibility layer according to settings
			frontiersLayer.SetActive (_showFrontiers);
		}

	#endregion



	#region Country highlighting

	
		bool GetCountryUnderMouse (Vector3 spherePoint, out int countryIndex, out int regionIndex)
		{
			PolygonPoint latlonPos = GetLatLonFromSpherePoint(spherePoint);

			// Check if current country is still under mouse
			if (_countryHighlightMinSize == 0 && _countryHighlightedIndex>=0 && _countryRegionHighlightedIndex>=0) {
				Region region = countries[_countryHighlightedIndex].regions[_countryRegionHighlightedIndex];
				if (ContainsPoint2D (region.latlon, latlonPos.X, latlonPos.Y)) {
					countryIndex = _countryHighlightedIndex;
					regionIndex = _countryRegionHighlightedIndex;
					return true;
				}
			}

			// Check other countries
			countryIndex = regionIndex = -1;

			// Get regions list sorted by distance from mouse point
			Vector2 mouseBillboardPos = Drawing.SphereToBillboardCoordinates(spherePoint);
			if (candidateRegions==null) candidateRegions = new CountryRegionRef[100];
			int regionsAdded = -1;
			int nearestC = -1;
			int nearestR = -1;
			float nearestFingerDistance = float.MaxValue;
			for (int c=0; c<sortedCountries.Length; c++) {
				int regionCount = sortedCountries[c].regions.Count;
				for (int cr=0; cr<regionCount; cr++) {
					Region region = sortedCountries [c].regions [cr];
					Rect rect = region.rect2D;
					if (rect.width < _countryHighlightMinSize || rect.height < _countryHighlightMinSize) {
						float w = Mathf.Max(_countryHighlightMinSize, rect.width);
						float h = Mathf.Max(_countryHighlightMinSize, rect.height);
						Vector2 newSize = new Vector2(w, h);
						rect.position = rect.center - newSize / 2f;
						rect.size = newSize;
					}
					if (rect.Contains(mouseBillboardPos)) {
						++regionsAdded;
						int cIndex = GetCountryIndex(sortedCountries[c]);
						candidateRegions[regionsAdded].countryIndex = cIndex;
						candidateRegions[regionsAdded].regionIndex = cr;
						if (_countryHighlightMinSize>0) {
							float pointerDistanceToFinger = (mouseBillboardPos - rect.center).sqrMagnitude;
							if (pointerDistanceToFinger < nearestFingerDistance) {
								nearestFingerDistance = pointerDistanceToFinger;
								nearestC = cIndex;
								nearestR = cr;
							}
						}
					}
				}
			}
			if (regionsAdded<0) return false;

			if (_countryHighlightMinSize>0) {
				countryIndex = nearestC;
				regionIndex = nearestR;
				return true;
			}

			for (int t=0;t<=regionsAdded;t++) {
				// Check if this region is visible and the mouse is inside
				int c = candidateRegions[t].countryIndex;
				int cr = candidateRegions[t].regionIndex;
				Region region = countries[c].regions[cr];
				if (ContainsPoint2D (region.latlon, latlonPos.X, latlonPos.Y)) {
					countryIndex = c;
					regionIndex = cr;
					return true;
				}
			}
			return false;
		}

		/// <summary>
		/// Disables all country regions highlights. This doesn't remove custom materials.
		/// </summary>
		public void HideCountryRegionHighlights (bool destroyCachedSurfaces) {
			HideCountryRegionHighlight();
			if (countries==null) return;
			for (int c=0;c<countries.Length;c++) {
				Country country = countries[c];
				if (country==null || country.regions == null) continue;
				for (int cr=0;cr<country.regions.Count;cr++) {
					Region region = country.regions[cr];
					int cacheIndex = GetCacheIndexForCountryRegion(c, cr);
					if (surfaces.ContainsKey(cacheIndex)) {
						GameObject surf = surfaces[cacheIndex];
						if (surf==null) {
							surfaces.Remove(cacheIndex);
						} else {
							if (destroyCachedSurfaces) {
								surfaces.Remove(cacheIndex);
								DestroyImmediate(surf);
							} else {
								if (region.customMaterial==null) {
									surf.SetActive(false);
								} else {
									ApplyMaterialToSurface (surf, region.customMaterial);
								}
							}
						}
					}
				}
			}
		}
		
		void HideCountryRegionHighlight () {
			if (_countryRegionHighlighted!=null && countryRegionHighlightedObj != null) {
				if (_countryRegionHighlighted!=null && _countryRegionHighlighted.customMaterial!=null) {
					ApplyMaterialToSurface (countryRegionHighlightedObj, _countryRegionHighlighted.customMaterial);
				} else {
					countryRegionHighlightedObj.SetActive (false);
				}
				countryRegionHighlightedObj = null;
			}
			_countryHighlighted = null;
			_countryHighlightedIndex = -1;
			_countryRegionHighlighted = null;
			_countryRegionHighlightedIndex = -1;
		}


		public GameObject HighlightCountryRegion (int countryIndex, int regionIndex, bool refreshGeometry, bool drawOutline) {
#if PAINT_MODE
			ToggleCountrySurface(countryIndex, true, Color.white);
			return null; 
#else

			if (countryRegionHighlightedObj!=null) HideCountryRegionHighlight();
			if (countryIndex<0 || countryIndex>=countries.Length || regionIndex<0 || regionIndex>=countries[countryIndex].regions.Count) return null;
			int cacheIndex = GetCacheIndexForCountryRegion (countryIndex, regionIndex); 
			bool existsInCache = surfaces.ContainsKey (cacheIndex);
			if (refreshGeometry && existsInCache) {
				GameObject obj = surfaces [cacheIndex];
				surfaces.Remove(cacheIndex);
				DestroyImmediate(obj);
				existsInCache = false;
			}
			if (existsInCache) {
				countryRegionHighlightedObj = surfaces [cacheIndex];
				if (countryRegionHighlightedObj==null) {
					surfaces.Remove(cacheIndex);
				} else {
					if (!countryRegionHighlightedObj.activeSelf)
						countryRegionHighlightedObj.SetActive (true);
					Renderer rr = countryRegionHighlightedObj.GetComponent<Renderer> ();
					if (rr.sharedMaterial!=hudMatCountry)
						rr.sharedMaterial = hudMatCountry;
				}
			} else {
				countryRegionHighlightedObj = GenerateCountryRegionSurface (countryIndex, regionIndex, hudMatCountry, drawOutline);
			}
			_countryHighlightedIndex = countryIndex;
			_countryRegionHighlighted = countries[countryIndex].regions[regionIndex];
			_countryRegionHighlightedIndex = regionIndex;
			_countryHighlighted = countries[countryIndex];
			return countryRegionHighlightedObj;
#endif

		}

		GameObject GenerateCountryRegionSurface (int countryIndex, int regionIndex, Material material, bool drawOutline) {
			Country country = countries[countryIndex];
			Region region = country.regions [regionIndex];
		
			Polygon poly = new Polygon(region.latlon);

			double maxTriangleSize = 10.0f;
			if (countryIndex == 6) { // Antarctica
				maxTriangleSize = 9.0f;
			}
			if ( Mathf.Abs (region.minMaxLat.x - region.minMaxLat.y) > maxTriangleSize || 
			    Mathf.Abs (region.minMaxLon.x - region.minMaxLon.y) > maxTriangleSize) { // special case; needs steiner points to reduce the size of triangles
				double step = maxTriangleSize/2;
				List<TriangulationPoint> steinerPoints = new List<TriangulationPoint>();
				for (double x = region.minMaxLat.x + step/2; x<region.minMaxLat.y - step/2;x += step) {
					for (double y = region.minMaxLon.x + step /2;y<region.minMaxLon.y - step / 2;y += step) {
						if (ContainsPoint2D(region.latlon, x, y)) {
							steinerPoints.Add(new TriangulationPoint(x, y));
						}
					}
				}
				poly.AddSteinerPoints(steinerPoints);
			}

			P2T.Triangulate(poly);

			int flip1, flip2;
			flip1 = 1; flip2 = 2;
			Vector3[] revisedSurfPoints = new Vector3[poly.Triangles.Count*3];
			for (int k=0;k<poly.Triangles.Count;k++) {
				DelaunayTriangle dt = poly.Triangles[k];
				revisedSurfPoints[k*3] = GetSpherePointFromLatLon2(dt.Points[0].X, dt.Points[0].Y);
				revisedSurfPoints[k*3+flip1] = GetSpherePointFromLatLon2(dt.Points[1].X, dt.Points[1].Y);
				revisedSurfPoints[k*3+flip2] = GetSpherePointFromLatLon2(dt.Points[2].X, dt.Points[2].Y);
			}
			int revIndex = revisedSurfPoints.Length-1;

			// Generate surface mesh
			int cacheIndex = GetCacheIndexForCountryRegion (countryIndex, regionIndex); 
			string cacheIndexSTR = cacheIndex.ToString();
			Transform t = surfacesLayer.transform.Find(cacheIndexSTR);
			if (t!=null) DestroyImmediate(t.gameObject);
			GameObject surf = Drawing.CreateSurface (cacheIndexSTR, revisedSurfPoints, revIndex, material);									
			surf.layer = gameObject.layer;
			surf.transform.SetParent (surfacesLayer.transform, false);
			surf.transform.localPosition = MiscVector.Vector3zero;
			surf.transform.localRotation = Quaternion.Euler (MiscVector.Vector3zero);
			if (surfaces.ContainsKey(cacheIndex)) surfaces.Remove(cacheIndex);
			surfaces.Add (cacheIndex, surf);

			// draw outline
			if (drawOutline) {
				DrawCountryRegionOutline(region, surf);
			}
			return surf;
		}

		/// <summary>
		/// Draws the country outline.
		/// </summary>
		GameObject DrawCountryRegionOutline (Region region, GameObject surf)
		{
			int[] indices = new int[region.points.Length + 1];
			Vector3[] outlinePoints = new Vector3[region.points.Length + 1];
			for (int k=0; k<region.points.Length; k++) {
				indices [k] = k;
				outlinePoints [k] = region.points [k]; // + (region.points [k] - region.center).normalized * 0.0001f;
			}
			indices [region.points.Length] = indices [0];
			outlinePoints [region.points.Length] = region.points [0]; // + (region.points [0] - region.center).normalized * 0.0001f;

			GameObject boldFrontiers = new GameObject ("boldFrontiers");
			boldFrontiers.layer = gameObject.layer;
			boldFrontiers.transform.SetParent (surf.transform, false);
			boldFrontiers.transform.localPosition = MiscVector.Vector3zero;
			boldFrontiers.transform.localRotation = Quaternion.Euler (MiscVector.Vector3zero);

			Mesh mesh = new Mesh ();
			mesh.vertices = outlinePoints; //region.points;
		
			mesh.SetIndices (indices, MeshTopology.LineStrip, 0);
			mesh.RecalculateBounds ();
			mesh.hideFlags = HideFlags.DontSave | HideFlags.HideInHierarchy;
				
			MeshFilter mf = boldFrontiers.AddComponent<MeshFilter> ();
			mf.sharedMesh = mesh;
				
			MeshRenderer mr = boldFrontiers.AddComponent<MeshRenderer> ();
			mr.receiveShadows = false;
			mr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
			mr.sharedMaterial = outlineMat;
			return boldFrontiers;
		}

		#endregion

		
		#region Geometric functions

		
		Quaternion GetCountryQuaternion (int countryIndex) {
			return GetQuaternion (countries [countryIndex].center);
		}

		Quaternion GetCountryRegionQuaternion (int countryIndex, int regionIndex) {
			return GetQuaternion (countries [countryIndex].regions [regionIndex].center);
		}

		#endregion
	
	}

}