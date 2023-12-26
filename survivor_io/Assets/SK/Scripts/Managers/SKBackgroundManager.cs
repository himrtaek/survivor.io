using System;
using System.Collections.Generic;
using Cysharp.Text;
using JHT.Scripts.Common.PerformanceExtension;
using JHT.Scripts.ResourceManager;
using UnityEngine;

namespace SK
{
    public class SKBackgroundManager
    {
	    public enum SKBackgroundType
	    {
		    InfinityAll,
		    InfinityVertical,
		    Box
	    }
	    
	    private enum TileMoveDirection
	    {
		    Up,
		    Down,
		    Left,
		    Right
	    }
	    
	    private Camera _mainCamera;
	    private Vector3 _mainCameraPrevPosition;
	    private Dictionary<Vector2Int, GameObject> _tileByIndex = new();
	    private Dictionary<Vector2Int, GameObject> _tileByIndexBuffer = new();
	    private Vector2Int _lbIndex;
	    private Vector2Int _rtIndex;
	    private SKBackgroundType _backgroundType;
	    public SKBackgroundType BackgroundType => _backgroundType;
	    public Rect BGRect;
	    public Rect CameraBoundRect;

	    public void Init(Camera mainCamera, uint stageId)
	    {
		    _mainCamera = mainCamera;
		    _mainCameraPrevPosition = _mainCamera.transform.position;

		    _backgroundType = (SKBackgroundType)(stageId - 1 % 3);
		    switch (_backgroundType)
		    {
			    case SKBackgroundType.InfinityAll:
				    CreateInfinityBackgroundImage(stageId, 4, 4);
				    break;
			    case SKBackgroundType.InfinityVertical:
				    CreateInfinityBackgroundImage(stageId, 1, 4);
				    break;
			    case SKBackgroundType.Box:
				    CreateInfinityBackgroundImage(stageId, 1, 1);
				    break;
			    default:
				    throw new ArgumentOutOfRangeException();
		    }

		    BGRect = CalcBGRect();
		    CameraBoundRect = CalcCameraBoundRect();
	    }

	    public Rect CalcBGRect()
	    {
		    switch (BackgroundType)
		    {
			    case SKBackgroundType.InfinityAll:
				    return Rect.MinMaxRect(float.MinValue, float.MinValue, float.MaxValue, float.MaxValue);
			    case SKBackgroundType.InfinityVertical:
				    return Rect.MinMaxRect(-SKConstants.BoxBackgroundWidth / 2, float.MinValue, SKConstants.BoxBackgroundWidth / 2, float.MaxValue);
			    case SKBackgroundType.Box:
				    return Rect.MinMaxRect(-SKConstants.BoxBackgroundWidth / 2, -SKConstants.BoxBackgroundHeight / 2, SKConstants.BoxBackgroundWidth / 2, SKConstants.BoxBackgroundHeight / 2);
			    default:
				    throw new ArgumentOutOfRangeException();
		    }
	    }

	    public Rect CalcCameraBoundRect()
	    {
		    switch (BackgroundType)
		    {
			    case SKBackgroundType.InfinityAll:
				    return Rect.MinMaxRect(
					    float.MinValue, 
					    float.MinValue, 
					    float.MaxValue, 
					    float.MaxValue);
			    case SKBackgroundType.InfinityVertical:
				    return Rect.MinMaxRect(
					    -SKConstants.BackgroundTileWidth / 2 - SKConstants.FiniteBackgroundCameraBuffer,
					    float.MinValue,
					    SKConstants.BackgroundTileWidth / 2 + SKConstants.FiniteBackgroundCameraBuffer,
					    float.MaxValue);
			    case SKBackgroundType.Box:
				    return Rect.MinMaxRect(
					    -SKConstants.BoxBackgroundWidth / 2 - SKConstants.FiniteBackgroundCameraBuffer,
					    -SKConstants.BoxBackgroundHeight / 2 - SKConstants.FiniteBackgroundCameraBuffer,
					    SKConstants.BoxBackgroundWidth / 2 + SKConstants.FiniteBackgroundCameraBuffer,
					    SKConstants.BoxBackgroundHeight / 2 + SKConstants.FiniteBackgroundCameraBuffer);
			    default:
				    throw new ArgumentOutOfRangeException();
		    }
	    }

	    public Vector3 BackgroundClampPosition(Vector3 position)
	    {
		    return position.Clamp(new Vector3(BGRect.xMin + SKConstants.BackgroundClampPositionOffset, BGRect.yMin + SKConstants.BackgroundClampPositionOffset, 0), new Vector3(BGRect.xMax - SKConstants.BackgroundClampPositionOffset, BGRect.yMax - SKConstants.BackgroundClampPositionOffset, 0));
	    }

	    private void CreateInfinityBackgroundImage(uint stageId, int width, int height)
	    {
		    var prefabPath = GetTilePrefabPath(stageId);

		    var startI = (width % 2 != 0 ? (width - 1) : width) / 2 * -1;
		    var startJ = (height % 2 != 0 ? (height - 1) : height) / 2 * -1;
		    var endI = startI * -1;
		    var endJ = startJ * -1;

		    _lbIndex.x = startI;
		    _lbIndex.y = startJ;
		    _rtIndex.x = endI;
		    _rtIndex.y = endJ;
		    
		    for (var i = startI; i <= endI; i++)
		    {
			    for (var j = startJ; j <= endJ; j++)
			    {
				    var position = new Vector3(SKConstants.BackgroundTileWidth * i,
					    SKConstants.BackgroundTileHeight * j, 0);
					    
				    var tile = ResourceManager.Instance.LoadInstance<GameObject>(prefabPath);
				    tile.transform.position = position;

				    _tileByIndex.Add(new Vector2Int(i, j), tile);
			    }
		    }
	    }

	    private string GetTilePrefabPath(uint stageId)
	    {
		    return ZString.Concat("SKMapData/", _backgroundType.ToStringCached(), "Tile");
	    }

	    public void GameLateUpdate(float deltaTime)
	    {
		    var mainCameraPosition = _mainCamera.transform.position;

		    var checkX = false;
		    var checkY = false;
		    switch (_backgroundType)
		    {
			    case SKBackgroundType.InfinityAll:
				    checkX = true;
				    checkY = true;
				    break;
			    case SKBackgroundType.InfinityVertical:
				    checkX = false;
				    checkY = true;
				    break;
			    case SKBackgroundType.Box:
				    checkX = false;
				    checkY = false;
				    break;
			    default:
				    throw new ArgumentOutOfRangeException();
		    }

		    if (checkX)
		    {
			    var indexXPrev = Mathf.RoundToInt(_mainCameraPrevPosition.x / SKConstants.BackgroundTileWidth);
			    var indexX = Mathf.RoundToInt(mainCameraPosition.x / SKConstants.BackgroundTileWidth);
			    if (indexXPrev < indexX)
			    {
				    TileMove(TileMoveDirection.Right, indexX - indexXPrev);
			    }
			    else if (indexX < indexXPrev)
			    {
				    TileMove(TileMoveDirection.Left, indexXPrev - indexX);
			    }
		    }

		    if (checkY)
		    {
			    var indexYPrev = Mathf.RoundToInt(_mainCameraPrevPosition.y / SKConstants.BackgroundTileHeight);
			    var indexY = Mathf.RoundToInt(mainCameraPosition.y / SKConstants.BackgroundTileHeight);
			    if (indexYPrev < indexY)
			    {
				    TileMove(TileMoveDirection.Up, indexY - indexYPrev);
			    }
			    else if (indexY < indexYPrev)
			    {
				    TileMove(TileMoveDirection.Down, indexYPrev - indexY);
			    }
		    }

		    _mainCameraPrevPosition = mainCameraPosition;
	    }

	    private void TileMove(TileMoveDirection tileMoveDirection, int count)
	    {
		    for (int i = 0; i < count; i++)
		    {
			    TileMove(tileMoveDirection);
		    }
	    }

	    private void TileMove(TileMoveDirection tileMoveDirection)
	    {
		    void MoveTileListBuffer()
		    {
			    _tileByIndexBuffer.Clear();
			    
			    foreach (var it in _tileByIndex)
			    {
				    _tileByIndexBuffer.Add(it.Key, it.Value);
			    }
			    
			    _tileByIndex.Clear();
		    }
		    
		    switch (tileMoveDirection)
		    {
			    case TileMoveDirection.Up:
			    {
				    MoveTileListBuffer();
				    foreach (var it in _tileByIndexBuffer)
				    {
					    if (it.Key.y == _lbIndex.y)
					    {
						    var newKey = it.Key;
						    newKey.y = _rtIndex.y + 1;

						    var newPosition = it.Value.transform.position;
						    newPosition.y = SKConstants.BackgroundTileHeight * newKey.y;
						    it.Value.transform.position = newPosition;
						    
						    _tileByIndex.Add(newKey, it.Value);
					    }
					    else
					    {
						    _tileByIndex.Add(it.Key, it.Value);
					    }
				    }

				    _lbIndex.y++;
				    _rtIndex.y++;
			    }
				    break;
			    case TileMoveDirection.Down:
			    {
				    MoveTileListBuffer();
				    foreach (var it in _tileByIndexBuffer)
				    {
					    if (it.Key.y == _rtIndex.y)
					    {
						    var newKey = it.Key;
						    newKey.y = _lbIndex.y - 1;

						    var newPosition = it.Value.transform.position;
						    newPosition.y = SKConstants.BackgroundTileHeight * newKey.y;
						    it.Value.transform.position = newPosition;
						    
						    _tileByIndex.Add(newKey, it.Value);
					    }
					    else
					    {
						    _tileByIndex.Add(it.Key, it.Value);
					    }
				    }

				    _lbIndex.y--;
				    _rtIndex.y--;
			    }
				    break;
			    case TileMoveDirection.Left:
			    {
				    MoveTileListBuffer();
				    foreach (var it in _tileByIndexBuffer)
				    {
					    if (it.Key.x == _rtIndex.x)
					    {
						    var newKey = it.Key;
						    newKey.x = _lbIndex.x - 1;

						    var newPosition = it.Value.transform.position;
						    newPosition.x = SKConstants.BackgroundTileWidth * newKey.x;
						    it.Value.transform.position = newPosition;
						    
						    _tileByIndex.Add(newKey, it.Value);
					    }
					    else
					    {
						    _tileByIndex.Add(it.Key, it.Value);
					    }
				    }

				    _lbIndex.x--;
				    _rtIndex.x--;
			    }
				    break;
			    case TileMoveDirection.Right:
			    {
				    MoveTileListBuffer();
				    foreach (var it in _tileByIndexBuffer)
				    {
					    if (it.Key.x == _lbIndex.x)
					    {
						    var newKey = it.Key;
						    newKey.x = _rtIndex.x + 1;

						    var newPosition = it.Value.transform.position;
						    newPosition.x = SKConstants.BackgroundTileWidth * newKey.x;
						    it.Value.transform.position = newPosition;
						    
						    _tileByIndex.Add(newKey, it.Value);
					    }
					    else
					    {
						    _tileByIndex.Add(it.Key, it.Value);
					    }
				    }

				    _lbIndex.x++;
				    _rtIndex.x++;
			    }
				    break;
			    default:
				    throw new ArgumentOutOfRangeException(nameof(tileMoveDirection), tileMoveDirection, null);
		    }
	    }
    }
}
