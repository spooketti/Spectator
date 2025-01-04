using Godot;
using System;
using System.Collections.Generic;
using System.IO.IsolatedStorage;
public partial class DungeonGenerator : Node
{
    //if you read the old comments i was writing over some heartbreak stuff
    //im going to get rid of those momentos with the new year
    //it's time to keep going on code code code 
    // Called when the node enters the scene tree for the first time.

    private List<Room> possibleVisitStack = new List<Room>();
    private Node3D player;
    private Random random = new Random();
    private static int dungeonWidth = 5;
    private static int dungeonHeight = 5;

    private int minimumRoomCount = 20; //absolute minimum = 2 (boss/exit and spawn)
    private int totalRoomCount = 0;
    private bool spawnedBossRoom = false;

    private Room[,] dungeon = new Room[dungeonWidth, dungeonHeight];
    private PackedScene spawnRoom = GD.Load<PackedScene>("res://Assets/Rooms/spawn.tscn");
    private static PackedScene fourWay = GD.Load<PackedScene>("res://Assets/Rooms/4_way.tscn");
    private static PackedScene bossRoom = GD.Load<PackedScene>("res://Assets/Rooms/boss.tscn");
    public static int tileX = 30;
    public static int tileZ = 30; //this is 1/2 the size and is to be used as a scale factor
                                  //eg 3 from the right in the matrix is 3*15=45 in global coordinates

    //rotations are counter clockwise

    private const int globalNorth = 180;
    private const int globalEast = 90;
    private const int globalSouth = 0;
    private const int globalWest = 270;

    class Room
    {
        public int x;
        public int z;
        public int rotation;
        public bool N;
        public bool E;
        public bool S;
        public bool W;
        public PackedScene roomType;
        public bool isNutty; //it's not a one by one
        public Room(int x, int z, int rotation, bool N, bool E, bool S, bool W, PackedScene roomType, bool isNutty)
        {
            this.x = x;
            this.z = z;
            this.rotation = rotation;
            this.N = N;
            this.E = E;
            this.S = S;
            this.W = W;
            this.roomType = roomType;
            this.isNutty = isNutty;
        }
    }

    public override void _Ready()
    {
        player = (Node3D)GetParent().GetNode("Player");
        StartRoom();
        // Node3D roomModel = spawnRoom.Instantiate<Node3D>();
        // AddChild(roomModel);
        // roomModel.GlobalPosition = new Vector3(50, 0, 50);
        // roomModel.Rotate(Vector3.Up, Mathf.DegToRad(90));
    }

    private void StartRoom()
    {
        int startX = random.Next(dungeonWidth);
        int startZ = random.Next(dungeonHeight);
        int randomRotation = random.Next(4) * 90;
        int nextX = startX;
        int nextZ = startZ;
        //startRoom points south by default
        switch (randomRotation)
        {
            case globalSouth:
                nextZ++;
                break;

            case globalEast:
                nextX++;
                break;

            case globalNorth:
                nextZ--;
                break;

            case globalWest:
                nextX--;
                break;
        }

        if (nextX < 0 || nextX == dungeonWidth || nextZ < 0 || nextZ == dungeonWidth)
        {
            StartRoom();
            return;
        }
        totalRoomCount++;
        Room roomToSpawn = new Room(startX, startZ, randomRotation, randomRotation == 180, randomRotation == 270, randomRotation == 0, randomRotation == 90, spawnRoom, true);
        dungeon[startZ, startX] = roomToSpawn;
        Node3D roomModel = roomToSpawn.roomType.Instantiate<Node3D>();
        AddChild(roomModel);
        int globalX = (roomToSpawn.x * tileX) + (tileX / 2);
        int globalZ = (roomToSpawn.z * tileZ) + (tileZ / 2);
        roomModel.GlobalPosition = new Vector3(globalX, 0, globalZ);
        roomModel.Rotate(Vector3.Up, Mathf.DegToRad(randomRotation));
        player.GlobalPosition = new Vector3(globalX, 200, globalZ);
        OneByOne(new Room(nextX, nextZ, random.Next(4) * 90, randomRotation == globalSouth, randomRotation == globalWest, randomRotation == globalNorth, randomRotation == globalEast, fourWay, false));
    }

    private bool roomBoundValidation(int x, int z)
    {
        if (x >= 0 && x < dungeonWidth && z >= 0 && z < dungeonHeight)
        {
            return dungeon[z, x] == null;
        }
        return false;
    }

    private void OneByOne(Room room)
    {
        GD.Print(totalRoomCount);
        var directions = new[]
        {
        new { Direction = "North", CheckX = room.x, CheckZ = room.z - 1, forcedConnection = room.N, opposite = room.S },
        new { Direction = "East", CheckX = room.x + 1, CheckZ = room.z, forcedConnection = room.E,opposite = room.W },
        new { Direction = "South", CheckX = room.x, CheckZ = room.z + 1, forcedConnection = room.S,opposite = room.N },
        new { Direction = "West", CheckX = room.x - 1, CheckZ = room.z, forcedConnection = room.W ,opposite = room.E}
        };

        dungeon[room.z, room.x] = room;
        int doorCount = 0;
        totalRoomCount++;
        bool boundPotential = false;
        bool isBoss = false;
        if (totalRoomCount == minimumRoomCount)
        {
            room.roomType = bossRoom;
            BossRoom(room);
            return;
        }
        foreach (var dir in directions)
        {
            if (roomBoundValidation(dir.CheckX, dir.CheckZ))
            {
                boundPotential = true;
                bool isDoor = random.Next(2) == 0;
                if (!isDoor)
                {
                    continue;
                }
                // Update the corresponding direction
                switch (dir.Direction)
                {
                    case "North":
                        if (!room.N)
                        {
                            doorCount++;
                            dungeon[room.z, room.x].N = true;
                            OneByOne(new Room(dir.CheckX, dir.CheckZ, 0, dir.Direction == "South", dir.Direction == "West", dir.Direction == "North", dir.Direction == "East", fourWay, false));
                        }
                        break;
                    case "East":
                        if (!room.E)
                        {
                            doorCount++;
                            dungeon[room.z, room.x].E = true;
                            OneByOne(new Room(dir.CheckX, dir.CheckZ, 0, dir.Direction == "South", dir.Direction == "West", dir.Direction == "North", dir.Direction == "East", fourWay, false));
                        }
                        break;
                    case "South":
                        if (!room.S)
                        {
                            doorCount++;
                            dungeon[room.z, room.x].S = true;
                            OneByOne(new Room(dir.CheckX, dir.CheckZ, 0, dir.Direction == "South", dir.Direction == "West", dir.Direction == "North", dir.Direction == "East", fourWay, false));
                        }
                        break;
                    case "West":
                        if (!room.W)
                        {
                            doorCount++;
                            dungeon[room.z, room.x].W = true;
                            OneByOne(new Room(dir.CheckX, dir.CheckZ, 0, dir.Direction == "South", dir.Direction == "West", dir.Direction == "North", dir.Direction == "East", fourWay, false));
                        }
                        break;
                }
            }
        }

        if (boundPotential && doorCount < 3 && !isBoss)
        {
            possibleVisitStack.Add(room);
        }
        if (doorCount == 0 && !isBoss)
        {
            if (totalRoomCount < minimumRoomCount)
            {
                if (boundPotential) //the room had potential to spawn something 
                {
                    if (doorCount < 3)
                    {
                        possibleVisitStack.Remove(possibleVisitStack[possibleVisitStack.Count - 1]);
                    }
                    totalRoomCount--;
                    OneByOne(room);
                    return;
                }
                if (possibleVisitStack.Count > 0)
                {
                    //if this is somehow fails we are screwed
                    Room stackedRoom = possibleVisitStack[possibleVisitStack.Count - 1];
                    possibleVisitStack.Remove(possibleVisitStack[possibleVisitStack.Count - 1]);
                    OneByOne(stackedRoom);
                    return;
                }
            }
        }


        Node3D roomModel = dungeon[room.z, room.x].roomType.Instantiate<Node3D>();
        AddChild(roomModel);
        foreach (var dir in directions)
        {
            switch (dir.Direction)
            {
                case "North":
                    if (dungeon[room.z, room.x].N)
                    {
                        roomModel.GetNode(dir.Direction).QueueFree();
                    }
                    break;
                case "East":
                    if (dungeon[room.z, room.x].E)
                    {
                        roomModel.GetNode(dir.Direction).QueueFree();
                    }
                    break;
                case "South":
                    if (dungeon[room.z, room.x].S)
                    {
                        roomModel.GetNode(dir.Direction).QueueFree();
                    }
                    break;
                case "West":
                    if (dungeon[room.z, room.x].W)
                    {
                        roomModel.GetNode(dir.Direction).QueueFree();
                    }
                    break;
            }
        }

        int globalX = (room.x * tileX) + (tileX / 2);
        int globalZ = (room.z * tileZ) + (tileZ / 2);
        roomModel.GlobalPosition = new Vector3(globalX, 0, globalZ);
        // roomModel.Rotate(Vector3.Up, Mathf.DegToRad(room.rotation));
    }

    private void BossRoom(Room room)
    {
        dungeon[room.z, room.x] = room;
        Node3D roomModel = bossRoom.Instantiate<Node3D>();
        AddChild(roomModel);
        int globalX = (room.x * tileX) + (tileX / 2);
        int globalZ = (room.z * tileZ) + (tileZ / 2);
        roomModel.GlobalPosition = new Vector3(globalX, 0, globalZ);
        var directionRotations = new Dictionary<string, int>
        {
            { "N", globalNorth },
            { "E", globalEast },
            {"S", globalSouth },
            { "W", globalWest }
        };
        foreach (var direction in directionRotations)
        {
            if ((bool)dungeon[room.z, room.x].GetType().GetField(direction.Key).GetValue(dungeon[room.z, room.x]))
            {
                roomModel.Rotate(Vector3.Up, Mathf.DegToRad(direction.Value));
                break;
            }
        }
    }

    private void TShape()
    {

    }


    public override void _Process(double delta)
    {

    }
}