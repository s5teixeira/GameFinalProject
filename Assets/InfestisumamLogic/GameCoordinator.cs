using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mono.Data.Sqlite;
using System.Data;
using System;
using TMPro;



public class GameCoordinator : MonoBehaviour
{

    [SerializeField]public static string DatabaseName = ".infestisumam.save";
    [SerializeField]public LevelManager levelManager;
    [SerializeField]public CharacterManager charManager;
    [SerializeField] public TMP_Text debugText;
    private bool isTransitioning = false;
    private Level currentLevel;
    private Path currentPath;


    IDbConnection dbcon;
    // Start is called before the first frame update
    void Awake()
    {
        Debug.Log("URI=file:" + Application.persistentDataPath + "/" + DatabaseName);
        dbcon = new SqliteConnection("URI=file:" + Application.persistentDataPath + "/" + DatabaseName);
        dbcon.Open();

        if (dbcon != null && dbcon.State == ConnectionState.Open)
        {
            charManager.SetDatabase(dbcon);
            levelManager.SetDatabase(dbcon);
        }
        else
        {
            Debug.LogError("FAILED TO OPEN DATABASE  " + dbcon.State);
        }


    }

    public bool SaveGameExists()
    {
        IDbCommand dbcmd = dbcon.CreateCommand();
        string q_createTable = "SELECT count(*) FROM sqlite_master WHERE type = 'table' AND name != 'sqlite_sequence';\r\n";
        int tablesExpected = 2;

        dbcmd.CommandText = q_createTable;
        Debug.Log("Listing tables");
        int tableCount = 0;
        using (IDataReader dataReader = dbcmd.ExecuteReader())
        {
            while (dataReader.Read())
            {
                tableCount = (int)dataReader.GetInt32(0);

            }
        }

        Debug.Log(tableCount);

        // clean up
        dbcmd.Dispose();

        return tableCount > 0;
    }

    public void CreateDatabase()
    {
        IDbCommand dbcmd = dbcon.CreateCommand();
        string characterCreate = "CREATE TABLE \"Character\" (\r\n\t\"char_id\"\tINTEGER NOT NULL,\r\n\t\"alive\"\tINTEGER NOT NULL DEFAULT 0,\r\n\t\"pathID\"\tINTEGER NOT NULL,\r\n\t\"lastEntryDirection\"\tTEXT,\r\n\tPRIMARY KEY(\"char_id\" AUTOINCREMENT)\r\n)";
        string pathCreate = "CREATE TABLE \"Path\" (\r\n\t\"path_id\"\tINTEGER NOT NULL,\r\n\t\"level_id\"\tINTEGER NOT NULL,\r\n\t\"xloc\"\tINTEGER NOT NULL,\r\n\t\"yloc\"\tINTEGER NOT NULL,\r\n\t\"zloc\"\tINTEGER NOT NULL,\r\n\tPRIMARY KEY(\"path_id\" AUTOINCREMENT)\r\n)";
        string levelCreate = "CREATE TABLE \"Level\" (\r\n\t\"level_id\"\tINTEGER NOT NULL,\r\n\t\"level_type\"\tTEXT NOT NULL,\r\n\t\"north_door_state\"\tINTEGER NOT NULL,\r\n\t\"east_door_state\"\tINTEGER NOT NULL,\r\n\t\"south_door_state\"\tINTEGER NOT NULL,\r\n\t\"west_door_state\"\tINTEGER NOT NULL,\r\n\tPRIMARY KEY(\"level_id\" AUTOINCREMENT)\r\n)";
        List<string> tables = new List<string>();
        tables.Add(characterCreate);
        tables.Add(pathCreate);
        tables.Add(levelCreate);

        foreach(string tableName in tables)
        {
            dbcmd.CommandText = tableName;
            dbcmd.Prepare();
            dbcmd.ExecuteNonQuery(); 
        }

  
    }





   


    public void LoadLevelData()
    {
        charManager.LoadLastCharacter();
        var levelData = levelManager.ResolvePath(charManager.GetLocation());
        currentLevel = levelData.resLevel;
        currentPath = levelData.resPath;
        Debug.LogWarning("CHARLOC: " + charManager.GetLocation() + "/ " + currentLevel.levelID);
        debugText.text = String.Format("LOCATION: {0}/{1}; LOC ID: {2}", currentPath.locX, currentPath.locY, currentPath.pathID);


    }


    public void InitiateGame()
    {
        charManager.LoadLastCharacter();
        var levelData = levelManager.ResolvePath(charManager.GetLocation());


        Debug.LogWarning(levelData.resLevel.levelID);

        if (levelData.resLevel.levelID == 0)
        {
            if (charManager.GetLocation() == 0)
            {
                var sceneData = levelManager.MakeSceneAtCoord(0, 0, 0, "default");

                if (sceneData.resLevel.levelID != 0)
                {
                    charManager.UpdateLocation(sceneData.resPath.pathID, "center");

                    Debug.LogError("CHAR CR: " + charManager.GetLocation() + "/" + sceneData.resPath.pathID);

                    levelManager.LoadScene(sceneData.resLevel);
                }
                else
                {
                    Debug.LogError("FAILED TO INIT LEVEL");
             
                    Debug.LogError("CHAR CR: " + charManager.GetLocation() + "/" + sceneData.resPath.pathID);

                }

            }
            // Path resolved
        }
        else{
            levelManager.LoadScene(levelData.resLevel);
        }

        currentPath = levelData.resPath;
        currentLevel = levelData.resLevel;

    }


    public void Transition(string direction)
    {
        if (!isTransitioning) // Legacy needs rework 
        {
            isTransitioning = true; // Locck to prevent double calls just in case, the upstream logic changed in way that absoletes usage of this but just in case lock it
            int xModifier = 0;
            int yModifier = 0;
            string newRoomEntryDirection = "";

            switch (direction)
            {
                case "north":
                    yModifier = 1;
                    newRoomEntryDirection = "south";
                    break;

                case "east":
                    xModifier = 1;
                    newRoomEntryDirection = "west";

                    break;

                case "south":
                    yModifier = (-1);
                    newRoomEntryDirection = "north";

                    break;

                case "west":
                    xModifier = (-1);
                    newRoomEntryDirection = "east";
                    break;
            }

            Debug.LogWarning((currentPath.locX + (xModifier)) + " / " + (currentPath.locY + (yModifier)));
           
            int pathIDAtCoords = levelManager.GetPathIDAtCoords(currentPath.locX + (xModifier), currentPath.locY + (yModifier), 0);
            Debug.LogWarning(pathIDAtCoords);
            
           if (pathIDAtCoords == -1)
           {
               var newLevel = levelManager.MakeSceneAtCoord( currentPath.locX + (xModifier), currentPath.locY + (yModifier), 0, direction);
               charManager.UpdateLocation(newLevel.resPath.pathID, newRoomEntryDirection);
               levelManager.LoadScene(newLevel.resLevel);
           }
           else
           {
               var levelAtCoords = levelManager.ResolvePath(pathIDAtCoords);
               charManager.UpdateLocation(pathIDAtCoords, newRoomEntryDirection);
               levelManager.LoadScene(levelAtCoords.resLevel);
           }

               

        }

    }


    // Update is called once per frame
    void Update()
    {

    }

    // GETTERS
    public Level GetCurrentLevel()
    {
        return currentLevel;
    }

    public string GetCharacterSpawn()
    {
        return charManager.GetLastEnteredDirection();
    }

    // SETTERS

    public Path GetCurrentPath()
    {
        return currentPath;
    }



}
