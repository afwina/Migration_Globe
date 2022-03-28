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
        private Dictionary<string, GameObject[]> Outlines = new Dictionary<string, GameObject[]>();

        public void CreateOutlines()
        {
            for (int i = 0; i < countries.Length; i++)
            {
                var regions = countries[i].regions;
                for (int j = 0; j < regions.Count; j++)
                {
                    int cacheIndex = GetCacheIndexForCountryRegion(i, j);
                    var outline = DrawCountryRegionOutline(regions[j], surfaces[cacheIndex]);
                    outline.SetActive(false);
                    if (Outlines.ContainsKey(countries[i].name))
                    {
                        Outlines[countries[i].name][j] = outline;
                    }
                    else
                    {
                        Outlines.Add(countries[i].name, new GameObject[regions.Count]);
                        Outlines[countries[i].name][0] = outline;
                    }
                }
            }
        }

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

                foreach (var outline in Outlines[country])
                {
                    outline.SetActive(false);
                }
            }
        }

        private void RegisterHighlightingAnim(string country, int index, Coroutine co)
        {
            if (HighlightingAnims.ContainsKey(country))
            {
                StopCoroutine(HighlightingAnims[country]);
            }

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

        public void GlowCountry(string country, float duration, Color endColor)
        {
            int index = GetCountryIndex(country);
            if (index != -1)
            {
                Coroutine co = StartCoroutine(FadeColor(index, endColor, duration));
                RegisterHighlightingAnim(country, index, co);
            }
        }

        public void OutlineCountry(string country, float duration, Gradient grad)
        {
            int index = GetCountryIndex(country);
            if (index != -1)
            {
                Coroutine co = StartCoroutine(PulseOutlineIndefinitely(country, grad, duration));
                RegisterHighlightingAnim(country, index, co);
            }
        }

        private IEnumerator PulseOutlineIndefinitely(string country, Gradient grad, float duration)
        {
            foreach (var outline in Outlines[country])
            {
                outline.SetActive(true);
            }

            float time = 0;
            bool cycle = true;
            var mesh = Outlines[country][0].GetComponent<MeshRenderer>().sharedMaterial;
            while (true)
            {
                if (time > duration)
                {
                    time = 0;
                    cycle = !cycle;
                }

                mesh.color = cycle ? grad.Evaluate(time / duration) : grad.Evaluate((duration - time) / duration);
                time += Time.deltaTime;
                yield return null;
            }
        }

        public void PulseCountry(string country, Gradient grad,  float period)
        {
            int index = GetCountryIndex(country);
            if (index != -1)
            {
                Coroutine co = StartCoroutine(PulseIndefinitely(index, grad, period));
                RegisterHighlightingAnim(country, index, co);
            }
        }

        private IEnumerator PulseIndefinitely(int countryIndex, Gradient grad, float period)
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
    
        public void Darken(string country)
        {
            int index = GetCountryIndex(country);
            if (index != -1)
            {
                var color = countries[index].valueColor;
                ToggleCountrySurface(index, true, new Color(color.r, color.g, color.b, 0.7f));
                countries[index].darkened = true;
            }
        }

        public void Undarken(string country)
        {
            int index = GetCountryIndex(country);
            if (index != -1)
            {
                ToggleCountrySurface(index, true, countries[index].valueColor);
                countries[index].darkened = false;
            }
        }
    }
}
