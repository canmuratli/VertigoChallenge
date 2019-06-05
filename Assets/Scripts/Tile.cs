using UnityEngine;
using System.Collections;
using System.Collections.Generic;



public class Tile : MonoBehaviour
{
    public int xIndex;
    public int yIndex;
    public Color color;
    Board m_board;


    // Start is called before the first frame update
    void Start()
    {
        SwipeManager.OnSwipeDetected += OnSwipeDetected;

    }
    private void Update()
    {

            GetComponent<SpriteRenderer>().color = color;
           

    }
    public void Init(int x, int y, Board board)
    {
        xIndex = x;
        yIndex = y;
        color = GetComponent<SpriteRenderer>().color;
        m_board = board;

    }
    private void OnMouseDown()
    {
        if(m_board != null)
        {
            m_board.ClikTile(this);

        }
    }


    void OnSwipeDetected(Swipe direction, Vector2 swipeVelocity)
    {
       if(direction == Swipe.Left)
        {
            m_board.ReleaseTile(1);
        }
       if(direction == Swipe.Right)
        {
            m_board.ReleaseTile(2);
        }
    }


}
