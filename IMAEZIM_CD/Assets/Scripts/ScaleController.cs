using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.UI;
using Unity.XR.CoreUtils;
using Unity.VisualScripting;

public class ScaleController : MonoBehaviour
{
    XROrigin m_XROrigin;
    public Slider scaleSlider;
    private void Awake()
    {
        m_XROrigin = GetComponent<XROrigin>();
    }

    // Start is called before the first frame update
    void Start()
    {
        scaleSlider.onValueChanged.AddListener(OnSliderValueChanged);
    }


    public void OnSliderValueChanged(float value)
    {
        if (scaleSlider != null)
        {
            //origin 크기 조정
            m_XROrigin.transform.localScale = Vector3.one / value;
        }
    }

}
