using GooglePlayGames;
using GooglePlayGames.BasicApi;
using GooglePlayGames.BasicApi.SavedGame;
using Realms;
using System;
using System.Text;
using TMPro;
using UnityEngine;

public class SaveManager : MonoBehaviour
{
    // UI
    public TMP_Text goldText;
    public TMP_Text heartText;
    public TMP_Text statusText;

    // PlayGames
    private enum SaveGameOpenReason { Load, Save }
    private SaveGameOpenReason saveGameOpenReason = SaveGameOpenReason.Load;
    private PlayGamesPlatform _platform;
    private bool Authenticated => _platform.localUser.authenticated;


    // Realm
    private Realm _realm;
    public Realm realm => _realm ??= Realm.GetInstance();

    public PlayerData playerData;


    private void Start()
    {
        InitializePlatform();
        SetStatusText("Not Authenticated", Color.yellow);
    }
    private void OnDestroy() => _realm?.Dispose();

    public void InitializePlatform()
    {
        if (_platform == null)
        {
            var config = new PlayGamesClientConfiguration
                .Builder()
                .EnableSavedGames()
                .Build();

            PlayGamesPlatform.InitializeInstance(config);
            PlayGamesPlatform.DebugLogEnabled = true;

            _platform = PlayGamesPlatform.Activate();
        }
    }


    // API Calls + Callbacks
    private void OpenSaveGame(SaveGameOpenReason reason)
    {
        if (Authenticated)
        {
            saveGameOpenReason = reason;
            _platform.SavedGame.OpenWithAutomaticConflictResolution(
                "SaveGame",
                DataSource.ReadCacheOrNetwork,
                ConflictResolutionStrategy.UseLongestPlaytime,
                OnSaveGameOpened
                );
        }
        else
        {
            SetStatusText("Not Authenticated, Loading Local Save Data", Color.red);

            playerData = realm.Find<PlayerData>(PlayerData.DefaultId);
            if (playerData == null)
            {
                realm.Write(() => playerData = realm.Add(new PlayerData(PlayerData.DefaultId)));
            }
            SetTexts();
        }
    }
    private void OnSaveGameOpened(SavedGameRequestStatus status, ISavedGameMetadata metaData)
    {
        if (status == SavedGameRequestStatus.Success)
        {
            SetStatusText("Save Game Opened Successfully", Color.green);

            switch (saveGameOpenReason)
            {
                case SaveGameOpenReason.Save: SavePlayerData(metaData); break;
                case SaveGameOpenReason.Load: LoadPlayerData(metaData); break;
                default: break;
            }
        }
        else
        {
            SetStatusText("Save Game Open Failed", Color.red);
        }
    }

    private void SavePlayerData(ISavedGameMetadata metaData)
    {
        byte[] dataToSave = Encoding.UTF8.GetBytes(playerData.ToString());
        var update = new SavedGameMetadataUpdate
            .Builder()
            .WithUpdatedDescription($"Saved At {DateTime.Now}")
            .Build();

        _platform.SavedGame.CommitUpdate(metaData, update, dataToSave, OnCommitUpdate);
    }
    private void OnCommitUpdate(SavedGameRequestStatus status, ISavedGameMetadata metaData)
    {
        switch (status)
        {
            case SavedGameRequestStatus.Success: SetStatusText("Successfully Saved Data", Color.green); break;
            default: SetStatusText($"Failed To Save Data. Status: {status}", Color.red); break;
        }
    }

    private void LoadPlayerData(ISavedGameMetadata metaData) => _platform.SavedGame.ReadBinaryData(metaData, OnLoadDataComplete);
    private void OnLoadDataComplete(SavedGameRequestStatus status, byte[] dataBytes)
    {
        switch (status)
        {
            case SavedGameRequestStatus.Success: SetStatusText("Successfully Loaded Data", Color.green); break;
            default: SetStatusText($"Failed To Load Data. Status: {status}", Color.red); return;
        }

        string userId = _platform.localUser.id;
        playerData = realm.Find<PlayerData>(userId);

        if (playerData == null)
        {
            PlayerData data;
            if (dataBytes == null || dataBytes.Length == 0)
            {
                data = new PlayerData(userId);
            }
            else
            {
                string dataString = Encoding.UTF8.GetString(dataBytes);
                data = new PlayerData(userId, dataString);
            }
            realm.Write(() => playerData = realm.Add(data));
        }

        SetTexts();
    }
    private void AuthenticateToPlayGames()
    {
        SetStatusText("Authenticating...", Color.yellow);
        _platform.Authenticate(SignInInteractivity.CanPromptAlways, OnAuthenticationComplete);
    }
    private void OnAuthenticationComplete(SignInStatus status)
    {
        if (status == SignInStatus.Success)
            SetStatusText($"Successfully Authenticated! ", Color.green);
        else
            SetStatusText($"Failed To Authenticate! {status}", Color.red);
    }
    private void SignOutFromPlayGames()
    {
        _platform.SignOut();
        SetStatusText("Signed Out", Color.yellow);
    }

    // Invoked from outside
    public void SignIn() => AuthenticateToPlayGames();
    public void SignOut() => SignOutFromPlayGames();
    public void SaveData() => OpenSaveGame(SaveGameOpenReason.Save);
    public void LoadData() => OpenSaveGame(SaveGameOpenReason.Load);
    public void AddGold()
    {
        realm.Write(() => playerData.Gold++);
        SetGoldText(playerData.Gold.ToString());
    }
    public void AddHearts()
    {
        realm.Write(() => playerData.Hearts++);
        SetHeartText(playerData.Hearts.ToString());
    }

    // UI
    private void SetTexts()
    {
        if (playerData == null) return;
        SetGoldText(playerData.Gold.ToString());
        SetHeartText(playerData.Hearts.ToString());
    }
    private void SetGoldText(string text) => goldText.text = text;
    private void SetHeartText(string text) => heartText.text = text;
    private void SetStatusText(string text, Color color)
    {
        statusText.text = text;
        statusText.color = color;
    }
}

// Helpful YouTube Videos
// https://www.youtube.com/watch?v=0AtXxdvdKcQ&t=1720s
// https://www.youtube.com/watch?v=joY-RQguwI4
// https://www.youtube.com/watch?v=zlCwDjFvDkE&t=709s