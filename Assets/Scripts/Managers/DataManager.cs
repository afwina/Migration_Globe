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
    private static Dictionary<string, int[]> TotalPopulation;
    private static Dictionary<string, uint[,]> MigrationData; // year[dest,origin]
    private static Dictionary<string, uint[]> TotalEmigrants; // [year][origin], people from origin to WORLD
    private static Dictionary<string, uint[]> TotalImmigrants; // [year][dest], people going into dest
    private static uint MaxEmigrationTotal = 0;
    private static uint MaxImmigrationTotal = 0;

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

    public static string GetOriginCountry(int index)
    {
        return Origins[index];
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

    public static uint[] GetTotalImmigrants(string year)
    {
        return TotalImmigrants[year];
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

    public static uint[] GetTotalEmigrants(string year)
    {
        return TotalEmigrants[year];
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
                        matrix[index, i-1] = counts[i-1];
                    }
                }
                else
                {
                    uint[,] migrationMatrix = new uint[Destinations.Count, Origins.Count];
                    for (int i = 1; i < counts.Length; i++)
                    {
                        migrationMatrix[index, i-1] = counts[i-1];
                    }
                    MigrationData.Add(year, migrationMatrix);
                }
            }
            Debug.Log("Migration data loaded.");
        }
    }
}
