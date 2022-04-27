using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System;

// This manager handle the scroll list by recycling content and having a coverage area 
// I am aware this code is not really optimised nor well written, but I'm running out of time !
public class ThumbnailManager : MonoBehaviour {

    #region Public parametring
    public int columns = 5;
    public float recycleThreshold = 0.2f;
    public float minCoverage = 1.5f;
    #endregion

    #region Components dependences 
    // This component should be able to get them itself, but I'm running out of time 
    public Transform container;
	public GameObject prefab;
    public ScrollRect scrollRect;
    #endregion

    #region Intern variables
    // Just for the pool handling and some viewports constants
    private List<RectTransform> _cellPool;
    private List<Thumbnail> _cachedCells;
    private Bounds _viewportBounds;
    private Vector3[] _viewportCorners = new Vector3[4];

    // Trackers 
    private bool _recycling = false;
    private float rowHeight = 0, _colPadding, _rowPadding;
    private int _totalRows = 0, _pointer = 0, _firstCellIndex = 0, _botCellIndex, _currentItem = 0;
    
    // View transforms
    private RectTransform transformContent;
    private RectTransform viewport;

    // Complete list of thumbnails
	private List<ThumbnailVO> _thumbnailVOList = new List<ThumbnailVO>();
    #endregion

    #region Helpers
    private float RowHeight()
        => rowHeight + _rowPadding;

    private void SetViewportBounds()
    {
        viewport.GetWorldCorners(_viewportCorners);
        float viewThreshold = recycleThreshold * (_viewportCorners[2].y - _viewportCorners[0].y);
        // Applying 20% on the min & max bounds
        _viewportBounds.min = new Vector3(_viewportCorners[0].x, _viewportCorners[0].y - viewThreshold);
        _viewportBounds.max = new Vector3(_viewportCorners[2].x, _viewportCorners[2].y + viewThreshold);

    }

    private Vector3[] GetWorldCorners(RectTransform rect)
    {
        Vector3[] corners = new Vector3[4];
        rect.GetWorldCorners(corners);

        return corners;
    }

    private float GetMaxY(RectTransform rect)
        => GetMaxY(GetWorldCorners(rect));

    private float GetMinY(RectTransform rect)
        => GetMinY(GetWorldCorners(rect));

    private float GetMaxY(Vector3[] corners)
        => corners[1].y;

    private float GetMinY(Vector3[] corners)
        => corners[0].y;

    #endregion

    // Init
    void Start()
    {
        // Create our list
        CreateThumbnailVOList();

        // Viewport init
        viewport = scrollRect.viewport;
        _viewportBounds = new Bounds();
        SetViewportBounds();
        transformContent = container.GetComponent<RectTransform>();

        // Get our paddings : 
        GridLayoutGroup grid = container.GetComponent<GridLayoutGroup>();
        _colPadding = grid.spacing.x;
        _rowPadding = grid.spacing.y;

        // Just to get the row heigh based on the sprite rather than hardcoding it
        rowHeight = SpriteManager.instance.GetSprite("0").rect.height;
        InitPool();


        scrollRect.onValueChanged.AddListener(OnScrollRectChanged);
    }

    private void CreateThumbnailVOList()
    {
        ThumbnailVO thumbnailVO;
        for (int i = 0; i < 1000; i++)
        {
            thumbnailVO = new ThumbnailVO();
            thumbnailVO.id = i.ToString();
            _thumbnailVOList.Add(thumbnailVO);
        }
    }

    private void InitPool()
    {

        _cellPool = new List<RectTransform>();
        _cachedCells = new List<Thumbnail>();

        GameObject gameObj;

        int currentCol = 1;
        float currentCoverage = 0;
        float areaToCover = Screen.height * minCoverage;
        while (currentCoverage < areaToCover) // We'll create prefab while we don't have the area covered : 
        {
            gameObj = (GameObject)Instantiate(prefab);
            gameObj.transform.SetParent(container, false);
            gameObj.name = "Cell " + _currentItem;

            Thumbnail thumbnail = gameObj.GetComponent<Thumbnail>();
            thumbnail.thumbnailVO = _thumbnailVOList[_currentItem];

            if (currentCol >= columns)
            {
                currentCoverage += RowHeight();
                currentCol = 1;
                _totalRows++;
            }
            else
                currentCol++;

            _currentItem++;
            if (currentCoverage >= areaToCover)
                _botCellIndex = _totalRows;

            _cellPool.Add(gameObj.GetComponent<RectTransform>());
            _cachedCells.Add(thumbnail);
        }
    }

    // Entry point
    private void OnScrollRectChanged(Vector2 direction)
    {
        if (_recycling) return;

        if (direction.y < 0 && _currentItem < (_thumbnailVOList.Count - 1) && GetMaxY(_cellPool[_botCellIndex]) > _viewportBounds.min.y)
            MoveTopToBottom();
        else if (direction.y > 0 && _currentItem > _totalRows * columns && GetMinY(_cellPool[_firstCellIndex]) < _viewportBounds.max.y)
            MoveBottomToTop();

        Debug.Log("P : " + _pointer);
    }

    private void MoveTopToBottom()
    {
        _recycling = true;
        if (GetMinY(_cellPool[_firstCellIndex]) > _viewportBounds.max.y)
        {
            for (int i = 0; i < columns; i++)
            {
                _currentItem++;
                _cellPool[_firstCellIndex + i].SetAsLastSibling();
                if (_currentItem < _thumbnailVOList.Count) // Just to avoid the error, it would be ideal to get more time and hide or something the last cell
                    _cachedCells[_firstCellIndex + i].thumbnailVO = _thumbnailVOList[_currentItem];
            }

            _pointer++;
            if (_pointer >= _totalRows) 
                _pointer = 0;

            _botCellIndex = _firstCellIndex;
            _firstCellIndex = _pointer * columns;            

            transformContent.anchoredPosition -= Vector2.up * RowHeight();
        }
        _recycling = false;
    }

    private void MoveBottomToTop()
    {
        _recycling = true;
        if (GetMaxY(_cellPool[_botCellIndex]) < _viewportBounds.min.y)
        {
            for (int i = 0; i < columns; i++)
            {
                _currentItem--;
                _cellPool[_botCellIndex + i].SetAsFirstSibling();
                _cachedCells[_botCellIndex + i].thumbnailVO = _thumbnailVOList[_currentItem - 30];
            }

            _pointer--;

            if (_pointer <= 0)
                _pointer = _totalRows - 1;

            _firstCellIndex = _botCellIndex;
            _botCellIndex = (_pointer - 1) * columns;

            transformContent.anchoredPosition += Vector2.up * RowHeight();
        }

        _recycling = false;
    }

}
