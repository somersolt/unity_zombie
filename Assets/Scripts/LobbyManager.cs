using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using TMPro;
using UnityEngine.UI;
using JetBrains.Annotations;


public class LobbyManager : MonoBehaviourPunCallbacks
{
    private string gameVersion = "1.0";

    public TextMeshProUGUI message;
    public Button button;

    private void Start()
    {
      
        if (!PhotonNetwork.IsConnected)
        {
            PhotonNetwork.GameVersion = gameVersion;
            PhotonNetwork.ConnectUsingSettings();
            message.text = "Conneting to Master Server";
            button.interactable = false;
        }
        else
        {
             button.interactable = true;

        }
    }

    public override void OnConnectedToMaster()
    {

        message.text = "Online : Connected to Master Server";
        button.interactable = true;

    }

    public override void OnDisconnected(DisconnectCause cause)
    {
        button.interactable = false;

        message.text = " Offline : DisConnected to Master Server";
    }

        public void OnJoin()
        {
            if (PhotonNetwork.IsConnected)
            {
                PhotonNetwork.JoinRandomRoom();
                button.interactable = false;
                message.text = "Joining room ...";
            }

            else
            {
                PhotonNetwork.ConnectUsingSettings();
                message.text = "Conneting to Master Server";
            }
        }

    public override void OnJoinedRoom()
    {
        message.text = "Success : Joined Room";
        PhotonNetwork.LoadLevel("Main");
    }
    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        this.message.text = "Fail : Joined Room";
        PhotonNetwork.CreateRoom(null, new RoomOptions { MaxPlayers = 4 });
    }


}
