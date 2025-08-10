using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class Card : MonoBehaviour
{
    public int cardId = 0;
    private bool isFlipped ;
    public Image cardImage;
    public gameManagerCard gameManager;
    //public 


    // Start is called before the first frame update
    void Start()
    {
        isFlipped = false;
        cardImage.sprite = gameManagerCard.Instance.cardback;
        StartCoroutine(ShowatOnce());
    }

    IEnumerator ShowatOnce()
    {
        cardImage.sprite = gameManager.cardfaces[cardId];
        yield return new WaitForSeconds(0.75f);
        
        HideCards();

    }


    public void FlipAnim()
    {
        isFlipped = !isFlipped;
        transform.DORotate(new(0, isFlipped ? 0f : 180f,0), 0.25f);
    }

    public void Flippedcard()
    {
        if (!isFlipped & gameManager.firstcard == null || gameManager.secondCard==null)
        {
            Debug.Log("point initially : ");
            isFlipped = true;
            FlipAnim();
            cardImage.sprite = gameManager.cardfaces[cardId];
            gameManager.CardFlipped(this);
        }
    }

    public void HideCards()
    {
        cardImage.sprite = gameManager.cardback;
        isFlipped = false;
        FlipAnim();
        Debug.Log("point 8 : ");
    }




    // Update is called once per frame
    void Update()
    {
        
    }
}
