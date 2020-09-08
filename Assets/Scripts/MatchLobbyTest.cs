using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using UnityEngine;

public class MatchLobbyTest : MonoBehaviourPunCallbacks
{
    #region Property

    string gameVersion = "1";
   
    public string textFiledString;
    public int playerTier;

    // 플레이어 티어에 해당하는 로비
    public int playerTier_Lobby_Idx;
    
    // 현재 플레이어가 위치한 로비
    public int current_Lobby_Index;

    // 티어 로비표의 마지막 로비의 인덱스
    int Last_Lobby_Index;

    // 현재 룸의 이름 (형식 : Room_로비인덱스_방인덱스)
    string current_Room_Name = "";
    // 현재 룸의 플레이어 수 
    int current_Room_PlayerCnt = 0;

    // 매치메이킹 코루틴
    IEnumerator Corout_MatchMaking;
    // 로비 및 룸에 들어가는 코루틴
    IEnumerator Corout_EnterLobby_Join;

    // 플레이어 고유 이름
    string playerName;

    #endregion

    private void Awake()
    {
        PhotonNetwork.AutomaticallySyncScene = true;
    }

    private void Start()
    {
        PhotonNetwork.ConnectUsingSettings();

        // 마지막 로비의 인덱스 (0부터 시작한 로비의 총 갯수)
        Last_Lobby_Index = 5;
    }

    private void Update()
    {
    }

    private void OnGUI()
    {
        #region Lobby, Room Test

        if (GUI.Button(new Rect(50, 50, 100, 50), "Lobby 1"))
        {
            PhotonNetwork.JoinLobby(new TypedLobby("1", LobbyType.Default));
        }
        if(GUI.Button(new Rect(200, 50, 100, 50), "Lobby 2"))
        {
            PhotonNetwork.JoinLobby(new TypedLobby("2", LobbyType.Default));
        }
        if(GUI.Button(new Rect(350, 50, 100, 50), "Random Join"))
        {
            PhotonNetwork.JoinRandomRoom();
        }
        if(GUI.Button(new Rect(460, 50, 100, 50), "Leave Room"))
        {
            PhotonNetwork.LeaveRoom();
        }
        if(GUI.Button(new Rect(570, 50, 100, 50), "Leave Lobby"))
        {
            PhotonNetwork.LeaveLobby();
        }
        #endregion

        #region Lobby & Room Info

        GUI.Box(new Rect(50, 150, 150, 70), "Current Lobby Name: "+current_Lobby_Index.ToString()
            + "\n\nCurrent Room Name : \n" + current_Room_Name);
        GUI.Box(new Rect(210, 150, 100, 50), "Room Players : \n" + current_Room_PlayerCnt.ToString());

        if (PhotonNetwork.InRoom)
        {
            current_Room_PlayerCnt = PhotonNetwork.PlayerList.Length;
            current_Room_Name = PhotonNetwork.CurrentRoom.Name;
        }
        else
        {
            current_Room_PlayerCnt = 0;
            current_Room_Name = "";
        }
        if (PhotonNetwork.InLobby) 
        { 
            // Lobby 인덱스가 있는 경우 인덱스를, Default Lobby인 경우 -1을 로비 인덱스에 대입
            current_Lobby_Index = 
                (PhotonNetwork.CurrentLobby.Name == null) ? -1 : int.Parse(PhotonNetwork.CurrentLobby.Name);
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
            Corout_MatchMaking = MatchMakingSystem(1);
            StartCoroutine(Corout_MatchMaking);
        }

        // Set Name
        playerName = GUI.TextField(new Rect(270, 250, 100, 50), playerName);

        #endregion
    }

    /* Non Call-back methods */

    /// <summary>
    /// 매개변수로 전달받은 인덱스의 로비로 입장
    /// 해당 인덱스 로비의 플레이어끼리 랜덤매칭 
    /// (방이 있으면 JoinRandomRoom, 없으면 CreateRoom)
    /// </summary>
    /// <param name="Lobby_Index"></param>
    /// <returns></returns>
    public IEnumerator EnterLobby_Join(int Lobby_Index, bool isDefaultLobby = false)
    {
        // 인덱스만 주어짐
        if(isDefaultLobby.Equals(false))
        {
            Debug.Log("현재 로비 인덱스 : " + Lobby_Index);

            //current_Lobby_Index = Lobby_Index;

            // 현재 로비 인덱스에 해당하는 로비로 들어감.
            TypedLobby newLobby = new TypedLobby(Lobby_Index.ToString(), LobbyType.Default);
            PhotonNetwork.JoinLobby(newLobby);

            // 로비에 들어가는 동안의 텀
            yield return new WaitForSeconds(1f);

            // 로비 내 랜덤매칭
            PhotonNetwork.JoinRandomRoom();

            yield return new WaitForSeconds(2f);

            // 2초 대기 후에도 플레이어가 들어오지 않는다면 로비를 떠남.
            if (PhotonNetwork.PlayerList.Length < 2)
            {
                PhotonNetwork.LeaveRoom();
            }
            else
            {// 플레이어가 2명 입장하면 
                // 매칭 코루틴 중지
                StopCoroutine(Corout_MatchMaking);
            }
        }
        else
        {
            Debug.Log("아무나와 매칭합니다.");

            // default lobby로 들어감
            PhotonNetwork.JoinLobby();

            // 로비에 들어가는 동안의 텀
            yield return new WaitForSeconds(1f);

            // 로비 내 랜덤매칭
            PhotonNetwork.JoinRandomRoom();
        }
    }

    /// <summary>
    /// 매칭 시스템 코루틴
    /// </summary>
    /// <param name="i">현재 로비에서 상대를 못찾았을 때 
    /// 다른 티어 로비로 이동할 때 사용하는 증감수</param>
    /// <returns></returns>
    public IEnumerator MatchMakingSystem(int i)
    {
        // 플레이어의 티어에 따라 플레이어 로비 할당
        playerTier_Lobby_Idx = SeparateTierLobbbyIndex(playerTier);
        Debug.Log("Now Match Making...");

        // 플레이어의 티어에 해당하는 로비에 들어감
        Corout_EnterLobby_Join = EnterLobby_Join(playerTier_Lobby_Idx);
        StartCoroutine(Corout_EnterLobby_Join);
        yield return new WaitForSeconds(3.5f);

        while (true)
        {
            #region 탐색 방법 1 - (0,1,2,3,4) 에서 2일 때 2-1-3 - 2-1-3-4-0

            //for (int IdxAdd = 1; IdxAdd <= i; IdxAdd++)
            //{
            //    // player tier + IdxAdd <= Last Lobby index 일 경우
            //    if (playerTier_Lobby_Idx
            //        + IdxAdd <= Last_Lobby_Index)
            //    {
            //        // 플레이어의 티어 + IdxAdd 에 해당하는 로비에 들어감
            //        Corout_EnterLobby_Join = EnterLobby_Join(playerTier_Lobby_Idx
            //            + IdxAdd);
            //        StartCoroutine(Corout_EnterLobby_Join);
            //        yield return new WaitForSeconds(3.5f);

            //        // 플레이어의 티어에 해당하는 로비에 들어감
            //        Corout_EnterLobby_Join = EnterLobby_Join(playerTier_Lobby_Idx);
            //        StartCoroutine(Corout_EnterLobby_Join);
            //        yield return new WaitForSeconds(3.5f);
            //    }


            //    // player tier - IdxAdd >= 0 일 경우
            //    if (playerTier_Lobby_Idx
            //        - IdxAdd >= 0)
            //    {
            //        // 플레이어의 티어 - IdxAdd 에 해당하는 로비에 들어감
            //        Corout_EnterLobby_Join = EnterLobby_Join(playerTier_Lobby_Idx
            //            - IdxAdd);
            //        StartCoroutine(Corout_EnterLobby_Join);
            //        yield return new WaitForSeconds(3.5f);

            //        // 플레이어의 티어에 해당하는 로비에 들어감
            //        Corout_EnterLobby_Join = EnterLobby_Join(playerTier_Lobby_Idx);
            //        StartCoroutine(Corout_EnterLobby_Join);
            //        yield return new WaitForSeconds(3.5f);
            //    }
            //}
            //i++;

            //// i를 더하고 뺐을 때의 범위가 존재하지 않을 경우
            //if (playerTier_Lobby_Idx + i > Last_Lobby_Index &&
            //    playerTier_Lobby_Idx - i < 0)
            //{
            //    i = 1;
            //}
            #endregion

            #region 탐색 방법 2 - (0,1,2,3,4) 에서 2일 때 2-1-3-4-0

            //// player tier + IdxAdd <= Last Lobby index 일 경우
            //if (playerTier_Lobby_Idx
            //    + i <= Last_Lobby_Index)
            //{
            //    // 플레이어의 티어 + IdxAdd 에 해당하는 로비에 들어감
            //    Corout_EnterLobby_Join = EnterLobby_Join(playerTier_Lobby_Idx
            //        + i);
            //    StartCoroutine(Corout_EnterLobby_Join);
            //    yield return new WaitForSeconds(3.5f);

            //    // 플레이어의 티어에 해당하는 로비에 들어감
            //    Corout_EnterLobby_Join = EnterLobby_Join(playerTier_Lobby_Idx);
            //    StartCoroutine(Corout_EnterLobby_Join);
            //    yield return new WaitForSeconds(3.5f);
            //}


            //// player tier - IdxAdd >= 0 일 경우
            //if (playerTier_Lobby_Idx
            //    - i >= 0)
            //{
            //    // 플레이어의 티어 - IdxAdd 에 해당하는 로비에 들어감
            //    Corout_EnterLobby_Join = EnterLobby_Join(playerTier_Lobby_Idx
            //        - i);
            //    StartCoroutine(Corout_EnterLobby_Join);
            //    yield return new WaitForSeconds(3.5f);

            //    // 플레이어의 티어에 해당하는 로비에 들어감
            //    Corout_EnterLobby_Join = EnterLobby_Join(playerTier_Lobby_Idx);
            //    StartCoroutine(Corout_EnterLobby_Join);
            //    yield return new WaitForSeconds(3.5f);
            //}

            //i++;

            //// i를 더하고 뺐을 때의 범위가 존재하지 않을 경우
            //if (playerTier_Lobby_Idx + i > Last_Lobby_Index &&
            //    playerTier_Lobby_Idx - i < 0)
            //{
            //    i = 1;
            //}
            #endregion

            #region 탐색 방법 3 (플레이어의 티어) ⇒ 플레이어 티어 +-1 ⇒ default Lobby

            // player tier + IdxAdd <= Last Lobby index 일 경우
            if (playerTier_Lobby_Idx
                + i <= Last_Lobby_Index)
            {
                // 플레이어의 티어 + IdxAdd 에 해당하는 로비에 들어감
                Corout_EnterLobby_Join = EnterLobby_Join(playerTier_Lobby_Idx
                    + i);
                StartCoroutine(Corout_EnterLobby_Join);
                yield return new WaitForSeconds(3.5f);
            }


            // player tier - IdxAdd >= 0 일 경우
            if (playerTier_Lobby_Idx
                - i >= 0)
            {
                // 플레이어의 티어 - IdxAdd 에 해당하는 로비에 들어감
                Corout_EnterLobby_Join = EnterLobby_Join(playerTier_Lobby_Idx
                    - i);
                StartCoroutine(Corout_EnterLobby_Join);
                yield return new WaitForSeconds(3.5f);
            }

            // Deault Lobby로 이동해서 바로 상대 탐색
            Corout_EnterLobby_Join = EnterLobby_Join(-1, true);
            StartCoroutine(Corout_EnterLobby_Join);
            yield return new WaitForSeconds(3.5f);

            break;

            #endregion
        }
    }

    #region Utility Methods

    /// <summary>
    /// 플레이어의 티어에 따라 로비 인덱스를 분별
    /// </summary>
    /// <param name="Tier"></param>
    /// <returns></returns>
    public int SeparateTierLobbbyIndex(int Tier)
    {
        int LobbyIndex = 0;

        #region Tier Rank
        /* 
         * 티어 랭크는 데이터로 따로 만들것.
         * Tier Rank
        000 : 0 ~ 99 
        001 : 100 ~ 119
        002 : 120 ~ 139
        003 : 140 ~ 159
        004 : 160 ~ 179
        005 : 180 ~ 200
         */
        int Tier_0_Lower = 0, Tier_0_Upper = 99;
        int Tier_1_Lower = 100, Tier_1_Upper = 119;
        int Tier_2_Lower = 120, Tier_2_Upper = 139;
        int Tier_3_Lower = 140, Tier_3_Upper = 159;
        int Tier_4_Lower = 160, Tier_4_Upper = 179;
        int Tier_5_Lower = 180, Tier_5_Upper = 200;

        if (Tier_0_Lower <= Tier && Tier <= Tier_0_Upper) LobbyIndex = 0;
        else if (Tier_1_Lower <= Tier && Tier <= Tier_1_Upper) LobbyIndex = 1;
        else if (Tier_2_Lower <= Tier && Tier <= Tier_2_Upper) LobbyIndex = 2;
        else if (Tier_3_Lower <= Tier && Tier <= Tier_3_Upper) LobbyIndex = 3;
        else if (Tier_4_Lower <= Tier && Tier <= Tier_4_Upper) LobbyIndex = 4;
        else if (Tier_5_Lower <= Tier && Tier <= Tier_5_Upper) LobbyIndex = 5;
        else
        {
            Debug.LogError("Player's Tier is Out of Range");
        }
        #endregion

        return LobbyIndex;
    }

    #endregion

    #region Photon relation methods

    public override void OnConnectedToMaster()
    {
        Debug.Log("Connected Master Server");

    }

    public override void OnJoinedRoom()
    {
        base.OnJoinedRoom();
    }

    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        base.OnJoinRandomFailed(returnCode, message);

        Debug.Log("JoinRandomRoom 실패!");

        if (PhotonNetwork.CurrentLobby.Name != null)
        {
            // Room 생성 - (default lobby가 아닐 때 )Room 이름에는 플레이어별 고유 코드가 들어가도록 해야겠다.
            PhotonNetwork.CreateRoom("NewRoom_" + playerName
                + "_Tier" + current_Lobby_Index,
                new RoomOptions { MaxPlayers = 2 },
                new TypedLobby(current_Lobby_Index.ToString(), LobbyType.Default));
        }
        else
        {
            // Room 생성 - (default lobby일 때)
            PhotonNetwork.CreateRoom(null, new RoomOptions { MaxPlayers = 2 });
        }
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

    public override void OnJoinedLobby()
    {
        // Default Lobby가 아니라면
        if (PhotonNetwork.CurrentLobby.Name != null)
            Debug.Log("로비에 참여하였습니다 : " + (int.Parse(PhotonNetwork.CurrentLobby.Name)));
        else
            // Default Lobby라면
            Debug.Log("로비에 참여하였습니다 : default");

        current_Lobby_Index =
            (PhotonNetwork.CurrentLobby.Name == null) ? -1 : (int.Parse(PhotonNetwork.CurrentLobby.Name));
    }


    public override void OnLeftLobby()
    {
        base.OnLeftLobby();

        Debug.Log("로비를 떠났습니다.");
    }

    #endregion
}