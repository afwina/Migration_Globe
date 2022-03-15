using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class InfoPanel : MonoBehaviour
{
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

    private const string ImmFocusTitleFormat = "Immigration to {0}";
    private const string EmFocusTitleFormat = "Emigration from {0}";
    private const string ImmTotalTitleFormat = "TOTAL IMMIGRATION WORLDWIDE";
    private const string EmTotalTitleFormat = "TOTAL EMIGRATION WORLDWIDE";
    public void DisplayCountryTotal(string country, string year)
    {
        Title.gameObject.SetActive(true);
        Year.gameObject.SetActive(true);
        Title.text = country.ToUpper();
        Year.text = year;

        HideFields();

        int totalIm = DataManager.GetTotalImmigrantsTo(country, year);
        int totalEm = DataManager.GetTotalEmigrantsFrom(country, year);
        if (totalEm == -1 || totalIm == -1)
        {
            NoData.SetActive(true);
        }
        else
        {
            ImmTotal.Display(NumberFormatter.Format(totalIm));
            EmTotal.Display(NumberFormatter.Format(totalEm));
        }
    }

    public void DisplayCountryFocus(string country, string year, FlowMode mode)
    {
        Year.text = year;
        int total;
        InfoField field;
        if (mode == FlowMode.Immigration)
        {
            Title.text = string.Format(ImmFocusTitleFormat, country.ToUpper());
            total = DataManager.GetTotalImmigrantsTo(country, year);
            field = ImmTotal;
        }
        else
        {
            Title.text = string.Format(EmFocusTitleFormat, country.ToUpper());
            total = DataManager.GetTotalEmigrantsFrom(country, year);
            field = EmTotal;
        }

        HideFields();

        if (total == -1)
        {
            NoData.SetActive(true);

        }
        else
        {
            NoData.SetActive(false);
            EmTotal.Display(NumberFormatter.Format(total));
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

        if (migrants == -1)
        {
            textField.Display(country, secondaryCountry, "No Data");

        }
        else
        {
            textField.Display(country, secondaryCountry, NumberFormatter.Format(migrants));
        }


        if (total == -1)
        {
            NoData.SetActive(true);
        }
        else
        {
            NoData.SetActive(false);
            totalField.Display(NumberFormatter.Format(total));
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
