using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using WPM;

namespace WPM_Editor
{
	[CustomEditor(typeof(WorldMapGlobe))]
	public class WorldMapGlobeInspector : Editor
	{
		WorldMapGlobe _map;
		Texture2D _headerTexture, _blackTexture;
		string[] earthStyleOptions, labelsQualityOptions;
		int[] earthStyleValues;
		GUIStyle blackStyle;

		void OnEnable ()
		{
			Color backColor = EditorGUIUtility.isProSkin ? new Color (0.18f, 0.18f, 0.18f) : new Color (0.7f, 0.7f, 0.7f);
			_blackTexture = MakeTex (4, 4, backColor);
			_blackTexture.hideFlags = HideFlags.DontSave;
			_map = (WorldMapGlobe)target;
			_headerTexture = Resources.Load<Texture2D> ("EditorHeader");
			earthStyleOptions = new string[] {
				"Natural", "Solid Color"
			};
			earthStyleValues = new int[] {
				(int)EARTH_STYLE.Natural, (int)EARTH_STYLE.SolidColor
			};

			labelsQualityOptions = new string[] {
				"Low",
				"Medium",
				"High"
			};
			blackStyle = new GUIStyle ();
			blackStyle.normal.background = _blackTexture;
		}

		public override void OnInspectorGUI ()
		{
			if (_map == null || _map.countries == null) return;
			_map.isDirty = false;

			EditorGUILayout.Separator ();
			GUI.skin.label.alignment = TextAnchor.MiddleCenter;  
			GUILayout.Label (_headerTexture, GUILayout.ExpandWidth (true));
			GUI.skin.label.alignment = TextAnchor.MiddleLeft;  
			EditorGUILayout.Separator ();

			EditorGUILayout.BeginVertical (blackStyle);

			EditorGUILayout.BeginHorizontal ();
			GUILayout.Label ("Show Earth", GUILayout.Width (120));
			_map.showEarth = EditorGUILayout.Toggle (_map.showEarth);

			if (GUILayout.Button ("Straighten")) {
				_map.StraightenGlobe (1.0f);
			}

			if (GUILayout.Button ("Tilt")) {
//				_map.TiltGlobe(new Vector3(19.7f, 198.5f, 349.4f), 1.0f);
				_map.TiltGlobe (new Vector3 (0, 0, 23.4f), 1.0f);
			}

			EditorGUILayout.EndHorizontal ();

			if (_map.showEarth) {
				EditorGUILayout.BeginHorizontal ();
				GUILayout.Label ("   Earth Style", GUILayout.Width (120));
				_map.earthStyle = (EARTH_STYLE)EditorGUILayout.IntPopup ((int)_map.earthStyle, earthStyleOptions, earthStyleValues);

				if (_map.earthStyle == EARTH_STYLE.SolidColor) {
					GUILayout.Label ("Color");
					_map.earthColor = EditorGUILayout.ColorField (_map.earthColor, GUILayout.Width (50));
				}
				EditorGUILayout.EndHorizontal ();
			}

			EditorGUILayout.BeginHorizontal ();
			GUILayout.Label ("Show Latitude Lines", GUILayout.Width (120));
			_map.showLatitudeLines = EditorGUILayout.Toggle (_map.showLatitudeLines);
			GUILayout.Label ("Stepping");
			_map.latitudeStepping = EditorGUILayout.IntSlider (_map.latitudeStepping, 5, 45);
			EditorGUILayout.EndHorizontal ();

			EditorGUILayout.BeginHorizontal ();
			GUILayout.Label ("Show Longitude Lines", GUILayout.Width (120));
			_map.showLongitudeLines = EditorGUILayout.Toggle (_map.showLongitudeLines);
			GUILayout.Label ("Stepping");
			_map.longitudeStepping = EditorGUILayout.IntSlider (_map.longitudeStepping, 5, 45);
			EditorGUILayout.EndHorizontal ();

			EditorGUILayout.BeginHorizontal ();
			GUILayout.Label ("Grid Color", GUILayout.Width (120));
			_map.gridLinesColor = EditorGUILayout.ColorField (_map.gridLinesColor, GUILayout.Width (50));
			EditorGUILayout.EndHorizontal ();

			EditorGUILayout.EndVertical (); 
			EditorGUILayout.Separator ();
			EditorGUILayout.BeginVertical (blackStyle);

			EditorGUILayout.BeginHorizontal ();
			GUILayout.Label ("Show Cities", GUILayout.Width (120));
			_map.showCities = EditorGUILayout.Toggle (_map.showCities);
			EditorGUILayout.EndHorizontal ();

			if (_map.showCities && _map.cities!=null) {
				EditorGUILayout.BeginHorizontal ();
				GUILayout.Label ("   Cities Color", GUILayout.Width (120));
				_map.citiesColor = EditorGUILayout.ColorField (_map.citiesColor, GUILayout.Width (50));
				EditorGUILayout.EndHorizontal ();

				EditorGUILayout.BeginHorizontal ();
				GUILayout.Label ("   Icon Size", GUILayout.Width (120));
				_map.cityIconSize = EditorGUILayout.Slider (_map.cityIconSize, 0.02f, 0.5f);
				EditorGUILayout.EndHorizontal ();


				EditorGUILayout.BeginHorizontal ();
				GUILayout.Label ("   Min Population (K)", GUILayout.Width (120));
				_map.minPopulation = EditorGUILayout.IntSlider (_map.minPopulation, 0, 19000);
				GUILayout.Label (_map.numCitiesDrawn + "/" + _map.cities.Count);
				EditorGUILayout.EndHorizontal ();
			}

			EditorGUILayout.EndVertical (); 
			EditorGUILayout.Separator ();
			EditorGUILayout.BeginVertical (blackStyle);

			EditorGUILayout.BeginHorizontal ();
			GUILayout.Label ("Show Countries", GUILayout.Width (120));
			_map.showFrontiers = EditorGUILayout.Toggle (_map.showFrontiers);
			EditorGUILayout.EndHorizontal ();

			if (_map.showFrontiers) {
				EditorGUILayout.BeginHorizontal ();
				GUILayout.Label ("   Frontiers Color", GUILayout.Width (120));
				_map.frontiersColor = EditorGUILayout.ColorField (_map.frontiersColor, GUILayout.Width (50));
				EditorGUILayout.EndHorizontal ();
			}

			EditorGUILayout.BeginHorizontal ();
			GUILayout.Label ("Country Highlight", GUILayout.Width (120));
			_map.enableCountryHighlight = EditorGUILayout.Toggle (_map.enableCountryHighlight);

			if (_map.enableCountryHighlight) {

				GUILayout.Label ("Highlight Color", GUILayout.Width (120));
				_map.fillColor = EditorGUILayout.ColorField (_map.fillColor, GUILayout.Width (50));
				EditorGUILayout.EndHorizontal ();

				EditorGUILayout.BeginHorizontal ();
				GUILayout.Label ("   Draw Outline", GUILayout.Width (120));
				_map.showOutline = EditorGUILayout.Toggle (_map.showOutline);
				if (_map.showOutline) {
					GUILayout.Label ("Outline Color", GUILayout.Width (120));
					_map.outlineColor = EditorGUILayout.ColorField (_map.outlineColor, GUILayout.Width (50));
					if (_map.surfacesCount > 100) {
						EditorGUILayout.EndHorizontal ();
						EditorGUILayout.BeginHorizontal ();
						GUIStyle warningLabelStyle = new GUIStyle (GUI.skin.label);
						warningLabelStyle.normal.textColor = new Color (0.31f, 0.38f, 0.56f);
						GUILayout.Label ("Consider disabling outline to improve performance", warningLabelStyle);
					}
				}
				EditorGUILayout.EndHorizontal ();
				EditorGUILayout.BeginHorizontal ();
				GUILayout.Label (new GUIContent("   Size Helper", "Small countries can be hard to select on mobile devices - if you set this value greater than 0, the selection of the countries with an horizontal / vertical size lower than this value will use a bigger boundary rectangle. This option is disabled on non-mobile devices."), GUILayout.Width (120));
				_map.countryHighlightMinSize = EditorGUILayout.Slider (_map.countryHighlightMinSize, 0f, 20f);
			}
			EditorGUILayout.EndHorizontal ();

			EditorGUILayout.EndVertical (); 
			EditorGUILayout.Separator ();
			EditorGUILayout.BeginVertical (blackStyle);

			EditorGUILayout.BeginHorizontal ();
			GUILayout.Label ("Show Country Names", GUILayout.Width (120));
			_map.showCountryNames = EditorGUILayout.Toggle (_map.showCountryNames);
			EditorGUILayout.EndHorizontal ();

			if (_map.showCountryNames) {
				EditorGUILayout.BeginHorizontal ();
				GUILayout.Label ("  Labels Quality", GUILayout.Width (120));
				_map.labelsQuality = (LABELS_QUALITY)EditorGUILayout.Popup ((int)_map.labelsQuality, labelsQualityOptions);
				EditorGUILayout.EndHorizontal ();

				EditorGUILayout.BeginHorizontal ();
				GUILayout.Label ("  Relative Size", GUILayout.Width (120));
				_map.countryLabelsSize = EditorGUILayout.Slider (_map.countryLabelsSize, 0.1f, 0.9f);
				EditorGUILayout.EndHorizontal ();

				EditorGUILayout.BeginHorizontal ();
				GUILayout.Label ("  Minimum Size", GUILayout.Width (120));
				_map.countryLabelsAbsoluteMinimumSize = EditorGUILayout.Slider (_map.countryLabelsAbsoluteMinimumSize, 0.29f, 2.5f);
				EditorGUILayout.EndHorizontal ();

				EditorGUILayout.BeginHorizontal ();
				GUILayout.Label ("  Elevation", GUILayout.Width (120));
				_map.labelsElevation = EditorGUILayout.Slider (_map.labelsElevation, 0.0f, 1.0f);
				EditorGUILayout.EndHorizontal ();

				EditorGUILayout.BeginHorizontal ();
				GUILayout.Label ("  Labels Color", GUILayout.Width (120));
				_map.countryLabelsColor = EditorGUILayout.ColorField (_map.countryLabelsColor, GUILayout.Width (50));
				EditorGUILayout.EndHorizontal ();

				EditorGUILayout.BeginHorizontal ();
				GUILayout.Label ("  Draw Shadow", GUILayout.Width (120));
				_map.showLabelsShadow = EditorGUILayout.Toggle (_map.showLabelsShadow);
				if (_map.showLabelsShadow) {
					GUILayout.Label ("Shadow Color", GUILayout.Width (120));
					_map.countryLabelsShadowColor = EditorGUILayout.ColorField (_map.countryLabelsShadowColor, GUILayout.Width (50));
				}
				EditorGUILayout.EndHorizontal ();

			}

			EditorGUILayout.EndVertical (); 
			EditorGUILayout.Separator ();
			EditorGUILayout.BeginVertical (blackStyle);

			EditorGUILayout.BeginHorizontal ();
			GUILayout.Label ("Autorotation Speed", GUILayout.Width (120));
			_map.autoRotationSpeed = EditorGUILayout.Slider (_map.autoRotationSpeed, -2f, 2f);
			if (GUILayout.Button ("Stop")) {
				_map.autoRotationSpeed = 0;
			}
			EditorGUILayout.EndHorizontal ();

			EditorGUILayout.BeginHorizontal ();
			GUILayout.Label ("Show Cursor", GUILayout.Width (120));
			_map.showCursor = EditorGUILayout.Toggle (_map.showCursor);

			if (_map.showCursor) {
				GUILayout.Label ("Cursor Color", GUILayout.Width (120));
				_map.cursorColor = EditorGUILayout.ColorField (_map.cursorColor, GUILayout.Width (50));
				EditorGUILayout.EndHorizontal ();
				EditorGUILayout.BeginHorizontal ();
				GUILayout.Label ("   Follow Mouse", GUILayout.Width (120));
				_map.cursorFollowMouse = EditorGUILayout.Toggle (_map.cursorFollowMouse);
			}
			EditorGUILayout.EndHorizontal ();

			EditorGUILayout.BeginHorizontal ();
			GUILayout.Label ("Allow User Rotation", GUILayout.Width (120));
			_map.allowUserRotation = EditorGUILayout.Toggle (_map.allowUserRotation);
			if (_map.allowUserRotation) {
				GUILayout.Label ("Speed");
				_map.mouseDragSensitivity = EditorGUILayout.Slider (_map.mouseDragSensitivity, 0.1f, 3);
				EditorGUILayout.EndHorizontal ();
				EditorGUILayout.BeginHorizontal ();
				GUILayout.Label ("   Right Click Centers", GUILayout.Width (120));
				_map.centerOnRightClick = EditorGUILayout.Toggle (_map.centerOnRightClick);
				EditorGUILayout.EndHorizontal ();
				EditorGUILayout.BeginHorizontal ();
				GUILayout.Label ("   Allow Keys (WASD)", GUILayout.Width (120));
				_map.allowUserKeys = EditorGUILayout.Toggle (_map.allowUserKeys);
				EditorGUILayout.EndHorizontal ();
				EditorGUILayout.BeginHorizontal ();
				GUILayout.Label (new GUIContent("   Threshold", "Amount of displacement to be considered a drag"), GUILayout.Width (120));
				_map.mouseDragThreshold = EditorGUILayout.FloatField (_map.mouseDragThreshold);
			}
			EditorGUILayout.EndHorizontal ();

			EditorGUILayout.BeginHorizontal ();
			GUILayout.Label ("Allow User Zoom", GUILayout.Width (120));
			_map.allowUserZoom = EditorGUILayout.Toggle (_map.allowUserZoom);
			GUILayout.Label ("Speed");
			_map.mouseWheelSensitivity = EditorGUILayout.Slider (_map.mouseWheelSensitivity, 0.1f, 3);
			EditorGUILayout.EndHorizontal ();

			EditorGUILayout.BeginHorizontal ();
			GUILayout.Label ("Navigation Time", GUILayout.Width (120));
			_map.navigationTime = EditorGUILayout.Slider (_map.navigationTime, 0, 10);
			EditorGUILayout.EndHorizontal ();
			
			EditorGUILayout.EndVertical ();

			// Extra components opener
			EditorGUILayout.Separator ();
 
			if (_map.isDirty) {
				EditorUtility.SetDirty (target);
			}
		}
	
		Texture2D MakeTex (int width, int height, Color col)
		{
			Color[] pix = new Color[width * height];
			
			for (int i = 0; i < pix.Length; i++)
				pix [i] = col;
			
			Texture2D result = new Texture2D (width, height);
			result.SetPixels (pix);
			result.Apply ();
			
			return result;
		}



	}

}