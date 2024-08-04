using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using Newtonsoft.Json;
using TMPro;

using Photon.Pun;

public class QuestionGetter : MonoBehaviourPunCallbacks
{
    public static QuestionGetter Instance { get; private set; }

    public string URL;
    public GameObject QuizPanel;
    public Text QuestionText;
    public Button OptionA;
    public Button OptionB;
    public Button OptionC;
    public Button OptionD;
    public TMP_Text TimerText; 

    

    public delegate void QuestionAnswered(bool isCorrect, string description);
    public static event QuestionAnswered OnQuestionAnswered;

    private List<SetQuestion> questionSets;
    private int currentQuestionIndex;
    private List<Question> currentQuestions = new List<Question>();
    private List<Question> DuplicateQuestions = new List<Question>();
    
    private float timeRemaining = 30f; // Timer set to 30 seconds
    private bool isTimerRunning = false;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }
    

    private void Start()
    {
        if (QuizPanel == null || QuestionText == null || OptionA == null || OptionB == null || OptionC == null || OptionD == null || TimerText == null)
        {
            Debug.LogError("Một hoặc nhiều thành phần UI chưa được gán trong Inspector.");
            return;
        }

        string setID = "";
        if(PhotonNetwork.IsConnected)
        {
            setID = GameSettings.multisettingsList[0].setQuestionId;
        }
        else setID = GameSettings.settingsList[0].setQuestionId;
        if (string.IsNullOrEmpty(setID))
        {
            setID = "ec74b952-469c-4fc4-a175-498776ac12e3";  // Thay bằng SetID mặc định của bạn
        }
        GetData(setID);
    }

    private void Update()
    {
        if (isTimerRunning)
        {
            if (timeRemaining > 0)
            {
                timeRemaining -= Time.deltaTime;
                TimerText.text = "Time: " + Mathf.Round(timeRemaining).ToString();
            }
            else
            {
                isTimerRunning = false;
                TimerText.text = "Time: 0";
                CheckAnswerWrapper(null); // Time's up, auto answer false
            }
        }
    }

    public void GetData(string setID)
    {
        if(setID=="") setID = "ec74b952-469c-4fc4-a175-498776ac12e3";
        StartCoroutine(FetchData(setID));
        Debug.Log("****QuizManager receive your SetID:**** " + setID);
    }

    public IEnumerator FetchData(string setID)
    {
        using (UnityWebRequest request = UnityWebRequest.Get(URL + setID))
        {
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.Log(request.error);
                //StartCoroutine(FetchData("ec74b952-469c-4fc4-a175-498776ac12e3"));  // Thay bằng SetID mặc định của bạn
            }
            else
            {
                questionSets = JsonConvert.DeserializeObject<List<SetQuestion>>(request.downloadHandler.text);
                if (questionSets != null && questionSets.Count > 0)
                {
                    currentQuestions = questionSets[0].questions;
                    DuplicateQuestions.AddRange(currentQuestions);
                    if(!PhotonNetwork.IsConnected)
                    {
                        currentQuestionIndex = Random.Range(0, currentQuestions.Count);
                    }
                    else
                    {
                        PhotonView PV = GetComponent<PhotonView>();
                        PV.RPC("SetNextQuestion", RpcTarget.All, Random.Range(0, currentQuestions.Count));
                    }
                    DisplayQuestion(currentQuestionIndex);
                }
                else
                {
                    Debug.Log("Không tìm thấy câu hỏi nào trong bộ câu hỏi.");
                    Debug.Log("****QuizManager receive your SetID:**** " + setID);
                }
            }
        }
    }


    [PunRPC]
    public void SetNextQuestion(int nextIndex)
    {
        currentQuestionIndex = nextIndex;
    }

    private void DisplayQuestion(int index)
    {
        if (currentQuestions == null || currentQuestions.Count == 0 || index < 0 || index >= currentQuestions.Count)
        {
            Debug.LogWarning("Không có câu hỏi hoặc chỉ số không hợp lệ.");
            return;
        }

        if (QuizPanel == null || QuestionText == null || OptionA == null || OptionB == null || OptionC == null || OptionD == null)
        {
            Debug.LogError("QuizPanel hoặc một trong các thành phần không được gán.");
            return;
        }

        Question test = currentQuestions[index];
        QuestionText.text = test.Content;
        Debug.Log("Question: " + test.Content);

        Text optionAText = OptionA.GetComponentInChildren<Text>();
        Text optionBText = OptionB.GetComponentInChildren<Text>();
        Text optionCText = OptionC.GetComponentInChildren<Text>();
        Text optionDText = OptionD.GetComponentInChildren<Text>();

        if (optionAText == null || optionBText == null || optionCText == null || optionDText == null)
        {
            Debug.LogError("Không tìm thấy thành phần Text bên trong một hoặc nhiều nút Option.");
            return;
        }

        Debug.Log("Option1: " + test.Option1);
        Debug.Log("Option2: " + test.Option2);
        Debug.Log("Option3: " + test.Option3);
        Debug.Log("Option4: " + test.Option4);

        optionAText.text = test.Option1;
        optionBText.text = test.Option2;
        optionCText.text = test.Option3;
        optionDText.text = test.Option4;

        OptionA.onClick.RemoveAllListeners();
        OptionB.onClick.RemoveAllListeners();
        OptionC.onClick.RemoveAllListeners();
        OptionD.onClick.RemoveAllListeners();

        if(!PhotonNetwork.IsConnected)
        {
            OptionA.onClick.AddListener(() => CheckAnswerWrapper(test.Option1));
            OptionB.onClick.AddListener(() => CheckAnswerWrapper(test.Option2));
            OptionC.onClick.AddListener(() => CheckAnswerWrapper(test.Option3));
            OptionD.onClick.AddListener(() => CheckAnswerWrapper(test.Option4));
        }
        else
        {
            OptionA.onClick.AddListener(() => 
            {
                PhotonView PV = GetComponent<PhotonView>();
                PV.RPC("CheckAnswerWrapper", RpcTarget.All, test.Option1);
            });
            OptionB.onClick.AddListener(() => 
            {
                PhotonView PV = GetComponent<PhotonView>();
                PV.RPC("CheckAnswerWrapper", RpcTarget.All, test.Option2);
            });
            OptionC.onClick.AddListener(() => 
            {
                PhotonView PV = GetComponent<PhotonView>();
                PV.RPC("CheckAnswerWrapper", RpcTarget.All, test.Option3);
            });
            OptionD.onClick.AddListener(() => 
            {
                PhotonView PV = GetComponent<PhotonView>();
                PV.RPC("CheckAnswerWrapper", RpcTarget.All, test.Option4);
            });
        }
        if(PhotonNetwork.IsConnected)
        {
            OptionA.interactable = false;
            OptionB.interactable = false;
            OptionC.interactable = false;
            OptionD.interactable = false;
        }
        // Reset the timer
        timeRemaining = 30f;
    }

    public void SetButton(bool YourButtonStatus)
    {
        OptionA.interactable = YourButtonStatus;
        OptionB.interactable = YourButtonStatus;
        OptionC.interactable = YourButtonStatus;
        OptionD.interactable = YourButtonStatus;
    }

    public void StartTimer()
    {
        timeRemaining = 30f;
        isTimerRunning = true;
    }

    private void CheckAnswer(Question question, string selectedAnswer)
    {
        bool isCorrect = question != null && question.Answer == selectedAnswer;

        if (isCorrect)
        {
            AudioPlayer.instance.QuizCorrect();

            Debug.Log("Chính xác!");
            currentQuestions.Remove(question);
            if (currentQuestions.Count > 0)
            {
                if (!PhotonNetwork.IsConnected)
                {
                    currentQuestionIndex = Random.Range(0, currentQuestions.Count);
                }
                else
                {
                    PhotonView PV = GetComponent<PhotonView>();
                    PV.RPC("SetNextQuestion", RpcTarget.All, Random.Range(0, currentQuestions.Count));
                }
                DisplayQuestion(currentQuestionIndex);
            }
            else
            {
                currentQuestions.AddRange(DuplicateQuestions);
                Debug.Log ("Duplicate Question count: "+DuplicateQuestions.Count);
                if (!PhotonNetwork.IsConnected)
                {
                    currentQuestionIndex = Random.Range(0, currentQuestions.Count);
                }
                else
                {
                    PhotonView PV = GetComponent<PhotonView>();
                    PV.RPC("SetNextQuestion", RpcTarget.All, Random.Range(0, currentQuestions.Count));
                }
                Debug.Log("Đã trả lời đúng tất cả các câu hỏi!");
                DisplayQuestion(currentQuestionIndex);
            }
        }
        else
        {
            AudioPlayer.instance.QuizWrong();

            Debug.Log("Trả lời sai");
            DisplayQuestion(currentQuestionIndex);
        }

        OnQuestionAnswered?.Invoke(isCorrect, question != null ? question.Description : string.Empty);
    }
[PunRPC]
    public void CheckAnswerWrapper(string selectedAnswer)
    {
        if (currentQuestions == null || currentQuestions.Count == 0 || currentQuestionIndex < 0 || currentQuestionIndex >= currentQuestions.Count)
            return;

        isTimerRunning = false; // Stop the timer when an answer is selected
        CheckAnswer(currentQuestions[currentQuestionIndex], selectedAnswer);
    }
}
