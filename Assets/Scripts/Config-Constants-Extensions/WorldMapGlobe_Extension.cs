using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace WPM
{
    public partial class WorldMapGlobe : MonoBehaviour
    {
        private Dictionary<string, Tuple<Coroutine, Color>> StoppableCoroutines = new Dictionary<string, Tuple<Coroutine, Color>>();
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

        public void GlowCountry(string country, float duration, Color endColor)
        {
            int index = GetCountryIndex(country);
            if (index != -1)
            {
                Coroutine co = StartCoroutine(FadeColor(index, endColor, duration));
                if (StoppableCoroutines.TryGetValue(country, out Tuple<Coroutine, Color> tuple))
                {
                    StoppableCoroutines[country] = new Tuple<Coroutine, Color>(co, countries[index].regions[0].customMaterial.color);
                }
                else
                {
                    StoppableCoroutines.Add(country, new Tuple<Coroutine, Color>(co, countries[index].regions[0].customMaterial.color));
                }
            }
        }

        public void StopGlowCountry(string country)
        {
            if (StoppableCoroutines.TryGetValue(country, out Tuple<Coroutine, Color> tuple))
            {
                StopCoroutine(tuple.Item1);
                FadeCountryIntoColor(country, tuple.Item2, 0.5f);
                StoppableCoroutines.Remove(country);
            }
        }

        public void PulseCountry(string country, Color color1, Color color2)
        {

        }
    }
}
