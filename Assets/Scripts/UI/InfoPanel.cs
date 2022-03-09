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
    private InfoField EmTotal;
    [SerializeField]
    private InfoField ImmTotal;
    [SerializeField]
    private InfoField EmFocus;
    [SerializeField]
    private InfoField ImmFocus;

    private const string ImmFocusTitleFormat = "Immigration to {0}";
    private const string EmFocusTitleFormat = "Emigration from {0}";
    public void DisplayTotal(string country, string year)
    {
        Title.gameObject.SetActive(true);
        Year.gameObject.SetActive(true);
        Title.text = country.ToUpper();
        Year.text = year;

        int totalIm = DataManager.GetTotalImmigrantsTo(country, year);
        int totalEm = DataManager.GetTotalEmigrantsFrom(country, year);
        if (totalEm == -1 || totalIm == -1)
        {
            NoData.SetActive(true);
            ImmTotal.Hide();
            EmTotal.Hide();
        }
        else
        {
            NoData.SetActive(false);
            ImmTotal.Display(NumberFormatter.Format(totalIm));
            EmTotal.Display(NumberFormatter.Format(totalEm));
        }

        EmFocus.Hide();
        ImmFocus.Hide();
    }

    public void DisplayCountryFocus(string country, string year, FlowMode mode)
    {
        if (mode == FlowMode.Immigration)
        {
            Title.text = string.Format(ImmFocusTitleFormat, country.ToUpper());
        }
        else
        {
            Title.text = string.Format(EmFocusTitleFormat, country.ToUpper());
        }
        Year.text = year;

        int totalIm = DataManager.GetTotalImmigrantsTo(country, year);
        int totalEm = DataManager.GetTotalEmigrantsFrom(country, year);
        if (totalEm == -1 || totalIm == -1)
        {
            NoData.SetActive(true);
            ImmTotal.Hide();
            EmTotal.Hide();
        }
        else
        {
            NoData.SetActive(false);
            ImmTotal.Display(NumberFormatter.Format(totalIm));
            EmTotal.Display(NumberFormatter.Format(totalEm));
        }

        EmFocus.Hide();
        ImmFocus.Hide();
    }

    public void DisplayCountry2CountryFocus(string country, string secondaryCountry, string year, FlowMode mode)
    {
        int migrants;
        InfoField textField;
        if (mode == FlowMode.Immigration)
        {
            Title.text = string.Format(ImmFocusTitleFormat, country.ToUpper());
            migrants = DataManager.GetImmigrantsToAFromB(country, secondaryCountry, year);
            textField = ImmFocus;
        }
        else
        {
            Title.text = string.Format(EmFocusTitleFormat, country.ToUpper());
            migrants = DataManager.GetEmigrantsFromAToB(country, secondaryCountry, year);
            textField = EmFocus;
        }
        Year.text = year;
        
        if (migrants == -1)
        {
            textField.Display(country, secondaryCountry, "No Data");
        }
        else
        {
            textField.Display(country, secondaryCountry, NumberFormatter.Format(migrants));
        }


        int totalIm = DataManager.GetTotalImmigrantsTo(country, year);
        int totalEm = DataManager.GetTotalEmigrantsFrom(country, year);
        if (totalEm == -1 || totalIm == -1)
        {
            NoData.SetActive(true);
            ImmTotal.Hide();
            EmTotal.Hide();
        }
        else
        {
            NoData.SetActive(false);
            ImmTotal.Display(NumberFormatter.Format(totalIm));
            EmTotal.Display(NumberFormatter.Format(totalEm));
        }
    }

    public void Hide()
    {
        ImmTotal.Hide();
        EmTotal.Hide();
        EmFocus.Hide();
        ImmFocus.Hide();
        Title.gameObject.SetActive(false);
        Year.gameObject.SetActive(false);
        NoData.SetActive(false);
    }
}
