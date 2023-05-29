using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UI;

public class Controller : MonoBehaviour
{
    //GameObjects
    public GameObject board;
    public GameObject[] cops = new GameObject[2];
    public GameObject robber;
    public Text rounds;
    public Text finalMessage;
    public Button playAgainButton;

    //Otras variables
    public Tile[] tiles = new Tile[Constants.NumTiles];
    private int roundCount = 0;
    private int state;
    private int clickedTile = -1;
    private int clickedCop = 0;

    void Start()
    {
        InitTiles();
        InitAdjacencyLists();
        state = Constants.Init;
    }

    //Rellenamos el array de casillas y posicionamos las fichas
    void InitTiles()
    {
        for (int fil = 0; fil < Constants.TilesPerRow; fil++)
        {
            GameObject rowchild = board.transform.GetChild(fil).gameObject;

            for (int col = 0; col < Constants.TilesPerRow; col++)
            {
                GameObject tilechild = rowchild.transform.GetChild(col).gameObject;
                tiles[fil * Constants.TilesPerRow + col] = tilechild.GetComponent<Tile>();
            }
        }

        cops[0].GetComponent<CopMovement>().currentTile = Constants.InitialCop0;
        cops[1].GetComponent<CopMovement>().currentTile = Constants.InitialCop1;
        robber.GetComponent<RobberMovement>().currentTile = Constants.InitialRobber;
    }

    public void InitAdjacencyLists()
    {
        // Matriz de adyacencia
        int[,] matriz = new int[Constants.NumTiles, Constants.NumTiles];

        // Inicializar matriz a 0's
        for (int i = 0; i < Constants.NumTiles; i++)
        {
            for (int j = 0; j < Constants.NumTiles; j++)
            {
                matriz[i, j] = 0;
            }
        }

        // Para cada posición, rellenar con 1's las casillas adyacentes (arriba, abajo, izquierda y derecha)
        for (int i = 0; i < Constants.NumTiles; i++)
        {
            for (int j = 0; j < Constants.NumTiles; j++)
            {
                bool derecha = true;

                for (int d = 0; d < Constants.TilesPerRow; d++)
                {
                    if ((j - (8 * d)) == 0)
                    {
                        derecha = false;
                    }
                }

                bool izquierda = true;

                for (int d = 0; d < Constants.TilesPerRow; d++)
                {
                    if (((j + 1) - (8 * d)) == 0)
                    {
                        izquierda = false;
                    }
                }

                // Comprobamos las casillas adyacentes y marcamos con 1 si son válidas
                if (j == i + 1 && derecha)
                {
                    matriz[i, j] = 1; // Casilla de la derecha
                }
                if (j == i - 1 && izquierda)
                {
                    matriz[i, j] = 1; // Casilla de la izquierda
                }
                if (j == i + 8 && i >= 0 && i < Constants.NumTiles)
                {
                    matriz[i, j] = 1; // Casilla de arriba
                }
                if (j == i - 8 && i >= 0 && i < Constants.NumTiles)
                {
                    matriz[i, j] = 1; // Casilla de abajo
                }
            }
        }

        // Rellenar la lista "adjacency" de cada casilla con los índices de sus casillas adyacentes
        for (int i = 0; i < Constants.NumTiles; i++)
        {
            List<int> adjacency = new List<int>();
            for (int j = 0; j < Constants.NumTiles; j++)
            {

                // Comprobamos las casillas adyacentes y agregamos sus índices a la lista adjacency
                if (matriz[i, j] == 1)
                {
                    adjacency.Add(j);
                }

            }
            tiles[i].adjacency = adjacency;
        }
    }

    //Reseteamos cada casilla: color, padre, distancia y visitada
    public void ResetTiles()
    {
        foreach (Tile tile in tiles)
        {
            tile.Reset();
        }
    }

    public void ClickOnCop(int cop_id)
    {
        switch (state)
        {
            case Constants.Init:
            case Constants.CopSelected:
                clickedCop = cop_id;
                clickedTile = cops[cop_id].GetComponent<CopMovement>().currentTile;
                tiles[clickedTile].current = true;

                ResetTiles();
                FindSelectableTiles(true);

                state = Constants.CopSelected;
                break;
        }
    }

    public void ClickOnTile(int t)
    {
        clickedTile = t;

        switch (state)
        {
            case Constants.CopSelected:
                //Si es una casilla roja, nos movemos
                if (tiles[clickedTile].selectable)
                {
                    cops[clickedCop].GetComponent<CopMovement>().MoveToTile(tiles[clickedTile]);
                    cops[clickedCop].GetComponent<CopMovement>().currentTile = tiles[clickedTile].numTile;
                    tiles[clickedTile].current = true;

                    state = Constants.TileSelected;
                }
                break;
            case Constants.TileSelected:
                state = Constants.Init;
                break;
            case Constants.RobberTurn:
                state = Constants.Init;
                break;
        }
    }

    public void FinishTurn()
    {
        switch (state)
        {
            case Constants.TileSelected:
                ResetTiles();

                state = Constants.RobberTurn;
                RobberTurn();
                break;
            case Constants.RobberTurn:
                ResetTiles();
                IncreaseRoundCount();
                if (roundCount <= Constants.MaxRounds)
                    state = Constants.Init;
                else
                    EndGame(false);
                break;
        }

    }

    public void RobberTurn()
    {
        clickedTile = robber.GetComponent<RobberMovement>().currentTile;
        tiles[clickedTile].current = true;
        FindSelectableTiles(false);

        Tile farest = robberMove();
        robber.GetComponent<RobberMovement>().MoveToTile(farest);
        robber.GetComponent<RobberMovement>().currentTile = farest.numTile;

        //robber.GetComponent<RobberMove>().MoveToTile(tiles[robber.GetComponent<RobberMove>().currentTile]);
    }

    public Tile robberMove()
    {
        Tile farest = null;
        float distance1 = 0;
        float distance2 = 0;
        foreach (Tile t in tiles)
        {
            if (t.selectable)
            {
                float distance1aux = Vector3.Distance(t.transform.position, cops[0].transform.position);
                float distance2aux = Vector3.Distance(t.transform.position, cops[1].transform.position);
                if (distance1 < distance1aux && distance2 < distance2aux)
                {
                    distance1 = distance1aux;
                    distance2 = distance2aux;

                    farest = t;
                }
            }
        }
        return farest;
    }

    public void EndGame(bool end)
    {
        if (end)
            finalMessage.text = "You Win!";
        else
            finalMessage.text = "You Lose!";
        playAgainButton.interactable = true;
        state = Constants.End;
    }

    public void PlayAgain()
    {
        cops[0].GetComponent<CopMovement>().Restart(tiles[Constants.InitialCop0]);
        cops[1].GetComponent<CopMovement>().Restart(tiles[Constants.InitialCop1]);
        robber.GetComponent<RobberMovement>().Restart(tiles[Constants.InitialRobber]);

        ResetTiles();

        playAgainButton.interactable = false;
        finalMessage.text = "";
        roundCount = 0;
        rounds.text = "Rounds: ";

        state = Constants.Restarting;
    }

    public void InitGame()
    {
        state = Constants.Init;

    }

    public void IncreaseRoundCount()
    {
        roundCount++;
        rounds.text = "Rounds: " + roundCount;
    }

    public void FindSelectableTiles(bool cop)
    {

        int indexcurrentTile;

        if (cop == true)
            indexcurrentTile = cops[clickedCop].GetComponent<CopMovement>().currentTile;
        else
            indexcurrentTile = robber.GetComponent<RobberMovement>().currentTile;

        //La ponemos rosa porque acabamos de hacer un reset
        tiles[indexcurrentTile].current = true;

        //Cola para el BFS
        Queue<Tile> nodes = new Queue<Tile>();

        //TODO: Implementar BFS. Los nodos seleccionables los ponemos como selectable=true
        nodes.Enqueue(tiles[indexcurrentTile]);

        while (nodes.Count > 0)
        {
            Tile currentNode = nodes.Dequeue();

            // Marcamos el nodo actual como seleccionable
            currentNode.selectable = true;

            // Si la distancia del nodo actual es menor que la distancia máxima, seguimos recorriendo
            if (currentNode.distance < Constants.Distance)
            {
                // Recorremos los vecinos del nodo actual
                foreach (int neighborIndex in currentNode.adjacency)
                {
                    Tile neighborTile = tiles[neighborIndex];

                    // Si el vecino no ha sido visitado y no es el nodo actual, lo agregamos a la cola
                    if (!neighborTile.visited && neighborTile != currentNode)
                    {
                        neighborTile.visited = true;
                        neighborTile.distance = currentNode.distance + 1;
                        nodes.Enqueue(neighborTile);
                    }
                }
            }
        }
    }
}
