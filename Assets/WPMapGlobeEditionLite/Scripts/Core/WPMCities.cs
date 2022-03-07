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
using System.Globalization;
using Poly2Tri;

namespace WPM {

	public partial class WorldMapGlobe : MonoBehaviour {

		const float CITY_HIT_PRECISION = 0.00085f;

		#region Internal variables

		// resources
		Material citiesMat;
		GameObject citiesLayer, citySpot;

		#endregion



	#region System initialization

		void ReadCitiesPackedString () {
			string cityCatalogFileName = "Geodata/cities50";
			TextAsset ta = Resources.Load<TextAsset> (cityCatalogFileName);
			string s = ta.text;
			ReadCitiesPackedString(s);
		}

		/// <summary>
		/// Reads the cities data from a packed string.
		/// </summary>
		void ReadCitiesPackedString (string s) {
			string[] cityList = s.Split (new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries);
			int cityCount = cityList.Length;
			cities = new List<City> (cityCount);
            Vector3 v;

			for (int k=0; k<cityCount; k++) {
				string[] cityInfo = cityList [k].Split (new char[] { '$' }, StringSplitOptions.RemoveEmptyEntries);
				string country = cityInfo [1];
				int countryIndex = GetCountryIndex(country);
				if (countryIndex>=0) {
					string name = cityInfo [0];
					int population = int.Parse (cityInfo [2]);
                    v.x = float.Parse(cityInfo[3], CultureInfo.InvariantCulture) / MAP_PRECISION;
                    v.y = float.Parse(cityInfo[4], CultureInfo.InvariantCulture) / MAP_PRECISION;
                    v.z = float.Parse(cityInfo[5], CultureInfo.InvariantCulture) / MAP_PRECISION;
                    City city = new City(name, countryIndex, population, v);
					cities.Add (city);
				}
			}
		}
	
	#endregion

	#region Drawing stuff

		/// <summary>
		/// Redraws the cities. This is automatically called by Redraw(). Used internally by the Map Editor. You should not need to call this method directly.
		/// </summary>
		public void DrawCities () {
			// Create cities layer
			Transform t = transform.Find ("Cities");
			if (t != null)
				DestroyImmediate (t.gameObject);
			citiesLayer = new GameObject ("Cities");
			citiesLayer.transform.SetParent (transform, false);

			// Draw city marks
			_numCitiesDrawn = 0;
			int minPopulation = _minPopulation * 1000;
			int cityCount = cities.Count;
            Transform parent = citiesLayer.transform;
			for (int k=0; k<cityCount; k++) {
				City city = cities [k];
				if (city.population >= minPopulation) {
					GameObject cityObj = Instantiate (citySpot);
					cityObj.GetComponent<Renderer> ().sharedMaterial = citiesMat;
					CityInfo cityInfo = cityObj.GetComponent<CityInfo> ();
					cityInfo.city = city;
					cityInfo.wmp = this;
					cityObj.name = k.ToString();
					cityObj.layer = gameObject.layer;
					cityObj.transform.SetParent (parent, false);
					cityObj.transform.localPosition = city.unitySphereLocation; // * 1.001f;
					cityObj.transform.LookAt (transform.position);
					cityObj.hideFlags = HideFlags.HideInHierarchy;
					_numCitiesDrawn++;
				}
			}

			// Toggle cities layer visibility according to settings
			citiesLayer.SetActive (_showCities);

			CityScaler cityScaler = citiesLayer.GetComponent<CityScaler>() ?? citiesLayer.AddComponent<CityScaler>();
			cityScaler.map = this;
			cityScaler.ScaleCities();
		}

	#endregion

		#region Internal Cities API

		/// <summary>
		/// Returns any city near the point specified in local coordinates.
		/// </summary>
		int GetCityNearPoint(Vector3 localPoint) {
			for (int c=0;c<cities.Count;c++) {
				Vector3 cityLoc = cities[c].unitySphereLocation;
				if ( (cityLoc-localPoint).magnitude < CITY_HIT_PRECISION) {
					return c;
				}
			}
			return -1;
		}


		/// <summary>
		/// Returns cities belonging to a provided country.
		/// </summary>
		List<City>GetCities(int countryIndex) {
			List<City>results = new List<City>(20);
			for (int c=0;c<cities.Count;c++) {
				if (cities[c].countryIndex==countryIndex) results.Add (cities[c]);
			}
			return results;
		}

		/// <summary>
		/// Updates the city scale.
		/// </summary>
		public void ScaleCities() {
			if (citiesLayer!=null) {
				CityScaler scaler = citiesLayer.GetComponent<CityScaler>();
				if (scaler!=null) scaler.ScaleCities();
			}
		}

		#endregion
	}

}