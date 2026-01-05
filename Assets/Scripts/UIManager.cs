using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System;
using System.Runtime.InteropServices.WindowsRuntime;

public class UIManager : MonoBehaviour
{
    [Header("Input Fields")]
    public Canvas customizationCanvas;
    [Space] public TMP_InputField widthInput;
    public TMP_InputField lengthInput;
    public Slider widthSlider;
    public Slider lengthSlider;

    private int minDimension = 5;
    private int maxDimension = 50;
    private Dictionary<TMP_InputField, Slider> inputPairs = new Dictionary<TMP_InputField, Slider>();

    [Space] public Button spawnPlayerButton;

    [Header("Point Markers"), SerializeField]
    private Canvas pointMarkersCanvas;

    [SerializeField] private Camera topDownCamera;
    [Space, SerializeField]
    private RectTransform startMarker;
    [SerializeField] private RectTransform endMarker;

    [Header("Others"), SerializeField] private Canvas victoryCanvas;
    [SerializeField] private Canvas markerCanvas;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        inputPairs.Add(widthInput, widthSlider);
        inputPairs.Add(lengthInput, lengthSlider);
    }

    public void HaveInputChangeSlider(TMP_InputField wanted)
    {
        if (!inputPairs.ContainsKey(wanted))
            return;

        Slider slider = inputPairs[wanted];

        if (wanted.text.Length <= 0)
        {
            slider.value = minDimension;
            return;
        }
        int i = Mathf.Clamp(Convert.ToInt32(wanted.text), minDimension, maxDimension);

        wanted.text = i.ToString();

        slider.value = i;
    }

    public void HaveSliderChangeInput(TMP_InputField wanted)
    {
        if (!inputPairs.ContainsKey(wanted))
            return;

        Slider slider = inputPairs[wanted];

        wanted.text = slider.value.ToString();
    }

    public void SetSpawnPlayerButton(bool state) => spawnPlayerButton.gameObject.SetActive(state);

    public void SetCustomizationCanvas(bool state) => customizationCanvas.gameObject.SetActive(state);

    public void SetPointMarkerCanvas(bool state) => pointMarkersCanvas.gameObject.SetActive(state);

    public void SetVictoryCanvas(bool state) => victoryCanvas.gameObject.SetActive(state);

    public void SetPositionMarkers(Vector3 startWorldPos, Vector3 endWorldPos)
    {
        PositionMarker(startMarker, startWorldPos);
        PositionMarker(endMarker, endWorldPos);
    }

    public void ResetPositionMarkers()
    {
        startMarker.anchoredPosition = new Vector2(-9999, -9999);
        endMarker.anchoredPosition = new Vector2(-9999, -9999);
    }

    void PositionMarker(RectTransform marker, Vector3 worldPosition)
    {
        Vector3 screenPos = topDownCamera.WorldToScreenPoint(worldPosition);

        if (screenPos.z < 0)
            return;

        RectTransform canvasRect = customizationCanvas.transform as RectTransform;

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvasRect,
            screenPos,
            customizationCanvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : topDownCamera,
            out Vector2 uiPos
        );

        marker.anchoredPosition = uiPos;
    }
}
