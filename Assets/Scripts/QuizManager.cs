using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Unity.VisualScripting;

public class Quiz
{
    public string question;
    public bool answer;
    public Quiz(string question, bool answer)
    {
        this.question = question;
        this.answer = answer;
    }
}

public class QuizManager : MonoBehaviour
{
    public GameObject quizPanel;
    public GameObject quizUI;
    public TextMeshProUGUI questionText;
    public Button trueButton;
    public Button falseButton;

    public Image revealAnswerImage;
    public Sprite correctSprite;
    public Sprite wrongSprite;

    public GameObject lift;
    public GameObject gedung;

    public TextMeshProUGUI timerText;
    public TextMeshProUGUI popUpText;

    public GameObject popUpUI;

    public Slider slider;

    public GameObject liftDoorL;
    public GameObject liftDoorR;

    public GameObject character;
    public GameObject allObject;
    public GameObject kampus;

    public GameObject mainCanvas;

    int maxLevel = 26;
    float time = 240;
    int roundPerTurn = 5;
    float durationMovePerLevel = 0.5f;
    Quiz[] quiz =
    {
        new Quiz("Q1", true),
        new Quiz("Q2", true),
        new Quiz("Q3", true),
        new Quiz("Q4", true),
        new Quiz("Q5", true),
        new Quiz("Q6", true),
        new Quiz("Q7", true),
        new Quiz("Q8", true),
        new Quiz("Q9", true),
        new Quiz("Q10", true),
        new Quiz("Q11", true),
        new Quiz("Q12", true),
        new Quiz("Q13", true),
        new Quiz("Q14", true),
        new Quiz("Q15", true),
        new Quiz("Q16", true),
        new Quiz("Q17", true),
        new Quiz("Q18", true),
        new Quiz("Q19", true),
        new Quiz("Q20", true),
        new Quiz("Q21", true),
        new Quiz("Q22", true),
        new Quiz("Q23", true),
        new Quiz("Q24", true),
        new Quiz("Q25", true)
    };

    List<Quiz> quizList = new List<Quiz>();
    List<int> randomIndex = new List<int>();

    int round = 0;
    int correctAnswer = 0;

    string sMinutes;
    string sSeconds;

    bool liftMoving = false;
    float totalDurationMove = 0;
    float timeElapsed = 0;
    Vector2 gedungStartPos;
    Vector2 gedungEndPos;

    bool firstMove = true;
    int level = 1;
    Vector2 liftStartPos;
    Vector2 liftEndPos;
    float firstDurationMove;
    float previousTimeElapsed;

    float prevValue;
    float nextValue;

    bool over = false;

    bool liftOpen = false;
    bool liftOpening = false;
    float liftTime = 0;
    float liftOpenAnimationTime = 0.5f;

    float liftCloseScale;
    float liftOpenScale;

    float liftClosePosL;
    float liftClosePosR;
    float liftOpenPosL;
    float liftOpenPosR;

    float distancePerLevel = 7.71f;
    float cameraMoveDistance = 4.45f;

    Vector3 distanceCharacterToLift;

    Vector3 zoomOutScale;
    Vector3 zoomInScale;
    Vector3 charStartPos;

    Vector3 zoomInStartPos;
    Vector3 zoomInEndPos;

    Vector3 zoomOutStartPos;
    Vector3 zoomOutEndPos;

    float liftPosX = 20.2f;
    float kickPosX = 13.4f;
    float kelasPosX = 0f;

    bool startScene = false;
    float startSceneDuration = 2.5f;
    float timeStartScene = 0;
    bool reverse = false;

    bool charWalk = false;
    float charWalkDuration = 3.0f;
    float timeCharWalk = 0;

    Vector3 kampusStartPos;
    Vector3 kampusEndPos;

    bool walkEnd = false;

    bool liftRun = false;
    float timeLiftRun = 0;
    bool back = false;

    bool start = false;

    void Start()
    {
        for (int i = 0; i < quiz.Length; i++)
        {
            randomIndex.Add(i);
        }

        for (int i = 0; i < quiz.Length; i++)
        {
            int idx = Random.Range(0, randomIndex.Count);
            quizList.Add(quiz[randomIndex[idx]]);
            randomIndex.RemoveAt(idx);
        }

        trueButton.onClick.AddListener(delegate { Answer(true); });
        falseButton.onClick.AddListener(delegate { Answer(false); });

        revealAnswerImage.enabled = false;
        popUpUI.SetActive(false);
        slider.value = 0;

        liftCloseScale = liftDoorL.transform.localScale.x;
        liftOpenScale = 0;

        quizPanel.SetActive(false);
        mainCanvas.SetActive(false);

        zoomInScale = allObject.transform.localScale;
        zoomOutScale = allObject.transform.localScale * 0.31f;

        zoomInEndPos = allObject.transform.position;
        zoomInStartPos = zoomInEndPos;
        zoomInStartPos.x = -20f;

        zoomOutStartPos = allObject.transform.position;
        zoomOutStartPos.y = -4.2f;

        allObject.transform.localScale = zoomOutScale;
        allObject.transform.position = zoomOutStartPos;

        charStartPos = character.transform.localPosition;
        charStartPos.x = 20.33f;

        character.transform.localPosition = charStartPos;

        zoomOutEndPos = zoomOutStartPos;
        zoomOutEndPos.y = -56.35f;

        StartCoroutine(StartGameCoroutine());
    }

    void Update()
    {
        if (start && !over)
        {
            time -= Time.deltaTime;
            int minutes = (int)time / 60;
            int seconds = (int)time % 60;
            sMinutes = "0" + minutes.ToString();
            if (seconds >= 10)
                sSeconds = seconds.ToString();
            else
                sSeconds = "0" + seconds.ToString();

            if (time > 0)
                timerText.SetText(sMinutes + ":" + sSeconds);
            else
                GameOver(false);
        }

        if (startScene)
        {
            timeStartScene += Time.deltaTime;
            if (timeStartScene < startSceneDuration)
            {
                if (!reverse)
                    allObject.transform.position = Vector3.Lerp(zoomOutStartPos, zoomOutEndPos, timeStartScene / startSceneDuration);
                else
                    allObject.transform.position = Vector3.Lerp(zoomOutEndPos, zoomOutStartPos, timeStartScene / startSceneDuration);

            }
            else
            {
                startScene = false;
                if (!reverse)
                    allObject.transform.position = zoomOutEndPos;
                else
                    allObject.transform.position = zoomOutStartPos;

            }
        }

        if (liftMoving && !over)
        {
            timeElapsed += Time.deltaTime;
            if (firstMove || level == maxLevel)
            {
                if (timeElapsed < firstDurationMove)
                {
                    if (firstMove)
                        lift.transform.position = Vector2.Lerp(liftStartPos, liftEndPos, timeElapsed / firstDurationMove);
                    else
                        gedung.transform.position = Vector2.Lerp(gedungStartPos, gedungEndPos, timeElapsed / firstDurationMove);

                    previousTimeElapsed = timeElapsed;

                    slider.value = prevValue + ((nextValue - prevValue) * timeElapsed / totalDurationMove);

                    character.transform.position = lift.transform.position - distanceCharacterToLift;
                }
                else if (timeElapsed < totalDurationMove)
                {
                    if (firstMove)
                    {
                        gedung.transform.position = Vector2.Lerp(gedungStartPos, gedungEndPos, (timeElapsed - previousTimeElapsed) / (totalDurationMove - firstDurationMove));
                        lift.transform.position = liftEndPos;
                    }
                    else
                    {
                        lift.transform.position = Vector2.Lerp(liftStartPos, liftEndPos, (timeElapsed - previousTimeElapsed) / (totalDurationMove - firstDurationMove));
                        gedung.transform.position = gedungEndPos;
                    }

                    slider.value = prevValue + ((nextValue - prevValue) * timeElapsed / totalDurationMove);

                    character.transform.position = lift.transform.position - distanceCharacterToLift;
                }
                else
                {
                    liftMoving = false;
                    firstMove = false;
                    lift.transform.position = liftEndPos;
                    gedung.transform.position = gedungEndPos;
                    slider.value = nextValue;
                    character.transform.position = lift.transform.position - distanceCharacterToLift;

                    if (level < maxLevel)
                    {
                        StartCoroutine(ActivateQuizCoroutine(true));
                    }
                    else
                    {
                        StartCoroutine(EndGameCoroutine());
                    }
                }
            }
            else
            {
                if (timeElapsed < totalDurationMove)
                {
                    gedung.transform.position = Vector2.Lerp(gedungStartPos, gedungEndPos, timeElapsed / totalDurationMove);
                    slider.value = prevValue + ((nextValue - prevValue) * timeElapsed / totalDurationMove);
                    character.transform.position = lift.transform.position - distanceCharacterToLift;
                }
                else
                {
                    liftMoving = false;
                    gedung.transform.position = gedungEndPos;
                    slider.value = nextValue;
                    character.transform.position = lift.transform.position - distanceCharacterToLift;

                    StartCoroutine(ActivateQuizCoroutine(true));
                }
            }
        }

        if (liftOpening && !over)
        {
            liftTime += Time.deltaTime;
            if (liftTime < liftOpenAnimationTime)
            {
                float scale = 0;
                float posL = 0;
                float posR = 0;
                if (liftOpen)
                {
                    scale = liftCloseScale - ((liftCloseScale - liftOpenScale) * liftTime / liftOpenAnimationTime);
                    posL = liftClosePosL - ((liftClosePosL - liftOpenPosL) * liftTime / liftOpenAnimationTime);
                    posR = liftClosePosR - ((liftClosePosR - liftOpenPosR) * liftTime / liftOpenAnimationTime);
                }
                else
                {
                    scale = liftOpenScale + ((liftCloseScale - liftOpenScale) * liftTime / liftOpenAnimationTime);
                    posL = liftOpenPosL + ((liftClosePosL - liftOpenPosL) * liftTime / liftOpenAnimationTime);
                    posR = liftOpenPosR + ((liftClosePosR - liftOpenPosR) * liftTime / liftOpenAnimationTime);
                }
                liftDoorL.transform.localScale = new Vector2(scale, liftDoorL.transform.localScale.y);
                liftDoorR.transform.localScale = new Vector2(scale, liftDoorR.transform.localScale.y);

                liftDoorL.transform.localPosition = new Vector2(posL, liftDoorL.transform.localPosition.y);
                liftDoorR.transform.localPosition = new Vector2(posR, liftDoorR.transform.localPosition.y);

                liftDoorL.SetActive(true);
                liftDoorR.SetActive(true);
            }
            else
            {
                if (liftOpen)
                {
                    liftDoorL.transform.localScale = new Vector2(liftOpenScale, liftDoorL.transform.localScale.y);
                    liftDoorR.transform.localScale = new Vector2(liftOpenScale, liftDoorR.transform.localScale.y);

                    liftDoorL.transform.localPosition = new Vector2(liftOpenPosL, liftDoorL.transform.localPosition.y);
                    liftDoorR.transform.localPosition = new Vector2(liftOpenPosR, liftDoorL.transform.localPosition.y);
                }
                else
                {
                    liftDoorL.transform.localScale = new Vector2(liftCloseScale, liftDoorL.transform.localScale.y);
                    liftDoorR.transform.localScale = new Vector2(liftCloseScale, liftDoorR.transform.localScale.y);

                    liftDoorL.transform.localPosition = new Vector2(liftClosePosL, liftDoorL.transform.localPosition.y);
                    liftDoorR.transform.localPosition = new Vector2(liftClosePosR, liftDoorR.transform.localPosition.y);
                }

                liftOpening = false;
            }
        }

        if (charWalk && !over)
        {
            timeCharWalk += Time.deltaTime;
            if (timeCharWalk < charWalkDuration)
            {
                kampus.transform.localPosition = Vector3.Lerp(kampusStartPos, kampusEndPos, timeCharWalk / charWalkDuration);
            }
            else
            {
                charWalk = false;
                kampus.transform.localPosition = kampusEndPos;
                if (walkEnd)
                    GameOver(true);
            }
        }

        if(liftRun && !over)
        {
            timeLiftRun += Time.deltaTime;
            if(timeLiftRun < (2*durationMovePerLevel))
            {
                if (!back)
                    lift.transform.position = Vector2.Lerp(liftStartPos, liftEndPos, timeLiftRun / (2 * durationMovePerLevel));
                else
                    lift.transform.position = Vector2.Lerp(liftEndPos, liftStartPos, timeLiftRun / (2 * durationMovePerLevel));
            }
            else
            {
                liftRun = false;
                if (!back)
                    lift.transform.position = liftEndPos;
                else
                    lift.transform.position = liftStartPos;
            }
        }
    }

    void GameOver(bool win)
    {
        over = true;
        StopAllCoroutines();

        if (win)
        {
            popUpText.SetText("Kamu Berhasil Masuk Kelas!");
        }
        else
        {
            if (liftMoving)
                liftMoving = false;
            else
                StartCoroutine(ActivateQuizCoroutine(false));

            popUpText.SetText("Kamu Terlambat Masuk Kelas!");
        }

        popUpUI.SetActive(true);
    }

    void Answer(bool answer)
    {
        if (answer == quizList[0].answer)
        {
            revealAnswerImage.sprite = correctSprite;
            correctAnswer++;
        }
        else
        {
            revealAnswerImage.sprite = wrongSprite;
            quizList.Add(quizList[0]);
        }

        quizList.RemoveAt(0);

        StartCoroutine(AnsweredCoroutine());
    }

    IEnumerator AnsweredCoroutine()
    {
        quizUI.SetActive(false);
        revealAnswerImage.enabled = true;

        yield return new WaitForSeconds(0.5f);

        revealAnswerImage.enabled = false;
        round++;

        if (round == roundPerTurn || quizList.Count == 0)
        {
            prevValue = (level - 1) * 1f / (maxLevel - 1) * 1f;
            level += correctAnswer;
            nextValue = (level - 1) * 1f / (maxLevel - 1) * 1f;

            if (correctAnswer > 0)
            {
                popUpText.SetText("Berhasil Naik " + correctAnswer.ToString() + " Lantai");
                StartCoroutine(ActivateQuizCoroutine(false));
            }
            else
            {
                popUpText.SetText("Gagal Naik Lantai");
                StartCoroutine(AnswerAllWrongCoroutine());
            }

            popUpUI.SetActive(true);
            yield return new WaitForSeconds(1.5f);
            popUpUI.SetActive(false);

            gedungStartPos = gedung.transform.position;
            gedungEndPos = new Vector2(gedung.transform.position.x, gedung.transform.position.y - (distancePerLevel * correctAnswer));

            totalDurationMove = durationMovePerLevel * correctAnswer;

            if (firstMove || level == maxLevel)
            {
                float distance = 1.175f;
                float duration = durationMovePerLevel / 5f;

                gedungEndPos.y += distance;

                liftStartPos = lift.transform.position;
                liftEndPos = new Vector2(lift.transform.position.x, lift.transform.position.y + distance);

                if (firstMove)
                    firstDurationMove = duration;
                else
                    firstDurationMove = totalDurationMove - duration;
            }

            if (correctAnswer > 0)
                liftMoving = true;


            timeElapsed = 0;
            round = 0;
            correctAnswer = 0;
        }

        if (!liftMoving)
        {
            quizUI.SetActive(true);
            questionText.SetText(quizList[0].question);
        }
    }

    IEnumerator ActivateQuizCoroutine(bool active)
    {
        if (active)
        {
            lift.transform.position = new Vector2(lift.transform.position.x + cameraMoveDistance, lift.transform.position.y);
            gedung.transform.position = new Vector2(gedung.transform.position.x + cameraMoveDistance, gedung.transform.position.y);
            character.transform.position = new Vector2(character.transform.position.x + cameraMoveDistance, character.transform.position.y);

            CalcLiftDoorLocalPos();
            liftTime = 0;
            liftOpen = true;
            liftOpening = true;
        }
        else
        {
            CalcLiftDoorLocalPos();
            liftTime = 0;
            liftOpen = false;
            liftOpening = true;
        }

        yield return new WaitForSeconds(liftOpenAnimationTime);

        if (active)
        {
            quizPanel.SetActive(true);
            quizUI.SetActive(true);
            questionText.SetText(quizList[0].question);
        }
        else
        {
            quizPanel.SetActive(false);
            lift.transform.position = new Vector2(lift.transform.position.x - cameraMoveDistance, lift.transform.position.y);
            gedung.transform.position = new Vector2(gedung.transform.position.x - cameraMoveDistance, gedung.transform.position.y);
            character.transform.position = new Vector2(character.transform.position.x - cameraMoveDistance, character.transform.position.y);
        }

    }

    IEnumerator StartGameCoroutine()
    {
        yield return new WaitForSeconds(1f);
        timeStartScene = 0;
        reverse = false;
        startScene = true;

        yield return new WaitForSeconds(startSceneDuration + 1f);
        timeStartScene = 0;
        reverse = true;
        startScene = true;

        yield return new WaitForSeconds(startSceneDuration + 1f);
        allObject.transform.localScale = zoomInScale;
        allObject.transform.position = zoomInStartPos;

        yield return new WaitForSeconds(0.5f);
        kampusStartPos = kampus.transform.localPosition;
        kampusEndPos = kampus.transform.localPosition;
        kampusEndPos.x = liftPosX;
        timeCharWalk = 0;
        charWalkDuration = 3f;
        charWalk = true;

        liftDoorL.SetActive(false);
        liftDoorR.SetActive(false);

        yield return new WaitForSeconds(charWalkDuration + 0.5f);
        lift.transform.position = new Vector2(lift.transform.position.x + cameraMoveDistance, lift.transform.position.y);
        gedung.transform.position = new Vector2(gedung.transform.position.x + cameraMoveDistance, gedung.transform.position.y);
        character.transform.position = new Vector2(character.transform.position.x + cameraMoveDistance, character.transform.position.y);

        CalcLiftDoorLocalPos();
        liftDoorL.transform.localPosition = new Vector2(liftOpenPosL, liftDoorL.transform.localPosition.y);
        liftDoorR.transform.localPosition = new Vector2(liftOpenPosR, liftDoorR.transform.localPosition.y);
        liftDoorL.transform.localScale = new Vector2(liftOpenScale, liftDoorL.transform.localScale.y);
        liftDoorR.transform.localScale = new Vector2(liftOpenScale, liftDoorR.transform.localScale.y);
        liftOpen = true;

        distanceCharacterToLift = lift.transform.position - character.transform.position;

        quizPanel.SetActive(true);
        quizUI.SetActive(true);
        questionText.SetText(quizList[0].question);
        mainCanvas.SetActive(true);
        start = true;
    }

    IEnumerator EndGameCoroutine()
    {
        CalcLiftDoorLocalPos();
        distanceCharacterToLift = lift.transform.position - character.transform.position;

        liftTime = 0;
        liftOpen = true;
        liftOpening = true;

        yield return new WaitForSeconds(liftOpenAnimationTime + 0.1f);
        walkEnd = true;
        kampusStartPos = kampus.transform.localPosition;
        kampusEndPos = kampus.transform.localPosition;
        kampusEndPos.x = kelasPosX;
        timeCharWalk = 0;
        charWalkDuration = 3f;
        charWalk = true;
    }

    IEnumerator AnswerAllWrongCoroutine()
    {
        quizPanel.SetActive(false);
        lift.transform.position = new Vector2(lift.transform.position.x - cameraMoveDistance, lift.transform.position.y);
        gedung.transform.position = new Vector2(gedung.transform.position.x - cameraMoveDistance, gedung.transform.position.y);
        character.transform.position = new Vector2(character.transform.position.x - cameraMoveDistance, character.transform.position.y);

        charWalkDuration = 1f;

        yield return new WaitForSeconds(0.2f);
        kampusStartPos = kampus.transform.localPosition;
        kampusEndPos = kampus.transform.localPosition;
        kampusEndPos.x = kickPosX;
        timeCharWalk = 0;
        charWalk = true;

        yield return new WaitForSeconds(charWalkDuration + 0.2f);
        CalcLiftDoorLocalPos();
        liftTime = 0;
        liftOpen = false;
        liftOpening = true;

        yield return new WaitForSeconds(liftOpenAnimationTime);
        liftStartPos = lift.transform.position;
        if (level < (maxLevel - 1))
            liftEndPos = new Vector2(lift.transform.position.x, lift.transform.position.y + distancePerLevel * 2);
        else
            liftEndPos = new Vector2(lift.transform.position.x, lift.transform.position.y - distancePerLevel * 2);
        back = false;
        timeLiftRun = 0;
        liftRun = true;

        yield return new WaitForSeconds(7f - (4 * durationMovePerLevel) - liftOpenAnimationTime - 0.2f);
        back = true;
        timeLiftRun = 0;
        liftRun = true;

        yield return new WaitForSeconds(2 * durationMovePerLevel + 0.1f);
        CalcLiftDoorLocalPos();
        liftTime = 0;
        liftOpen = true;
        liftOpening = true;

        yield return new WaitForSeconds(liftOpenAnimationTime + 0.1f);
        kampusStartPos = kampus.transform.localPosition;
        kampusEndPos = kampus.transform.localPosition;
        kampusEndPos.x = liftPosX;
        timeCharWalk = 0;
        charWalkDuration = 1f;
        charWalk = true;

        yield return new WaitForSeconds(charWalkDuration + 0.1f);
        lift.transform.position = new Vector2(lift.transform.position.x + cameraMoveDistance, lift.transform.position.y);
        gedung.transform.position = new Vector2(gedung.transform.position.x + cameraMoveDistance, gedung.transform.position.y);
        character.transform.position = new Vector2(character.transform.position.x + cameraMoveDistance, character.transform.position.y);
        quizPanel.SetActive(true);
        quizUI.SetActive(true);
        questionText.SetText(quizList[0].question);
    }

    void CalcLiftDoorLocalPos()
    {
        if(liftOpen)
        {
            liftClosePosL = liftDoorL.transform.localPosition.x + 1.2f;
            liftClosePosR = liftDoorR.transform.localPosition.x - 1.2f;
            liftOpenPosL = liftDoorL.transform.localPosition.x;
            liftOpenPosR = liftDoorR.transform.localPosition.x;
        }
        else
        {
            liftClosePosL = liftDoorL.transform.localPosition.x;
            liftClosePosR = liftDoorR.transform.localPosition.x;
            liftOpenPosL = liftDoorL.transform.localPosition.x - 1.2f;
            liftOpenPosR = liftDoorR.transform.localPosition.x + 1.2f;
        }
    }
}
