using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;

public class GamePiece : MonoBehaviour
{
    public int xIndex;
    public int yIndex;
    public Color color;
    Board m_board;

    public Text timerText;
    bool m_isMoving = false;
    public bool isBomb = false;
    public int Timer=0;
    public InterpType interpolation = InterpType.SmootherStep;



    public enum InterpType
    {
        Linear,
        EaseOut,
        EaseIn,
        SmoothStep,
        SmootherStep
    }

    // Start is called before the first frame update
    void Start()
    {
        EventTrigger trigger = GetComponent<EventTrigger>();
    }

    // Update is called once per frame
    void Update()
    {

        if(isBomb && Timer > 0)
        {
            if (Timer - m_board.normalizedMoveCount > 0)
            {
                timerText.text = "" + (Timer - m_board.normalizedMoveCount);
            }
        }
        if(isBomb && Timer-m_board.normalizedMoveCount ==0)
        {
            SceneManager.LoadScene("GameOver");
        }
    }
    public void Init(Board board)
    {

        m_board = board;
    }
    public void SetCoord(int x, int y)
    {
        xIndex = x;
        yIndex = y;
        color = GetComponent<SpriteRenderer>().color;

    }
    public void Move(int destX, int destY, float timeToMove)
    {
        if (!m_isMoving)
        {
            StartCoroutine(MoveRoutine(m_board.m_allTiles[destX, destY], timeToMove));
        }

    }
    IEnumerator MoveRoutine(Tile destination, float timeToMove)
    {
        Transform startTransform = transform;

        bool reachedDestination = false;
        float elapsedTime = 0f;

        m_isMoving = true;

        while (!reachedDestination)
        {
            if (Vector3.Distance(transform.position, destination.transform.position) < 0.01f)
            {
                reachedDestination = true;

                if (m_board != null)
                {
                    m_board.PlaceGamePiece(this, destination.xIndex, destination.yIndex);
                }
                break;
            }

            elapsedTime += Time.deltaTime;

            float t = Mathf.Clamp(elapsedTime / timeToMove, 0f, 1f);
            switch (interpolation)
            {
                case InterpType.Linear:
                    break;
                case InterpType.EaseOut:
                    t = Mathf.Sin(t * Mathf.PI + 0.5f);
                    break;
                case InterpType.EaseIn:
                    t = 1 - Mathf.Cos(t * Mathf.PI * 0.5f);
                    break;
                case InterpType.SmoothStep:
                    t = t * t * (3 - 2 * t);
                    break;
                case InterpType.SmootherStep:
                    t = t * t * t * (t * (t * 6 - 15) + 10);
                    break;
            }


            transform.position = Vector3.Lerp(startTransform.position, destination.transform.position, t);



            yield return null;
        }
        m_isMoving = false;

    }

}
