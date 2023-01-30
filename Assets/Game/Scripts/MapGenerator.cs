using System;
using System.Collections.Generic;
using Mirror;
using UnityEngine;
using UnityEngine.Tilemaps;
using Random = System.Random;

namespace Game.Scripts
{
    [RequireComponent(typeof(Tilemap))]
    [RequireComponent(typeof(TilemapCollider2D))]
    public class MapGenerator : NetworkBehaviour
    {
        private enum RoomType
        {
            AllOpenSpawn,
            AllOpenExit,
            LeftRightOpen,
            AllOpen,
            Optional
        }
        
        public Vector2 SpawnPosition { get; private set; }
        
        public delegate void OnRegenerate();
        public event OnRegenerate RegenerateEvent;

        private static readonly Dictionary<RoomType, int> RoomTypePriority = new()
        {
            { RoomType.Optional, 0 },
            { RoomType.LeftRightOpen, 1 },
            { RoomType.AllOpen, 2 },
            { RoomType.AllOpenSpawn, 3 },
            { RoomType.AllOpenExit, 3 },
        };

        [SerializeField] private Tile[] solidTiles;
        [SerializeField] private Tile[] spikeTiles;
        [SerializeField] private GameObject[] enemyPrefabs;
        [SerializeField] private GameObject spikePrefab;
        [SerializeField] private Transform exitTransform;

        private const int MapSize = 10;
        private const int MaxHorizontalMoves = MapSize;
        private readonly RoomType[] _rooms = new RoomType[MapSize * MapSize];
        private Random _random;
        private Tilemap _tilemap;
        private TilemapCollider2D _tilemapCollider;
        private int _lastSeed;
        private List<GameObject> _spawnedPrefabs;

        private void SetRoom(int x, int y, RoomType roomType)
        {
            if (x is < 0 or >= MapSize || y is < 0 or >= MapSize) return;
            var oldRoomType = GetRoom(x, y);
            if (RoomTypePriority[roomType] < RoomTypePriority[oldRoomType]) return;

            _rooms[x + y * MapSize] = roomType;
        }
        
        private RoomType GetRoom(int x, int y)
        {
            if (x is < 0 or >= MapSize || y is < 0 or >= MapSize) return RoomType.Optional;
            return _rooms[x + y * MapSize];
        }

        private void Start()
        {
            _spawnedPrefabs = new List<GameObject>();
            _tilemap = GetComponent<Tilemap>();
            _tilemapCollider = GetComponent<TilemapCollider2D>();
            _random = new Random();

            if (isServer)
            {
                Generate(_random.Next());
            }
            else
            {
                CmdRequestRegenerate();
            }
        }

        private void Generate(int seed)
        {
            _lastSeed = seed;
            ChooseRooms(seed);
            GenerateRooms();
        }

        private T Choose<T>(T[] array)
        {
            return array[_random.Next(array.Length)];
        }

        private void GenerateRooms()
        {
            foreach (var spawnedEnemy in _spawnedPrefabs)
            {
                NetworkServer.Destroy(spawnedEnemy);
            }
            
            _spawnedPrefabs.Clear();

            const int mapWidth = MapSize * Rooms.RoomWidth;
            const int mapHeight = MapSize * Rooms.RoomHeight;

            var borderTile = Choose(solidTiles);
            SetTileRect(-1, -1, mapWidth + 2, 1, borderTile);
            SetTileRect(-1, -1, 1, mapHeight + 2, borderTile);
            SetTileRect(-1, mapHeight, mapWidth + 2, 1, borderTile);
            SetTileRect(mapWidth, -1, 1, mapHeight + 2, borderTile);

            for (var x = 0; x < MapSize; x++)
            {
                for (var y = 0; y < MapSize; y++)
                {
                    var roomTemplate = Choose(GetRoom(x, y) switch
                    {
                        RoomType.LeftRightOpen => Rooms.LeftRightOpen,
                        RoomType.AllOpen => Rooms.AllOpen,
                        RoomType.AllOpenSpawn => Rooms.AllOpenSpawn,
                        RoomType.AllOpenExit => Rooms.AllOpenExit,
                        _ => Rooms.Optional
                    });

                    var roomX = x * Rooms.RoomWidth;
                    var roomY = y * Rooms.RoomHeight;
                    
                    for (var i = 0; i < roomTemplate.Length; i++)
                    {
                        var tileX = i % Rooms.RoomWidth;
                        var tileY = Rooms.RoomHeight - 1 - i / Rooms.RoomWidth;
                        var tileTemplate = roomTemplate[i];
                        var tilePosition = new Vector3Int(roomX + tileX, roomY + tileY, 0);
                        var centeredTilePosition = tilePosition + new Vector3(0.5f, 0.5f);

                        switch (tileTemplate)
                        {
                            case Rooms.Spawn:
                                SpawnPosition = centeredTilePosition;
                                break;
                            case Rooms.Exit:
                                exitTransform.position = centeredTilePosition;
                                break;
                            case Rooms.Spike:
                                if (!isServer) break;
                                SpawnPrefab(centeredTilePosition, spikePrefab);
                                break;
                            case Rooms.Enemy:
                                if (!isServer) break;
                                SpawnPrefab(centeredTilePosition, Choose(enemyPrefabs));
                                break;
                        }
                        
                        var tile = tileTemplate switch
                        {
                            Rooms.Solid => Choose(solidTiles),
                            Rooms.Spike => Choose(spikeTiles),
                            _ => null
                        };
                        
                        _tilemap.SetTile(tilePosition, tile);
                    }
                }
            }

            _tilemapCollider.ProcessTilemapChanges();
            
            foreach (var spawnedEnemy in _spawnedPrefabs)
            {
                spawnedEnemy.SetActive(true);
            }
        }
        
        private void ChooseRooms(int seed)
        {
            _random = new Random(seed);
            Array.Fill(_rooms, RoomType.Optional);
            var x = _random.Next(MapSize);
            var horizontalMoves = 0;
            
            SetRoom(x, MapSize - 1, RoomType.AllOpenSpawn);
            
            for (var y = MapSize - 1; y >= 0;)
            {
                SetRoom(x, y, RoomType.LeftRightOpen);

                var nextMove = _random.Next(11);

                if (horizontalMoves > MaxHorizontalMoves || nextMove == 10 || x < 0 || x >= MapSize)
                {
                    x = Math.Clamp(x, 0, MapSize - 1);
                    SetRoom(x, y, RoomType.AllOpen);
                    y--;
                    SetRoom(x, y, RoomType.AllOpen);
                    horizontalMoves = 0;
                    continue;
                }

                horizontalMoves++;
                
                switch (nextMove)
                {
                    case < 5:
                        x--;
                        continue;
                    default:
                        x++;
                        break;
                }
            }
            
            SetRoom(x, 0, RoomType.AllOpenExit);
        }

        private void SetTileRect(int x, int y, int width, int height, Tile tile)
        {
            for (var xi = 0; xi < width; xi++)
            {
                for (var yi = 0; yi < height; yi++)
                {
                    _tilemap.SetTile(new Vector3Int(x + xi, y + yi, 0), tile);
                }
            }
        }

        private void SpawnPrefab(Vector3 position, GameObject prefab)
        {
            var newPrefab = Instantiate(prefab, position, Quaternion.identity, transform);
            newPrefab.SetActive(false);
            _spawnedPrefabs.Add(newPrefab);
            NetworkServer.Spawn(newPrefab);
        }

        public void Regenerate()
        {
            if (!isServer) return;
            RegenerateEvent?.Invoke();
            Generate(_random.Next());
            RpcRegenerate(_lastSeed);
        }

        [ClientRpc]
        private void RpcRegenerate(int seed)
        {
            if (isServer) return;
            RegenerateEvent?.Invoke();
            Generate(seed);
        }

        [Command(requiresAuthority = false)]
        private void CmdRequestRegenerate(NetworkConnectionToClient sender = null)
        {
            if (sender is null) return;
            TargetRegenerate(sender.identity.connectionToClient, _lastSeed);
        }

        [TargetRpc]
        private void TargetRegenerate(NetworkConnection target, int seed)
        {
            Generate(seed);
        }
    }
}