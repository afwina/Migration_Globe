using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace WPM {

	public class Province: IAdminEntity {
		public string name { get; set; }
		public int countryIndex;
		public List<Region> regions { get; set; }
		public Vector3 center { get; set; }
		/// <summary>
		/// Index of the biggest region
		/// </summary>
		public int mainRegionIndex { get; set; }

		#region internal fields
		/// Used internally. Don't change this value.
		//		public bool isColored;
		public Material customMaterial { get; set; }
		public Color color;
		public string packedRegions;
		#endregion

		public Province (string name, int countryIndex) {
			this.name = name;
			this.countryIndex = countryIndex;
			this.regions = null; // lazy load during runtime due to size of data
			this.center = MiscVector.Vector3zero;
		}

	}
}

