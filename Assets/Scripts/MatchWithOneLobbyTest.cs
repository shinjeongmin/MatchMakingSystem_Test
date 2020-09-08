using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MatchWithOneLobbyTest : MonoBehaviourPunCallbacks
{
    #region Property

    string gameVersion = "1";

    // OnGUI string
    public string textFiledString;
    public string nowMatching = "";

    // 최대 티어점수
    public int MaxTier;

    // 플레이어의 티어
    public int playerTier;
    // 플레이어 고유 이름
    string playerName;

    // 방목록 리스트
    public List<RoomInfo> RoomList = new List<RoomInfo>();

    // 매치메이킹 코루틴
    IEnumerator Corout_MatchMaking;
    // 다른 룸 탐색 코루틴
    IEnumerator Corout_SearchDifferentRoom;

    #endregion

    private void Awake()
    {
        PhotonNetwork.AutomaticallySyncScene = true;
    }

    private void Start()
    {
        PhotonNetwork.ConnectUsingSettings();
        // 최대 티어 지정. (언젠가 Data로 빼야하는데..)
        MaxTier = 200;
    }

    private void Update()
    {
    }

    private void OnGUI()
    {
        #region Lobby, Room Test

        if (GUI.Button(new Rect(50, 50, 100, 50), "Non"))
        {

        }
        if (GUI.Button(new Rect(200, 50, 100, 50), "Create Room"))
        {
            CreatePlayerRoom();
        }
        if (GUI.Button(new Rect(350, 50, 100, 50), "Random Join"))
        {
            PhotonNetwork.JoinRandomRoom();
        }
        if (GUI.Button(new Rect(460, 50, 100, 50), "Leave Room"))
        {
            PhotonNetwork.LeaveRoom();
        }
        if (GUI.Button(new Rect(570, 50, 100, 50), "Leave Lobby"))
        {
            PhotonNetwork.LeaveLobby();
        }
        #endregion

        #region Room Info

        if(PhotonNetwork.InRoom)
        {
            GUI.Box(new Rect(50, 150, 150, 70), "Current Room Name : \n" + PhotonNetwork.CurrentRoom.Name);
            GUI.Box(new Rect(210, 150, 100, 50), "Room Players : \n" + PhotonNetwork.PlayerList.Length.ToString());
        }
        // 전체 룸 개수
        GUI.Box(new Rect(320, 150, 150, 50), "전체 Room 개수 : " + PhotonNetwork.CountOfRooms
            + "\nRoom에 있는 사람 수 : " + PhotonNetwork.CountOfPlayersInRooms);

        #endregion

        #region Input Tier, Name and MatchMakingButton

        // Input
        textFiledString = GUI.TextField(new Rect(50, 250, 100, 50), textFiledString);
        if (textFiledString != "")
            playerTier = (int.Parse(textFiledString));

        // Random Matching Button
        if (GUI.Button(new Rect(160, 250, 100, 50), "Matching Start"))
        {
            // 매치메이킹 코루틴 변수에 코루틴 대입 
            Corout_MatchMaking = MatchMakingSystem();
            StartCoroutine(Corout_MatchMaking);
        }

        // Set Name
        playerName = GUI.TextField(new Rect(270, 250, 100, 50), playerName);
        PhotonNetwork.LocalPlayer.NickName = playerName;

        // Matching Status
        GUI.Box(new Rect(380, 250, 100, 50), nowMatching);

        #endregion
    }

    /* Non Call-back methods */

    #region Matching Related Coroutine

    /// <summary>
    /// 매칭 시스템 코루틴
    /// </summary>
    public IEnumerator MatchMakingSystem()
    {
        Debug.Log("Now Match Making...");

        // default 로비에 들어감.
        PhotonNetwork.JoinLobby();

        yield return new WaitForSeconds(1f);

        Debug.Log("현재 룸의 개수 : " + RoomList.Count);

        // 룸이 존재하는지 확인
        if(RoomList.Count == 0)
        {
            // 룸이 아예 없다면 룸을 생성.
            CreatePlayerRoom();
            
            yield break;
        }
        // 룸이 있으면 룸 리스트 중에서 가장 가까운 티어와 매칭
        else
        {
            RoomInfo room = null;
            // 티어점수 한도 범위 안에서 플레이어 티어로부터 가까운 순서로 룸 탐색
            for(int i = 0; 0 <= playerTier - i || playerTier + i <= MaxTier ; i++)
            {
                // playerTier + i가 있을 경우
                if (RoomList.Exists(x => x.Name.Split('_')[1] == (playerTier + i).ToString("D4"))
                    && playerTier + i <= MaxTier)
                {
                    room = RoomList.Find(x => x.Name.Split('_')[1] == (playerTier + i).ToString("D4"));
                    break;
                }
                // playerTier - i가 있을 경우
                else if (RoomList.Exists(x => x.Name.Split('_')[1] == (playerTier - i).ToString("D4"))
                    && 0 <= playerTier - i)
                {
                    room = RoomList.Find(x => x.Name.Split('_')[1] == (playerTier - i).ToString("D4"));
                    break;
                }
            }

            if(room != null)
            {
                //Debug.LogError("룸에 입장 / 남은 룸의 수 : " + RoomList.Count);

                // 탐색한 룸에 입장
                PhotonNetwork.JoinRoom(room.Name);
            }
            // 룸을 탐색하지 못함
            else
            {
                Debug.LogError("룸 탐색 도중 오류");
            }
        }
    }

    /// <summary>
    /// 방에 들어왔을 때 일정 시간 상대가 들어오지 않으면 매칭 실패로 룸을 나가는 코루틴
    /// </summary>
    /// <returns></returns>
    public IEnumerator SearchDifferentRoom()
    {
        // 랜덤 시간 대기
        float durationMin = 3f, durationMax = 5f;
        yield return new WaitForSeconds(Random.Range(durationMin, durationMax));

        // 현재 방의 총 인원수가 1이면
        if (PhotonNetwork.PlayerList.Length == 1)
        {
            Debug.Log(string.Format("다른 룸 {0} 개 존재. 룸 탐색...", PhotonNetwork.CountOfRooms));
            // 현재 다른 룸이 있다면
            if (PhotonNetwork.CountOfRooms != 0)
            {
                Debug.LogError("매칭에 실패하였습니다. 다시 시도해주세요.");
            }
            else
            {
                Debug.LogError("현재 매칭할 플레이어가 없습니다. 잠시후 다시 시도해주세요.");
            }
            // 현재 룸 나가기
            PhotonNetwork.LeaveRoom();
        }
    }

    #endregion

    #region Methods

    public void CreatePlayerRoom()
    {
        PhotonNetwork.CreateRoom(
            string.Format("Room_{0}_{1}", playerTier.ToString("D4"), playerName),
            new RoomOptions { MaxPlayers = 2 });
    }

    #endregion

    #region Photon relation methods

    public override void OnConnectedToMaster()
    {
        Debug.Log("Connected Master Server");

    }

    public override void OnJoinedLobby()
    {
        // Default Lobby가 아니라면
        if (PhotonNetwork.CurrentLobby.Name != null)
            Debug.Log("로비에 참여하였습니다 : " + (int.Parse(PhotonNetwork.CurrentLobby.Name)));
        else
            // Default Lobby라면
            Debug.Log("로비에 참여하였습니다 : default");
    }


    public override void OnLeftLobby()
    {
        base.OnLeftLobby();

        Debug.Log("로비를 떠났습니다.");
    }

    public override void OnJoinedRoom()
    {
        base.OnJoinedRoom();

        // 매칭 대기 후 룸 떠나기
        Corout_SearchDifferentRoom = SearchDifferentRoom();
        StartCoroutine(Corout_SearchDifferentRoom);
    }

    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        base.OnJoinRandomFailed(returnCode, message);

        Debug.Log("JoinRandomRoom 실패!");

        // Room 생성 - Room 이름에는 플레이어별 고유 코드가 들어가도록 해야겠다.
        CreatePlayerRoom();
    }

    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        base.OnCreateRoomFailed(returnCode, message);

        Debug.Log("Create Room Failed");
    }

    public override void OnLeftRoom()
    {
        base.OnLeftRoom();

        Debug.Log("방을 떠났습니다.");
    }

    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        base.OnRoomListUpdate(roomList);

        int roomCount = roomList.Count;
        for (int i = 0; i < roomCount; i++)
        {
            if (!roomList[i].RemovedFromList)
            {
                if (!RoomList.Contains(roomList[i])) RoomList.Add(roomList[i]);
                else RoomList[RoomList.IndexOf(roomList[i])] = roomList[i];
            }
            else if (RoomList.IndexOf(roomList[i]) != -1) RoomList.RemoveAt(RoomList.IndexOf(roomList[i]));
        }
    }

    #endregion
}