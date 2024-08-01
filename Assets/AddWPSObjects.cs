using UnityEngine;
using Niantic.Experimental.Lightship.AR.WorldPositioning;
using System;
using System.Collections.Generic;

public class AddWPSObjects : MonoBehaviour
{
    [SerializeField] ARWorldPositioningObjectHelper positioningHelper;
    [SerializeField] Camera trackingCamera;
    [SerializeField] UnityEngine.UI.Text dbgText;
    [SerializeField] GameObject cube;
    [SerializeField] GameObject gpsCube;
    [SerializeField] GameObject ButtonStartGPS;
    [SerializeField] GameObject ButtonStopGPS;

    // replace the coordinates here with your location
    public List<float> lastKnownCoordinatesX; // Latitude
    public List<float> lastKnownCoordinatesY; // Longitude
    double latitude = -16.6716403;
    double longitude = -49.2526512;
    double altitude = 0.0; // We're using camera-relative positioning so make the cube appear at the same height as the camera

    // Start is called before the first frame update
    void Start()
    {
        // instantiate a cube, scale it up for visibility (make it even bigger if you need), then update its location
        cube.transform.localScale *= 2.0f;
        positioningHelper.AddOrUpdateObject(cube, latitude, longitude, altitude, Quaternion.identity);

        InvokeRepeating("GetCurrentPos", 1, 1); // Get position each seconds
        ButtonStartGPS.SetActive(false);
        ButtonStopGPS.SetActive(true);
    }

    // Create a second cube and move it to the position predicted using the raw GPS + compass
    //private GameObject gpsCube = null;
    void Update()
    {
        // Create a second cube if we don't already have one:
        //if (gpsCube == null)
        //{
        //gpsCube = GameObject.CreatePrimitive(PrimitiveType.Cube);
        //gpsCube.GetComponent<Renderer>().material.color = Color.red;
        //}

        if (Input.location.isEnabledByUser)
        {
            double deviceLatitude = Input.location.lastData.latitude;
            double deviceLongitude = Input.location.lastData.longitude;
            dbgText.text = "Pos = " + deviceLatitude + "," + deviceLongitude;

            Vector2 eastNorthOffsetMetres = EastNorthOffset(latitude, longitude, deviceLatitude, deviceLongitude);
            Vector3 trackingOffsetMetres = Quaternion.Euler(0, 0, Input.compass.trueHeading) * new Vector3(eastNorthOffsetMetres[0], (float)altitude, eastNorthOffsetMetres[1]);
            Vector3 trackingMetres = trackingCamera.transform.localPosition + trackingOffsetMetres;
            gpsCube.transform.localPosition = trackingMetres;
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
            positioningHelper.AddOrUpdateObject(cube, GetAverageX(), GetAverageY(), altitude, Quaternion.identity);
        }
        else
        {
            lastKnownCoordinatesY.RemoveAt(0);
            lastKnownCoordinatesY.Add(Input.location.lastData.longitude);
        }
    }

    public void UpdateObjectLocation()
    {
        positioningHelper.AddOrUpdateObject(cube, GetAverageX(), GetAverageY(), altitude, Quaternion.identity);
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