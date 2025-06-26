using UnityEngine;

public class FirstPanelController : MonoBehaviour
{
    public GameObject firstPanel;
    public GameObject settingPanel;

    public Vector3 firstPanelOrigin = new Vector3(-2066, 727, 0);
    public Vector3 center = Vector3.zero;

    public void OnFirstOpen()
    {
        firstPanel.SetActive(true);
        firstPanel.transform.localPosition = center;
    }

    public void OnClose()
    {
        firstPanel.transform.localPosition = firstPanelOrigin;
    }

    public void OnOpenSetting()
    {
        firstPanel.SetActive(false);
        settingPanel.SetActive(true);
        settingPanel.transform.localPosition = center;
    }
}