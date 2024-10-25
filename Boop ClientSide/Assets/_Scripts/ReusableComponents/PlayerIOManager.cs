using UnityEngine;
using System.Collections.Generic;
using PlayerIOClient;
using System;
using System.Linq;

public class HandledMessage {
    public Action<string[]> onMessage;
    public int infosLength;

    public HandledMessage(int infosLength) {
        this.infosLength = infosLength;
    }
}

public class PlayerIOManager {
    #region Variables
    private Connection _connection;
    private Client _client;

    private bool _processing;
    private List<Message> _messages = new List<Message>();
    private Dictionary<string, HandledMessage> _handledMessageTypes = new Dictionary<string, HandledMessage>();
    #endregion


    public void Init(string gameID, string userID, Action onSuccess) {
        if (string.IsNullOrEmpty(gameID)) {
            CommonUtils.ErrorOnParams("PlayerIOManager", "Init");
            return;
        }

        Application.runInBackground = true;

        PlayerIO.Authenticate(
            gameID,                                     //Game ID         
            "public",                                   //Connection ID
            new Dictionary<string, string> {            //Auth arguments
				{ "userId", userID },
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
            "Standard",
            true,
            new Dictionary<string, string> {
                { GlobalManager.Instance.CommonConst.numberOfPlayerKey, $"{GlobalManager.Instance.numberOfPlayer}" },
                { GlobalManager.Instance.CommonConst.gameVersionKey, $"{Application.version}" }
            },
            (string roomID) => {
                JoinRoom(roomID, null, null);
                onSuccess?.Invoke(roomID);
            },
            (PlayerIOError error) => {
                Utils.LogError(this, "CreateRoom", error.Message);
            }
        );
    }

    public void JoinRoom(string roomID, Action onSuccess, Action onError) {
        if (string.IsNullOrEmpty(roomID)) {
            CommonUtils.ErrorOnParams("PlayerIOManager", "Init");
            onError?.Invoke();
            return;
        }

        Utils.Log(this, "JoinRoom");

        if (!CheckClient())
            return;

        _client.Multiplayer.JoinRoom(
            roomID,                             //Room id. If set to null a random roomid is used
            new Dictionary<string, string> {
                { GlobalManager.Instance.CommonConst.gameVersionKey, $"{Application.version}" },
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
        if (sender == null || m == null) {
            CommonUtils.ErrorOnParams("PlayerIOManager", "ReceiveMessage");
            return;
        }

        CommonUtils.LogMessage(m);

        _messages.Add(m);

        if (_processing)
            return;

        ProcessMessages();
    }

    public void SendMessage(string type, params object[] parameters) {
        if (string.IsNullOrEmpty(type)) {
            CommonUtils.ErrorOnParams("PlayerIOManager", "SendMessage");
            return;
        }

        if (!CheckConnection())
            return;

        Message m = Message.Create(type, parameters);
        _connection.Send(m);
    }

    public void ProcessMessages() {
        _processing = true;

        while (_messages.Count > 0) {
            Message m = _messages.First();

            if (_handledMessageTypes.ContainsKey(m.Type)) {
                string[] infos = CommonUtils.GetMessageParams(m);
                if (infos == null || infos.Length < _handledMessageTypes[m.Type].infosLength) {
                    CommonUtils.LogMessage(m);
                    Utils.LogError(this, "ProcessMessages", "infos are wrong");
                }
                else
                    _handledMessageTypes[m.Type].onMessage?.Invoke(infos);
            }
            else {
                Utils.LogError(this, "ProcessMessages", $"message of type {m.Type} is not handled");
            }

            _messages.Remove(m);
        }

        _processing = false;
    }

    public void HandleMessage(string id, Action<string[]> action, int infosLength = 0) {
        if (string.IsNullOrEmpty(id) || action == null) {
            CommonUtils.ErrorOnParams("PlayerIOManager", "HandleMessage");
            return;
        }

        HandledMessage m = null;

        if (_handledMessageTypes.ContainsKey(id)) {
            m = _handledMessageTypes[id];
        }
        else {
            m = new HandledMessage(infosLength);
            _handledMessageTypes.Add(id, m);
        }

        m.onMessage += action;
    }

    public void UnhandleMessage(string id, Action<string[]> action) {
        if (string.IsNullOrEmpty(id) || action == null) {
            CommonUtils.ErrorOnParams("PlayerIOManager", "RemoveMessage");
            return;
        }

        if (_handledMessageTypes.ContainsKey(id))
            _handledMessageTypes[id].onMessage -= action;
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
            Utils.LogError(this, "CheckConnection", "_connection is null");
            return false;
        }

        return true;
    }
}
