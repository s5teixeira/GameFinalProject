using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using TMPro;
using System;

public class LanceVrotController : Pawn
{

    [SerializeField] Animator _animation;
    [SerializeField] Rigidbody2D MyRigidBody;
    [SerializeField] float moveScale = 0.5f;
    [SerializeField] SpriteRenderer MySpriteRenderer;
    [SerializeField] Sprite NorthStationary;
    [SerializeField] Sprite EastStationary;
    [SerializeField] Sprite SouthStationary;
    [SerializeField] Sprite WestStationary;
    [SerializeField] GameObject Soul; // Child object that controlls 
    [SerializeField] Camera chaserCamera;
    [SerializeField] UIDocument characterUI;
    [SerializeField] DamageDealer currentWeapon;
   
    // UI controllers
    private VisualElement heartHost;
    private TextMeshPro DebugOutput;
    private List<VisualElement> hearts = new List<VisualElement>();



    //Character control
    private string direction = "south"; // east, south, west, north
    private string prevDirection = "south"; // east, south, west, north

    private bool moving = false;
    



    // Start is called before the first frame update
    void Start()
    {
        //_animation.Play("CharacterWalkup");
        moving = false;
        DebugOutput = Soul.GetComponent<TextMeshPro>();
        Debug.LogWarning(characterUI.rootVisualElement);
        heartHost = characterUI.rootVisualElement.Q<VisualElement>("Hearts");
        health = 3.5f;
        UpdateHealthUI();
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        Debug.Log(collision.gameObject.name);

    }


    // Update is called once per frame
    void Update()
    {
        if (!isDead)
        {
            float xAmount = Input.GetAxis("Horizontal");
            float yAmount = Input.GetAxis("Vertical");

            DebugOutput.SetText("Moving: " + moving + "; " + MyRigidBody.velocity.x + "/" + MyRigidBody.velocity.y + "; " + direction + "; CAN ATTACK: " + currentWeapon.CanAttack()) ;
            ProcessControlls(xAmount, yAmount);

            moving = !((MyRigidBody.velocity.x == 0 && MyRigidBody.velocity.y == 0) && (xAmount == 0 && yAmount == 0));
            prevDirection = direction;

            UpdateMotionState(xAmount, yAmount);

            if (prevDirection != direction || !moving)
            {
                UpdateAnims(xAmount, yAmount);
            }

            chaserCamera.transform.position = new Vector3(transform.position.x, transform.position.y, -10);

            if (Input.GetMouseButtonDown(0))
            {
                currentWeapon.Attack();
            }

        }
    }


    private void UpdateMotionState(float xAmount, float yAmount)
    {
        if (xAmount == 0 && yAmount == 0)
        {
            // No XY input

            if (MyRigidBody.velocity.x < 0.0005 && MyRigidBody.velocity.y < 0.0005)
            {
                // No residual force applied
                moving = false;
            }
            else
            {
                // There is still speed on body
            }
        }
        else
        {
            // XY input present so we are on the move
            moving = true;

            if (Mathf.Abs(xAmount) > Mathf.Abs(yAmount))
            {
                // Horizontal motion is higher than vertical
                direction = (xAmount > 0) ? "east" : "west";
            }
            else
            {
                // Vertical motion is highan than horizontal
                direction = (yAmount > 0) ? "north" : "south";

            }


        }

    }


    


    private void UpdateAnims(float xAmount, float yAmount)
    {

        _animation.enabled = moving;
        currentWeapon.RotateWeapon(transform, direction);

        switch (direction)
        {
            case "east":
                //Debug.Log("Stopped EAST");
                if (moving)
                {
                    MySpriteRenderer.flipX = false;
                    
                    _animation.Play("CharacterWalking");

                }
                else
                {
                    MySpriteRenderer.sprite = NorthStationary;
                }

                break;
            case "west":
                //Debug.Log("Stopped WEST");
                if (moving)
                {
                    MySpriteRenderer.flipX = true;
                    _animation.Play("CharacterWalking");

                }
                else
                {
                    MySpriteRenderer.sprite = WestStationary;

                }

                break;
            case "south":
                //Debug.Log("Stopped SOUTH");
                if (moving)
                {
                    _animation.Play("CharacterWalkdown");

                }
                else
                {
                    MySpriteRenderer.sprite = SouthStationary;
                }
                break;
            case "north":
                //Debug.Log("Stopped SOUTH");
                if (moving)
                {
                    _animation.Play("CharacterWalkup");

                }
                else
                {
                    MySpriteRenderer.sprite = NorthStationary;

                }


                break;
        }




        /*

        if (!moving)
        {

            // We are fully stationary
            // _animation.Play("CharacterIDLE");
            _animation.enabled = false; 
                switch (direction)
                {
                    case "east":
                    //Debug.Log("Stopped EAST");

                        MySpriteRenderer.sprite = NorthStationary;
                        break;
                    case "west":
                    //Debug.Log("Stopped WEST");

                        MySpriteRenderer.sprite = WestStationary;
                        break;
                    case "south":
                    //Debug.Log("Stopped SOUTH");

                    MySpriteRenderer.sprite = SouthStationary;
                        break;
                    case "north":
                    //Debug.Log("Stopped SOUTH");

                    MySpriteRenderer.sprite = NorthStationary;
                        break;
                }

        }
        else
        {
            _animation.enabled = true;

            switch (direction)
            {
               
                case "east":
                     MySpriteRenderer.flipX = false;

                    _animation.Play("CharacterWalking");
                    break;
                case "west":
                    MySpriteRenderer.flipX = true;
                    _animation.Play("CharacterWalking");
                    break;
                case "south":
                    _animation.Play("CharacterWalkdown");
                    break;
                case "north":
                    _animation.Play("CharacterWalkup");
                    break;
            }

        }
        */

    }


    private void ProcessControlls(float xAmount, float yAmount)
    {



        if (Mathf.Abs(xAmount) > 0 || Mathf.Abs(yAmount) > 0)
        {
            //transform.position += new Vector3(moveScale * xAmount * Time.deltaTime, moveScale * yAmount * Time.deltaTime, 0);
            Vector3 newPos = new Vector3(moveScale * xAmount * Time.deltaTime, moveScale * yAmount * Time.deltaTime, 0);

            transform.Translate(newPos);
            //MyRigidBody.velocity  = (new Vector3((moveScale * xAmount) * Time.deltaTime, (moveScale * yAmount) * Time.deltaTime, 0));
        }

    }


   
    private void UpdateHealthUI()
    {

        int fullHearts = (int)Math.Floor(health);
        bool partialDamage = ((health % 1) != 0);


        heartHost.Clear();
        
        for(int i = 0; i < fullHearts; i++)
        {
            VisualElement heart = new VisualElement();
            heart.AddToClassList("heartHealth");
            heart.AddToClassList("heart100");
            heartHost.Add(heart);

        }

        if (partialDamage)
        {
            VisualElement heart = new VisualElement();
            heart.AddToClassList("heartHealth");
            heart.AddToClassList("heart50");
            heartHost.Add(heart);
        }


    }

    override protected void OnDamageTaken()
    {
        UpdateHealthUI();
    }

    override public void Die()
    {
        Debug.LogError("DIED!");
    }

}
