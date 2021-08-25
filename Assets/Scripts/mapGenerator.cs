using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class mapGenerator : MonoBehaviour {
  public GameObject[] Room = new GameObject[5];
  public int TileSize = 12;
  public int MapSize = 5;
  int MapCount = 0;
  List<List<int>> ExitRoom = new List<List<int>>();
  List<List<int>> IndexRoomByExits = new List<List<int>>();

  List<Vector3> TilesFilled = new List<Vector3>();
  List<Vector3> TilesToFill = new List<Vector3>();
  List<List<int>> EntriesToAdd = new List<List<int>>();

  void Start() {
    ExitRoom.Add(new List<int>() { 0 });
    ExitRoom.Add(new List<int>() { 0, 180 });
    ExitRoom.Add(new List<int>() { 90, 180 });
    ExitRoom.Add(new List<int>() { 90, 180, 270 });
    ExitRoom.Add(new List<int>() { 0, 90, 180, 270 });

    AddTile(0, new Vector3(0f, 0f, 0f), Random.Range(0, 3) * 90);
    GenerateMap();
    CompleteMap();
  }

  void GenerateMap() {
    if (MapCount < MapSize) {
      //Debug.Log("Add a room at " + TilesToFill[0]);
      MapCount++;
      int IndexRoomToAdd = RoomPicker(0);
      Vector3 RoomPosition = TilesToFill[0];
      int RotationRoomToAdd = RotationCalculator(0, IndexRoomToAdd);
      //Debug.Log("Let's add a room" + IndexRoomToAdd + " at " + TilesToFill[0] + " and rotate it to " + RotationRoomToAdd);
      TilesToFill.RemoveAt(0);
      EntriesToAdd.RemoveAt(0);
      AddTile(IndexRoomToAdd, RoomPosition, RotationRoomToAdd);
      GenerateMap();
    }
  }

  void AddTile(int RoomIndex, Vector3 RoomPosition, int RoomRotation) {
    //Debug.Log("The new room will be a room" + RoomIndex);
    GameObject Tile = Instantiate(Room[RoomIndex], RoomPosition, Quaternion.Euler(0f, RoomRotation, 0f));
    TilesFilled.Add(RoomPosition);
    AddExits(RoomIndex, RoomPosition, RoomRotation);
  }

  void AddExits(int RoomIndex, Vector3 RoomPosition, int RoomRotation) {
    //Debug.Log("There is a room" + RoomIndex + " at " + RoomPosition + " turned at " + RoomRotation + "°");
    //Debug.Log("This room has " + ExitRoom[RoomIndex].Count + " exit(s) so there must be:");
    for (int i = 0; i < ExitRoom[RoomIndex].Count; i++) {
      int ExitAngle = (ExitRoom[RoomIndex][i] + RoomRotation + 360) % 360;
      Vector3 NewTileToAdd = AngleToTranslation(ExitAngle, RoomPosition);
      if (!TilesFilled.Contains(NewTileToAdd)) {
        //Debug.Log("One tile at " + NewTileToAdd + " with an exit at " + (ExitAngle + 180) % 360);
        if (TilesToFill.IndexOf(NewTileToAdd) == -1) {
          TilesToFill.Add(NewTileToAdd);
          EntriesToAdd.Add(new List<int> { (ExitAngle + 180) % 360 });
        } else {
          EntriesToAdd[TilesToFill.IndexOf(NewTileToAdd)].Add((ExitAngle + 180) % 360);
        }
      }
    }
  }

  Vector3 AngleToTranslation(int Angle, Vector3 PositionToIncrement) {
    Vector3 NextPosition = PositionToIncrement;
    if (Angle == 0) {
      NextPosition += new Vector3(0f, 0f, -TileSize);
    } else if (Angle == 90) {
      NextPosition += new Vector3(-TileSize, 0f, 0f);
    } else if (Angle == 180) {
      NextPosition += new Vector3(0f, 0f, TileSize);
    } else if (Angle == 270) {
      NextPosition += new Vector3(TileSize, 0f, 0f);
    }
    return NextPosition;
  }

  List<int> KnowBlockedExits(int indexToCheck) {
    List<int> KnownBlockedExits = new List<int>();
    for (var i = 0; i < 4; i++) {
      if (!EntriesToAdd[indexToCheck].Contains(i * 90)) {
        //Debug.Log("Will check if there is something at " + AngleToTranslation(i * 90, TilesToFill[indexToCheck]));
        if (TilesFilled.Contains(AngleToTranslation(i * 90, TilesToFill[indexToCheck]))) {
          //Debug.Log("There is a room this way: " + i * 90);
          KnownBlockedExits.Add(i * 90);
        }
      }
    }
    return KnownBlockedExits;
  }

  int RoomPicker(int i) {
    List<int> NextPickChoices = new List<int>();
    List<int> BlockedExits = KnowBlockedExits(i);
    int minExit = EntriesToAdd[i].Count;
    int maxExit = 4 - BlockedExits.Count;
    //Debug.Log("The room must have minimum " + minExit + " exit(s) and maximum " + maxExit + " exit(s)");
    for (var j = 0; j < ExitRoom.Count; j++) {
      if (ExitRoom[j].Count >= minExit && ExitRoom[j].Count <= maxExit) {
        NextPickChoices.Add(j);
        //Debug.Log("The room" + j + " can work");
      }
      if (NextPickChoices.Count > 1 && NextPickChoices.IndexOf(0) != -1) {
        NextPickChoices.Remove(0);
      }
      if (minExit == 2) {
        if ((EntriesToAdd[i][0] + EntriesToAdd[i][1]) % 180 == 90) {
          NextPickChoices.Remove(1);
        } else if ((EntriesToAdd[i][0] + EntriesToAdd[i][1]) % 180 == 0) {
          NextPickChoices.Remove(2);
        }
      } else if (minExit == 1 && maxExit == 2) {
        if ((BlockedExits[0] + BlockedExits[1]) % 180 == 90) {
          NextPickChoices.Remove(1);
        } else if ((BlockedExits[0] + BlockedExits[1]) % 180 == 0) {
          NextPickChoices.Remove(2);
        }
      } else if (minExit == 1 && maxExit == 3) {
        if ((BlockedExits[0] + EntriesToAdd[i][0]) % 180 == 0) {
          NextPickChoices.Remove(1);
        } else {
          NextPickChoices.Remove(2);
        }
      }
    }
    return NextPickChoices[Random.Range(0, NextPickChoices.Count)];
  }

  int RotationCalculator(int i, int indexRoom) {
    List<int> PossibleRotations = new List<int>();
    for (int r = 0; r < 4; r++) {
      int ExitsFilled = 0;
      int BlockedExit = 0;
      //Debug.Log("Test rotate room" + indexRoom + " at " + TilesToFill[i] + " to " + (r * 90));
      for (int c = 0; c < ExitRoom[indexRoom].Count; c++) {
        int entranceToCheck = (ExitRoom[indexRoom][c] + (90 * r)) % 360;
        //Debug.Log("Check if the entrance at " + entranceToCheck + " matches the needed:");
        if (EntriesToAdd[i].Contains(entranceToCheck)) {
          ExitsFilled++;
          //Debug.Log("YES, ExitsFilled now = " + ExitsFilled + "/" + EntriesToAdd[i].Count + " entries to add.");
        } //else { Debug.Log("NO, ExitsFilled now = " + ExitsFilled + "/" + EntriesToAdd[i].Count + " entries to add."); }
        //Debug.Log("Let's check if there is a wrong exit :");
        if (KnowBlockedExits(i).Contains(entranceToCheck)) {
          //Debug.Log("YES, BlockedExit now = " + BlockedExit);
          BlockedExit++;
        } //else { Debug.Log("NO, BlockedExit now = " + BlockedExit); }
      }
      //Debug.Log("If ExitsFilled (" + ExitsFilled + ") = " + EntriesToAdd[i].Count + " entries to add, The list must contain a new angle (" + r * 90 + ")");
      if (ExitsFilled == EntriesToAdd[i].Count && BlockedExit == 0) {
        PossibleRotations.Add(r * 90);
        //Debug.Log("It is the case, " + (r * 90) + " has been added.");
      } //else { Debug.Log("Nope, it's okay, " + (r * 90) + " was wrong"); }
    }
    //TEST
    //Debug.Log("Does it work? PossibleRotations.Count = " + PossibleRotations.Count);
    //for (int k = 0; k < PossibleRotations.Count; k++) {
    //  Debug.Log("Possible rotation: " + PossibleRotations[k]);
    //}
    return PossibleRotations[Random.Range(0, PossibleRotations.Count)];
  }

  void CompleteMap() {
    for (var i = 0; i < TilesToFill.Count; i++) {
      //Debug.Log("A final tile here: " + TilesToFill[i] + " with " + EntriesToAdd[i].Count + " entries to add");
      int LastRoomIndex = 0;
      if (EntriesToAdd[i].Count == 1) {
        LastRoomIndex = 0;
      } else if (EntriesToAdd[i].Count == 2) {
        if ((EntriesToAdd[i][0] + EntriesToAdd[i][1]) % 180 == 0) {
          LastRoomIndex = 1;
        } else if ((EntriesToAdd[i][0] + EntriesToAdd[i][1]) % 180 == 90) {
          LastRoomIndex = 2;
        }
      } else if (EntriesToAdd[i].Count == 3) {
        LastRoomIndex = 3;
      } else if (EntriesToAdd[i].Count == 4) {
        LastRoomIndex = 4;
      }
      int LastRoomRotation = RotationCalculator(i, LastRoomIndex);
      //Debug.Log("It will be a room" + LastRoomIndex + " and will be rotate by " + LastRoomRotation);
      AddTile(LastRoomIndex, TilesToFill[i], LastRoomRotation);
    }
  }
}
