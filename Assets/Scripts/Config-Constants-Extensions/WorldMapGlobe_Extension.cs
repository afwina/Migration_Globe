using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace WPM
{
    public partial class WorldMapGlobe : MonoBehaviour
    {
        private Dictionary<string, Coroutine> HighlightingAnims = new Dictionary<string, Coroutine>();
        private List<Coroutine> UpdateAnims = new List<Coroutine>();

        public void ColorAllRegionsInstant(Color color)
        {
            foreach (var country in countries)
            {
                ToggleCountrySurface(country.name, true, color);
                country.valueColor = color;
            }
        }

        public void UpdateCountry(string country, Color color, float duration)
        {
            int index = GetCountryIndex(country);
            if (index != -1)
            {
                if (!countries[index].highlighted)
                {
                    var co = StartCoroutine(FadeColor(index, color, duration));
                    UpdateAnims.Add(co);
                }
                countries[index].valueColor = color;
            }
        }

        private IEnumerator FadeColor(int countryIndex, Color end, float duration, Action onComplete = null)
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

            for (int i = 0; i < regions.Count; i++)
            {
                Material finalMat = GetColoredTexturedMaterial(end);
                regions[i].customMaterial = finalMat;
                ApplyMaterialToSurface(surfs[i], finalMat);
            }

            onComplete?.Invoke();
        }

        public void StopFading()
        {
            foreach(var anim in UpdateAnims)
            {
                StopCoroutine(anim);
            }
            UpdateAnims.Clear();
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
                if (HighlightingAnims.ContainsKey(country))
                {
                    StopCoroutine(HighlightingAnims[country]);
                }

                Coroutine co = StartCoroutine(FadeColor(index, endColor, duration));
                countries[index].highlighted = true;
                if (HighlightingAnims.ContainsKey(country))
                {
                    HighlightingAnims[country] = co;
                }
                else
                {
                    HighlightingAnims.Add(country, co);
                }
            }
        }

        public void StopCountryCoroutine(string country)
        {
            if (HighlightingAnims.TryGetValue(country, out Coroutine co))
            {
                StopCoroutine(co);

                int index = GetCountryIndex(country);
                if (index != -1)
                {
                    countries[index].highlighted = false;
                    StartCoroutine(FadeColor(index, countries[index].valueColor, 0.1f));
                }
            }
        }

        public void PulseCountry(string country, Gradient grad,  float period)
        {
            int index = GetCountryIndex(country);
            if (index != -1)
            {
                if (HighlightingAnims.ContainsKey(country))
                {
                    StopCoroutine(HighlightingAnims[country]);
                }

                Coroutine co = StartCoroutine(PulseIndefinitely(index, grad, period));
                countries[index].highlighted = true;
                if (HighlightingAnims.ContainsKey(country))
                {
                    HighlightingAnims[country] = co;
                }
                else
                {
                    HighlightingAnims.Add(country, co);
                }
            }
        }

        public IEnumerator PulseIndefinitely(int countryIndex, Gradient grad, float period)
        {
            var regions = countries[countryIndex].regions;
            GameObject[] surfs = new GameObject[regions.Count];
            for (int i = 0; i < regions.Count; i++)
            {
                int cacheIndex = GetCacheIndexForCountryRegion(countryIndex, i);
                surfs[i] = surfaces[cacheIndex];
                UpdateSurfaceCount();
            }

            float time = 0;
            bool cycle = true;
            Color start = regions[0].customMaterial.color;
            while(time < period)
            {
                Color color = Color.Lerp(start, grad.Evaluate(0), time / period);
                for (int i = 0; i < regions.Count; i++)
                {
                    Material coloredMat = GetColoredTexturedMaterial(color);
                    regions[i].customMaterial = coloredMat;
                    ApplyMaterialToSurface(surfs[i], coloredMat);
                }
                time += Time.deltaTime;
                yield return null;
            }

            time = 0;
            while (true)
            {
                if (time > period)
                {
                    time = 0;
                    cycle = !cycle;
                }

                Color color = cycle ? grad.Evaluate(time/period) : grad.Evaluate((period-time)/period);
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
    }
}
