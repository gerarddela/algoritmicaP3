using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RobberMovement : Movement
{
    public GameObject controller;

    public bool isFinishing = false;
    public bool isRestarting = false;

    private float finishX = 0;
    private float finishZ = 0;

    public void Restart(Tile tile)
    {
        currentTile = tile.numTile;
        MoveToTile(tile);
        isFinishing = false;
        moveSpeed = Constants.MoveSpeed;
        isRestarting = true;
    }

    private void Update()
    {
        if (moving)
        {
            Move();
        }
        if (isFinishing)
        {
            Finish();
        }
    }

    private void Move()
    {
        if (path.Count > 0)
        {
            DoMove();
        }
        else
        {
            moving = false;
            controller.GetComponent<Controller>().FinishTurn();

            if (isRestarting)
            {
                transform.rotation = Quaternion.identity;
                isRestarting = false;
                controller.GetComponent<Controller>().InitGame();
            }
        }
    }

    private void Finish()
    {
        Vector3 target = new Vector3(finishX, 2.5f, finishZ);

        if ((Mathf.Abs(transform.position.x - target.x) >= 0.05f) || (Mathf.Abs(transform.position.z - target.z) >= 0.05f))
        {
            CalculateHeading(target);
            SetVelocity();

            transform.forward = heading;
            transform.position += velocity * Time.deltaTime * 2;
        }
        else
        {
            transform.position = target;
            Physics.gravity = new Vector3(0, -1f, 0);
            isFinishing = false;
            moving = false;
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag == "Cop")
        {
            isFinishing = true;
            moveSpeed = Constants.EndSpeed;

            float vertical = collision.gameObject.GetComponent<CopMovement>().transform.position.x - transform.position.x;
            float horizontal = collision.gameObject.GetComponent<CopMovement>().transform.position.z - transform.position.z;

            if (Mathf.Abs(vertical) < Mathf.Abs(horizontal))
            {
                if (horizontal > 0)
                {
                    finishX = transform.position.x;
                    finishZ = transform.position.z - 15f;
                }
                else
                {
                    finishX = transform.position.x;
                    finishZ = transform.position.z + 15f;
                }
            }
            else
            {
                if (vertical > 0)
                {
                    finishX = transform.position.x - 15f;
                    finishZ = transform.position.z;
                }
                else
                {
                    finishX = transform.position.x + 15f;
                    finishZ = transform.position.z;
                }
            }
            controller.GetComponent<Controller>().EndGame(true);
        }
    }
}