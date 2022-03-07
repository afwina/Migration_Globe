using UnityEngine;
using System.Collections;

namespace WPM {
	public class CityInfo : MonoBehaviour {

		public City city;
		public WorldMapGlobe wmp;

		void Start() {
			if (this.wmp==null) {
				Destroy (this);
			}
		}

		void OnMouseEnter () {
			wmp.cityHighlighted = city;
			wmp.mouseIsOver = true;
		}

		void OnMouseExit () {
			wmp.cityHighlighted = null;
		}

	}
}
