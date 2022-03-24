using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class InfoPanel : MonoBehaviour
{
    private const int TOP_STATS_COUNT = 3;
    private const float UPDATE_DELAY = 0.1f;

    [SerializeField]
    private TextMeshProUGUI Title;
    [SerializeField]
    private TextMeshProUGUI Year;

    [SerializeField]
    private GameObject NoData;
    [SerializeField]
    private InfoField TotalMigrants;
    [SerializeField]
    private InfoField EmTotal;
    [SerializeField]
    private InfoField ImmTotal;
    [SerializeField]
    private InfoField EmFocus;
    [SerializeField]
    private InfoField ImmFocus;
    [SerializeField]
    private InfoField EmTop;
    [SerializeField]
    private InfoField ImmTop;

    private const string ImmFocusTitleFormat = "Immigration to {0}";
    private const string EmFocusTitleFormat = "Emigration from {0}";
    private const string ImmTotalTitleFormat = "IMMIGRATION WORLDWIDE";
    private const string EmTotalTitleFormat = "EMIGRATION WORLDWIDE";
    public void DisplayCountryTotal(string country, string year, FlowMode mode)
    {
        Title.gameObject.SetActive(true);
        Year.gameObject.SetActive(true);
        Title.text = country.ToUpper();
        Year.text = year;

        HideFields();

        int total;
        InfoField field;
        if (mode == FlowMode.Immigration)
        {
            total = DataManager.GetTotalImmigrantsTo(country, year);
            field = ImmTotal;
        }
        else
        {
            total= DataManager.GetTotalEmigrantsFrom(country, year);
            field = EmTotal;

        }

        if (total == -1)
        {
            NoData.SetActive(true);
        }
        else
        {
            field.Display(NumberFormatter.Format(total));
        }
    }

    public void DisplayCountryFocus(string country, string year, FlowMode mode)
    {
        Year.text = year;
        int total;
        InfoField field;
        List<System.Tuple<string, int>> top;
        InfoField topField;
        if (mode == FlowMode.Immigration)
        {
            Title.text = string.Format(ImmFocusTitleFormat, country.ToUpper());
            total = DataManager.GetTotalImmigrantsTo(country, year);
            field = ImmTotal;
            top = DataManager.GetTopImmigrantOrigins(country, year, TOP_STATS_COUNT);
            topField = ImmTop;
        }
        else
        {
            Title.text = string.Format(EmFocusTitleFormat, country.ToUpper());
            total = DataManager.GetTotalEmigrantsFrom(country, year);
            field = EmTotal;
            top = DataManager.GetTopEmigrantDestinations(country, year, TOP_STATS_COUNT);
            topField = EmTop;
        }

        HideFields();

        if (total == -1)
        {
            NoData.SetActive(true);
        }
        else
        {
            NoData.SetActive(false);
            field.Display(NumberFormatter.Format(total));

            string[] args = new string[TOP_STATS_COUNT * 2];
            for(int i = 0; i < TOP_STATS_COUNT; i++)
            {
                args[i*2] = top[i].Item1;
                args[i * 2 + 1] = NumberFormatter.Format(top[i].Item2);
            }
            topField.Display(UPDATE_DELAY, args);
        }

    }

    public void DisplayCountry2CountryFocus(string country, string secondaryCountry, string year, FlowMode mode)
    {
        Year.text = year;

        int migrants, total;
        InfoField textField, totalField;
        if (mode == FlowMode.Immigration)
        {
            Title.text = string.Format(ImmFocusTitleFormat, country.ToUpper());
            migrants = DataManager.GetImmigrantsToAFromB(country, secondaryCountry, year);
            total = DataManager.GetTotalImmigrantsTo(country, year);
            textField = ImmFocus;
            totalField = ImmTotal;
        }
        else
        {
            Title.text = string.Format(EmFocusTitleFormat, country.ToUpper());
            migrants = DataManager.GetEmigrantsFromAToB(country, secondaryCountry, year);
            total = DataManager.GetTotalEmigrantsFrom(country, year);
            textField = EmFocus;
            totalField = EmTotal;
        }

        HideFields();

        if (total == -1)
        {
            NoData.SetActive(true);
        }
        else
        {
            NoData.SetActive(false);
            totalField.Display(NumberFormatter.Format(total));
        }

        if (migrants == -1)
        {
            textField.Display(UPDATE_DELAY, country, secondaryCountry, "No Data");

        }
        else
        {
            textField.Display(UPDATE_DELAY, country, secondaryCountry, NumberFormatter.Format(migrants));
        }
    }

    public void Hide()
    {
        HideFields();
        Title.gameObject.SetActive(false);
        Year.gameObject.SetActive(false);
    }

    public void HideFields()
    {
        TotalMigrants.Hide();
        ImmTotal.Hide();
        EmTotal.Hide();
        EmFocus.Hide();
        ImmFocus.Hide();
        EmTop.Hide();
        ImmTop.Hide();
        NoData.SetActive(false);
    }

    public void DisplayTotalTitle(FlowMode mode, string year)
    {
        HideFields();
        TotalMigrants.Display(NumberFormatter.Format(DataManager.GetTotalMigrants(year)));
        if (mode == FlowMode.Immigration)
        {
            Title.text = ImmTotalTitleFormat;
        }
        else
        {
            Title.text = EmTotalTitleFormat;
        }
        Year.text = year;
    }
}
