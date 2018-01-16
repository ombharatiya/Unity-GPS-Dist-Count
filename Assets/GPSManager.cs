using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public enum LocationState {
    Disabled, TimedOut, Failed, Enabled
}
public class GPSManager : MonoBehaviour {

    public static int SCREEN_DENSITY;
    private GUIStyle debugStyle;

    const float EARTH_RADIUS = 6371;
    private LocationState state;

    private float latitude;
    private float longitude;

    private float distance;

    private int coins;



    IEnumerator Start() {
        if(Screen.dpi>0f) {
            SCREEN_DENSITY = (int)(Screen.dpi / 160f);
        } else {
            SCREEN_DENSITY = (int)(Screen.currentResolution.height / 600);
        }
        debugStyle = new GUIStyle();
        debugStyle.fontSize = 16 * SCREEN_DENSITY;
        debugStyle.normal.textColor = Color.white;

        state = LocationState.Disabled;
        latitude = 0f;
        longitude = 0f;
        distance = 0f;
        coins = 0;


        if (Input.location.isEnabledByUser)
        {
            Input.location.Start();

            int maxWait = 20;

            while (Input.location.status == LocationServiceStatus.Initializing && maxWait > 0) {
                yield return new WaitForSeconds(1);
                maxWait--;
            }
            if (maxWait <= 0) {
                state = LocationState.TimedOut;
            }
            else if (Input.location.status == LocationServiceStatus.Failed) {
                state = LocationState.Failed;
            }
            else {
                state = LocationState.Enabled;
                latitude = Input.location.lastData.latitude;
                longitude = Input.location.lastData.longitude;
            }
        }
    }

    IEnumerator OnApplicationPause(bool pauseState) {
        if(pauseState) {
            Input.location.Stop();
            state = LocationState.Disabled;
        } else {
            Input.location.Start();

            int maxWait = 20;

            while (Input.location.status == LocationServiceStatus.Initializing && maxWait > 0)
            {
                yield return new WaitForSeconds(1);
                maxWait--;
            }
            if (maxWait <= 0)
            {
                state = LocationState.TimedOut;
            }
            else if (Input.location.status == LocationServiceStatus.Failed)
            {
                state = LocationState.Failed;
            }
            else
            {
                state = LocationState.Enabled;
                latitude = Input.location.lastData.latitude;
                longitude = Input.location.lastData.longitude;
            }
        }

    }

    void OnGUI() {
        Rect guiBoxRect = new Rect(40, 20, Screen.width - 80, Screen.height - 40);

        GUI.skin.box.fontSize = 32 * SCREEN_DENSITY;
        GUI.Box(guiBoxRect, "GPS Demo");
        float buttonHeight = guiBoxRect.height / 7;
        switch (state)
        {
            case LocationState.Enabled:
                GUILayout.Label("latitude " + latitude.ToString(), debugStyle, GUILayout.Width(Screen.width));
                GUILayout.Label("longitude " + longitude.ToString(), debugStyle, GUILayout.Width(Screen.width));

                Rect distanceTextRect = new Rect(guiBoxRect.x + 40, guiBoxRect.y + guiBoxRect.height / 2, guiBoxRect.width - 80, buttonHeight);
                GUI.skin.label.fontSize = 40 * SCREEN_DENSITY;
                GUI.skin.label.alignment = TextAnchor.UpperCenter;
                GUI.Label(distanceTextRect, "Distance Walked: " + distance.ToString() + "m");

                Rect coinsTextRect = new Rect(guiBoxRect.x + 40, guiBoxRect.y + guiBoxRect.height * 2, guiBoxRect.width - 80, buttonHeight);
                GUI.skin.label.fontSize = 40 * SCREEN_DENSITY;
                GUI.skin.label.alignment = TextAnchor.UpperCenter;
                GUI.Label(coinsTextRect, "coins: " + coins.ToString() + "g");
                break;
            case LocationState.Disabled:
                Rect disabledTextRect = new Rect(guiBoxRect.x + 40, guiBoxRect.y + guiBoxRect.height /2, guiBoxRect.width - 80, buttonHeight*2);
                GUI.skin.label.fontSize = 40 * SCREEN_DENSITY;
                GUI.skin.label.alignment = TextAnchor.UpperCenter;
                GUI.Label(disabledTextRect, "GPS is disabled.GPS must be enabled\n " + "in order to use this application");
                break;
            case LocationState.Failed:
                Rect failedTextRect = new Rect(guiBoxRect.x + 40, guiBoxRect.y + guiBoxRect.height / 2, guiBoxRect.width - 80, buttonHeight*2);
                GUI.skin.label.fontSize = 40 * SCREEN_DENSITY;
                GUI.skin.label.alignment = TextAnchor.UpperCenter;
                GUI.Label(failedTextRect, "Failed to initialise location service .\n" + "Try again later");
                break;
            case LocationState.TimedOut:
                Rect timeOutTextRect = new Rect(guiBoxRect.x + 40, guiBoxRect.y + guiBoxRect.height / 2, guiBoxRect.width - 80, buttonHeight);
                GUI.skin.label.fontSize = 40 * SCREEN_DENSITY;
                GUI.skin.label.alignment = TextAnchor.UpperCenter;
                GUI.Label(timeOutTextRect, "Connection timed out .try again later.");
                break;
        }

    }
    float Haversine(ref float lastLatitude,ref float lastLongitude)
    {
        float newLatitude = Input.location.lastData.latitude;
        float newLongitude = Input.location.lastData.longitude;
        float deltaLatitude = (newLatitude - lastLatitude) * Mathf.Deg2Rad;
        float deltaLongitude = (newLongitude - lastLongitude) * Mathf.Deg2Rad;
        float a = Mathf.Pow(Mathf.Sin(deltaLatitude / 2), 2) + Mathf.Cos(lastLatitude * Mathf.Deg2Rad) * Mathf.Cos(newLatitude * Mathf.Deg2Rad) *
            Mathf.Pow(Mathf.Sin(deltaLongitude / 2), 2);
        lastLatitude = newLatitude;
        lastLongitude = newLongitude;
        float c = 2 * Mathf.Atan2(Mathf.Sqrt(a), Mathf.Sqrt(1 - a));
        return EARTH_RADIUS * c;
        
          
    }
    private void Update()
    {
        if (state == LocationState.Enabled)
        {
            float deltaDistance = Haversine(ref latitude, ref longitude) * 1000f;
            if (deltaDistance > 0f)
            {
                distance += deltaDistance;
                coins = (int)(distance / 100f);
            }
        }
    }
}
