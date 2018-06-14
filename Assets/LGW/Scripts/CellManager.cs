﻿using System.Collections.Generic;
using UniRx;
using UnityEngine;
using UnityEngine.Assertions;

namespace LGW
{
    /// <summary>
    /// <see cref="CellController"/>を管理するクラス
    /// </summary>
    public sealed class CellManager : MonoBehaviour
    {
        [SerializeField]
        private CellController cellPrefab;

        private const int Capacity = 100000;

        public readonly List<CellController> CellList = new List<CellController>(Capacity);

        public readonly Dictionary<Point, CellController> CellDictionary = new Dictionary<Point, CellController>(Capacity);

        public readonly List<Point> NewCells = new List<Point>(Capacity);

        public readonly List<Point> DeathCells = new List<Point>(Capacity);

        public readonly Dictionary<Point, bool> ProcessedCells = new Dictionary<Point, bool>(Capacity);

        private PoolableCell poolableCell;

        private Vector3 dragPosition;

        private void Awake()
        {
            this.poolableCell = new PoolableCell(this.cellPrefab);
            this.poolableCell.PreloadAsync(1000, 3).Subscribe();
        }

        private void Update()
        {
            if(Input.GetKeyDown(KeyCode.Space))
            {
                this.NextGeneratioin();
            }
            if(Input.GetKey(KeyCode.Return))
            {
                this.NextGeneratioin();
            }

            var camera = Camera.main;
            var worldPoint = camera.ScreenToWorldPoint(Input.mousePosition);
            var id = new Point { x = Mathf.RoundToInt(worldPoint.x), y = Mathf.RoundToInt(worldPoint.y) };

            if(Input.GetMouseButton(0))
            {
                if(!this.CellDictionary.ContainsKey(id))
                {
                    this.CreateCell(id);
                }
            }
            if(Input.GetMouseButton(1))
            {
                if(this.CellDictionary.ContainsKey(id))
                {
                    this.RemoveCell(id);
                }
            }
            if(Input.GetMouseButtonDown(2))
            {
                this.dragPosition = worldPoint;
            }
            if(Input.GetMouseButton(2))
            {
                var diff = worldPoint - this.dragPosition;
                camera.transform.position -= diff;
                this.dragPosition = worldPoint;
            }

            var cameraSize = camera.orthographicSize;
            cameraSize -= Input.mouseScrollDelta.y * 1f;
            cameraSize = Mathf.Max(1, cameraSize);
            camera.orthographicSize = cameraSize;
        }

        public void NextGeneratioin()
        {
            this.NewCells.Clear();
            this.DeathCells.Clear();
            this.ProcessedCells.Clear();
            for(var i=0; i<this.CellList.Count; ++i)
            {
                this.CellList[i].NextGeneration(this);
            }

            for (var i = 0; i < this.NewCells.Count; ++i)
            {
                this.CreateCell(this.NewCells[i]);
            }
            for (var i = 0; i < this.DeathCells.Count; ++i)
            {
                this.RemoveCell(this.DeathCells[i]);
            }
        }

        public void CreateCell(Point id)
        {
            Assert.IsFalse(this.CellDictionary.ContainsKey(id), $"id = {id}にセルが存在するのに生成しようとしました");
            var cell = this.poolableCell.Rent();
            cell.Id = id;
            this.CellList.Add(cell);
            this.CellDictionary.Add(id, cell);
        }

        public void RemoveCell(Point id)
        {
            Assert.IsTrue(this.CellDictionary.ContainsKey(id), $"id = {id}にセルが無いのに削除しようとしました");
            var cell = this.CellDictionary[id];
            this.poolableCell.Return(cell);
            this.CellList.Remove(cell);
            this.CellDictionary.Remove(id);
        }

        public void ToggleCell(Point id)
        {
            if(this.CellDictionary.ContainsKey(id))
            {
                this.RemoveCell(id);
            }
            else
            {
                this.CreateCell(id);
            }
        }

        public int GetAdjacentNumber(Point id)
        {
            var result = 0;
            var min = new Point { x = id.x - 1, y = id.y - 1 };
            var max = new Point { x = id.x + 1, y = id.y + 1 };
            for(var y = min.y; y <= max.y; ++y)
            {
                for(var x = min.x; x <= max.x; ++x)
                {
                    var targetId = new Point { x = x, y = y };
                    if (id == targetId)
                    {
                        continue;
                    }
                    result += this.CellDictionary.ContainsKey(targetId) ? 1 : 0;
                }
            }

            return result;
        }
    }
}
