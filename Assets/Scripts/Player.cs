using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class Player : MovingObject
{
    public int wallDamage = 1;
    public int pointsPerFood = 10;
    public int pointsPerSoda = 20;
    public float restartLevelDelay = 1f;
    public Text foodText;
    public AudioClip moveSound1;
    public AudioClip moveSound2;
    public AudioClip eatSound1;
    public AudioClip eatSound2;
    public AudioClip DrinkSound1;
    public AudioClip DrinkSound2;
    public AudioClip gameOverSound;

    private Animator animator;
    private int HP;


    protected override void Start()
    {
        animator = GetComponent<Animator>();
        HP = GameManager.instance.playerFoodPoints;
        foodText.text = "HP: " + HP;
        base.Start();
    }
    private void OnDisable()
    {
        GameManager.instance.playerFoodPoints = HP;
    }

    void Update()
    {
        if (!GameManager.instance.playersTurn) return;
        int horizontal=0;
        int vertical=0;
        horizontal = (int)Input.GetAxisRaw("Horizontal");
        vertical = (int)Input.GetAxisRaw("Vertical");
        if (horizontal != 0) vertical = 0;
        if (horizontal != 0 || vertical != 0) AttemptMove<Wall>(horizontal, vertical);
    }
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.tag == "Exit")
        {
            Invoke("Restart", restartLevelDelay);
            enabled = false;
        }
        else if (other.tag == "Food")
        {
            HP += pointsPerFood;
            foodText.text = "+" + pointsPerFood + "HP: " + HP;
            SoundManager.instance.RandomizeSfx(eatSound1,eatSound2);
            other.gameObject.SetActive(false);
        }
        else if (other.tag == "Soda")
        {
            HP += pointsPerSoda;
            foodText.text = "+" + pointsPerSoda + "HP: " + HP;
            SoundManager.instance.RandomizeSfx(DrinkSound1,DrinkSound2);
            other.gameObject.SetActive(false);
        }
    }
    protected override void AttemptMove<T>(int xDir, int yDir)
    {
        HP--;
        foodText.text = "HP: " + HP;
        base.AttemptMove<T>(xDir, yDir);
        RaycastHit2D hit;
        if (Move(xDir, yDir, out hit)) SoundManager.instance.RandomizeSfx(moveSound1, moveSound2);

        CheckIfGameOver();
        GameManager.instance.playersTurn = false;
    }
    protected override void OnCantMove<T>(T component)
    {
        Wall hitWall = component as Wall;
        hitWall.DamageWall(wallDamage);
        animator.SetTrigger("PlayerChop");
    }
    private void Restart()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex, LoadSceneMode.Single);
        //   Application.LoadLevel(Application.loadedLevel);
    }
    public void LoseHP (int loss)
    {
        animator.SetTrigger("PlayerHit");
        HP -= loss;
        foodText.text = "-" + loss + "HP: " + HP;
        CheckIfGameOver();
    }

    private void CheckIfGameOver()
    {
        if(HP <= 0)
        {
            SoundManager.instance.PlaySingle(gameOverSound);
            SoundManager.instance.music.Stop();
            GameManager.instance.GameOver();
        }
    }
}
