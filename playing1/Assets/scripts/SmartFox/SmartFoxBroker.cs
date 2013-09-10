using UnityEngine;
using System;
using System.Collections;

using Sfs2X;
using Sfs2X.Core;
using Sfs2X.Entities;
using Sfs2X.Entities.Data;
using Sfs2X.Requests;
using Sfs2X.Logging;

// this class will broker messages between us and the server
public sealed class SmartFoxBroker : MonoBehaviour
{
    static public readonly string ExtName = "playing1";
    static public readonly string ExtClass = "kai.game.playing1.Playing1ServerExtension";

    bool _started = false;
    void Start()
    {
        var sf = SmartFoxConnection.Connection;

        sf.AddEventListener(SFSEvent.EXTENSION_RESPONSE, OnExtensionResponse);
        sf.AddEventListener(SFSEvent.USER_EXIT_ROOM, OnUserLeaveRoom);
        sf.AddEventListener(SFSEvent.CONNECTION_LOST, OnConnectionLost);

        _started = true; // KAI: the sample code does something like this, not sure we need to
    }

    void FixedUpdate()
    {
        if (_started)
        {
            SmartFoxConnection.Connection.ProcessEvents();
        }
    }

    private void OnExtensionResponse(BaseEvent evt)
    {
        try
        {
            string cmd = (string)evt.Params["cmd"];
            ISFSObject dt = (SFSObject)evt.Params["params"];

            Debug.Log("Extension response " + cmd + ", " + dt.ToString());
        }
        catch (Exception e)
        {
            Debug.Log("Exception handling response: " + e.Message + " >>> " + e.StackTrace);
        }
    }

    private void OnUserLeaveRoom(BaseEvent evt)
    {
        User user = (User)evt.Params["user"];
        Room room = (Room)evt.Params["room"];

        //PlayerManager.Instance.DestroyEnemy(user.Id);
        Debug.Log("User " + user.Name + " left");
    }
    private void OnConnectionLost(BaseEvent evt)
    {
        SmartFoxConnection.Connection.RemoveAllEventListeners();
        
        //Screen.lockCursor = false;
        //Screen.showCursor = true;
        //Application.LoadLevel("lobby");
    }

    void OnApplicationQuit()
    {
        SmartFoxConnection.Connection.RemoveAllEventListeners();
    }

}
