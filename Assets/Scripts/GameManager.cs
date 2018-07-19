using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using Microsoft.Mixer;
using System.IO;

public class GameManager : MonoBehaviour
{
    #region member Variables

    #region prompt variables
    public GameObject promptPanel;
    public GameObject imagePrompt;
    public GameObject textPrompt;

    public List<string> textPrompts;
    public List<Sprite> imagePrompts;
    #endregion

    #region Voting Variables
    public GameObject votingPanel;
    public Text votingText;
    #endregion

    #region CountDown Variables
    public Text countdownText;
    public int countDownTimer;
    #endregion

    #region Voting Variables
    public GameObject votingResultsPanel;
    public GameObject topScoreText;
    public GameObject topScoreOverallText;

    public List<MixerButton> playersWhoResponded;
    [SerializeField]
    public List<MixerScene> mixerScenes;
    public MixerSceneHolder mixerSceneHolder;
    #endregion

    #region Logic Variables
    [SerializeField]
    public List<Player> players;
    public UIState currentState;
    int buttonsPerRow = 8;
    int buttonsPerColumn = 2;
    int buttonWidth = 10;
    int buttonHeight = 8;
    bool isFirstRun = true;
    #endregion

    #region Mixer Listeners
    bool hasInput = false;
    bool hasScenes = false;
    bool hasGroups = false;
    int run = 0;
    #endregion

    #endregion

    #region Enums
    public enum PromptType
    {
        text, image
    }

    public enum UIState
    {
        prompt, voting, @default
    }

    public enum MethodType
    {
        deleteGroup, deleteScene, createControls, deleteControls, createScenes, onSceneDelete, createGroups
    }
    #endregion

    #region GameLogic
    void Start()
    {
        MixerInteractive.GoInteractive();

        MixerInteractive.OnInteractivityStateChanged += CheckMixerInitialized;
    }

    void CheckMixerInitialized(object sender, InteractivityStateChangedEventArgs e)
    {
        InteractivityState interactiveState = e.State;
        if(interactiveState == InteractivityState.Initialized)
        {
            MixerInteractive.OnInteractiveTextControlEvent += CheckTextEvents;
            MixerInteractive.OnInteractiveButtonEvent += CheckButtonPresses;
            MixerInteractive.OnInteractiveMessageEvent += ListenForMessages;

            players = new List<Player>();

            GetPrompts();

            InvokeRepeating("CheckPlayers", 1, 0.5f);

            StartCoroutine(Run());
        }
    }

    private void OnApplicationQuit()
    {
        MixerInteractive.StopInteractive();
        MixerInteractive.Dispose();
    }

    //TODO: reset text after use (so that the second time around the input isn't there
    void CheckTextEvents(object sender, InteractiveTextEventArgs e)
    {
        if(e.ControlID == "textbox" + run)
        {
            Player player = GetPlayerByName(e.Participant.UserName);
            player.answer = e.Text;
            SetParticipantGroup(e.Participant, "default");
        }
    }

    //TODO: get control custom property of next/previous scene
    void CheckButtonPresses(object sender, InteractiveButtonEventArgs e)
    {
        if(currentState == UIState.voting)
        {
            Debug.Log("button pressed");
            string controlID = e.ControlID;
            
            if (controlID == ">>")
            {
                //string nextScene = GetNextSceneForControlID(controlID);
                //SetParticipantGroup(e.Participant, nextScene);
            }
            else if (controlID == "<<")
            {
                //string prevScene = GetNextSceneForControlID(controlID);
                //SetParticipantGroup(e.Participant, prevScene);
            }
            else
            {
                if (e.IsPressed)
                {
                    Player playerToAddScore = GetPlayerForControlID(controlID);
                    playerToAddScore.currentScore++;
                    playerToAddScore.overallScore++;
                    SetParticipantGroup(e.Participant, "default");
                }
            }
        }
    }

    IEnumerator Run()
    {
        yield return new WaitForSeconds(1);

        while (true)
        {
            PromptPhase();
            StartCoroutine(CountdownTimer(countDownTimer));
            yield return new WaitForSeconds(countDownTimer);

            VotingPhase();
            StartCoroutine(CountdownTimer(countDownTimer));
            yield return new WaitForSeconds(countDownTimer);
            
            ScoringPhase();
            StartCoroutine(CountdownTimer(countDownTimer));
            yield return new WaitForSeconds(countDownTimer);
            run++;
        }
    }
    #endregion

    #region Message Listening
    void ListenForMessages(object send, InteractiveMessageEventArgs e)
    {
        string message = e.Message;
        if (message.Contains("\"method\":\"onControlCreate\"") && message.Contains("\"sceneID\":\"prompt\""))
        {
            hasInput = true;
        }

        if (message.Contains("onSceneCreate"))
        {
            hasScenes = true;
            //string message1 = e.Message;
        }

        if (message.Contains("onGroupCreate"))
        {
            hasGroups = true;
        }
    }
    #endregion

    #region PromptPhase
    void PromptPhase(){
        Clean();
        ChangeState(UIState.prompt);
        SetupPrompt();
        CreateInput();
    }

    void Clean()
    {
        StartCoroutine(DeleteInputMessage());
        CleanScoresAndAnswers();
        CleanGroupsAndScenes();
    }
    IEnumerator DeleteInputMessage()
    {
        if (!isFirstRun)
        {
            yield return new WaitUntil(() => hasInput == true);
            DeleteControlsParams parameters = new DeleteControlsParams("prompt", new string[] { "textbox" + (run-1) });
            JSONDeleteControls message = new JSONDeleteControls(MethodType.deleteControls, parameters);
            SendDeleteControlsMessage(message);

            hasInput = false;
        }
        else
        {
            yield return null;
            isFirstRun = false;
        }
        
    }

    void CleanGroupsAndScenes()
    {
        IList<InteractiveGroup> groups = MixerInteractive.Groups;
        foreach (InteractiveGroup group in groups)
        {
            if (group.GroupID != "default" && group.GroupID != "prompt")
            {
                DeleteGroupsParams parameters = new DeleteGroupsParams(group.GroupID, "default");
                JSONDeleteGroup message = new JSONDeleteGroup(MethodType.deleteGroup, parameters);
                SendDeleteGroupMessage(message);
            }
        }

        IList<InteractiveScene> scenes = MixerInteractive.Scenes;
        foreach (InteractiveScene scene in scenes)
        {
            if (scene.SceneID != "default" && scene.SceneID != "prompt")
            {
                DeleteSceneParams parameters = new DeleteSceneParams(scene.SceneID, "default");
                JSONDeleteScene message = new JSONDeleteScene(MethodType.deleteScene, parameters);
                SendDeleteSceneMessage(message);
            }
        }
    }


    void CreateInput(){
        CreateInputMessage();
    }

    void CreateInputMessage(){
        ControlsParams parameters = new ControlsParams("prompt"
            , "textbox"
            , new List<string>() { "textbox" + run }
            , new List<string>() { "textbox" + run }
            , new Position[] {new Position("large", 30,15,1,1)}
            );

        JSONCreateControls message = new JSONCreateControls(MethodType.createControls, parameters);
		
        SendCreateControlsMessage(message);
        hasInput = false;
    }
    #endregion

    //TODO in CreateMixerButtons(), don't answer button for current user
    #region VotingPhase
    void VotingPhase()
    {
        ChangeState(UIState.voting);
        CreateMixerButtons();
        CreateMixerScenes();
        CreateScenesMessage();
        StartCoroutine(CreateGroupsMessage());
    }
    
    void CreateMixerButtons()
    {
        playersWhoResponded = GetPlayersWhoResponded();
        if(playersWhoResponded.Count == 0)
        {
            votingText.text = "No one answered :(";
        }
    }

    void CreateMixerScenes()
    {
        mixerSceneHolder = new MixerSceneHolder();
        
        int buttonsPerScene = buttonsPerColumn * buttonsPerRow;
        
        foreach(Player player in players)
        {
            List<MixerScene> scenesForPlayer = new List<MixerScene>();
            List<MixerButton> buttons = GetShuffledMixerButtons(player);
            int numberOfScenes = (buttons.Count / buttonsPerScene) + 1;

            for (int sceneId = 0; sceneId < numberOfScenes; sceneId++)
            {
                string scene = player.playerName + sceneId;
                MixerScene newScene = new MixerScene(scene, player);

                for (int x = 0; x < buttonsPerRow; x++)
                {
                    for (int y = 0; y < buttonsPerColumn; y++)
                    {
                        int currentIndex = x + (y * buttonsPerRow);
                        if(currentIndex >= buttons.Count)
                        {
                            continue;
                        }
                        else if (currentIndex < buttons.Count)
                        {
                            MixerButton button = buttons[currentIndex];
                            button.position = new Position("large", buttonWidth, buttonHeight, x * buttonWidth, y * buttonHeight);
                            button.scene = scene;
                            newScene.buttons.Add(button);
                        }
                    }
                }
                scenesForPlayer.Add(newScene);
            }
            mixerSceneHolder.AddSceneList(player.playerName, scenesForPlayer);
        }
        SetSceneTraversal();
    }

    void SetSceneTraversal()
    {
        foreach(Player player in players)
        {
            List<MixerScene> scenes = mixerSceneHolder.GetScenesForPlayer(player.playerName);
            for (int i = 0; i < scenes.Count; i++)
            {
                if (i == 0)
                {
                    scenes[i].previousScene = null;
                    if (scenes.Count > 1)
                    {
                        scenes[i].nextScene = scenes[i + 1];
                    }
                }
                else if (i > 0 && i < scenes.Count - 1)
                {
                    scenes[i].previousScene = scenes[i - 1];
                    scenes[i].nextScene = scenes[i + 1];
                }
                else if (i > 0 && i == scenes.Count - 1)
                {
                    scenes[i].previousScene = scenes[i - 1];
                    scenes[i].nextScene = null;
                }
            }
        }
    }

    void CreateScenesMessage()
    {
        AddScenesToMixerScenes();
        ControlsParams[] controlsParams = new ControlsParams[mixerScenes.Count];
        
        foreach(MixerScene scene in mixerScenes)
        {
            AddSceneChangeButtons(scene);
            List<string> controlIDs = new List<string>();
            List<string> texts = new List<string>();
            Position[] positions = new Position[scene.buttons.Count];

            foreach (MixerButton button in scene.buttons)
            {
                controlIDs.Add(button.player.answer);
                texts.Add(button.player.answer);
                positions[scene.buttons.IndexOf(button)] = button.position;
            }
            controlsParams[mixerScenes.IndexOf(scene)] = new ControlsParams(scene.sceneID, "button", controlIDs, texts, positions);
            
        }

        CreateScenesParams parameters = new CreateScenesParams(controlsParams);
        JSONCreateScene message = new JSONCreateScene(MethodType.createScenes, parameters);

        hasScenes = false;
        SendCreateScenesMessage(message);
    }

    IEnumerator CreateGroupsMessage()
    {
        Group[] groups = new Group[mixerScenes.Count];

        foreach (MixerScene scene in mixerScenes)
        {
            groups[mixerScenes.IndexOf(scene)] = new Group(scene.sceneID, scene.sceneID);
        }

        CreateGroupsParams parameters = new CreateGroupsParams(groups);
        JSONCreateGroups message = new JSONCreateGroups(MethodType.createGroups, parameters);

        //while (!hasScenes)
        //{
        //    yield return null;
        //}

        yield return new WaitUntil(() => hasScenes == true);
        yield return new WaitForSeconds(0.1f);
        hasGroups = false;
        SendCreateGroupsMessage(message);
        hasScenes = false;

        StartCoroutine(SetPlayerDefaultScenes());
    }

    void AddScenesToMixerScenes()
    {
        mixerScenes = mixerSceneHolder.GetSceneList();
    }
    
    void AddSceneChangeButtons(MixerScene scene)
    {
        if(scene.previousScene != null)
        {
            MixerButton backButton = new MixerButton(scene.player);
            backButton.position = new Position("large", 16, 16, 4, 4);
            backButton.player = new Player("back");
            backButton.player.answer = "<<";
            backButton.scene = scene.sceneID;
            scene.buttons.Add(backButton);
        }
        
        if(scene.nextScene != null)
        {
            MixerButton nextButton = new MixerButton(scene.player);
            nextButton.position = new Position("large", 20, 16, 4, 4);
            nextButton.player = new Player("next");
            nextButton.player.answer = ">>";
            nextButton.scene = scene.sceneID;
            scene.buttons.Add(nextButton);
        }
    }

    IEnumerator SetPlayerDefaultScenes()
    {
        Debug.Log("set player default scenes");
        //while (!hasGroups)
        //{
        //    yield return null;
        //}
        yield return new WaitUntil(() => hasGroups == true);
        yield return new WaitForSeconds(0.5f);
        hasGroups = false;

        IList<InteractiveGroup> groups = MixerInteractive.Groups;
        IList<InteractiveParticipant> parts = MixerInteractive.Participants;
        IList<InteractiveScene> se = MixerInteractive.Scenes;
        foreach (Player player in players)
        {
            InteractiveParticipant participant = GetParticipant(player.playerName);
            InteractiveGroup group = MixerInteractive.GetGroup(player.playerName + "0");
            participant.Group = group;
        }

        IList<InteractiveGroup> groups1 = MixerInteractive.Groups;
        IList<InteractiveParticipant> parts1 = MixerInteractive.Participants;
        IList<InteractiveScene> se1 = MixerInteractive.Scenes;
    }

    string GetDefaultSceneForPlayer(string playerName)
    {
        string defaultSceneID = playerName + "0";
        List<MixerScene> defaultScene = mixerScenes.Where(scene => scene.sceneID == defaultSceneID).ToList();
        if (defaultScene.Count > 0)
        {
            return defaultScene[0].sceneID;
        }
        else
        {
            return null;
        }
    }
    #endregion

    #region ScoringPhase
    void ScoringPhase()
    {
        ChangeState(UIState.@default);
        ShowTopScores();
    }

    void ShowTopScores()
    {
        //Current Top Scores
        Text currentTopScores = topScoreText.GetComponent<Text>();
        string currentTopScore = GetCurrentTopScores();
        if (currentTopScore != "")
        {
            currentTopScores.text = "Current Top 10\n" + currentTopScore;
        }
        else
        {
            currentTopScores.text = "Current Top 10\nN/A";
        }


        //Overall Top Scores
        Text overallTopScores = topScoreOverallText.GetComponent<Text>();
        string overallTopScore = GetOverallTopScores();
        if (currentTopScore != "")
        {
            overallTopScores.text = "Today's Top 10\n" + overallTopScore;
        }
        else
        {
            overallTopScores.text = "Today's Top 10\nN/A";
        }
    }

    string GetCurrentTopScores()
    {
        SortPlayersByCurrentScore();
        List<Player> topPlayers = players.Take(10).ToList<Player>();

        string result = "";

        foreach (Player player in topPlayers)
        {
            result += player.playerName + " " + player.currentScore + "\n  " + player.answer + "\n";
        }

        return result;
    }

    string GetOverallTopScores()
    {
        SortPlayersByOverallScore();
        List<Player> topPlayers = players.Take(10).ToList<Player>();

        string result = "";

        foreach (Player player in topPlayers)
        {
            result += player.playerName + " " + player.overallScore + "\n\t" + player.answer + "\n";
        }

        return result;
    }
    #endregion

    #region PlayerUtilities
    void SortPlayersByCurrentScore()
    {
        players = players.OrderBy(player => player.currentScore).ToList();
    }

    void SortPlayersByOverallScore()
    {
        players = players.OrderBy(player => player.overallScore).ToList();
    }

    List<MixerButton> GetPlayersWhoResponded()
    {
        List<MixerButton> responded = new List<MixerButton>();
        foreach (Player player in players)
        {
            if (player.answer != "" && player.answer != null)
                responded.Add(new MixerButton(player));
        }

        return responded;
    }

    InteractiveParticipant GetParticipant(string withName)
    {
        foreach (InteractiveParticipant participant in MixerInteractive.Participants)
        {
            if (participant.UserName == withName)
                return participant;
        }

        return null;
    }

    Player GetPlayerForControlID(string controlID)
    {
        string playerName = controlID;
        foreach (Player player in players)
        {
            if (player.answer == playerName)
                return player;
        }
        return null;
    }
    
    void CheckPlayers()
    {
        List<InteractiveParticipant> participants = GetAllParticipants();
        foreach (InteractiveParticipant participant in participants)
        {
            if (participant.State == InteractiveParticipantState.Joined)
            {
                IEnumerable<Player> playerMatches = players.Where(p => p.playerName == participant.UserName);

                var count = playerMatches.Count();
                if (count == 0)
                {
                    participant.Group = MixerInteractive.GetGroup(currentState.ToString());
                    Player newPlayer = new Player(participant.UserName);
                    players.Add(newPlayer);
                }
            }
            else if (participant.State == InteractiveParticipantState.Left)
            {
                foreach (Player player in players.ToList())
                {
                    if (player.playerName == participant.UserName)
                    {
                        players.Remove(player);
                    }
                }
            }
            else if (participant.State == InteractiveParticipantState.InputDisabled)
            {
                Debug.Log(participant.UserName + " isDisabled");
            }
        }
    }
    
    Player GetPlayerByName(string participantName)
    {
        foreach (Player player in players)
        {
            if (player.playerName == participantName)
                return player;
        }

        return null;
    }

    public List<MixerButton> GetShuffledMixerButtons(Player player)
    {
        List<MixerButton> list = playersWhoResponded;
        var random = new System.Random();
        IOrderedEnumerable<MixerButton> result = list.OrderBy(item => random.Next());
        List<MixerButton> randomizedList = result.ToList();

        MixerButton buttonToRemove = null;
        foreach (MixerButton button in randomizedList)
        {
            if (button.player.playerName == player.playerName)
            {
                buttonToRemove = button;
            }
        }

        if (buttonToRemove != null)
        {
            randomizedList.Remove(buttonToRemove);
        }

        return randomizedList;
    }

    void CleanScoresAndAnswers()
    {
        foreach(Player player in players)
        {
            player.answer = "";
            player.currentScore = 0;
        }
    }
    #endregion

    #region GroupUtilities
    void SetParticipantGroup(InteractiveParticipant participant, string groupName)
    {
        InteractiveGroup group = MixerInteractive.GetGroup(groupName);
        participant.Group = group;

        Player player = GetPlayerByName(participant.UserName);
        player.currentScene = groupName;
    }

    void PutAllIntoGroup(string group)
    {
        InteractiveGroup groupToPut = MixerInteractive.GetGroup(group);
        List<InteractiveParticipant> participants = GetAllParticipants();
        foreach (InteractiveParticipant participant in participants)
        {
            if (participant.Group != groupToPut && participant.State.Equals(InteractiveParticipantState.Joined))
                SetParticipantGroup(participant, groupToPut.GroupID);
        }
    }

    List<InteractiveParticipant> GetAllParticipants()
    {
        List<InteractiveParticipant> participants = new List<InteractiveParticipant>();
        
        IList<InteractiveGroup> groups = MixerInteractive.Groups;
        foreach(InteractiveGroup group in groups)
        {
            if(group.Participants.Count > 0)
            {
                participants.AddRange(group.Participants);
            }
        }

        return participants;
    }
    #endregion

    #region UIUtilities
    IEnumerator CountdownTimer(int time){
        float currentCountdown = time;
        while (currentCountdown > 0){
            if (currentState == UIState.prompt){
                countdownText.text = "Make us laugh: ";
            }
            else if (currentState == UIState.voting){
                countdownText.text = "Pick your favorite";
            }
            else if (currentState == UIState.@default){
                countdownText.text = "New game in: ";
            }
            countdownText.text += currentCountdown.ToString();
            yield return new WaitForSeconds(1);
            currentCountdown--;
        }

    }

    void ChangeState(UIState state)
    {
        currentState = state;
        countDownTimer = 10;

        promptPanel.SetActive(false);
        votingPanel.SetActive(false);
        votingResultsPanel.SetActive(false);

        if (state == UIState.prompt){
            PutAllIntoGroup(state.ToString());
            promptPanel.SetActive(true);
        }
        else if (state == UIState.voting)
        {
            PutAllIntoGroup("default");
            votingPanel.SetActive(true);
        }
        else if (state == UIState.@default)
        {
            PutAllIntoGroup(state.ToString());
            votingResultsPanel.SetActive(true);
        }
    }
    #endregion

    #region JSONMessages
    public void SendDeleteControlsMessage(JSONDeleteControls JSONmessage)
    {
        string message = JSONmessage.SaveToString();
        Debug.Log("JSONMESSAGE: " + message);
        MixerInteractive.SendInteractiveMessage(message);
    }
    public void SendCreateControlsMessage(JSONCreateControls JSONmessage)
    {
        string message = JSONmessage.SaveToString();
        Debug.Log("JSONMESSAGE: " + message);
        MixerInteractive.SendInteractiveMessage(message);
    }
    public void SendCreateScenesMessage(JSONCreateScene JSONmessage)
    {
        string message = JSONmessage.SaveToString();
        Debug.Log("JSONMESSAGE: " + message);
        MixerInteractive.SendInteractiveMessage(message);
    }
    public void SendCreateGroupsMessage(JSONCreateGroups JSONmessage)
    {
        string message = JSONmessage.SaveToString();
        Debug.Log("JSONMESSAGE: " + message);
        MixerInteractive.SendInteractiveMessage(message);
    }
    public void SendDeleteGroupMessage(JSONDeleteGroup JSONmessage)
    {
        string message = JSONmessage.SaveToString();
        Debug.Log("JSONMESSAGE: " + message);
        MixerInteractive.SendInteractiveMessage(message);
    }
    public void SendDeleteSceneMessage(JSONDeleteScene JSONmessage)
    {
        string message = JSONmessage.SaveToString();
        Debug.Log("JSONMESSAGE: " + message);
        MixerInteractive.SendInteractiveMessage(message);
    }
    #endregion

    #region Prompting
    void SetupPrompt()
    {
        PromptType promptType = getPromptType();
        if (promptType == PromptType.image)
        {
            textPrompt.SetActive(false);
            imagePrompt.SetActive(true);
            Sprite prompt = imagePrompts[UnityEngine.Random.Range(0, imagePrompts.Count)];
            imagePrompt.GetComponent<Image>().sprite = prompt;
        }
        else if (promptType == PromptType.text)
        {
            textPrompt.SetActive(true);
            imagePrompt.SetActive(false);
            string prompt = textPrompts[UnityEngine.Random.Range(0, textPrompts.Count)];
            Text text = textPrompt.GetComponent<Text>();
            text.text = prompt;
        }
    }

    PromptType getPromptType()
    {
        Array promptValues = System.Enum.GetValues(typeof(PromptType));
        System.Random random = new System.Random();
        return (PromptType)promptValues.GetValue(random.Next(promptValues.Length));
    }

    void GetPrompts()
    {
        GetImagePrompts();
        GetTextPrompts();
    }

    void GetImagePrompts()
    {
        string searchPattern = "*.jpg, *.png";
        string[] sp = searchPattern.Split(',');
        foreach (string pattern in sp)
        {
            foreach (string file in Directory.GetFiles("Assets/Resources", pattern))
            {
                string[] splitFileNames = file.Split('\\');
                string[] splitFileFromExtension = splitFileNames[splitFileNames.Length - 1].Split('.');
                imagePrompts.Add(Resources.Load<Sprite>(splitFileFromExtension[0]));
            }
        }
    }

    void GetTextPrompts()
    {
        string text = File.ReadAllText(Path.Combine(Path.Combine(Application.streamingAssetsPath, "TextPrompts"), "prompts.txt"));
        string[] splitText = text.Split(new string[] { System.Environment.NewLine }, StringSplitOptions.None);
        foreach (string prompt in splitText)
        {
            textPrompts.Add(prompt + " ____.");
        }
    }
    #endregion
}