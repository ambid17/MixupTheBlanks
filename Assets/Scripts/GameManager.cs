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
    List<MixerScene> buttonScenes;
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

        MixerInteractive.OnInteractivityStateChanged += CheckMixerInitialized;    }

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
            if (MixerInteractive.HasSubmissions("Input"))
            {
                var results = MixerInteractive.GetText("Input");
                for (int i = 0; i < results.Count; i++)
                {
                    Player player = GetPlayerByName(results[i].Participant.UserName);
                    player.answer = results[i].Text;
                    results[i].Participant.Group = MixerInteractive.GetGroup("default");
                }
            }
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

                    //This doesn't change the user's group
                    Player playerWhoVoted = GetPlayerByName(e.Participant.UserName);
                    InteractiveParticipant participant = GetParticipant(playerWhoVoted.playerName);
                    participant.Group = MixerInteractive.GetGroup("default"); 
                    
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
    void PromptPhase()
    {
        Clean();
        ChangeState(UIState.prompt);
        SetupPrompt();
    }

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

    void Clean()
    {
        CleanScoresAndAnswers();
    }
    #endregion 

    //TODO in CreateMixerButtons(), don't answer button for current user
    #region VotingPhase
    void VotingPhase()
    {
        ChangeState(UIState.voting);
        CreateMixerButtons();
        //SetupSceneChangeButtons();
        SendButtonMessage();
    }

    
    
    void CreateMixerButtons()
    {
        List<Player> playersWhoResponded = GetPlayersWhoResponded();
        answerButtons = new List<MixerButton>();

        if (playersWhoResponded.Count > 0)
        {
            buttonScenes = new List<MixerScene>();
            List<MixerButton> answerButtons = new List<MixerButton>();

            foreach (Player player in playersWhoResponded)
            {
                MixerButton newButton = new MixerButton(player);
                answerButtons.Add(newButton);
            }

            CreateMixerScenes(answerButtons);
        }
        else
        {
            votingText.text = "No one answered :(";
        }
    }

    void CreateMixerScenes(List<MixerButton> buttons)
    {
        Shuffle(buttons);
        int buttonsPerScene = buttonsPerColumn * buttonsPerRow;
        int numberOfScenes = (buttons.Count / buttonsPerScene) + 1;
        

        for (int sceneId = 0; sceneId < numberOfScenes; sceneId++)
        {
            MixerScene newScene = new MixerScene(sceneId);
            buttonScenes.Add(newScene);
            
            for (int x = 0; x < buttonsPerRow; x++)
            {
                for (int y = 0; y < buttonsPerColumn; y++)
                {
                    int currentIndex = x + (y * buttonsPerRow);
                    if (currentIndex < buttons.Count)
                    {
                        MixerButton button = buttons[currentIndex];
                        button.position.x = x * buttonWidth;
                        button.position.y = y * buttonHeight;
                        button.position.width = buttonWidth;
                        button.position.height = buttonHeight;
                        button.scene = sceneId;
                        newScene.buttons.Add(button);
                        answerButtons.Add(button);
                    } 
                }
            }
        }
    }
    
    void SendButtonMessage()
    {
        List<string> controlIDs = new List<string>();
        List<string> texts = new List<string>();
        Position[] positions = new Position[answerButtons.Count];

        foreach (MixerButton button in answerButtons)
        {
            controlIDs.Add(button.player.answer + "_");
            texts.Add(button.player.answer);
            positions[answerButtons.IndexOf(button)] = button.position;
        }

        JSONMessage json = new JSONMessage(currentState.ToString(), controlIDs, texts, positions);
        
        string message = json.SaveToString();
        Debug.Log("message " + message);
        MixerInteractive.SendInteractiveMessage(message);
    }

    //TODO: don't think we can do this, maybe the user just gets
    //a random subset of the answers to choose from instead
    void SetupSceneChangeButtons()
    {
        if (MixerInteractive.HasSubmissions("<<"))
        {
            var results = MixerInteractive.GetText("<<");
            foreach (InteractiveTextResult result in results)
            {
                //change group?
            }
        }

        if (MixerInteractive.HasSubmissions(">>"))
        {
            var results = MixerInteractive.GetText(">>");
            foreach (InteractiveTextResult result in results)
            {
                //change group?
            }
        }
    }
    #endregion

    #region ScoringPhase
    void ScoringPhase()
    {
        ChangeState(UIState.@default);
        GameModeVoting();
        ShowTopScores();
    }

    //TODO
    void GameModeVoting()
    {

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
        string playerName = controlID.Substring(0, controlID.Length - 1); //chop off the "_" at the end
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
        foreach (InteractiveParticipant participant in participants)
        {
            if(participant.Group != groupToPut)
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
    public static void Shuffle(List<MixerButton> list)
    {
        int n = list.Count;
        while (n > 1)
        {
            n--;
            System.Random random = new System.Random();
            int k = random.Next(n + 1);
            MixerButton value = list[k];
            list[k] = list[n];
            list[n] = value;
        }
    }
    
    IEnumerator CountdownTimer(int time)
    {
        float currentCountdown = time;
        while (currentCountdown > 0)
        {
            if (currentState == UIState.prompt)
            {
                countdownText.text = "Make us laugh: ";
            }
            else if (currentState == UIState.voting)
            {
                countdownText.text = "";
            }
            else if (currentState == UIState.@default)
            {
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

        if (state == UIState.prompt)
        {
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