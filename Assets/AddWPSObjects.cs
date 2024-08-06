using UnityEngine;
using Niantic.Experimental.Lightship.AR.WorldPositioning;
using System;
using System.Collections.Generic;

public class AddWPSObjects : MonoBehaviour
{
    [SerializeField] ARWorldPositioningObjectHelper positioningHelper;
    [SerializeField] Camera trackingCamera;
    [SerializeField] UnityEngine.UI.Text dbgText;
    [SerializeField] GameObject obj1;
    [SerializeField] GameObject ButtonStartGPS;
    [SerializeField] GameObject ButtonStopGPS;

    // replace the coordinates here with your location
    public List<float> lastKnownCoordinatesX; // Latitude
    public List<float> lastKnownCoordinatesY; // Longitude
    double[,] arrayLocations = { 
        { -16.678107640860347, -49.2405682896889  ,  0.0}, //Rua do bloco H - EMC
        { -16.709285789878162, -49.259750950023985,  0.0}, //Esquina do areião, próximo à PF. 
        { -16.709681185835677, -49.25481384461595 ,  0.0}, //Hugo
        { -16.71044798286731 , -49.227500582309716, 20.0},//Palácio da música 
        { -16.710579668811036, -49.22846871554045 , 20.0},//Monumento das direitos humanos
        { -16.711154803217013, -49.22845067224935 , 20.0},// museu de arte contemporânea
        { -16.711344011787887, -49.22775075346965 , 20.0},// Biblioteca
    };

    double longitude;
    double latitude;
    double altitude; // We're using camera-relative positioning so make the obj1 appear at the same height as the camera

    // Start is called before the first frame update
    void Start()
    {
        latitude = arrayLocations[1, 0];
        longitude = arrayLocations[1, 1];
        altitude = arrayLocations[1, 2];
        // instantiate a obj1, scale it up for visibility (make it even bigger if you need), then update its location
        obj1.transform.localScale *= 2.0f;
        positioningHelper.AddOrUpdateObject(obj1, latitude, longitude, altitude, Quaternion.identity);

        InvokeRepeating("GetCurrentPos", 1, 1); // Get position each seconds
        ButtonStartGPS.SetActive(false);
        ButtonStopGPS.SetActive(true);
    }

    void Update()
    {

        if (Input.location.isEnabledByUser)
        {
            double deviceLatitude = Input.location.lastData.latitude;
            double deviceLongitude = Input.location.lastData.longitude;
            dbgText.text = "Pos = " + deviceLatitude + "," + deviceLongitude;

            Vector2 eastNorthOffsetMetres = EastNorthOffset(latitude, longitude, deviceLatitude, deviceLongitude);
            Vector3 trackingOffsetMetres = Quaternion.Euler(0, 0, Input.compass.trueHeading) * new Vector3(eastNorthOffsetMetres[0], (float)altitude, eastNorthOffsetMetres[1]);
            Vector3 trackingMetres = trackingCamera.transform.localPosition + trackingOffsetMetres;
        }
    }

    public Vector2 EastNorthOffset(double latitudeDegreesA, double longitudeDegreesA, double latitudeDegreesB, double longitudeDegreesB)
    {
        double DEGREES_TO_METRES = 111139.0;
        float lonDifferenceMetres = (float)(Math.Cos((latitudeDegreesA + latitudeDegreesB) * 0.5 * Math.PI / 180.0) * (longitudeDegreesA - longitudeDegreesB) * DEGREES_TO_METRES);
        float latDifferenceMetres = (float)((latitudeDegreesA - latitudeDegreesB) * DEGREES_TO_METRES);
        return new Vector2(lonDifferenceMetres, latDifferenceMetres);
    }

    public float GetAverageX() // Calculate average latitude
    {
        float totalX = 0;
        int cpt = 0;
        foreach (float x in lastKnownCoordinatesX)
        {
            totalX += x;
            cpt++;
        }
        return totalX / cpt;
    }

    public float GetAverageY() // Calculate average longitude
    {
        float totalY = 0;
        int cpt = 0;
        foreach (float y in lastKnownCoordinatesY)
        {
            totalY += y;
            cpt++;
        }
        return totalY / cpt;
    }

    void GetCurrentPos() // Get position and add to lists
    {
        // Latitude
        if (lastKnownCoordinatesX.Count < 10)
        {
            lastKnownCoordinatesX.Add(Input.location.lastData.latitude);
        }
        else // If list contains 10 items
        {
            lastKnownCoordinatesX.RemoveAt(0);
            lastKnownCoordinatesX.Add(Input.location.lastData.latitude);
        }
        // Longitude
        if (lastKnownCoordinatesY.Count < 10)
        {
            lastKnownCoordinatesY.Add(Input.location.lastData.longitude);
            positioningHelper.AddOrUpdateObject(obj1, GetAverageX(), GetAverageY(), altitude, Quaternion.identity);
        }
        else
        {
            lastKnownCoordinatesY.RemoveAt(0);
            lastKnownCoordinatesY.Add(Input.location.lastData.longitude);
        }
    }

    public void UpdateObjectLocation()
    {
        positioningHelper.AddOrUpdateObject(obj1, GetAverageX(), GetAverageY(), altitude, Quaternion.identity);
    }

    public void StopGPSUpdate()
    {
        CancelInvoke("GetCurrentPos");
        ButtonStartGPS.SetActive(true);
        ButtonStopGPS.SetActive(false);
    }

    public void StartGPSUpdate()
    {
        InvokeRepeating("GetCurrentPos", 1, 1); // Get position each seconds
        ButtonStartGPS.SetActive(false);
        ButtonStopGPS.SetActive(true);
    }

}