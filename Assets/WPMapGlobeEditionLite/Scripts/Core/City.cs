using UnityEngine;
using System.Collections;

namespace WPM {
	public class City {
		public string name;
		public int countryIndex;
		public Vector3 unitySphereLocation;
		public int population;

		public City (string name, int countryIndex, int population, Vector3 location) {
			this.name = name;
			this.countryIndex = countryIndex;
			this.population = population;
			this.unitySphereLocation = location;
		}

		public City Clone() {
			City c = new City(name, countryIndex, population, unitySphereLocation);
			return c;
		}
	}
}