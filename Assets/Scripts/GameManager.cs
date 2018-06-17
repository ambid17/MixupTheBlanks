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

    public List<MixerButton> answerButtons;
    public MixerInput mixerInput;
    [SerializeField]
    public List<MixerScene> mixerScenes;
    #endregion

    #region Logic Variables
    [SerializeField]
    public List<Player> players;
    public UIState currentState;
    int buttonsPerRow = 8;
    int buttonsPerColumn = 2;
    int buttonWidth = 10;
    int buttonHeight = 8;
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
        if(e.ControlID == "Input")
        {
            Player player = GetPlayerByName(e.Participant.UserName);
            player.answer = e.Text;
            e.Participant.Group = MixerInteractive.GetGroup("default");
        }
    }

    //TODO: once a user votes change them to default
    void CheckButtonPresses(object sender, InteractiveButtonEventArgs e)
    {
        if(currentState == UIState.voting)
        {
            Debug.Log("button pressed");
            string controlID = e.ControlID;
            if(controlID == ">>")
            {
                //e.Participant.Group = nextScene
            }else if (controlID == "<<")
            {
                //e.Participant.Group = lastScene
            }else
            {
                if (e.IsPressed)
                {
                    Player playerToAddScore = GetPlayerForControlID(controlID);
                    playerToAddScore.currentScore++;
                    playerToAddScore.overallScore++;
                    
                    e.Participant.Group = MixerInteractive.GetGroup("default");
                }
            }
        }
    }

    IEnumerator Run()
    {
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
        }
    }
    #endregion

    //TODO create JSON to create empty input box with submit button
    #region PromptPhase
    void PromptPhase(){
        Clean();
        ChangeState(UIState.prompt);
        SetupPrompt();
        CreateInput();
    }

    void SetupPrompt()
    {
        PromptType promptType = getPromptType();
        if (promptType == PromptType.image){
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

    PromptType getPromptType(){
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

    void Clean(){
        CleanScoresAndAnswers();
        DeleteInputMessage();
    }

    void CreateInput(){
        mixerInput = new MixerInput("prompt", 1,1,5,5);
        CreateInputMessage();
    }

    void DeleteInputMessage(){
        Parameters parameters = new DeleteControlsParams("prompt", new string[] { "Input" });

        //JSONMessage message = new JSONMessage(JSONMessage.MethodType.deleteControls, parameters);

        //SendJSONMessage(message);
    }

    void CreateInputMessage(){
        List<string> controlIDs = new List<string>() { "Input" };
        List<string> texts = new List<string>() {"Input answer"};
        Position[] positions = new Position[] {new Position(mixerInput.position.size, mixerInput.position.width, mixerInput.position.height, mixerInput.position.x, mixerInput.position.x)};

        ControlsParams parameters = new ControlsParams("prompt"
            , "input"
            , new List<string>() { "input" }
            , new List<string>() { "input" }
            , new Position[] {new Position("Large", 5,5,1,1)}
            );

        JSONMessage1 message = new JSONMessage1(JSONMessage1.MethodType.createControls, parameters);
		
        SendJSONMessage1(message);
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
    }
    
    void CreateMixerButtons()
    {
        List<Player> playersWhoResponded = GetPlayersWhoResponded();
        answerButtons = new List<MixerButton>();

        if (playersWhoResponded.Count > 0)
        {
            foreach (Player player in playersWhoResponded)
            {
                MixerButton newButton = new MixerButton(player);
                answerButtons.Add(newButton);
            }
        }
        else
        {
            votingText.text = "No one answered :(";
        }
    }

    void CreateMixerScenes()
    {
        mixerScenes = new List<MixerScene>();
        
        int buttonsPerScene = buttonsPerColumn * buttonsPerRow;
        
        foreach(Player player in players)
        {
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
                            button.position = new Position("Large", x * buttonWidth, y * buttonHeight, buttonWidth, buttonHeight);
                            button.scene = scene;
                            newScene.buttons.Add(button);
                        }
                    }
                }
                mixerScenes.Add(newScene);
            }
        }
        SetSceneTraversal();
    }

    void SetSceneTraversal()
    {
        for(int i = 0; i < mixerScenes.Count; i++)
        {
            if (i == 0)
            {
                mixerScenes[i].previousScene = null;
                if(mixerScenes.Count > 1)
                {
                    mixerScenes[i].nextScene = mixerScenes[i + 1];
                }
            }
            else if (i > 0 && i < mixerScenes.Count-1)
            {
                mixerScenes[i].previousScene = mixerScenes[i - 1];
                mixerScenes[i].nextScene = mixerScenes[i + 1];
            }
            else if(i > 0 && i == mixerScenes.Count-1)
            {
                mixerScenes[i].previousScene = mixerScenes[i - 1];
                mixerScenes[i].nextScene = null;
            }
        }
    }

    void CreateScenesMessage()
    {
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

        Parameters parameters = new CreateScenesParams(controlsParams);
       // JSONMessage message = new JSONMessage(JSONMessage.MethodType.createScenes, parameters);

       // SendJSONMessage(message);
    }
    
    void AddSceneChangeButtons(MixerScene scene)
    {
        MixerButton backButton = new MixerButton(scene.player);
        backButton.position = new Position("Large", 16, 16, 4, 4);
        backButton.scene = scene.sceneID;

        MixerButton nextButton = new MixerButton(scene.player);
        nextButton.position = new Position("Large", 20, 16, 4, 4);
        nextButton.scene = scene.sceneID;

        scene.buttons.Add(backButton);
        scene.buttons.Add(nextButton);
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

    List<Player> GetPlayersWhoResponded()
    {
        List<Player> responded = new List<Player>();
        foreach (Player player in players)
        {
            if (player.answer != "" && player.answer != null)
                responded.Add(player);
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
        Debug.Log("numPlayers: " + participants.Count);
        foreach (InteractiveParticipant participant in participants)
        {
            if (participant.State == InteractiveParticipantState.Joined)
            {
                Debug.Log(participant.UserName + " joined");
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
                Debug.Log(participant.UserName + " left");
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
    void PutAllIntoGroup(string group)
    {
        InteractiveGroup groupToPut = MixerInteractive.GetGroup(group);
        List<InteractiveParticipant> participants = GetAllParticipants();
		Debug.Log(group);
        foreach (InteractiveParticipant participant in participants)
        {
            if(participant.Group != groupToPut && participant.State.Equals(InteractiveParticipantState.Joined))
                participant.Group = groupToPut;
        }
    }

    List<InteractiveParticipant> GetAllParticipants()
    {
        List<InteractiveParticipant> participants = new List<InteractiveParticipant>();
        if (MixerInteractive.GetGroup("default").Participants != null)
        {
            participants.AddRange(MixerInteractive.GetGroup("default").Participants);
        }
        if (MixerInteractive.GetGroup("prompt") != null)
        {
            participants.AddRange(MixerInteractive.GetGroup("prompt").Participants);
        }
        if (MixerInteractive.GetGroup("voting") != null)
        {
            participants.AddRange(MixerInteractive.GetGroup("voting").Participants);
        }

        return participants;
    }
    #endregion

    #region UIUtilities
    public void SendJSONMessage(JSONMessage JSONmessage){
        string message = JSONmessage.SaveToString();
        Debug.Log("JSONMESSAGE: " + message);
        MixerInteractive.SendInteractiveMessage(message);
    }
	public void SendJSONMessage1(JSONMessage1 JSONmessage) {
		string message = JSONmessage.SaveToString();
		Debug.Log("JSONMESSAGE: " + message);
		MixerInteractive.SendInteractiveMessage(message);
	}
	public List<MixerButton> GetShuffledMixerButtons(Player player){
        List<MixerButton> list = answerButtons;
        var random = new System.Random();
        IOrderedEnumerable<MixerButton> result = list.OrderBy(item => random.Next());
        List<MixerButton> randomizedList = result.ToList();

        MixerButton buttonToRemove = null;
        foreach(MixerButton button in randomizedList){
            if(button.player.playerName == player.playerName){
                buttonToRemove = button;
            }
        }

        if(buttonToRemove != null){
            randomizedList.Remove(buttonToRemove);
        }

        return randomizedList;
    }
    
    IEnumerator CountdownTimer(int time){
        float currentCountdown = time;
        while (currentCountdown > 0){
            if (currentState == UIState.prompt){
                countdownText.text = "Make us laugh: ";
            }
            else if (currentState == UIState.voting){
                countdownText.text = "";
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
        MixerInteractive.SetCurrentScene(state.ToString());
        PutAllIntoGroup(state.ToString());

        promptPanel.SetActive(false);
        votingPanel.SetActive(false);
        votingResultsPanel.SetActive(false);

        if (state == UIState.prompt){
            promptPanel.SetActive(true);
        }
        else if (state == UIState.voting)
        {
            votingPanel.SetActive(true);
        }
        else if (state == UIState.@default)
        { 
            votingResultsPanel.SetActive(true);
        }
    }
    #endregion
}