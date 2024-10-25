using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;
using System.Linq;
using TMPro;
using System;

public class SlotBehaviour : MonoBehaviour
{
    [SerializeField]
    private RectTransform mainContainer_RT;

    [Header("Sprites")]
    [SerializeField]
    private Sprite[] myImages;  //images taken initially

    [Header("Slot Images")]
    [SerializeField]
    private List<SlotImage> images;     //class to store total images
    [SerializeField]
    private List<SlotImage> Tempimages;     //class to store the result matrix

    [Header("Slots Objects")]
    [SerializeField]
    private GameObject[] Slot_Objects;
    [Header("Slots Elements")]
    [SerializeField]
    private LayoutElement[] Slot_Elements;

    [Header("Slots Transforms")]
    [SerializeField]
    private Transform[] Slot_Transform;

    [Header("Line Button Objects")]
    [SerializeField]
    private List<GameObject> StaticLine_Objects;

    [Header("Line Button Texts")]
    [SerializeField]
    private List<TMP_Text> StaticLine_Texts;

    private Dictionary<int, string> y_string = new Dictionary<int, string>();

    [Header("Buttons")]
    [SerializeField]
    private Button SlotStart_Button;
    [SerializeField]
    private Button AutoSpin_Button;
    [SerializeField]
    private Button AutoSpinStop_Button;
    [SerializeField]
    private Button MaxBet_Button;
    [SerializeField]
    private Button LineBetPlus_Button;
    [SerializeField]
    private Button LineBetMinus_Button;

    [Header("Animated Sprites")]
    [SerializeField]
    private Sprite[] Ten_Sprite;
    [SerializeField]
    private Sprite[] J_Sprite;
    [SerializeField]
    private Sprite[] Q_Sprite;
    [SerializeField]
    private Sprite[] K_Sprite;
    [SerializeField]
    private Sprite[] A_Sprite;
    [SerializeField]
    private Sprite[] Sycee_Sprite;
    [SerializeField]
    private Sprite[] Fan_Sprite;
    [SerializeField]
    private Sprite[] Teapot_Sprite;
    [SerializeField]
    private Sprite[] Coins_Sprite;
    [SerializeField]
    private Sprite[] Scroll_Sprite;
    [SerializeField]
    private Sprite[] Jackpot_Sprite;
    [SerializeField]
    private Sprite[] Wild_Sprite;
    [SerializeField]
    private Sprite[] Scatter_Sprite;
    [SerializeField]
    private Sprite[] Bonus_Sprite;
    [SerializeField]
    private Sprite[] Freespin_Sprite;

    [Header("Miscellaneous UI")]
    [SerializeField]
    private TMP_Text Balance_text;
    [SerializeField]
    private TMP_Text TotalBet_text;
    [SerializeField]
    private TMP_Text LineBet_text;
    [SerializeField]
    private TMP_Text TotalWin_text;

    [Header("Audio Management")]
    [SerializeField]
    private AudioController audioController;

    [SerializeField]
    private UIManager uiManager;

    int tweenHeight = 0;  //calculate the height at which tweening is done

    [SerializeField]
    private GameObject Image_Prefab;    //icons prefab

    [SerializeField]
    private PayoutCalculation PayCalculator;

    private List<Tweener> alltweens = new List<Tweener>();


    [SerializeField]
    private List<ImageAnimation> TempList;  //stores the sprites whose animation is running at present 

    [SerializeField]
    private int IconSizeFactor = 100;       //set this parameter according to the size of the icon and spacing

    private int numberOfSlots = 5;          //number of columns

    [SerializeField]
    int verticalVisibility = 3;

    protected int Lines = 20;

    [SerializeField]
    private SocketIOManager SocketManager;

    [Header("Free Spins Board")]
    [SerializeField]
    private GameObject FSBoard_Object;
    [SerializeField]
    private TMP_Text FSnum_text;

    private Coroutine AutoSpinRoutine = null;
    private Coroutine FreeSpinRoutine = null;
    private Coroutine tweenroutine;
    private Tweener WinTween;
    private bool IsAutoSpin = false;
    private bool IsFreeSpin = false;
    private bool IsSpinning = false;
    internal bool CheckPopups = false;
    private bool CheckSpinAudio = false;
    private int BetCounter = 0;
    private double currentBalance = 0;
    private double currentTotalBet = 0;

    [Header("BonusGame Popup")]
    [SerializeField]
    private BonusController _bonusManager;

    private void Start()
    {
        IsAutoSpin = false;
        if (SlotStart_Button) SlotStart_Button.onClick.RemoveAllListeners();
        if (SlotStart_Button) SlotStart_Button.onClick.AddListener(delegate { StartSlots(); });

        if (LineBetPlus_Button) LineBetPlus_Button.onClick.RemoveAllListeners();
        if (LineBetPlus_Button) LineBetPlus_Button.onClick.AddListener(delegate { ChangeBet(true); });
        if (LineBetMinus_Button) LineBetMinus_Button.onClick.RemoveAllListeners();
        if (LineBetMinus_Button) LineBetMinus_Button.onClick.AddListener(delegate { ChangeBet(false); });

        if (MaxBet_Button) MaxBet_Button.onClick.RemoveAllListeners();
        if (MaxBet_Button) MaxBet_Button.onClick.AddListener(MaxBet);

        if (AutoSpin_Button) AutoSpin_Button.onClick.RemoveAllListeners();
        if (AutoSpin_Button) AutoSpin_Button.onClick.AddListener(AutoSpin);


        if (AutoSpinStop_Button) AutoSpinStop_Button.onClick.RemoveAllListeners();
        if (AutoSpinStop_Button) AutoSpinStop_Button.onClick.AddListener(StopAutoSpin);

        tweenHeight = (15 * IconSizeFactor) - 280;
        //CheckBetButtons();
    }

    private void AutoSpin()
    {
        if (!IsAutoSpin)
        {

            IsAutoSpin = true;
            if (AutoSpinStop_Button) AutoSpinStop_Button.gameObject.SetActive(true);
            if (AutoSpin_Button) AutoSpin_Button.gameObject.SetActive(false);

            if (AutoSpinRoutine != null)
            {
                StopCoroutine(AutoSpinRoutine);
                AutoSpinRoutine = null;
            }
            AutoSpinRoutine = StartCoroutine(AutoSpinCoroutine());

        }
    }

    internal void FreeSpin(int spins)
    {
        if (!IsFreeSpin)
        {
            if (FSnum_text) FSnum_text.text = spins.ToString();
            if (FSBoard_Object) FSBoard_Object.SetActive(true);
            IsFreeSpin = true;
            ToggleButtonGrp(false);

            if (FreeSpinRoutine != null)
            {
                StopCoroutine(FreeSpinRoutine);
                FreeSpinRoutine = null;
            }
            FreeSpinRoutine = StartCoroutine(FreeSpinCoroutine(spins));

        }
    }

    private void StopAutoSpin()
    {
        if (IsAutoSpin)
        {
            IsAutoSpin = false;
            if (AutoSpinStop_Button) AutoSpinStop_Button.gameObject.SetActive(false);
            if (AutoSpin_Button) AutoSpin_Button.gameObject.SetActive(true);
            StartCoroutine(StopAutoSpinCoroutine());
        }

    }

    private IEnumerator AutoSpinCoroutine()
    {
        while (IsAutoSpin)
        {
            StartSlots(IsAutoSpin);
            yield return tweenroutine;
        }
    }

    private void CompareBalance()
    {
        if (currentBalance < currentTotalBet)
        {
            Debug.Log("Comparing Balance...");
            uiManager.LowBalPopup();
            //if (AutoSpin_Button) AutoSpin_Button.interactable = false;
            //if (SlotStart_Button) SlotStart_Button.interactable = false;
        }
        //else
        //{
        //    if (AutoSpin_Button) AutoSpin_Button.interactable = true;
        //    if (SlotStart_Button) SlotStart_Button.interactable = true;
        //}
    }

    private IEnumerator FreeSpinCoroutine(int spinchances)
    {
        int i = 0;
        while (i < spinchances)
        {
            StartSlots(IsAutoSpin);
            yield return tweenroutine;
            yield return new WaitForSeconds(2);
            i++;
            if (FSnum_text) FSnum_text.text = (spinchances - i).ToString();
        }
        if (FSBoard_Object) FSBoard_Object.SetActive(false);
        ToggleButtonGrp(true);
        IsFreeSpin = false;
    }

    private IEnumerator StopAutoSpinCoroutine()
    {
        yield return new WaitUntil(() => !IsSpinning);
        ToggleButtonGrp(true);
        if (AutoSpinRoutine != null || tweenroutine != null)
        {
            StopCoroutine(AutoSpinRoutine);
            StopCoroutine(tweenroutine);
            tweenroutine = null;
            AutoSpinRoutine = null;
            StopCoroutine(StopAutoSpinCoroutine());
        }
    }

    //Fetch Lines from backend
    internal void FetchLines(string LineVal, int count)
    {
        y_string.Add(count + 1, LineVal);
        StaticLine_Texts[count].text = (count + 1).ToString();
        StaticLine_Objects[count].SetActive(true);
    }

    //Generate Static Lines from button hovers
    internal void GenerateStaticLine(TMP_Text LineID_Text)
    {
        DestroyStaticLine();
        int LineID = 1;
        try
        {
            LineID = int.Parse(LineID_Text.text);
        }
        catch (Exception e)
        {
            Debug.Log("Exception while parsing " + e.Message);
        }
        List<int> y_points = null;
        y_points = y_string[LineID]?.Split(',')?.Select(Int32.Parse)?.ToList();
        PayCalculator.GeneratePayoutLinesBackend(y_points, y_points.Count, true);
    }

    //Destroy Static Lines from button hovers
    internal void DestroyStaticLine()
    {
        PayCalculator.ResetStaticLine();
    }

    private void MaxBet()
    {
        if (audioController) audioController.PlayButtonAudio();
        BetCounter = SocketManager.initialData.Bets.Count - 1;
        if (LineBet_text) LineBet_text.text = SocketManager.initialData.Bets[BetCounter].ToString("f2");
        if (TotalBet_text) TotalBet_text.text = (SocketManager.initialData.Bets[BetCounter] * Lines).ToString("f2");
        currentTotalBet = SocketManager.initialData.Bets[BetCounter] * Lines;
        if (currentTotalBet < currentBalance)
            CompareBalance();
    }


    private void ChangeBet(bool IncDec)
    {
        if (audioController) audioController.PlayButtonAudio();
        if (IncDec)
        {
            if (BetCounter < SocketManager.initialData.Bets.Count - 1)
            {
                BetCounter++;
                //CheckBetButtons();
            }
        }
        else
        {
            if (BetCounter > 0)
            {
                BetCounter--;
                //CheckBetButtons();
            }
        }

        if (LineBet_text) LineBet_text.text = SocketManager.initialData.Bets[BetCounter].ToString("f2");
        if (TotalBet_text) TotalBet_text.text = (SocketManager.initialData.Bets[BetCounter] * Lines).ToString("f2");
        currentTotalBet = SocketManager.initialData.Bets[BetCounter] * Lines;
        CompareBalance();
    }


    //just for testing purposes delete on production
    //private void Update()
    //{
    //    if (Input.GetKeyDown(KeyCode.Space) && SlotStart_Button.interactable)
    //    {
    //        StartSlots();
    //    }
    //}

    //populate the slots with the values recieved from backend
    //internal void PopulateInitalSlots(int number, List<int> myvalues)
    //{
    //    PopulateSlot(myvalues, number);
    //}

    private void OnApplicationFocus(bool focus)
    {
        audioController.CheckFocusFunction(focus, CheckSpinAudio);
    }

    internal void SetInitialUI()
    {
        BetCounter = 0;
        if (LineBet_text) LineBet_text.text = SocketManager.initialData.Bets[BetCounter].ToString("f2");
        if (TotalBet_text) TotalBet_text.text = (SocketManager.initialData.Bets[BetCounter] * Lines).ToString("f2");
        if (TotalWin_text) TotalWin_text.text = "0.00";
        if (Balance_text) Balance_text.text = SocketManager.playerdata.Balance.ToString("f2");
        currentBalance = SocketManager.playerdata.Balance;
        currentTotalBet = SocketManager.initialData.Bets[BetCounter] * Lines;
        _bonusManager.PopulateWheel(SocketManager.bonusdata);
        CompareBalance();
        uiManager.InitialiseUIData(SocketManager.initUIData.AbtLogo.link, SocketManager.initUIData.AbtLogo.logoSprite, SocketManager.initUIData.ToULink, SocketManager.initUIData.PopLink, SocketManager.initUIData.paylines);
    }

    //reset the layout after populating the slots
    internal void LayoutReset(int number)
    {
        if (Slot_Elements[number]) Slot_Elements[number].ignoreLayout = true;
        if (SlotStart_Button) SlotStart_Button.interactable = true;
    }

    //private void PopulateSlot(List<int> values, int number)
    //{
    //    if (Slot_Objects[number]) Slot_Objects[number].SetActive(true);
    //    for (int i = 0; i < values.Count; i++)
    //    {
    //        GameObject myImg = Instantiate(Image_Prefab, Slot_Transform[number]);
    //        images[number].slotImages.Add(myImg.GetComponent<Image>());
    //        images[number].slotImages[i].sprite = myImages[values[i]];
    //    }
    //    for (int k = 0; k < 2; k++)
    //    {
    //        GameObject mylastImg = Instantiate(Image_Prefab, Slot_Transform[number]);
    //        images[number].slotImages.Add(mylastImg.GetComponent<Image>());
    //        images[number].slotImages[images[number].slotImages.Count - 1].sprite = myImages[values[k]];
    //    }
    //    if (mainContainer_RT) LayoutRebuilder.ForceRebuildLayoutImmediate(mainContainer_RT);
    //    tweenHeight = (values.Count * IconSizeFactor) - 280;
    //    GenerateMatrix(number);
    //}

    //function to populate animation sprites accordingly
    private void PopulateAnimationSprites(ImageAnimation animScript, int val)
    {
        animScript.textureArray.Clear();
        animScript.textureArray.TrimExcess();
        animScript.AnimationSpeed = 16f;
        switch (val)
        {
            case 0:
                for (int i = 0; i < Ten_Sprite.Length; i++)
                {
                    animScript.textureArray.Add(Ten_Sprite[i]);
                }
                break;
            case 1:
                for (int i = 0; i < J_Sprite.Length; i++)
                {
                    animScript.textureArray.Add(J_Sprite[i]);
                }
                break;
            case 2:
                for (int i = 0; i < Q_Sprite.Length; i++)
                {
                    animScript.textureArray.Add(Q_Sprite[i]);
                }
                break;
            case 3:
                for (int i = 0; i < K_Sprite.Length; i++)
                {
                    animScript.textureArray.Add(K_Sprite[i]);
                }
                break;
            case 4:
                for (int i = 0; i < A_Sprite.Length; i++)
                {
                    animScript.textureArray.Add(A_Sprite[i]);
                }
                break;
            case 5:
                for (int i = 0; i < Sycee_Sprite.Length; i++)
                {
                    animScript.textureArray.Add(Sycee_Sprite[i]);
                }
                break;
            case 6:
                for (int i = 0; i < Fan_Sprite.Length; i++)
                {
                    animScript.textureArray.Add(Fan_Sprite[i]);
                }
                break;
            case 7:
                for (int i = 0; i < Teapot_Sprite.Length; i++)
                {
                    animScript.textureArray.Add(Teapot_Sprite[i]);
                }

                break;
            case 8:
                for (int i = 0; i < Coins_Sprite.Length; i++)
                {
                    animScript.textureArray.Add(Coins_Sprite[i]);
                }
                break;
            case 9:
                for (int i = 0; i < Scroll_Sprite.Length; i++)
                {
                    animScript.textureArray.Add(Scroll_Sprite[i]);
                }
                break;
            case 10:
                for (int i = 0; i < Jackpot_Sprite.Length; i++)
                {
                    animScript.textureArray.Add(Jackpot_Sprite[i]);
                }
                animScript.AnimationSpeed = 30f;
                break;
            case 11:
                for (int i = 0; i < Wild_Sprite.Length; i++)
                {
                    animScript.textureArray.Add(Wild_Sprite[i]);
                }
                animScript.AnimationSpeed = 30f;
                break;
            case 12:
                for (int i = 0; i < Scatter_Sprite.Length; i++)
                {
                    animScript.textureArray.Add(Scatter_Sprite[i]);
                }
                animScript.AnimationSpeed = 30f;
                break;
            case 13:
                for (int i = 0; i < Bonus_Sprite.Length; i++)
                {
                    animScript.textureArray.Add(Bonus_Sprite[i]);
                }
                animScript.AnimationSpeed = 30f;
                break;
            case 14:
                for (int i = 0; i < Freespin_Sprite.Length; i++)
                {
                    animScript.textureArray.Add(Freespin_Sprite[i]);
                }
                animScript.AnimationSpeed = 30f;
                break;
        }
    }


    //starts the spin process
    private void StartSlots(bool autoSpin = false)
    {
        if (audioController) audioController.PlaySpinButtonAudio();

        if (!autoSpin)
        {
            if (AutoSpinRoutine != null)
            {
                StopCoroutine(AutoSpinRoutine);
                StopCoroutine(tweenroutine);
                tweenroutine = null;
                AutoSpinRoutine = null;
            }
        }

        WinningsAnim(false);

        if (SlotStart_Button) SlotStart_Button.interactable = false;

        if (TempList.Count > 0)
        {
            StopGameAnimation();
        }
        PayCalculator.ResetLines();
        tweenroutine = StartCoroutine(TweenRoutine());
    }

    //manage the Routine for spinning of the slots
    private IEnumerator TweenRoutine()
    {
        Debug.Log("Entered Tween Routine...");
        Debug.Log("Current Balance is " + currentBalance.ToString());
        if (currentBalance < currentTotalBet && !IsFreeSpin)
        {
            Debug.Log("Tween Routine Executing...");
            CompareBalance();
            StopAutoSpin();
            yield return new WaitForSeconds(1);
            ToggleButtonGrp(true);
            yield break;
        }
        if (audioController) audioController.PlayWLAudio("spin");
        IsSpinning = true;
        CheckSpinAudio = true;
        ToggleButtonGrp(false);
        for (int i = 0; i < numberOfSlots; i++)
        {
            InitializeTweening(Slot_Transform[i]);
            yield return new WaitForSeconds(0.1f);
        }

        if (!IsFreeSpin)
        {
            double bet = 0;
            double balance = 0;
            try
            {
                bet = double.Parse(TotalBet_text.text);
            }
            catch (Exception e)
            {
                Debug.Log("Error while conversion " + e.Message);
            }

            try
            {
                balance = double.Parse(Balance_text.text);
            }
            catch (Exception e)
            {
                Debug.Log("Error while conversion " + e.Message);
            }
            double initAmount = balance;

            balance = balance - bet;

            DOTween.To(() => initAmount, (val) => initAmount = val, balance, 0.8f).OnUpdate(() =>
            {
                if (Balance_text) Balance_text.text = initAmount.ToString("f2");
            });
        }
        SocketManager.AccumulateResult(BetCounter);

        yield return new WaitUntil(() => SocketManager.isResultdone);


        for (int j = 0; j < SocketManager.resultData.ResultReel.Count; j++)
        {
            List<int> resultnum = SocketManager.resultData.FinalResultReel[j]?.Split(',')?.Select(Int32.Parse)?.ToList();
            for (int i = 0; i < 5; i++)
            {
                if (images[i].slotImages[images[i].slotImages.Count - 5 + j]) images[i].slotImages[images[i].slotImages.Count - 5 + j].sprite = myImages[resultnum[i]];
                PopulateAnimationSprites(images[i].slotImages[images[i].slotImages.Count - 5 + j].gameObject.GetComponent<ImageAnimation>(), resultnum[i]);
            }
        }

        yield return new WaitForSeconds(0.5f);

        for (int i = 0; i < numberOfSlots; i++)
        {
            yield return StopTweening(5, Slot_Transform[i], i);
        }
        yield return new WaitForSeconds(0.5f);
        CheckPayoutLineBackend(SocketManager.resultData.linesToEmit, SocketManager.resultData.FinalsymbolsToEmit, SocketManager.resultData.jackpot);
        KillAllTweens();

        currentBalance = SocketManager.playerdata.Balance;

        CheckPopups = true;

        if (TotalWin_text) TotalWin_text.text = SocketManager.playerdata.currentWining.ToString("f2");

        if (Balance_text) Balance_text.text = SocketManager.playerdata.Balance.ToString("f2");

        if (SocketManager.resultData.jackpot > 0)
        {
            uiManager.PopulateWin(4, SocketManager.resultData.jackpot);
            yield return new WaitUntil(() => !CheckPopups);
            CheckPopups = true;
        }

        if (SocketManager.resultData.isBonus)
        {
            CheckBonusGame();
        }
        else
        {
            CheckWinPopups();
        }

        yield return new WaitUntil(() => !CheckPopups);
        if (!IsAutoSpin && !IsFreeSpin)
        {
            ToggleButtonGrp(true);
            IsSpinning = false;
        }
        else
        {
            yield return new WaitForSeconds(2f);
            IsSpinning = false;
        }
        if (SocketManager.resultData.freeSpins.isNewAdded)
        {
            if (IsFreeSpin)
            {
                IsFreeSpin = false;
                if (FreeSpinRoutine != null)
                {
                    StopCoroutine(FreeSpinRoutine);
                    FreeSpinRoutine = null;
                }
            }
            uiManager.FreeSpinProcess((int)SocketManager.resultData.freeSpins.count);
            if (IsAutoSpin)
            {
                StopAutoSpin();
                yield return new WaitForSeconds(0.5f);
            }
        }

    }

    internal void shuffleInitialMatrix()
    {
        for (int i = 0; i < Tempimages.Count; i++)
        {
            for (int j = 0; j < 3; j++)
            {
                int randomIndex = UnityEngine.Random.Range(0, 14);
                Tempimages[i].slotImages[j].sprite = myImages[randomIndex];
            }
        }
    }

    internal void CheckWinPopups()
    {
        if (SocketManager.resultData.WinAmout >= currentTotalBet * 10 && SocketManager.resultData.WinAmout < currentTotalBet * 15)
        {
            uiManager.PopulateWin(1, SocketManager.resultData.WinAmout);
        }
        else if (SocketManager.resultData.WinAmout >= currentTotalBet * 15 && SocketManager.resultData.WinAmout < currentTotalBet * 20)
        {
            uiManager.PopulateWin(2, SocketManager.resultData.WinAmout);
        }
        else if (SocketManager.resultData.WinAmout >= currentTotalBet * 20)
        {
            uiManager.PopulateWin(3, SocketManager.resultData.WinAmout);
        }
        else
        {
            CheckPopups = false;
        }
    }

    internal void CallCloseSocket()
    {
        SocketManager.CloseSocket();
    }

    //bool once = false;

    internal void CheckBonusGame()
    {
        if (SocketManager.resultData.isBonus)
        {
            _bonusManager.StartBonus((int)SocketManager.resultData.BonusStopIndex);
        }
        else
        {
            CheckPopups = false;
        }

        if (SocketManager.resultData.freeSpins.count > 0)
        {
            if (IsAutoSpin)
            {
                StopAutoSpin();
            }
        }
    }

    void ToggleButtonGrp(bool toggle)
    {

        if (SlotStart_Button) SlotStart_Button.interactable = toggle;
        if (MaxBet_Button) MaxBet_Button.interactable = toggle;
        if (AutoSpin_Button) AutoSpin_Button.interactable = toggle;

        //if (toggle)
        //{
        //    CheckBetButtons();
        //}
        //else
        //{
            if (LineBetMinus_Button) LineBetMinus_Button.interactable = toggle;
            if (LineBetPlus_Button) LineBetPlus_Button.interactable = toggle;
        //}

    }

    private void TurnBetButtons(int m_config_bet)
    {
        switch (m_config_bet)
        {
            case 0:
                LineBetPlus_Button.interactable = false;
                LineBetMinus_Button.interactable = true;
                break;
            case 1:
                LineBetPlus_Button.interactable = true;
                LineBetMinus_Button.interactable = false;
                break;
            case 2:
                LineBetMinus_Button.interactable = true;
                LineBetPlus_Button.interactable = true;
                break;
        }
    }

    private void CheckBetButtons()
    {
        if(BetCounter >= SocketManager.initialData.Bets.Count - 1)
        {
            TurnBetButtons(1);
        }
        else if(BetCounter <= 0)
        {
            TurnBetButtons(0);
        }
        else
        {
            TurnBetButtons(2);
        }
    }

    //start the icons animation
    private void StartGameAnimation(GameObject animObjects)
    {
        ImageAnimation temp = animObjects.GetComponent<ImageAnimation>();
        temp.StartAnimation();
        TempList.Add(temp);
    }

    //stop the icons animation
    private void StopGameAnimation()
    {
        for (int i = 0; i < TempList.Count; i++)
        {
            TempList[i].StopAnimation();
        }
        TempList.Clear();
        TempList.TrimExcess();
    }

    //Win Animation When A Line Is Matched
    private void WinningsAnim(bool IsStart)
    {
        if (IsStart)
        {
            WinTween = TotalWin_text.gameObject.GetComponent<RectTransform>().DOScale(new Vector2(1.5f, 1.5f), 1f).SetLoops(-1, LoopType.Yoyo).SetDelay(0);
        }
        else
        {
            WinTween.Kill();
            TotalWin_text.gameObject.GetComponent<RectTransform>().localScale = Vector3.one;
        }
    }

    //generate the payout lines generated 
    private void CheckPayoutLineBackend(List<int> LineId, List<string> points_AnimString, double jackpot = 0)
    {
        List<int> y_points = null;
        List<int> points_anim = null;
        if (LineId.Count > 0 && points_AnimString.Count > 0)
        {
            if (audioController) audioController.PlayWLAudio("win");

            for (int i = 0; i < LineId.Count; i++)
            {
                y_points = y_string[LineId[i] + 1]?.Split(',')?.Select(Int32.Parse)?.ToList();
                PayCalculator.GeneratePayoutLinesBackend(y_points, y_points.Count);
            }

            if (jackpot > 0)
            {
                for (int i = 0; i < Tempimages.Count; i++)
                {
                    for (int k = 0; k < Tempimages[i].slotImages.Count; k++)
                    {
                        StartGameAnimation(Tempimages[i].slotImages[k].gameObject);
                    }
                }
            }
            else
            {
                for (int i = 0; i < points_AnimString.Count; i++)
                {
                    points_anim = points_AnimString[i]?.Split(',')?.Select(Int32.Parse)?.ToList();

                    for (int k = 0; k < points_anim.Count; k++)
                    {
                        if (points_anim[k] >= 10)
                        {
                            StartGameAnimation(Tempimages[(points_anim[k] / 10) % 10].slotImages[points_anim[k] % 10].gameObject);
                        }
                        else
                        {
                            StartGameAnimation(Tempimages[0].slotImages[points_anim[k]].gameObject);
                        }
                    }
                }
            }
            WinningsAnim(true);
        }
        else
        {
            if (audioController) audioController.StopWLAaudio();
            //if (audioController) audioController.PlayWLAudio("lose");
        }
        CheckSpinAudio = false;
    }

    //generate the result matrix
    private void GenerateMatrix(int value)
    {
        for (int j = 0; j < 3; j++)
        {
            Tempimages[value].slotImages.Add(images[value].slotImages[images[value].slotImages.Count - 5 + j]);
        }
    }

    #region TweeningCode
    private void InitializeTweening(Transform slotTransform)
    {
        slotTransform.localPosition = new Vector2(slotTransform.localPosition.x, 0);
        Tweener tweener = slotTransform.DOLocalMoveY(-tweenHeight, 0.2f).SetLoops(-1, LoopType.Restart).SetDelay(0);
        tweener.Play();
        alltweens.Add(tweener);
    }



    private IEnumerator StopTweening(int reqpos, Transform slotTransform, int index)
    {
        alltweens[index].Pause();
        int tweenpos = (reqpos * IconSizeFactor) - IconSizeFactor;
        alltweens[index] = slotTransform.DOLocalMoveY(-tweenpos + 100, 0.5f).SetEase(Ease.OutElastic);
        yield return new WaitForSeconds(0.2f);
    }


    private void KillAllTweens()
    {
        for (int i = 0; i < numberOfSlots; i++)
        {
            alltweens[i].Kill();
        }
        alltweens.Clear();

    }
    #endregion

}

[Serializable]
public class SlotImage
{
    public List<Image> slotImages = new List<Image>(10);
}

