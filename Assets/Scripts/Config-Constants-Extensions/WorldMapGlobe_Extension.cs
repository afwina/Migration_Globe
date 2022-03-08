using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace WPM
{
    public partial class WorldMapGlobe : MonoBehaviour
    {
        public void ColorAllRegionsInstant(Color color)
        {           
            foreach (var country in countries)
            {
                ToggleCountrySurface(country.name, true, color);
            }
        }

        public void FadeCountryIntoColor(string country, Color color, float duration)
        {
            int index = GetCountryIndex(country);
            if (index != -1)
            {
                StartCoroutine(FadeColor(index, color, duration));
            }
        }

        private IEnumerator FadeColor(int countryIndex, Color end, float duration)
        {
            var regions = countries[countryIndex].regions;
            GameObject[] surfs = new GameObject[regions.Count];
            for (int i = 0; i < regions.Count; i++)
            {
                int cacheIndex = GetCacheIndexForCountryRegion(countryIndex, i);
                surfs[i] = surfaces[cacheIndex];
                UpdateSurfaceCount();
            }

            Color start = regions[0].customMaterial.color;
            float time = 0;

            while (time < duration)
            {
                Color color = Color.Lerp(start, end, time / duration);
                for (int i = 0; i < regions.Count; i++)
                {
                    Material coloredMat = GetColoredTexturedMaterial(color);
                    regions[i].customMaterial = coloredMat;
                    ApplyMaterialToSurface(surfs[i], coloredMat);
                }
                time += Time.deltaTime;
                yield return null;
            }
        }

        public void StopFading()
        {
            StopAllCoroutines();
        }

        public int CheckCountryHover(Vector3 mousePos)
        {
            Ray ray = Camera.main.ScreenPointToRay(mousePos);
            RaycastHit[] hits;
            hits = Physics.RaycastAll(Camera.main.transform.position, ray.direction, 8, layerMask);
            if (hits.Length > 0)
            {
                for (int k = 0; k < hits.Length; k++)
                {
                    if (hits[k].collider.gameObject == gameObject)
                    {
                        var location = transform.InverseTransformPoint(hits[k].point);

                        if (GetCountryUnderMouse(location, out int c, out int cr))
                        {
                            return c;
                        }
                    }
                }
            }

            return -1;
        }

        public void EnlargeCountry(string country)
        {
            int index = GetCountryIndex(country);
            if (index != -1)
            {
                var regions = countries[index].regions;
                GameObject[] surfs = new GameObject[regions.Count];
                for (int i = 0; i < regions.Count; i++)
                {
                    int cacheIndex = GetCacheIndexForCountryRegion(index, i);
                    surfs[i] = surfaces[cacheIndex];
                    surfs[i].transform.localScale = new Vector3(1.05f,1.05f, 1.05f);
                }
            }
        }
    }
}
