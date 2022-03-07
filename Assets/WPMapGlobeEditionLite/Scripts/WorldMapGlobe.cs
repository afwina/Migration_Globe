// World Political Map - Globe Edition for Unity - Main Script
// Copyright 2015 - 2017 by Ramiro Oliva
// Don't modify this script - changes could be lost if you upgrade to a more recent version of WPM
// ***************************************************************************
// This is the public API file - every property or public method belongs here
// ***************************************************************************

using UnityEngine;
using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace WPM {

	public enum EARTH_STYLE {
		Natural = 0,
		SolidColor = 4,
	}

	public enum LABELS_QUALITY {
		Low = 0,
		Medium =1,
		High = 2
	}

	public enum MARKER_TYPE {
		CIRCLE = 0
	}

	[Serializable]
	public partial class WorldMapGlobe : MonoBehaviour {

		/// <summary>
		/// Complete list of countries and the continent name they belong to.
		/// </summary>
		[NonSerialized]
		public Country[] countries;
		
		/// <summary>
		/// Complete list of cities with their names and country names.
		/// </summary>
		[NonSerialized]
		public List<City>
			cities;
		
		/// <summary>
		/// Returns City under mouse position or null if none.
		/// </summary>
		[NonSerialized]
		public City cityHighlighted;
		
		Country _countryHighlighted;
		/// <summary>
		/// Returns Country under mouse position or null if none.
		/// </summary>
		public Country countryHighlighted { get { return _countryHighlighted; } }
		
		int	_countryHighlightedIndex = -1;
		/// <summary>
		/// Returns currently highlighted country index in the countries list.
		/// </summary>
		public int countryHighlightedIndex { get { return _countryHighlightedIndex; } }
		
		Region _countryRegionHighlighted;
		/// <summary>
		/// Returns currently highlightd country's region.
		/// </summary>
		/// <value>The country region highlighted.</value>
		public Region countryRegionHighlighted { get { return _countryRegionHighlighted; } }
		
		int _countryRegionHighlightedIndex = -1;
		/// <summary>
		/// Returns currently highlighted region of the country.
		/// </summary>
		public int countryRegionHighlightedIndex { get { return _countryRegionHighlightedIndex; } }
		
		int _countryLastClicked;
		/// <summary>
		/// Returns the last clicked country.
		/// </summary>
		public int countryLastClicked { get { return _countryLastClicked; } }
		
		bool _mouseIsOver;
		/// <summary>
		/// Returns true is mouse has entered the Earth's collider.
		/// </summary>
		public bool	mouseIsOver {
			get {
				return _mouseIsOver;
			}
			set {
				_mouseIsOver = value;
			}
		}

		bool _hasDragged;
		/// <summary>
		/// Returns true if the user has dragged the globe since last interaction
		/// </summary>
		public bool hasDragged {
			get {
				return _hasDragged;
			}
		}
		
		[SerializeField]
		bool
			_enableCountryHighlight = true;
		/// <summary>
		/// Enable/disable country highlight when mouse is over.
		/// </summary>
		public bool enableCountryHighlight {
			get {
				return _enableCountryHighlight;
			}
			set {
				if (_enableCountryHighlight != value) {
					_enableCountryHighlight = value;
					isDirty = true;
				}
			}
		}

		
		
		[SerializeField]
		[Range(0f, 8.0f)]
		float
			_countryHighlightMinSize = 0f;
		/// <summary>
		/// Extra space around countries to allow interaction with. Useful for mobile screens on small countries.
		/// </summary>
		public float countryHighlightMinSize {
			get {
				return _countryHighlightMinSize;
			}
			set {
				if (_countryHighlightMinSize != value) {
					_countryHighlightMinSize = value;
					isDirty = true;
				}
			}
		}

		
		
		[SerializeField]
		[Range(1.0f, 16.0f)]
		float
			_navigationTime = 4.0f;
		/// <summary>
		/// The navigation time in seconds.
		/// </summary>
		public float navigationTime {
			get {
				return _navigationTime;
			}
			set {
				if (_navigationTime != value) {
					_navigationTime = value;
					isDirty = true;
				}
			}
		}
		
		[SerializeField]
		bool
			_showFrontiers = true;
		
		/// <summary>
		/// Toggle frontiers visibility.
		/// </summary>
		public bool showFrontiers { 
			get {
				return _showFrontiers; 
			}
			set {
				if (value != _showFrontiers) {
					_showFrontiers = value;
					isDirty = true;
					
					if (frontiersLayer != null) {
						frontiersLayer.SetActive (_showFrontiers);
					} else if (_showFrontiers) {
						DrawFrontiers ();
					}
				}
			}
		}
		
		[SerializeField]
		bool
			_showCities = true;
		
		/// <summary>
		/// Toggle cities visibility.
		/// </summary>
		public bool showCities { 
			get {
				return _showCities; 
			}
			set {
				if (_showCities != value) {
					_showCities = value;
					isDirty = true;
					if (citiesLayer != null) {
						citiesLayer.SetActive (_showCities);
					} else if (_showCities) {
						DrawCities ();
					}
				}
			}
		}
		
		[NonSerialized]
		int
			_numCitiesDrawn = 0;
		
		/// <summary>
		/// Gets the number cities drawn.
		/// </summary>
		public int numCitiesDrawn { get { return _numCitiesDrawn; } }
		
		
		[SerializeField]
		bool
			_showWorld = true;
		/// <summary>
		/// Toggle Earth visibility.
		/// </summary>
		public bool showEarth { 
			get {
				return _showWorld; 
			}
			set {
				if (value != _showWorld) {
					_showWorld = value;
					isDirty = true;
					gameObject.GetComponent<MeshRenderer> ().enabled = _showWorld;
					DrawAtmosphere();
				}
			}
		}
		
		
		[SerializeField]
		Color
			_fillColor = new Color (1, 0, 0, 0.7f);
		
		/// <summary>
		/// Fill color to use when the mouse hovers a country's region.
		/// </summary>
		public Color fillColor {
			get {
				if (hudMatCountry != null) {
					return hudMatCountry.color;
				} else {
					return _fillColor;
				}
			}
			set {
				if (_fillColor!=value) {
					_fillColor = value;
					isDirty = true;
					if (hudMatCountry != null && _fillColor != hudMatCountry.color) {
						hudMatCountry.color = _fillColor;
					}
				}
			}
		}

		[SerializeField]
		Color
			_frontiersColor = Color.green;
		
		/// <summary>
		/// Global color for frontiers.
		/// </summary>
		public Color frontiersColor {
			get {
				if (frontiersMat != null) {
					return frontiersMat.color;
				} else {
					return _frontiersColor;
				}
			}
			set {
				if (value != _frontiersColor) {
					_frontiersColor = value;
					isDirty = true;
					
					if (frontiersMat != null && _frontiersColor != frontiersMat.color) {
						frontiersMat.color = _frontiersColor;
					}
				}
			}
		}

		[SerializeField]
		Color
			_citiesColor = Color.white;
		
		/// <summary>
		/// Global color for cities.
		/// </summary>
		public Color citiesColor {
			get {
				if (citiesMat != null) {
					return citiesMat.color;
				} else {
					return _citiesColor;
				}
			}
			set {
				if (value != _citiesColor) {
					_citiesColor = value;
					isDirty = true;
					
					if (citiesMat != null && _citiesColor != citiesMat.color) {
						citiesMat.color = _citiesColor;
					}
				}
			}
		}
		
		
		[SerializeField]
		float _cityIconSize = 0.2f;
		
		/// <summary>
		/// The size of the cities icon (dot).
		/// </summary>
		public float cityIconSize {
			get {
				return _cityIconSize;
			}
			set {
				if (value!=_cityIconSize) {
					_cityIconSize = value;
					isDirty = true;
					ScaleCities();
				}
			}
		}
		
		
		[SerializeField]
		bool
			_showOutline = true;
		
		/// <summary>
		/// Toggle frontiers visibility.
		/// </summary>
		public bool showOutline { 
			get {
				return _showOutline; 
			}
			set {
				if (value != _showOutline) {
					_showOutline = value;
					Redraw (); // recreate surfaces layer
					isDirty = true;
				}
			}
		}
		
		[SerializeField]
		Color
			_outlineColor = Color.black;
		
		/// <summary>
		/// Global color for frontiers.
		/// </summary>
		public Color outlineColor {
			get {
				if (outlineMat != null) {
					return outlineMat.color;
				} else {
					return _outlineColor;
				}
			}
			set {
				if (value != _outlineColor) {
					_outlineColor = value;
					isDirty = true;
					
					if (outlineMat != null && _outlineColor != outlineMat.color) {
						outlineMat.color = _outlineColor;
					}
				}
			}
		}
		
		static WorldMapGlobe _instance;
		
		/// <summary>
		/// Instance of the world map. Use this property to access World Map functionality.
		/// </summary>
		public static WorldMapGlobe instance {
			get {
				if (_instance == null) {
					GameObject obj = GameObject.Find ("WorldMapGlobe");
					if (obj == null) {
						Debug.LogWarning ("'WorldMapGlobe' GameObject could not be found in the scene. Make sure it's created with this name before using any map functionality.");
					} else {
						_instance = obj.GetComponent<WorldMapGlobe> ();
					}
				}
				return _instance;
			}
		}
		
		[Range(0, 17000)]
		[SerializeField]
		int
			_minPopulation = 0;
		
		public int minPopulation {
			get {
				return _minPopulation;
			}
			set {
				if (value != _minPopulation) {
					_minPopulation = value;
					isDirty = true;
					DrawCities ();
				}
			}
		}
		
		[SerializeField]
		EARTH_STYLE
			_earthStyle = EARTH_STYLE.Natural;
		
		public EARTH_STYLE earthStyle {
			get {
				return _earthStyle;
			}
			set {
				if (value != _earthStyle) {
					_earthStyle = value;
					isDirty = true;
					RestyleEarth ();
				}
			}
		}

		[SerializeField]
		Color
			_earthColor = Color.black;
		
		/// <summary>
		/// Color for Earth (for SolidColor style)
		/// </summary>
		public Color earthColor {
			get {
				return _earthColor;
			}
			set {
				if (value != _earthColor) {
					_earthColor = value;
					isDirty = true;
					
					if (_earthStyle == EARTH_STYLE.SolidColor) {
						Material mat = GetComponent<Renderer> ().sharedMaterial;
						mat.color = _earthColor;
					}
				}
			}
		}

		/// <summary>
		/// Toggles atmosphere visibility.
		/// </summary>
		/// <value><c>true</c> if earth atmosphere visible; otherwise, <c>false</c>.</value>
		public bool earthAtmosphereVisible {
			get {
				return transform.Find("WorldMapGlobeAtmosphere").gameObject.activeSelf;
			}
			set {
				if (value!=earthAtmosphereVisible) {
					transform.Find("WorldMapGlobeAtmosphere").gameObject.SetActive(value);
				}
			}
		}
		
		[SerializeField]
		[Range(-2f, 2f)]
		float
			_autoRotationSpeed = 0.02f;
		
		public float autoRotationSpeed {
			get { return _autoRotationSpeed; }
			set {
				if (value != _autoRotationSpeed) {
					_autoRotationSpeed = value;
					isDirty = true;
				}
			}
		}
		
		[SerializeField]
		bool
			_allowUserKeys = false;

		/// <summary>
		/// Whether WASD keys can rotate the globe.
		/// </summary>
		/// <value><c>true</c> if allow user keys; otherwise, <c>false</c>.</value>
		public bool	allowUserKeys {
			get { return _allowUserKeys; }
			set {
				if (value != _allowUserKeys) {
					_allowUserKeys = value;
					isDirty = true;
				}
			}
		}
		
		
		[SerializeField]
		bool
			_allowUserRotation = true;
		
		public bool	allowUserRotation {
			get { return _allowUserRotation; }
			set {
				if (value != _allowUserRotation) {
					_allowUserRotation = value;
					isDirty = true;
				}
			}
		}
		
		[SerializeField]
		bool
			_centerOnRightClick = true;
		
		public bool	centerOnRightClick {
			get { return _centerOnRightClick; }
			set {
				if (value != _centerOnRightClick) {
					_centerOnRightClick = value;
					isDirty = true;
				}
			}
		}
		
		[SerializeField]
		bool
			_allowUserZoom = true;
		
		public bool allowUserZoom {
			get { return _allowUserZoom; }
			set {
				if (value != _allowUserZoom) {
					_allowUserZoom = value;
					isDirty = true;
				}
			}
		}
		
		[SerializeField]
		[Range(0.1f, 3)]
		float
			_mouseDragSensitivity = 0.5f;
		
		public float mouseDragSensitivity {
			get { return _mouseDragSensitivity; }
			set {
				if (value != _mouseDragSensitivity) {
					_mouseDragSensitivity = value;
					isDirty = true;
				}
			}
		}


		[SerializeField]
		float
			_mouseDragThreshold = 0f;
		
		public float mouseDragThreshold {
			get { return _mouseDragThreshold; }
			set {
				if (value != _mouseDragThreshold) {
					_mouseDragThreshold = value;
					isDirty = true;
				}
			}
		}


		
		
		[SerializeField]
		[Range(0.1f, 3)]
		float
			_mouseWheelSensitivity = 0.5f;
		
		public float mouseWheelSensitivity {
			get { return _mouseWheelSensitivity; }
			set {
				if (value != _mouseWheelSensitivity) {
					_mouseWheelSensitivity = value;
					isDirty = true;
				}
			}
		}

		[SerializeField]
		bool
			_showCursor = true;
		
		/// <summary>
		/// Toggle cursor lines visibility.
		/// </summary>
		public bool showCursor { 
			get {
				return _showCursor; 
			}
			set {
				if (value != _showCursor) {
					_showCursor = value;
					isDirty = true;
					
					if (cursorLayer != null) {
						cursorLayer.SetActive (_showCursor);
					}
				}
			}
		}
		
		/// <summary>
		/// Cursor lines color.
		/// </summary>
		[SerializeField]
		Color
			_cursorColor = new Color (0.56f, 0.47f, 0.68f);
		
		public Color cursorColor {
			get {
				if (cursorMat != null) {
					return cursorMat.color;
				} else {
					return _cursorColor;
				}
			}
			set {
				if (value != _cursorColor) {
					_cursorColor = value;
					isDirty = true;
					
					if (cursorMat != null && _cursorColor != cursorMat.color) {
						cursorMat.color = _cursorColor;
					}
				}
			}
		}
		
		[SerializeField]
		bool
			_cursorFollowMouse = true;
		
		/// <summary>
		/// Makes the cursor follow the mouse when it's over the World.
		/// </summary>
		public bool cursorFollowMouse { 
			get {
				return _cursorFollowMouse; 
			}
			set {
				if (value != _cursorFollowMouse) {
					_cursorFollowMouse = value;
					isDirty = true;
				}
			}
		}
		
		[NonSerialized]
		Vector3
			_cursorLocation;
		
		public Vector3
		cursorLocation {
			get {
				return _cursorLocation;
			}
			set {
				if (_cursorLocation != value) {
					_cursorLocation = value;
					if (cursorLayer != null) {
						cursorLayer.transform.localRotation = Quaternion.LookRotation (cursorLocation);
					}
				}
			}
		}
		
		[SerializeField]
		bool
			_showLatitudeLines = true;
		
		/// <summary>
		/// Toggle latitude lines visibility.
		/// </summary>
		public bool showLatitudeLines { 
			get {
				return _showLatitudeLines; 
			}
			set {
				if (value != _showLatitudeLines) {
					_showLatitudeLines = value;
					isDirty = true;
					
					if (latitudeLayer != null) {
						latitudeLayer.SetActive (_showLatitudeLines);
					}
				}
			}
		}
		
		[SerializeField]
		[Range(5.0f, 45.0f)]
		int
			_latitudeStepping = 15;
		/// <summary>
		/// Specify latitude lines separation.
		/// </summary>
		public int latitudeStepping { 
			get {
				return _latitudeStepping; 
			}
			set {
				if (value != _latitudeStepping) {
					_latitudeStepping = value;
					isDirty = true;
					
					DrawLatitudeLines ();
				}
			}
		}
		
		[SerializeField]
		bool
			_showLongitudeLines = true;
		
		/// <summary>
		/// Toggle longitude lines visibility.
		/// </summary>
		public bool showLongitudeLines { 
			get {
				return _showLongitudeLines; 
			}
			set {
				if (value != _showLongitudeLines) {
					_showLongitudeLines = value;
					isDirty = true;
					
					if (longitudeLayer != null) {
						longitudeLayer.SetActive (_showLongitudeLines);
					}
				}
			}
		}
		
		[SerializeField]
		[Range(5.0f, 45.0f)]
		int
			_longitudeStepping = 15;
		/// <summary>
		/// Specify longitude lines separation.
		/// </summary>
		public int longitudeStepping { 
			get {
				return _longitudeStepping; 
			}
			set {
				if (value != _longitudeStepping) {
					_longitudeStepping = value;
					isDirty = true;
					
					DrawLongitudeLines ();
				}
			}
		}
		
		[SerializeField]
		Color
			_gridColor = new Color (0.16f, 0.33f, 0.498f);
		
		/// <summary>
		/// Color for imaginary lines (longitude and latitude).
		/// </summary>
		public Color gridLinesColor {
			get {
				if (gridMat != null) {
					return gridMat.color;
				} else {
					return _gridColor;
				}
			}
			set {
				if (value != _gridColor) {
					_gridColor = value;
					isDirty = true;
					
					if (gridMat != null && _gridColor != gridMat.color) {
						gridMat.color = _gridColor;
					}
				}
			}
		}
		
		[SerializeField]
		bool
			_showCountryNames = false;
		
		public bool showCountryNames {
			get {
				return _showCountryNames;
			}
			set {
				if (value != _showCountryNames) {
					#if TRACE_CTL
					Debug.Log ("CTL " + DateTime.Now + ": showcountrynames!");
					#endif
					_showCountryNames = value;
					isDirty = true;
					if (gameObject.activeInHierarchy) {
						if (!showCountryNames) {
							DestroyMapLabels ();
						} else {
							DrawMapLabels ();
							// Cool scrolling animation for map labels following...
							if (Application.isPlaying) {
								for (int k=0; k<countries.Length; k++) {
									GameObject o = countries [k].labelGameObject;
									LabelAnimator anim = o.AddComponent<LabelAnimator> ();
									anim.destPos = o.transform.localPosition;
									anim.startPos = o.transform.localPosition + Vector3.right * 100.0f * Mathf.Sign (o.transform.localPosition.x);
									anim.duration = 1.0f;
								}
							}
						}
					}
				}
			}
		}
		
		[SerializeField]
		float
			_countryLabelsAbsoluteMinimumSize = 0.5f;
		
		public float countryLabelsAbsoluteMinimumSize {
			get {
				return _countryLabelsAbsoluteMinimumSize;
			} 
			set {
				if (value != _countryLabelsAbsoluteMinimumSize) {
					_countryLabelsAbsoluteMinimumSize = value;
					isDirty = true;
					if (_showCountryNames)
						DrawMapLabels ();
				}
			}
		}
		
		[SerializeField]
		float
			_countryLabelsSize = 0.25f;
		
		public float countryLabelsSize {
			get {
				return _countryLabelsSize;
			} 
			set {
				if (value != _countryLabelsSize) {
					_countryLabelsSize = value;
					isDirty = true;
					if (_showCountryNames)
						DrawMapLabels ();
				}
			}
		}
		
		[SerializeField]
		LABELS_QUALITY
			_labelsQuality = LABELS_QUALITY.Medium;
		
		public LABELS_QUALITY labelsQuality {
			get {
				return _labelsQuality;
			}
			set {
				if (value != _labelsQuality) {
					_labelsQuality = value;
					isDirty = true;
					if (_showCountryNames) {
						DestroyOverlay (); // needs to recreate the render texture
						DrawMapLabels ();
					}
				}
			}
		}
		
		
		[SerializeField]
		float _labelsElevation = 0;
		
		public float labelsElevation {
			get {
				return _labelsElevation;
			}
			set {
				if (value != _labelsElevation) {
					_labelsElevation = value;
					isDirty = true;
					if (sphereOverlayLayer!=null) {
						AdjustSphereOverlayLayerScale();
					}
				}
			}
		}
		
		
		[SerializeField]
		bool
			_showLabelsShadow = true;
		
		/// <summary>
		/// Draws a shadow under map labels. Specify the color using labelsShadowColor.
		/// </summary>
		/// <value><c>true</c> if show labels shadow; otherwise, <c>false</c>.</value>
		public bool showLabelsShadow {
			get {
				return _showLabelsShadow;
			}
			set {
				if (value != _showLabelsShadow) {
					_showLabelsShadow = value;
					isDirty = true;
					if (gameObject.activeInHierarchy) {
						RedrawMapLabels();
					}
				}
			}
		}
		
		[SerializeField]
		Color
			_countryLabelsColor = Color.white;
		
		/// <summary>
		/// Color for map labels.
		/// </summary>
		public Color countryLabelsColor {
			get {
				return _countryLabelsColor;
			}
			set {
				if (value != _countryLabelsColor) {
					_countryLabelsColor = value;
					isDirty = true;
					if (gameObject.activeInHierarchy) {
						labelsFont.material.color = _countryLabelsColor;
					}
				}
			}
		}
		
		[SerializeField]
		Color
			_countryLabelsShadowColor = Color.black;
		
		/// <summary>
		/// Color for map labels.
		/// </summary>
		public Color countryLabelsShadowColor {
			get {
				return _countryLabelsShadowColor;
			}
			set {
				if (value != _countryLabelsShadowColor) {
					_countryLabelsShadowColor = value;
					isDirty = true;
					if (gameObject.activeInHierarchy) {
						labelsShadowMaterial.color = _countryLabelsShadowColor;
					}
				}
			}
		}
		
		int _surfacesCount;
		/// <summary>
		/// Returns number of visible (active) colorized surfaces.
		/// </summary>
		public int surfacesCount { get { return _surfacesCount; } }


	#region Common Gameloop events
	
		void Update () {
			if (!Application.isPlaying) {
				// when saving the scene from Editor, the material of the sphere label layer is cleared - here's a fix to recreate it
				if (_showCountryNames && sphereOverlayLayer != null && sphereOverlayLayer.GetComponent<Renderer> () == null) {
					CreateOverlay ();
				}
				return;
			}

			// Check scale and adapts atmosphere
			if (transform.localScale!=lastGlobeScaleCheck) {
				lastGlobeScaleCheck = transform.localScale;
				transform.Find("WorldMapGlobeAtmosphere").GetComponent<Light>().range = Mathf.Min (lastGlobeScaleCheck.x, lastGlobeScaleCheck.y, lastGlobeScaleCheck.z) * 1.2f;
			}

			// Verify if mouse enter a country boundary - we only check if mouse is inside the sphere of world
			if (mouseIsOver) { // dragDamping == 0 && wheelAccel == 0) {
				CheckMousePos ();
			}

			// Check if navigateTo... has been called and in this case rotate the globe until the country is centered
			if (flyToActive) {
				RotateGlobeToDestination ();
			} else {
				// subtle/slow continuous rotation
				if (autoRotationSpeed != 0) {
					gameObject.transform.Rotate (Vector3.up, -autoRotationSpeed);
				}
			}

			// Handle user input
			CheckUserInteractionNormalMode();
		}

	#endregion

	
	#region Public API area

		/// <summary>
		/// Sets the zoom level
		/// </summary>
		/// <param name="zoomLevel">Value from 0 to 1</param>
		public void SetZoomLevel (float zoomLevel) {
			zoomLevel = Mathf.Clamp01(zoomLevel);
			// Gets the max distance from the map
			float fv = Camera.main.fieldOfView;
			float radAngle = fv * Mathf.Deg2Rad;
			float sphereY = transform.localScale.y * 0.5f * Mathf.Sin (radAngle);
			float sphereX = transform.localScale.y * 0.5f * Mathf.Cos (radAngle);
			float frustumDistance = sphereY / Mathf.Tan (radAngle * 0.5f) + sphereX;

			Vector3 oldCamPos = Camera.main.transform.position;
			float minDistance = (transform.localScale.z * 0.5f + Camera.main.nearClipPlane + 0.01f);
			float dist = Mathf.Lerp(minDistance, frustumDistance, zoomLevel); 
			Vector3 camPos = transform.position + (Camera.main.transform.position - transform.position).normalized * dist;
			Camera.main.transform.position = camPos;
			Camera.main.transform.LookAt (transform.position);
			float radiusSqr = minDistance * minDistance;
			if ((Camera.main.transform.position - transform.position).sqrMagnitude < radiusSqr) {
				Camera.main.transform.position = oldCamPos;
			}
		}
		
		/// <summary>
		/// Gets the current zoom level (0..1)
		/// </summary>
		public float GetZoomLevel () {
			// Gets the max distance from the map
			float fv = Camera.main.fieldOfView;
			float radAngle = fv * Mathf.Deg2Rad;
			float sphereY = transform.localScale.y * 0.5f * Mathf.Sin (radAngle);
			float sphereX = transform.localScale.y * 0.5f * Mathf.Cos (radAngle);
			float frustumDistance = sphereY / Mathf.Tan (radAngle * 0.5f) + sphereX;

			// Takes the distance from the focus point and adjust it according to the zoom level
			float dist = Vector3.Distance(transform.position, Camera.main.transform.position);
			float minZoomDistance = transform.localScale.z * 0.5f + Camera.main.nearClipPlane + 0.01f;
			return (dist - minZoomDistance) / (frustumDistance - minZoomDistance);
		}

		/// <summary>
		/// Makes the globe's north points upwards smoothly
		/// </summary>
		public void StraightenGlobe (float duration)
		{
			Vector3 v1 = Camera.main.transform.position - transform.position;
			float angleY = SignedAngleBetween (v1, transform.right, transform.up) + 90.0f;
			Quaternion oldRotation = transform.localRotation;
			transform.localRotation = Camera.main.transform.localRotation;
			transform.Rotate (Vector3.up * angleY, Space.Self);
			flyToEndQuaternion = transform.localRotation;
			transform.localRotation = oldRotation;
			if (!Application.isPlaying) duration = 0;
			flyToDuration = duration;
			flyToStartQuaternion = transform.localRotation;
			flyToStartTime = Time.time;
			flyToActive = true;
			if (flyToDuration == 0)
				RotateGlobeToDestination ();
		}

		/// <summary>
		/// Set Earth rotations and moves smoothly.
		/// </summary>
		public void TiltGlobe (Vector3 angles, float duration)
		{
			Vector3 v1 = Camera.main.transform.position - transform.position;
			float angleY = SignedAngleBetween (v1, transform.right, transform.up) + 90.0f;
			flyToEndQuaternion = Quaternion.Euler (angles) * Quaternion.Euler (Vector3.up * angleY);
			if (!Application.isPlaying) duration = 0;
			flyToDuration = duration;
			flyToStartQuaternion = transform.localRotation;
			flyToStartTime = Time.time;
			flyToActive = true;
			if (flyToDuration == 0)
				RotateGlobeToDestination ();
		}


		/// <summary>
		/// Returns the overlay base layer (parent gameObject), useful to overlay stuff on the map that needs to be overlayed (ie. flat icons or labels). It will be created if it doesn't exist.
		/// </summary>
		public GameObject GetOverlayLayer (bool createIfNotExists) {
			if (overlayLayer != null && sphereOverlayLayer != null) {
				overlayLayer.transform.localScale = new Vector3(1.0f/transform.localScale.x, 1.0f/transform.localScale.y, 1.0f/transform.localScale.z);
				overlayLayer.transform.position = new Vector3 (5000, 5000, 0);
				overlayLayer.layer = gameObject.layer;
				return overlayLayer;
			} else if (createIfNotExists) {
				DestroyOverlay ();
				return CreateOverlay ();
			} else {
				return null;
			}
		}

		/// <summary>
		/// Renames the country. Name must be unique, different from current and one letter minimum.
		/// </summary>
		/// <returns><c>true</c> if country was renamed, <c>false</c> otherwise.</returns>
		public bool CountryRename (string oldName, string newName) {
			if (newName == null || newName.Length == 0)
				return false;
			int countryIndex = GetCountryIndex (oldName);
			int newCountryIndex = GetCountryIndex (newName);
			if (countryIndex < 0 || newCountryIndex >= 0)
				return false;
			countries [countryIndex].name = newName;
			lastCountryLookupCount = -1;
			return true;
			
		}
		


		/// <summary>
		/// Returns the index of a country in the countries collection by its name.
		/// </summary>
		public int GetCountryIndex (string countryName) {
			if (countryLookup!=null && countryLookup.ContainsKey(countryName)) 
				return countryLookup[countryName];
			else
				return -1;
		}

		/// <summary>
		/// Returns the index of a country in the countries collection by its reference.
		/// </summary>
		public int GetCountryIndex (Country country) {
			if (countryLookup.ContainsKey (country.name)) 
				return countryLookup [country.name];
			else
				return -1;
		}


		/// <summary>
		/// Used by Editor. Returns the country index by screen position defined by a ray in the Scene View.
		/// </summary>
		public bool GetCountryIndex (Ray ray, out int countryIndex, out int regionIndex) {
			RaycastHit[] hits = Physics.RaycastAll (ray, 5000, layerMask);
			if (hits.Length > 0) {
				for (int k=0; k<hits.Length; k++) {
					if (hits [k].collider.gameObject == gameObject) {
						Vector3 localHit = transform.InverseTransformPoint (hits [k].point);
						if (GetCountryUnderMouse (localHit, out countryIndex, out regionIndex)) {
							return true;
						}
					}
				}
			}
			countryIndex = -1;
			regionIndex = -1;
			return false;
		}

		/// <summary>
		/// Starts navigation to target country. Returns false if country is not found.
		/// </summary>
		public bool FlyToCountry (string name) {
			int countryIndex = GetCountryIndex(name);
			if (countryIndex>=0) {
				FlyToCountry (countryIndex);
				return true;
			}
			return false;
		}

		/// <summary>
		/// Starts navigation to target country. with specified duration, ignoring NavigationTime property.
		/// Set duration to zero to go instantly.
		/// Returns false if country is not found. 
		/// </summary>
		public bool FlyToCountry (string name, float duration) {
			int countryIndex = GetCountryIndex(name);
			if (countryIndex>=0) {
				FlyToCountry (countryIndex, duration);
				return true;
			}
			return false;
		}

		/// <summary>
		/// Starts navigation to target country by index in the countries collection. Returns false if country is not found.
		/// </summary>
		public void FlyToCountry (int countryIndex) {
			FlyToCountry (countryIndex, _navigationTime);
		}

		/// <summary>
		/// Starts navigating to target country by index in the countries collection with specified duration, ignoring NavigationTime property.
		/// Set duration to zero to go instantly.
		/// </summary>
		public void FlyToCountry (int countryIndex, float duration) {
			flyToEndQuaternion = GetCountryQuaternion (countryIndex);
			flyToStartQuaternion = transform.rotation;
			flyToStartTime = Time.time;
			flyToDuration = duration;
			flyToActive = true;
			if (flyToDuration == 0)
				RotateGlobeToDestination ();
		}
	
		/// <summary>
		/// Starts navigation to target city. Returns false if not found.
		/// </summary>
		public bool FlyToCity (string name) {
			int cityIndex = GetCityIndex(name);
			if (cityIndex<0) return false;
			FlyToCity (cities[cityIndex]);
			return false;
		}

		/// <summary>
		/// Starts navigation to target city by index in the cities collection. Returns false if not found.
		/// </summary>
		public void FlyToCity (int cityIndex) {
			FlyToCity(cities[cityIndex]);
		}

		/// <summary>
		/// Starts navigation to target city. Returns false if not found.
		/// </summary>
		public void FlyToCity (City city) {
			FlyToCity(city, _navigationTime);
		}

		/// <summary>
		/// Starts navigation to target city with duration (seconds). Returns false if not found.
		/// </summary>
		public void FlyToCity (City city, float duration) {
			flyToEndQuaternion = GetQuaternion (city.unitySphereLocation);
			flyToStartQuaternion = transform.rotation;
			flyToActive = true;
			flyToDuration = duration;
			flyToStartTime = Time.time;
		}

		/// <summary>
		/// Starts navigation to target location in local spherical coordinates.
		/// </summary>
		public void FlyToLocation(Vector3 destination) {
			FlyToLocation(destination.x, destination.y, destination.z, _navigationTime);
		}

		/// <summary>
		/// Starts navigation to target location in local spherical coordinates.
		/// </summary>
		public void FlyToLocation(Vector3 destination, float duration) {
			FlyToLocation(destination.x, destination.y, destination.z, duration);
		}

		/// <summary>
		/// Starts navigation to target location in local spherical coordinates.
		/// </summary>
		public void FlyToLocation (float x, float y, float z) {
			FlyToLocation(x,y,z,_navigationTime);
		}

		/// <summary>
		/// Starts navigation to target location in local spherical coordinates.
		/// </summary>
		public void FlyToLocation (float x, float y, float z, float duration) {
			flyToEndQuaternion = GetQuaternion (new Vector3 (x, y, z).normalized * 0.5f);
			flyToStartQuaternion = transform.rotation;
			flyToDuration = duration;
			flyToActive = true;
			flyToStartTime = Time.time;
		}


		
		/// <summary>
		/// Returns an array with the city names.
		/// </summary>
		public string[] GetCityNames () {
			List<string> c = new List<string> (cities.Count);
			for (int k=0; k<cities.Count; k++) {
				c.Add (cities [k].name + " (" + k + ")");
			}
			c.Sort ();
			return c.ToArray ();
		}

		/// <summary>
		/// Returns the index of the city by its name in the cities collection.
		/// </summary>
		public int GetCityIndex(string cityName) {
			for (int k=0; k<cities.Count; k++) {
				if (cityName.Equals (cities [k].name)) {
					return k;
				}
			}
			return -1;
		}


		/// <summary>
		/// Returns the index of a city in the global countries collection. Note that country index needs to be supplied due to repeated city names.
		/// </summary>
		public int GetCityIndex (int countryIndex, string cityName) {
			if (countryIndex >= 0 && countryIndex < countries.Length) {
				for (int k=0; k<cities.Count; k++) {
					if (cities [k].name.Equals (cityName) && cities [k].countryIndex == countryIndex)
						return k;
				}
			} else {
				// Try to select city by its name alone
				for (int k=0; k<cities.Count; k++) {
					if (cities [k].name.Equals (cityName))
						return k;
				}
			}
			return -1;
		}

		/// <summary>
		/// Returns the city index by screen position.
		/// </summary>
		public bool GetCityIndex (Ray ray, out int cityIndex) {
			RaycastHit[] hits = Physics.RaycastAll (ray, 5000, layerMask);
			if (hits.Length > 0) {
				for (int k=0; k<hits.Length; k++) {
					if (hits [k].collider.gameObject == gameObject) {
						Vector3 localHit = transform.InverseTransformPoint (hits [k].point);
						int c = GetCityNearPoint (localHit);
						if (c >= 0) {
							cityIndex = c;
							return true;
						}
					}
				}
			}
			cityIndex = -1;
			return false;
		}



		/// <summary>
		/// Clears any city highlighted (color changed) and resets them to default city color
		/// </summary>
		public void HideCityHighlights () {
			if (citiesLayer == null)
				return;
			Renderer[] rr = citiesLayer.GetComponentsInChildren<Renderer>(true);
			for (int k=0;k<rr.Length;k++) {
				rr[k].sharedMaterial = citiesMat;
			}
		}

		/// <summary>
		/// Toggles the city highlight.
		/// </summary>
		/// <param name="cityIndex">City index.</param>
		/// <param name="color">Color.</param>
		/// <param name="highlighted">If set to <c>true</c> the color of the city will be changed. If set to <c>false</c> the color of the city will be reseted to default color</param>
		public void ToggleCityHighlight (int cityIndex, Color color, bool highlighted) {
			if (citiesLayer == null)
				return;
			Transform t = citiesLayer.transform.Find ("City" + cityIndex);
			if (t == null)
				return;
			Renderer rr = t.gameObject.GetComponent<Renderer> ();
			if (rr == null)
				return;
			if (highlighted) {
				Material mat = Instantiate (rr.sharedMaterial);
				mat.hideFlags = HideFlags.DontSave;
				mat.color = color;
				rr.sharedMaterial = mat;
			} else {
				rr.sharedMaterial = citiesMat;
			}
		}
		
		/// <summary>
		/// Flashes specified city by index in the global city collection.
		/// </summary>
		public void BlinkCity (int cityIndex, Color color1, Color color2, float duration, float blinkingSpeed) {
			if (citiesLayer == null)
				return;
			Transform t = citiesLayer.transform.Find ("City" + cityIndex);
			if (t == null)
				return;
			CityBlinker sb = t.gameObject.AddComponent<CityBlinker> ();
			sb.blinkMaterial = citiesMat;
			sb.color1 = color1;
			sb.color2 = color2;
			sb.duration = duration;
			sb.speed = blinkingSpeed;
		}

		/// <summary>
		/// Colorize all regions of specified country by name. Returns false if not found.
		/// </summary>
		public bool ToggleCountrySurface (string name, bool visible, Color color) {
			int countryIndex = GetCountryIndex(name);
			if (countryIndex>=0) {
				ToggleCountrySurface (countryIndex, visible, color);
				return true;
			}
			return false;
		}


		/// <summary>
		/// Iterates for the countries list and colorizes those belonging to specified continent name.
		/// </summary>
		public void ToggleContinentSurface(string continentName, bool visible, Color color) {
			for (int colorizeIndex =0; colorizeIndex < countries.Length; colorizeIndex++) {
				if (countries [colorizeIndex].continent.Equals (continentName)) {
					ToggleCountrySurface (countries [colorizeIndex].name, visible, color);
			}

		}
		}

		
		/// <summary>
		/// Uncolorize/hide specified countries beloning to a continent.
		/// </summary>
		public void HideContinentSurface (string continentName) {
			for (int colorizeIndex =0; colorizeIndex < countries.Length; colorizeIndex++) {
				if (countries [colorizeIndex].continent.Equals (continentName)) {
					HideCountrySurface(colorizeIndex);
				}
			}
		}

		/// <summary>
		/// Colorize all regions of specified country by index in the countries collection.
		/// </summary>
		public void ToggleCountrySurface (int countryIndex, bool visible, Color color) {
			if (!visible) {
				HideCountrySurface(countryIndex);
				return;
			}
			for (int r=0; r<countries[countryIndex].regions.Count; r++) {
				ToggleCountryRegionSurface(countryIndex, r, visible, color);
			}
		}

		/// <summary>
		/// Uncolorize/hide specified country by index in the countries collection.
		/// </summary>
		public void HideCountrySurface (int countryIndex) {
			for (int r=0; r<countries[countryIndex].regions.Count; r++) {
				HideCountryRegionSurface(countryIndex, r);
			}
		}

		/// <summary>
		/// Colorize main region of a country by index in the countries collection.
		/// </summary>
		public void ToggleCountryMainRegionSurface(int countryIndex, bool visible, Color color) {
			ToggleCountryRegionSurface(countryIndex, countries[countryIndex].mainRegionIndex,visible, color);
		}

		/// <summary>
		/// Colorize specified region of a country by indexes.
		/// </summary>
		public void ToggleCountryRegionSurface (int countryIndex, int regionIndex, bool visible, Color color) {
			if (!visible) {
				HideCountryRegionSurface(countryIndex, regionIndex);
				return;
			}
			GameObject surf = null;
			Region region = countries[countryIndex].regions[regionIndex];
			int cacheIndex = GetCacheIndexForCountryRegion (countryIndex, regionIndex);
			// Checks if current cached surface contains a material with a texture, if it exists but it has not texture, destroy it to recreate with uv mappings
			if (surfaces.ContainsKey (cacheIndex) && surfaces[cacheIndex]!=null) 
				surf = surfaces [cacheIndex];

			// Should the surface be recreated?
			Material surfMaterial;
			if (surf!=null) {
				surfMaterial = surf.GetComponent<Renderer>().sharedMaterial;
			}
			// If it exists, activate and check proper material, if not create surface
			bool isHighlighted = countryHighlightedIndex == countryIndex && countryRegionHighlightedIndex == regionIndex;
			if (surf!=null) {
				if (!surf.activeSelf) surf.SetActive (true);
				UpdateSurfaceCount();

				// Check if material is ok
				surfMaterial = surf.GetComponent<Renderer>().sharedMaterial;
				if (surfMaterial.color!=color && !isHighlighted) {
					Material goodMaterial = GetColoredTexturedMaterial(color);
					region.customMaterial = goodMaterial;
					ApplyMaterialToSurface(surf, goodMaterial);
				}
			} else {
				surfMaterial = GetColoredTexturedMaterial(color);
				surf = GenerateCountryRegionSurface (countryIndex, regionIndex, surfMaterial, _showOutline);
				region.customMaterial = surfMaterial;
				UpdateSurfaceCount();
			}
			// If it was highlighted, highlight it again
			if (region.customMaterial!=null && isHighlighted && region.customMaterial.color!=hudMatCountry.color) {
				Material clonedMat = Instantiate(region.customMaterial);
				clonedMat.name = region.customMaterial.name;
				clonedMat.color = hudMatCountry.color;
				surf.GetComponent<Renderer>().sharedMaterial = clonedMat;
				countryRegionHighlightedObj = surf;
			}
		}

		
		/// <summary>
		/// Uncolorize/hide specified country by index in the countries collection.
		/// </summary>
		public void HideCountryRegionSurface (int countryIndex, int regionIndex) {
			int cacheIndex = GetCacheIndexForCountryRegion (countryIndex, regionIndex);
			if (surfaces.ContainsKey (cacheIndex)) {
				if (surfaces[cacheIndex]!=null) {
					surfaces [cacheIndex].SetActive (false);
				} else surfaces.Remove(cacheIndex);
				UpdateSurfaceCount();
			}
			countries[countryIndex].regions[regionIndex].customMaterial = null;
		}

		/// <summary>
		/// Highlights the country region specified.
		/// Internally used by the Editor component, but you can use it as well to temporarily mark a country region.
		/// </summary>
		/// <param name="refreshGeometry">Pass true only if you're sure you want to force refresh the geometry of the highlight (for instance, if the frontiers data has changed). If you're unsure, pass false.</param>
		public GameObject ToggleCountryRegionSurfaceHighlight (int countryIndex, int regionIndex, Color color, bool drawOutline) {
			GameObject surf;
			Material mat = Instantiate (hudMatCountry);
			mat.hideFlags = HideFlags.DontSave;
			mat.color = color;
			mat.renderQueue--;
			int cacheIndex = GetCacheIndexForCountryRegion (countryIndex, regionIndex); 
			bool existsInCache = surfaces.ContainsKey (cacheIndex);
			if (existsInCache) {
				surf = surfaces [cacheIndex];
				if (surf == null) {
					surfaces.Remove (cacheIndex);
				} else {
					surf.SetActive (true);
					surf.GetComponent<Renderer> ().sharedMaterial = mat;
				}
			} else {
				surf = GenerateCountryRegionSurface (countryIndex, regionIndex, mat, drawOutline);
			}
			return surf;
		}


		/// <summary>
		/// Hides all colorized regions of all countries.
		/// </summary>
		public void HideCountrySurfaces () {
			for (int c=0; c<countries.Length; c++) {
				HideCountrySurface (c);
			}
		}

		/// <summary>
		/// Flashes specified country by index in the countries collection.
		/// </summary>
		public void BlinkCountry (int countryIndex, Color color1, Color color2, float duration, float blinkingSpeed) {
			// CHANGE BY ATHIRAH -- changed to make entire country blinkg, not just one region.
			var regions = countries [countryIndex].regions;
			
			foreach(var region in regions)
            {
				BlinkCountry(countryIndex, region.regionIndex, color1, color2, duration, blinkingSpeed);
			}
		}

		/// <summary>
		/// Flashes specified country's region.
		/// </summary>
		public void BlinkCountry (int countryIndex, int regionIndex, Color color1, Color color2, float duration, float blinkingSpeed) {
			int cacheIndex = GetCacheIndexForCountryRegion (countryIndex, regionIndex);
			GameObject surf;
			bool disableAtEnd;
			if (surfaces.ContainsKey (cacheIndex)) {
				surf = surfaces [cacheIndex];
				disableAtEnd = !surf.activeSelf;
			} else {
				surf = GenerateCountryRegionSurface (countryIndex, regionIndex, hudMatCountry, _showOutline);
				disableAtEnd = true;
			}
			SurfaceBlinker sb = surf.AddComponent<SurfaceBlinker> ();
			sb.blinkMaterial = hudMatCountry;
			sb.color1 = color1;
			sb.color2 = color2;
			sb.duration = duration;
			sb.speed = blinkingSpeed;
			sb.disableAtEnd = disableAtEnd;
			sb.customizableSurface = countries[countryIndex].regions[regionIndex];
			surf.SetActive (true);
		}

		/// <summary>
		/// Returns an array of country names. The returning list can be grouped by continent.
		/// </summary>
		public string[] GetCountryNames (bool groupByContinent) {
			List<string> c = new List<string> ();
			if (countries == null)
				return c.ToArray ();
			string previousContinent = "";
			for (int k=0; k<countries.Length; k++) {
				Country country = countries [k];
				if (groupByContinent) {
					if (!country.continent.Equals (previousContinent)) {
						c.Add (country.continent);
						previousContinent = country.continent;
					}
					c.Add (country.continent + "|" + country.name + " (" + k + ")");
				} else {
					c.Add (country.name + " (" + k + ")");
				}
			}
			c.Sort ();

			if (groupByContinent) {
				int k = -1;
				while (++k<c.Count) {
					int i = c [k].IndexOf ('|');
					if (i > 0) {
						c [k] = "  " + c [k].Substring (i + 1);
					}
				}
			}
			return c.ToArray ();
		}

		/// <summary>
		/// Adds a custom marker (gameobject) to the globe on specified location and with custom scale.
		/// </summary>
		public void AddMarker(GameObject marker, Vector3 sphereLocation, float markerScale) {
			mAddMarker(marker, sphereLocation, markerScale);

		}

		/// <summary>
		/// Adds a custom marker (polygon) to the globe on specified location and with custom size in km.
		/// </summary>
		/// <param name="type">Polygon type.</param>
		/// <param name="sphereLocation">Sphere location.</param>
		/// <param name="kmRadius">Radius in KM.</param>
		/// <param name="ringWidthStart">Ring inner limit (0..1). Pass 0 to draw a full circle.</param>
		/// <param name="ringWidthEnd">Ring outer limit (0..1). Pass 1 to draw a full circle.</param>
		/// <param name="color">Color</param>
		public GameObject AddMarker(MARKER_TYPE type, Vector3 sphereLocation, float kmRadius, float ringWidthStart, float ringWidthEnd, Color color) {
			return mAddMarker(type, sphereLocation, kmRadius, ringWidthStart, ringWidthEnd, color);
		}


		/// <summary>
		/// Adds a line to the globe with options (returns the line gameobject).
		/// </summary>
		/// <param name="start">starting location on the sphere</param>
		/// <param name="end">end location on the sphere</param>
		/// <param name="Color">line color</param>
		/// <param name="arcElevation">arc elevation relative to the sphere size.</param>
		/// <param name="duration">drawing speed (0 for instant drawing)</param>
		/// <param name="fadeOutAfter">duration of the line once drawn after which it fades out (set this to zero to make the line stay)</param>
		public GameObject AddLine(Vector3 start, Vector3 end, Color color, float arcElevation, float duration, float lineWidth, float fadeOutAfter) {
			CheckMarkersLayer();
			GameObject newLine = new GameObject("MarkerLine");
			newLine.transform.SetParent(markersLayer.transform, false);
			newLine.layer = markersLayer.layer;
			LineMarkerAnimator lma =  newLine.AddComponent<LineMarkerAnimator>();
			lma.start = start;
			lma.end = end;
			lma.color = color;
			lma.arcElevation = arcElevation;
			lma.duration = duration;
			lma.lineWidth = lineWidth;
			lma.lineMaterial = markerMat;
			lma.autoFadeAfter = fadeOutAfter;
			return newLine;
		}

		/// <summary>
		/// Deletes all custom markers and lines
		/// </summary>
		public void ClearMarkers() {
			if (markersLayer==null) return;
			Destroy (markersLayer);
		}


		/// <summary>
		/// Removes all marker lines.
		/// </summary>
		public void ClearLineMarkers() {
			if (markersLayer==null) return;
			LineRenderer[] t = markersLayer.transform.GetComponentsInChildren<LineRenderer>();
			for (int k=0;k<t.Length;k++)
				Destroy (t[k].gameObject);
		}

		#endregion


	}

}