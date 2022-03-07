#define LIGHTSPEED
using UnityEngine;
using System.Collections;

namespace WPM
{
	public class Demo : MonoBehaviour
	{

		WorldMapGlobe map;
		GUIStyle labelStyle, labelStyleShadow, buttonStyle, sliderStyle, sliderThumbStyle;
		ColorPicker colorPicker;
		bool changingFrontiersColor;
		bool minimizeState = false;
		bool animatingField;
		float zoomLevel = 1.0f;
		public GameObject citySpot;

		void Awake ()
		{
			// Get a reference to the World Map API:
			map = WorldMapGlobe.instance;

#if LIGHTSPEED
			Camera.main.fieldOfView = 180;
			animatingField = true;
#endif

		}

		void Start ()
		{
			// UI Setup - non-important, only for this demo
			labelStyle = new GUIStyle ();
			labelStyle.alignment = TextAnchor.MiddleCenter;
			labelStyle.normal.textColor = Color.white;
			labelStyleShadow = new GUIStyle (labelStyle);
			labelStyleShadow.normal.textColor = Color.black;
			buttonStyle = new GUIStyle (labelStyle);
			buttonStyle.alignment = TextAnchor.MiddleLeft;
			buttonStyle.normal.background = Texture2D.whiteTexture;
			buttonStyle.normal.textColor = Color.white;
			colorPicker = gameObject.GetComponent<ColorPicker> ();
			sliderStyle = new GUIStyle ();
			sliderStyle.normal.background = Texture2D.whiteTexture;
			sliderStyle.fixedHeight = 4.0f;
			sliderThumbStyle = new GUIStyle ();
			sliderThumbStyle.normal.background = Resources.Load<Texture2D> ("thumb");
			sliderThumbStyle.overflow = new RectOffset (0, 0, 8, 0);
			sliderThumbStyle.fixedWidth = 20.0f;
			sliderThumbStyle.fixedHeight = 12.0f;

			// setup GUI resizer - only for the demo
			GUIResizer.Init (800, 500); 

			// Some example commands below
//			map.ToggleCountrySurface("Brazil", true, Color.green);
//			map.ToggleCountrySurface(35, true, Color.green);
//			map.ToggleCountrySurface(33, true, Color.green);
//			map.FlyToCountry(33);
//			map.FlyToCountry("Brazil");

//			map.navigationTime = 0;
//			map.FlyToCountry ("India");

			if (citySpot != null) {
				citySpot = Instantiate (citySpot);
				citySpot.transform.SetParent (map.transform, false);
				citySpot.SetActive (false);
			}

		}
		
		void OnGUI ()
		{
			// Animates the camera field of view (just a cool effect at the begining)
			if (animatingField) {
				if (Camera.main.fieldOfView > 60) {
					Camera.main.fieldOfView -= (181.0f - Camera.main.fieldOfView) / (220.0f - Camera.main.fieldOfView); 
				} else {
					Camera.main.fieldOfView = 60;
					animatingField = false;
				}
			}

			// Do autoresizing of GUI layer
			GUIResizer.AutoResize ();

			// Check whether a country or city is selected, then show a label
			if (map.mouseIsOver) {
				string text;
				Vector3 mousePos = Input.mousePosition;
				float x, y;
				if (map.countryHighlighted != null || map.cityHighlighted != null) {
					if (map.cityHighlighted != null) {
						citySpot.transform.localPosition = map.cityHighlighted.unitySphereLocation;
						citySpot.transform.LookAt (map.transform.position);
						citySpot.SetActive (true);
						text = "City: " + map.cityHighlighted.name + " (" + map.countries [map.cityHighlighted.countryIndex].name + ")";
						map.autoRotationSpeed = 0;
					} else {
						citySpot.SetActive (false);
						if (map.countryHighlighted != null) {
							text = map.countryHighlighted.name + " (" + map.countryHighlighted.continent + ")";
						} else {
							text = "";
						}
					}
					x = GUIResizer.authoredScreenWidth * (mousePos.x / Screen.width);
					y = GUIResizer.authoredScreenHeight - GUIResizer.authoredScreenHeight * (mousePos.y / Screen.height) - 20 * (Input.touchSupported ? 3 : 1); // slightly up for touch devices
					GUI.Label (new Rect (x - 1, y - 1, 0, 10), text, labelStyleShadow);
					GUI.Label (new Rect (x + 1, y + 2, 0, 10), text, labelStyleShadow);
					GUI.Label (new Rect (x + 2, y + 3, 0, 10), text, labelStyleShadow);
					GUI.Label (new Rect (x + 3, y + 4, 0, 10), text, labelStyleShadow);
					GUI.Label (new Rect (x, y, 0, 10), text, labelStyle);
				}
			}

			// Assorted options to show/hide frontiers, cities, Earth and enable country highlighting
			map.showFrontiers = GUI.Toggle (new Rect (10, 20, 150, 30), map.showFrontiers, "Toggle Frontiers");
			map.showEarth = GUI.Toggle (new Rect (10, 50, 150, 30), map.showEarth, "Toggle Earth");
			map.showCities = GUI.Toggle (new Rect (10, 80, 150, 30), map.showCities, "Toggle Cities");
			map.showCountryNames = GUI.Toggle (new Rect (10, 110, 150, 30), map.showCountryNames, "Toggle Labels");
			map.enableCountryHighlight = GUI.Toggle (new Rect (10, 140, 170, 30), map.enableCountryHighlight, "Enable Country Highlight");

			GUI.backgroundColor = new Color (0.1f, 0.1f, 0.3f, 0.5f);

			// Add buttons to show the color picker and change colors for the frontiers or fill
			if (GUI.Button (new Rect (10, 170, 160, 30), "  Change Frontiers Color", buttonStyle)) {
				colorPicker.showPicker = true;
				changingFrontiersColor = true;
			}
			if (GUI.Button (new Rect (10, 210, 160, 30), "  Change Fill Color", buttonStyle)) {
				colorPicker.showPicker = true;
				changingFrontiersColor = false;
			}
			if (colorPicker.showPicker) {
				if (changingFrontiersColor) {
					map.frontiersColor = colorPicker.setColor;
				} else {
					map.fillColor = colorPicker.setColor;
				}
			}

			// Add a button which demonstrates the navigateTo functionality -- pass the name of a country
			// For a list of countries and their names, check map.Countries collection.
			if (GUI.Button (new Rect (10, 250, 180, 30), "  Fly to Australia (Country)", buttonStyle)) {
				FlyToCountry ("Australia"); 
			}
			if (GUI.Button (new Rect (10, 285, 180, 30), "  Fly to Mexico (Country)", buttonStyle)) {
				FlyToCountry ("Mexico");
			}
			if (GUI.Button (new Rect (10, 325, 180, 30), "  Fly to San Francisco (City)", buttonStyle)) {
				FlyToCity ("San Francisco");
			}
			if (GUI.Button (new Rect (10, 365, 180, 30), "  Fly to Madrid (City)", buttonStyle)) {
				FlyToCity ("Madrid");
			}

			// Slider to show the new set zoom level API in V4.1
			GUI.Button (new Rect (10, 430, 85, 30), "  Zoom Level", buttonStyle);
			float prevZoomLevel = zoomLevel;
			GUI.backgroundColor = Color.white;
			zoomLevel = GUI.HorizontalSlider (new Rect (100, 445, 80, 85), zoomLevel, 0, 1, sliderStyle, sliderThumbStyle);
			GUI.backgroundColor = new Color (0.1f, 0.1f, 0.3f, 0.95f);
			if (zoomLevel != prevZoomLevel) {
				prevZoomLevel = zoomLevel;
				map.SetZoomLevel (zoomLevel);
			}


			// Add a button to colorize countries
			if (GUI.Button (new Rect (GUIResizer.authoredScreenWidth - 190, 20, 180, 30), "  Colorize Europe", buttonStyle)) {
				map.FlyToCity ("Brussels");
				for (int colorizeIndex =0; colorizeIndex < map.countries.Length; colorizeIndex++) {
					if (map.countries [colorizeIndex].continent.Equals ("Europe")) {
						Color color = new Color (Random.Range (0.0f, 1.0f), Random.Range (0.0f, 1.0f), Random.Range (0.0f, 1.0f), 0.5f);
						map.ToggleCountrySurface (map.countries [colorizeIndex].name, true, color);
					}
				}
			}

			// Colorize random country and fly to it
			if (GUI.Button (new Rect (GUIResizer.authoredScreenWidth - 190, 60, 180, 30), "  Colorize Random", buttonStyle)) {
				map.FlyToCity ("Brussels");
				int countryIndex = Random.Range (0, map.countries.Length);
				Color color = new Color (Random.Range (0.0f, 1.0f), Random.Range (0.0f, 1.0f), Random.Range (0.0f, 1.0f));
				map.ToggleCountrySurface (countryIndex, true, color);
				map.FlyToCountry (countryIndex);
			}
			
			// Button to clear colorized countries
			if (GUI.Button (new Rect (GUIResizer.authoredScreenWidth - 190, 100, 180, 30), "  Reset countries", buttonStyle)) {
				map.HideCountrySurfaces ();
			}

			// Moving the Earth sample
			if (GUI.Button (new Rect (GUIResizer.authoredScreenWidth - 190, 140, 180, 30), "  Toggle Minimize", buttonStyle)) {
				ToggleMinimize ();
			}

			// Add marker sample (gameobject)
			if (GUI.Button (new Rect (GUIResizer.authoredScreenWidth - 190, 180, 180, 30), "  Add Marker (Object)", buttonStyle)) {
				AddMarkerGameObjectOnRandomCity ();
			}
		
			// Add marker sample (gameobject)
			if (GUI.Button (new Rect (GUIResizer.authoredScreenWidth - 190, 220, 180, 30), "  Add Marker (Circle)", buttonStyle)) {
				AddMarkerCircleOnRandomPosition ();
			}

			if (GUI.Button (new Rect (GUIResizer.authoredScreenWidth - 190, 260, 180, 30), "  Add Trajectories", buttonStyle)) {
				AddTrajectories ();
			}
		}

		// Sample code to show how to:
		// 1.- Navigate and center a country in the map
		// 2.- Add a blink effect to one country (can be used on any number of countries)
		void FlyToCountry (string countryName)
		{
			int countryIndex = map.GetCountryIndex (countryName);
			map.FlyToCountry (countryIndex);
			map.BlinkCountry (countryIndex, Color.black, Color.green, 4, 0.2f);
		}

		// Sample code to show how to navigate to a city:
		void FlyToCity (string cityName)
		{
			map.FlyToCity (cityName);
		}

		// The globe can be moved and scaled at wish
		void ToggleMinimize ()
		{
			minimizeState = !minimizeState;

			Camera.main.transform.position = Vector3.back * 1.1f;
			Camera.main.transform.rotation = Quaternion.Euler (MiscVector.Vector3zero);
			if (minimizeState) {
				map.gameObject.transform.localScale = MiscVector.Vector3one * 0.20f;
				map.gameObject.transform.localPosition = new Vector3 (0.0f, -0.5f, 0);
				map.allowUserZoom = false;
				map.earthColor = Color.black;
				map.longitudeStepping = 4;
				map.latitudeStepping = 40;
				map.showFrontiers = false;
				map.showCities = false;
				map.showCountryNames = false;
				map.gridLinesColor = new Color (0.06f, 0.23f, 0.398f);
			} else {
				map.gameObject.transform.localScale = MiscVector.Vector3one;
				map.gameObject.transform.localPosition = MiscVector.Vector3zero;
				map.allowUserZoom = true;
				map.earthStyle = EARTH_STYLE.Natural;
				map.longitudeStepping = 15;
				map.latitudeStepping = 15;
				map.showFrontiers = true;
				map.showCities = true;
				map.showCountryNames = true;
				map.gridLinesColor = new Color (0.16f, 0.33f, 0.498f);
			}
			map.earthAtmosphereVisible = !minimizeState;
		}


		/// <summary>
		/// Illustrates how to add custom markers over the globe using the AddMarker API.
		/// In this example a building prefab is added to a random city (see comments for other options).
		/// </summary>
		void AddMarkerGameObjectOnRandomCity ()
		{

			// Every marker is put on a spherical-coordinate (assuming a radius = 0.5 and relative center at zero position)
			Vector3 sphereLocation;

			// Add a marker on a random city
			City city = map.cities [Random.Range (0, map.cities.Count)];
			sphereLocation = city.unitySphereLocation;

			// or... choose a city by its name:
			//		int cityIndex = map.GetCityIndex("Moscow");
			//		sphereLocation = map.cities[cityIndex].unitySphereLocation;

			// or... use the centroid of a country
			//		int countryIndex = map.GetCountryIndex("Greece");
			//		sphereLocation = map.countries[countryIndex].center;

			// Send the prefab to the AddMarker API setting a scale of 0.02f (this depends on your marker scales)
			GameObject building = Instantiate (Resources.Load<GameObject> ("Building/Building"));

			map.AddMarker (building, sphereLocation, 0.02f);


			// Fly to the destination and see the building created
			map.FlyToLocation (sphereLocation);

			// Optionally add a blinking effect to the marker
			MarkerBlinker.AddTo (building, 4, 0.2f);
		}

		void AddMarkerCircleOnRandomPosition ()
		{
			// Draw a beveled circle
			Vector3 sphereLocation = Random.onUnitSphere * 0.5f;
			float km = Random.value * 500 + 500; // Circle with a radius of (500...1000) km

//			sphereLocation = map.cities[map.GetCityIndex("Paris")].unitySphereLocation;
//			km = 1053;
//			sphereLocation = map.cities[map.GetCityIndex("New York")].unitySphereLocation;
//			km = 500;
			map.AddMarker (MARKER_TYPE.CIRCLE, sphereLocation, km, 0.975f, 1.0f, new Color (0.85f, 0.45f, 0.85f, 0.9f));
			map.AddMarker (MARKER_TYPE.CIRCLE, sphereLocation, km, 0, 0.975f, new Color (0.5f, 0, 0.5f, 0.9f));
			map.FlyToLocation (sphereLocation);
		}


		/// <summary>
		/// Example of how to add custom lines to the map
		/// Similar to the AddMarker functionality, you need two spherical coordinates and then call AddLine
		/// </summary>
		void AddTrajectories ()
		{

			// In this example we will add random lines from 5 cities to another cities (see AddMaker example above for other options to get locations)

			for (int line=0; line<5; line++) {
				// Get two random cities
				int city1 = Random.Range (0, map.cities.Count);
				int city2 = Random.Range (0, map.cities.Count);

				// Get their sphere-coordinates
				Vector3 start = map.cities [city1].unitySphereLocation;
				Vector3 end = map.cities [city2].unitySphereLocation;

				// Add lines with random color, speeds and elevation
				Color color = new Color (Random.Range (0.5f, 1), Random.Range (0.5f, 1), Random.Range (0.5f, 1));
				float elevation = Random.Range (0, 0.5f); 	// elevation is % relative to the Earth radius
				float drawingDuration = 4.0f;
				float lineWidth = 0.002f;
				float fadeAfter = 2.0f; // line stays for 2 seconds, then fades out - set this to zero to avoid line removal
				map.AddLine (start, end, color, elevation, drawingDuration, lineWidth, fadeAfter);
			}
		}

		IEnumerator test ()
		{
			int CityOneIndex = map.GetCityIndex ("Madrid");
			int CityTwoIndex = map.GetCityIndex ("Berlin");
			int CityThreeIndex = map.GetCityIndex ("Paris");
			Vector3 start1 = map.cities [CityOneIndex].unitySphereLocation;
			Vector3 end1 = map.cities [CityTwoIndex].unitySphereLocation;
			Vector3 start2 = map.cities [CityTwoIndex].unitySphereLocation;
			Vector3 end2 = map.cities [CityThreeIndex].unitySphereLocation;

			Color color = new Color (1, 1, 1);
			float elevation = 0.1f; 
			float drawingDuration = 1.5f;
			float lineWidth = 0.002f;
			float fadeAfter = 1.0f; // line stays for X seconds, then fades out - set this to zero to avoid line removal
			map.FlyToCity (CityOneIndex);
			yield return new WaitForSeconds (2f);
			map.AddLine (start1, end1, color, elevation, drawingDuration, lineWidth, fadeAfter);
			map.FlyToCity (CityTwoIndex);
			yield return new WaitForSeconds (2f);
			map.AddLine (start2, end2, color, elevation, drawingDuration, lineWidth, fadeAfter);
			map.FlyToCity (CityThreeIndex);
		}

		void Update() {
			if (Input.GetMouseButtonUp(0)) {
				if (!map.hasDragged && map.countryLastClicked>=0) {
					Debug.Log ("Country clicked: " + map.countries[map.countryLastClicked].name);
				}
			}
		}

	}


}