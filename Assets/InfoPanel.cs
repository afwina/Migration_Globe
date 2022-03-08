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
