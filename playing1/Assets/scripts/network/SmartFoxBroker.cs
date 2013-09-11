using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

using Sfs2X;
using Sfs2X.Core;
using Sfs2X.Entities;
using Sfs2X.Entities.Data;
using Sfs2X.Requests;
using Sfs2X.Logging;

using playing1.world;

namespace playing1.network
{
    using dbg = System.Diagnostics.Debug;

    // this class will broker messages between us and the server
    public sealed class SmartFoxBroker : MonoBehaviour
    {
        static public readonly string ExtName = "playing1";
        static public readonly string ExtClass = "kai.game.playing1.Playing1ServerExtension";

        readonly Dictionary<string, Action<ISFSObject>> _handlers = new Dictionary<string, Action<ISFSObject>>();

        World _world;
        void Start()
        {
            var sf = SmartFoxConnection.Connection;

            sf.AddEventListener(SFSEvent.EXTENSION_RESPONSE, OnExtensionResponse);
            sf.AddEventListener(SFSEvent.USER_EXIT_ROOM, OnUserLeaveRoom);
            sf.AddEventListener(SFSEvent.CONNECTION_LOST, OnConnectionLost);

            AddExtensionResponseHandlers();
            
            //KAI: weird stuff that needs a more thought-out home?
            _world = new World(GameObject.Find("_nonvisibleState").GetComponent<PrefabLookup>());
            SendSpawnRequest();
        }

        void FixedUpdate()
        {
            if (_world != null)
            {
                SmartFoxConnection.Connection.ProcessEvents();
            }
        }

        void SendSpawnRequest()
        {
            Room room = SmartFoxConnection.Connection.LastJoinedRoom;
            ExtensionRequest request = new ExtensionRequest("spawnMe", new SFSObject(), room);
            SmartFoxConnection.Connection.Send(request);
        }

        void OnExtensionResponse(BaseEvent evt)
        {
            try
            {
                string cmd = (string)evt.Params["cmd"];
                Action<ISFSObject> handler = null;

                if (_handlers.TryGetValue(cmd, out handler) && handler != null)
                {
                    handler((ISFSObject)evt.Params["params"]);
                }
                else
                {
                    Debug.LogWarning("Unhandled extension response: " + cmd);
                }
            }
            catch (Exception e)
            {
                Debug.Log("Exception handling response: " + e.Message + " >>> " + e.StackTrace);
            }
        }

        void OnUserLeaveRoom(BaseEvent evt)
        {
            User user = (User)evt.Params["user"];
            Room room = (Room)evt.Params["room"];

            //PlayerManager.Instance.DestroyEnemy(user.Id);
            Debug.Log(string.Format("User {0} left {1}", user.Name, room.Name));
        }
        void OnConnectionLost(BaseEvent evt)
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

        void AddExtensionResponseHandlers()
        {
            dbg.Assert(_handlers.Count == 0);

            _handlers.Add("spawn", OnSpawn);
            _handlers.Add("pos", OnPosition);
        }

        void OnSpawn(ISFSObject param)
        {
            _world.SpawnActor(param);
        }
        void OnPosition(ISFSObject param)
        {
            _world.MoveActor(param);
        }
    }
}
