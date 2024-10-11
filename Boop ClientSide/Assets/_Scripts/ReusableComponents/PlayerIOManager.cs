using UnityEngine;
using System.Collections.Generic;
using PlayerIOClient;
using System;
using System.Linq;

public class PlayerIOManager {
    #region Variables
    private Connection _connection;
    private Client _client;
    private bool _processing;

    private List<Message> _messages = new List<Message>();
    private Dictionary<string, Action<string[]>> _handledMessageTypes = new Dictionary<string, Action<string[]>>();
    #endregion


    public void Init(string gameId, string userId, Action onSuccess) {
        Application.runInBackground = true;

        PlayerIO.Authenticate(
            gameId,                                     //Game ID         
            "public",                                   //Connection ID
            new Dictionary<string, string> {            //Auth arguments
				{ "userId", userId },
            },
            null,                                   //PlayerInsight segments
            (Client client) => {
                _client = client;
                onSuccess?.Invoke();
                AuthenticateSuccess();
            },
            (PlayerIOError error) => {
                Utils.LogError(this, "Init", error.Message);
            }
        );
    }

    private void AuthenticateSuccess() {
        Utils.Log(this, "AuthenticateSuccess");

        if (!CheckClient())
            return;

        if (GlobalManager.Instance.useLocalPlayerIO) {
            Utils.Log(this, "AuthenticateSuccess", "Create serverEndpoint");
            _client.Multiplayer.DevelopmentServer = new ServerEndpoint("localhost", 8184);
        }
    }

    //Room
    public void CreateRoom(Action<string> onSuccess) {
        Utils.Log(this, "CreateRoom");

        if (!CheckClient())
            return;

        _client.Multiplayer.CreateRoom(
            null,
            "Lobby",
            true,
            null,
            (string roomId) => {
                JoinRoom(roomId, null, null);
                onSuccess?.Invoke(roomId);
            },
            (PlayerIOError error) => {
                Utils.LogError(this, "CreateRoom", error.Message);
            }
        );
    }

    public void JoinRoom(string roomId, Action onSuccess, Action onError) {
        Utils.Log(this, "JoinRoom");

        if (!CheckClient())
            return;

        _client.Multiplayer.JoinRoom(
            roomId,                             //Room id. If set to null a random roomid is used
            new Dictionary<string, string> {
                { "gameVersion", $"{Application.version}" }
            },
            (Connection connection) => {
                _connection = connection;
                _connection.OnMessage += ReceiveMessage;
                onSuccess?.Invoke();
            },
            (PlayerIOError error) => {
                Utils.LogError(this, "JoinRoom", error.Message);
                onError?.Invoke();
            }
        );
    }

    public void LeaveRoom() {
        Utils.Log(this, "LeaveRoom");

        if (_connection == null)
            return;

        _connection.Disconnect();
    }


    //Messages
    private void ReceiveMessage(object sender, Message m) {
        CommonUtils.LogMessage(m);

        _messages.Add(m);

        if (_processing)
            return;

        ProcessMessages();
    }

    public void SendMessage(string type, params object[] parameters) {
        if (!CheckConnection())
            return;

        Message m = Message.Create(type, parameters);
        _connection.Send(m);
    }

    public void ProcessMessages() {
        _processing = true;

        while (_messages.Count > 0) {
            Message m = _messages.First();

            if (_handledMessageTypes.ContainsKey(m.Type))
                _handledMessageTypes[m.Type]?.Invoke(CommonUtils.GetMessageParams(m));

            _messages.Remove(m);
        }

        _processing = false;
    }

    public void HandleMessage(string id, Action<string[]> action) {
        if (_handledMessageTypes.ContainsKey(id))
            _handledMessageTypes[id] += action;
        else
            _handledMessageTypes.Add(id, action);
    }


    //Null checks
    private bool CheckClient() {
        if (_client == null) {
            Utils.LogError(this, "CheckClient", "_client is null");
            return false;
        }

        return true;
    }

    private bool CheckConnection() {
        if (_connection == null) {
            Utils.LogError(this, "CheckClient", "_connection is null");
            return false;
        }

        return true;
    }
}
