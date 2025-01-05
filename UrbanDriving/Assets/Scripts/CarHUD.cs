using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using TMPro;
using System;

public class CarHUD : MonoBehaviour
{
    [SerializeField]
    private GameObject gearSwitchMenu;
    [SerializeField]
    private CarPhysics carPhysics;
    [SerializeField]
    private CarController carController;
    [Header("HUD Displays")]
    [SerializeField]
    private TextMeshProUGUI speedDisplay;
    [SerializeField]
    private TextMeshProUGUI rpmDisplay;
    [SerializeField]
    private TextMeshProUGUI gearDisplay;

    [Header("Gear Switch Images")]
    [SerializeField]
    private Image gearP;
    [SerializeField]
    private Image gearN;
    [SerializeField]
    private Image gearD;
    [SerializeField]
    private Image gearR;
    [SerializeField]
    private RectTransform gearArrow;

    [SerializeField]
    private InputActionReference gearRadial;
    [SerializeField]
    private InputActionReference gearSelectButton;

    bool gearSelectWasPressed = false;
    AutomaticGear radialSelectedGear = AutomaticGear.None;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        speedDisplay.text = "Скорость: " + Mathf.RoundToInt(carPhysics.GetCarSpeed(true)).ToString() + " км/ч";
        rpmDisplay.text = "Обороты/мин: " + (Mathf.Round(carPhysics.GetRPM() * 100) / 100).ToString() + " тыс.";
        gearDisplay.text = "Режим: ";

        switch (carPhysics.GetAutomaticGear())
        {
            case AutomaticGear.Neutral:
                gearDisplay.text += "N";
                break;
            case AutomaticGear.Parking:
                gearDisplay.text += "P";
                break;
            case AutomaticGear.Driving:
                gearDisplay.text += "D";
                break;
            case AutomaticGear.Reverse:
                gearDisplay.text += "R";
                break;
        }

        
        if (gearSelectButton.action.IsPressed() && carController.GetBrakeValue() > 0.95f && carController.GetGasValue() < 0.01f)
        {
            gearSwitchMenu.SetActive(true);
            gearSelectWasPressed = true;
            RotateArrow(gearRadial.action.ReadValue<Vector2>());

            gearD.color = Color.gray;
            gearP.color = Color.gray;
            gearN.color = Color.gray;
            gearR.color = Color.gray;
            radialSelectedGear = GetRadialGearSelect();
            switch (radialSelectedGear)
            {
                case AutomaticGear.Neutral:
                    gearN.color = Color.white;
                    break;
                case AutomaticGear.Parking:
                    gearP.color = Color.white;
                    break;
                case AutomaticGear.Driving:
                    gearD.color = Color.white;
                    break;
                case AutomaticGear.Reverse:
                    gearR.color = Color.white;
                    break;
            }
        }
        else if (gearSelectWasPressed)
        {
            gearSelectWasPressed = false;
            if (radialSelectedGear != AutomaticGear.None) carPhysics.SetAutomaticGear(radialSelectedGear);
            gearSwitchMenu.SetActive(false);
        }
    }

    void RotateArrow(Vector2 inputAngle)
    {
        gearArrow.localEulerAngles = Vector3.forward * Mathf.LerpAngle(gearArrow.localEulerAngles.z, VectorToAngle(inputAngle) - 90.0f, 0.3f);
    }

    float VectorToAngle(Vector2 input)
    {
        return (float)Math.Atan2(input.y, input.x) * Mathf.Rad2Deg;
    }

    AutomaticGear GetRadialGearSelect()
    {
        float arrowAngle = gearArrow.localEulerAngles.z;
        arrowAngle = -arrowAngle;
        
        if (arrowAngle < 0.0f)
        {
            arrowAngle += 360.0f;
        }

        if (arrowAngle > 5.0f && arrowAngle < 85.0f)
        {
            return AutomaticGear.Driving;
        }
        else if (arrowAngle > 95.0f && arrowAngle < 175.0f)
        {
            return AutomaticGear.Reverse;
        }
        else if (arrowAngle > 185.0f && arrowAngle < 265.0f)
        {
            return AutomaticGear.Neutral;
        }
        else if (arrowAngle > 275.0f && arrowAngle < 355.0f)
        {
            return AutomaticGear.Parking;
        }

        return AutomaticGear.None;
    }
}
