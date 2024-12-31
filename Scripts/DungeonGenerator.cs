using Godot;
using System;
using System.Collections.Generic;
using System.Reflection.Metadata;
using System.Runtime.CompilerServices;
using System.Runtime.ConstrainedExecution;

public partial class DungeonGenerator : Node
{
	//if you read the old comments i was writing over some heartbreak stuff
	//im going to get rid of those momentos with the new year
	//it's time to keep going on code code code 
    // Called when the node enters the scene tree for the first time.
    private Random random = new Random();
    private static int dungeonWidth = 5;
    private static int dungeonHeight = 5;

    private int minimumRoomCount = 10; //absolute minimum = 2 (boss/exit and spawn)

    private Room[,] dungeon = new Room[dungeonWidth, dungeonHeight];
    private PackedScene spawnRoom = GD.Load<PackedScene>("res://Assets/Rooms/spawn.tscn");
    private static PackedScene fourWay = GD.Load<PackedScene>("res://Assets/Rooms/4_way.tscn");
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
            case 0:
                nextZ++;
                break;

            case 90:
                nextX++;
                break;

            case 180:
                nextZ--;
                break;

            case 270:
                nextX--;
                break;
        }

        if (nextX < 0 || nextX == dungeonWidth || nextZ < 0 || nextZ == dungeonWidth)
        {
            StartRoom();
            return;
        }
        Room roomToSpawn = new Room(startX, startZ, randomRotation, randomRotation == 180, randomRotation == 270, randomRotation == 0, randomRotation == 90, spawnRoom, true);
        dungeon[startZ, startX] = roomToSpawn;
        Node3D roomModel = roomToSpawn.roomType.Instantiate<Node3D>();
        AddChild(roomModel);
        int globalX = (roomToSpawn.x * tileX) + (tileX / 2);
        int globalZ = (roomToSpawn.z * tileZ) + (tileZ / 2);
        roomModel.GlobalPosition = new Vector3(globalX, 0, globalZ);
        roomModel.Rotate(Vector3.Up, Mathf.DegToRad(randomRotation));
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
        var directions = new[]
        {
        new { Direction = "N", CheckX = room.x, CheckZ = room.z - 1 },
        new { Direction = "E", CheckX = room.x + 1, CheckZ = room.z },
        new { Direction = "S", CheckX = room.x, CheckZ = room.z + 1 },
        new { Direction = "W", CheckX = room.x - 1, CheckZ = room.z }
        };

        dungeon[room.z, room.x] = room;

        foreach (var dir in directions)
        {
            if (roomBoundValidation(dir.CheckX, dir.CheckZ))
            {
                // Update the corresponding direction
                switch (dir.Direction)
                {
                    case "N":
                        if (!room.N) 
                        {
                            int answer = random.Next(2);
                            GD.Print(answer);
                            GD.Print(answer==0);
                            dungeon[room.z, room.x].N = random.Next(2) == 0;
                        }
                        if (dungeon[room.z, room.x].N)
                        {
                            finalonbeyone(new Room(room.x, room.z - 1, 0, false, false, false, false, fourWay, false));
                        }
                        break;
                    case "E":
                        if (!room.E) dungeon[room.z, room.x].E = random.Next(2) == 0;
                        if (dungeon[room.z, room.x].E)
                        {
                            finalonbeyone(new Room(room.x+1, room.z, 0, false, false, false, false, fourWay, false));
                        }
                        break;
                    case "S":
                        if (!room.S) dungeon[room.z, room.x].S = random.Next(2) == 0;
                        if (dungeon[room.z, room.x].S)
                        {
                            finalonbeyone(new Room(room.x, room.z + 1, 0, false, false, false, false, fourWay, false));
                        }
                        break;
                    case "W":
                        if (!room.W) dungeon[room.z, room.x].W = random.Next(2) == 0;
                        if (dungeon[room.z, room.x].W)
                        {
                            finalonbeyone(new Room(room.x-1, room.z , 0, false, false, false, false, fourWay, false));
                        }
                        break;
                }
            }
        }

        // Room nextRoom = new Room(0,0,0,false,false,false,false,fourWay,false);

        Node3D roomModel = room.roomType.Instantiate<Node3D>();
        AddChild(roomModel);
        int globalX = (room.x * tileX) + (tileX / 2);
        int globalZ = (room.z * tileZ) + (tileZ / 2);
        roomModel.GlobalPosition = new Vector3(globalX, 0, globalZ);
        roomModel.Rotate(Vector3.Up, Mathf.DegToRad(room.rotation));
    }

    private void finalonbeyone(Room room)
    {
        Node3D roomModel = room.roomType.Instantiate<Node3D>();
        AddChild(roomModel);
        int globalX = (room.x * tileX) + (tileX / 2);
        int globalZ = (room.z * tileZ) + (tileZ / 2);
        roomModel.GlobalPosition = new Vector3(globalX, 0, globalZ);
        roomModel.Rotate(Vector3.Up, Mathf.DegToRad(room.rotation));
    }

    private void TShape()
    {

    }


    public override void _Process(double delta)
    {

    }
}