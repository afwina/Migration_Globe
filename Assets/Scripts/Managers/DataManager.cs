using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Linq;
using System;

public static class DataManager
{
    private const string TotalPopulationEditorFilePath = "/Data/TotalPopulation_Thousands.csv";
    private const string MigrationEditorFilePath = "/Data/MigrantStockByOriginAndDestination.csv";

    private static List<string> Years;
    public static List<string> Origins { private set; get; }
    public static List<string> Destinations { private set; get; }
    private static Dictionary<string, int[]> TotalPopulation; // [country][year], population in thousands
    private static Dictionary<string, int> TotalMigrants;
    private static Dictionary<string, uint[,]> MigrationData; // [year][dest,origin]
    private static Dictionary<string, uint[]> TotalEmigrants; // [year][origin], people from origin to WORLD
    private static Dictionary<string, uint[]> TotalImmigrants; // [year][dest], people going into dest
    private static uint MaxEmigrationTotal = 0;
    private static uint MaxImmigrationTotal = 0;

    public static int GetTotalPopulation(string country, string year)
    {
        return TotalPopulation[country][Years.IndexOf(year)] * 1000;
    }

    public static int GetTotalMigrants(string year)
    {
        return TotalMigrants[year];
    }

    public static uint GetMaxTotal(FlowMode mode)
    {
        if (mode == FlowMode.Emigration)
        {
            return MaxEmigrationTotal;
        }
        else
        {
            return MaxImmigrationTotal;
        }
    }

    public static uint[] GetImmigrationTo(string dest, string year)
    {
        int index = Destinations.IndexOf(dest);
        if (index != -1)
        {
            var data = new uint[Origins.Count];
            for (int i = 0; i < Origins.Count; i++)
            {
                data[i] = MigrationData[year][index, i];
            }
            return data;
        }

        return new uint[0];
    }
   
    public static int GetImmigrantsToAFromB(string A, string B, string year)
    {
        int indexA = Destinations.IndexOf(A);
        int indexB = Origins.IndexOf(B);
        if (indexA != -1 && indexB != -1)
        {
            return (int)MigrationData[year][indexA, indexB];
        }

        return -1;
    }

    public static uint[] GetTotalImmigrants(string year)
    {
        return TotalImmigrants[year];
    }

    public static float[] GetTotalImmigrantsPercent(string year)
    {
        uint[] value = GetTotalImmigrants(year);
        float[] percent = new float[value.Length];
        for(int i = 0; i < Destinations.Count; i++)
        {
            percent[i] = value[i] / (float) GetTotalPopulation(Destinations[i],year);
        }
        return percent;
    }

    public static int GetTotalImmigrantsTo(string country, string year)
    {
        int index = Destinations.IndexOf(country);
        if (index != -1)
        {
            return (int)TotalImmigrants[year][index];
        }
        else
        {
            return -1;
        }
    }

    public static uint[] GetEmigrantsFrom(string origin, string year)
    {
        int index = Origins.IndexOf(origin);
        if (index != -1)
        {
            var data = new uint[Destinations.Count];
            for (int i = 0; i < Destinations.Count; i++)
            {
                data[i] = MigrationData[year][i, index];
            }
            return data;
        }

        return new uint[0];
    }

    public static int GetEmigrantsFromAToB(string A, string B, string year)
    {
        int indexA = Origins.IndexOf(A);
        int indexB = Destinations.IndexOf(B);
        if (indexA != -1 && indexB != -1)
        {
            return (int) MigrationData[year][indexB, indexA];
        }

        return -1;
    }

    public static uint[] GetTotalEmigrants(string year)
    {
        return TotalEmigrants[year];
    }

    public static float[] GetTotalEmigrantsPercent(string year)
    {
        uint[] value = GetTotalEmigrants(year);
        float[] percent = new float[value.Length];
        int yearIndex = Years.IndexOf(year);
        for (int i = 0; i < Origins.Count; i++)
        {
            percent[i] = value[i] / (float) GetTotalPopulation(Origins[i],year);
        }
        return percent;
    }

    public static int GetTotalEmigrantsFrom(string country, string year)
    {
        int index = Origins.IndexOf(country);
        if (index != -1)
        {
            return (int)TotalEmigrants[year][index];
        }
        else
        {
            return -1;
        }
    }

    public static List<Tuple<string, int>> GetTopEmigrantDestinations(string fromCountry, string year, int amount)
    {
        var top = new List<Tuple<string, int>>();
        var all = GetEmigrantsFrom(fromCountry, year);
        for (int i = 0; i< all.Length; i++)
        {
            top.Add(new Tuple<string, int>(Destinations[i], (int)all[i]));
        }
        return top.Count > 0 ? top.OrderByDescending(x => x.Item2).Take(amount).ToList() : null;
    }

    public static List<Tuple<string, int>> GetTopImmigrantOrigins(string toCountry, string year, int amount)
    {
        var top = new List<Tuple<string, int>>();
        var all = GetImmigrationTo(toCountry, year);
        for (int i = 0; i < all.Length; i++)
        {
            top.Add(new Tuple<string, int>(Origins[i], (int)all[i]));
        }
        return top.OrderByDescending(x => x.Item2).Take(amount).ToList();
    }

    public static int GetRoundedImmMax(string country)
    {
        List<int> maxs = new List<int>();
        foreach (string y in Years)
        {
            var data = GetImmigrationTo(country, y);
            maxs.Add((int)data.Max());
        }

        return NumberFormatter.Round(maxs.Max());
    }

    public static int GetRoundedEmMax(string country)
    {
        List<int> maxs = new List<int>();
        foreach (string y in Years)
        {
            var data = GetEmigrantsFrom(country, y);
            maxs.Add((int)data.Max());
        }

        return NumberFormatter.Round(maxs.Max());
    }

    public static List<string> GetYears()
    {
        return Years;
    }

    public static void LoadData()
    {
        LoadTotalPopulation();
        LoadMigrationData();
    }

    private static void LoadTotalPopulation()
    {
        using (var reader = new StreamReader(Application.streamingAssetsPath + TotalPopulationEditorFilePath))
        {
            TotalPopulation = new Dictionary<string, int[]>();
            Years = new List<string>();
            var columns = reader.ReadLine().Split(',');
            for (int i = 1; i < columns.Length; i++)
            {
                Years.Add(columns[i]);
            }

            while (!reader.EndOfStream)
            {
                var line = reader.ReadLine();
                var values = line.Split(',');
                string[] populations =  new string[Years.Count];
                Array.Copy(values, 1, populations, 0, Years.Count);
                TotalPopulation.Add(values[0], Array.ConvertAll(populations, int.Parse));
            }
            Debug.Log("Total Population Loaded");
        }
    }

    private static void LoadMigrationData()
    {
        if (Years == null || Years.Count < 1) 
        {
            Debug.LogError("No years registered! Load population data or ensure population data is valid.");
            return;
        }

        using (var reader = new StreamReader(Application.streamingAssetsPath + MigrationEditorFilePath))
        {
            MigrationData = new Dictionary<string, uint[,]>();
            TotalEmigrants = new Dictionary<string, uint[]>();
            TotalImmigrants = new Dictionary<string, uint[]>();
            TotalMigrants = new Dictionary<string, int>();

            Destinations = reader.ReadLine().Split(',').Skip(1).ToList();
            Origins = reader.ReadLine().Split(',').Skip(3).ToList();

            while (!reader.EndOfStream)
            {
                var line = reader.ReadLine().Split(',');
                string year = line[0];
                string destination = line[1];
                uint[] counts = Array.ConvertAll(line.Skip(2).ToArray(), (s) => { if (string.IsNullOrEmpty(s)) return (uint)0; else { uint.TryParse(s, out uint result); return result; } });

                if (string.Equals(destination, "WORLD"))
                {
                    TotalEmigrants.Add(year,counts.Skip(1).ToArray());
                    MaxEmigrationTotal = Math.Max(counts.Skip(1).Max(), MaxEmigrationTotal);
                    TotalMigrants.Add(year, (int) counts[0]);
                    continue;
                }

                int index = Destinations.IndexOf(destination);
                if (TotalImmigrants.TryGetValue(year, out uint[] total))
                {
                    total[index] = counts[0];
                }
                else
                {
                    uint[] totalImm = new uint[Destinations.Count];
                    totalImm[index] = counts[0];
                    TotalImmigrants.Add(year, totalImm);
                }

                if (!destination.Equals("WORLD"))
                {
                    MaxImmigrationTotal = Math.Max(counts[0], MaxImmigrationTotal);
                }

                if (MigrationData.TryGetValue(year, out uint[,] matrix))
                {
                    for (int i = 1; i < counts.Length; i++)
                    {
                        matrix[index, i-1] = counts[i];
                    }
                }
                else
                {
                    uint[,] migrationMatrix = new uint[Destinations.Count, Origins.Count];
                    for (int i = 1; i < counts.Length; i++)
                    {
                        migrationMatrix[index, i-1] = counts[i];
                    }
                    MigrationData.Add(year, migrationMatrix);
                }
            }
            Debug.Log("Migration data loaded.");
        }
    }
}
