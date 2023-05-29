using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CopMovement : Movement
{    
    public GameObject controller;
                                    
    void Update()
    {       
        if(moving)
        {            
            Move();                     
        }
    }

    public void Restart(Tile t)
    {
        currentTile = t.numTile;
        MoveToTile(t);
    }

    private void OnMouseDown()
    {        
        controller.GetComponent<Controller>().ClickOnCop(id);        
    }
           
    public void Move()
    {
        //Si aún nos quedan casillas a las que desplazarnos
        if(path.Count > 0)
        {
            DoMove();
        }
        //Ya hemos llegado a la casilla destino
        else
        {
            moving = false;
            controller.GetComponent<Controller>().FinishTurn();
        }
    }
}