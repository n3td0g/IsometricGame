using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Assets.Scripts.IsometricTools
{
    [ExecuteInEditMode]
    public class IsometricSprite : MonoBehaviour
    {
        public Vector3 Position = Vector3.zero;
        public Vector3 Point = Vector3.zero;
        public Vector3 Size = Vector3.one;
        
        [HideInInspector]
        public HashSet<IsometricSprite> SpritesBehind = new HashSet<IsometricSprite>();
        public static readonly List<IsometricSprite> Sprites = new List<IsometricSprite>();

        private Vector3 _size = Vector3.one;
        private Vector3 _position = Vector3.zero;
        private Vector3 _point = Vector3.zero;
        private Vector3 _point0 = Vector3.zero;
        private Vector3 _point1 = Vector3.one;
        private int _sortingOrder;
        private SpriteRenderer _sprite;
        private bool _visitedFlag = false;

        #region Sprites sorting
        private void Topological(IsometricSprite sprite)
        {
            foreach (var s in Sprites)
            {
                if (s != sprite)
                {
                    if (s._point1.x > sprite._point0.x && s._point1.y > sprite._point0.y && s._point0.z < sprite._point1.z)
                    {
                        if (!sprite.SpritesBehind.Contains(s))
                            sprite.SpritesBehind.Add(s);
                    }
                    else
                    {
                        if (sprite.SpritesBehind.Contains(s))
                            sprite.SpritesBehind.Remove(s);
                    }

                    if (sprite._point1.x > s._point0.x && sprite._point1.y > s._point0.y && sprite._point0.z < s._point1.z)
                    {
                        if (!s.SpritesBehind.Contains(sprite))
                            s.SpritesBehind.Add(sprite);
                    }
                    else
                    {
                        if (s.SpritesBehind.Contains(sprite))
                            s.SpritesBehind.Remove(sprite);
                    }
                }
                s._visitedFlag = false;
            }
            _sortingOrder = 0;
            foreach (var s in Sprites)
                if (!s._visitedFlag)
                    VisitNode(s);
        }

        private void VisitNode(IsometricSprite sprite)
        {
            sprite._visitedFlag = true;
            if (sprite.SpritesBehind.Count > 0)
            {
                foreach (var value in sprite.SpritesBehind)
                    if (!value._visitedFlag)
                        VisitNode(value);
            }
            sprite.SetSortOrder(_sortingOrder++);
        }

        void SetSortOrder(int sortingOrder)
        {
            if (_sprite == null)
                _sprite = GetComponent<SpriteRenderer>();
            if (_sprite == null)
            {
                Debug.Log("Can't find SpriteRenderer");
                return;
            }
            _sprite.sortingOrder = sortingOrder;
        }

        #endregion

        #region Set sprite points
        void SetPoint1()
        {
            _point1.x = _point0.x + _size.x * transform.localScale.x;
            _point1.y = _point0.y + _size.y * transform.localScale.y;
            _point1.z = _point0.z + _size.z * transform.localScale.z;
        }

        void SetWorldPosition()
        {
            Vector3 position = IsometricGrid.TwoDToIso(_position.x, _position.y);
            position.y += _position.z;
            position.z = transform.position.z;
            transform.position = position;
            transform.hasChanged = false;
        }

        public static float FindMinValues(float x, float y)
        {
            return Math.Abs(x) < Math.Abs(y) ? x : y;
        }

        void SnapToGrid()
        {
            var isometricGrid = FindObjectOfType<IsometricGrid>();
            if (isometricGrid != null)
            {
                var p0 = _point0;
                var p1 = _point1;
                var p0Ceiling = new Vector2((float)Math.Ceiling(p0.x / isometricGrid.GridStep) * isometricGrid.GridStep, (float)Math.Ceiling(p0.y / isometricGrid.GridStep) * isometricGrid.GridStep);
                var p0Floor = new Vector2(p0Ceiling.x - isometricGrid.GridStep, p0Ceiling.y - isometricGrid.GridStep);
                p0Ceiling.x -= p0.x;
                p0Ceiling.y -= p0.y;
                p0Floor.x -= p0.x;
                p0Floor.y -= p0.y;
                var p1Ceiling = new Vector2((float)Math.Ceiling(p1.x / isometricGrid.GridStep) * isometricGrid.GridStep, (float)Math.Ceiling(p1.y / isometricGrid.GridStep) * isometricGrid.GridStep);
                var p1Floor = new Vector2(p1Ceiling.x - isometricGrid.GridStep, p1Ceiling.y - isometricGrid.GridStep);
                p1Ceiling.x -= p1.x;
                p1Ceiling.y -= p1.y;
                p1Floor.x -= p1.x;
                p1Floor.y -= p1.y;
                var dx = FindMinValues(FindMinValues(p0Ceiling.x, p0Floor.x), FindMinValues(p1Ceiling.x, p1Floor.x));
                var dy = FindMinValues(FindMinValues(p0Ceiling.y, p0Floor.y), FindMinValues(p1Ceiling.y, p1Floor.y));
                _point0.x += dx;
                _point0.y += dy;
                _point1.x += dx;
                _point1.y += dy;
                _position.x += dx;
                _position.y += dy;
            }
        }
        #endregion

        // Use this for initialization
        void Start ()
        {
            _size = Size;
            _position = Position;
            _point = Point;
            _point0 = _position + _point;
            SetPoint1();
            if (Sprites.Contains(this))
                OnDestroy();
            Sprites.Add(this);
            Topological(this);
        }

        // Update is called once per frame
        void Update()
        {
            bool needSort = false;
            if (transform.hasChanged)
            {
                var point = IsometricGrid.IsoTo2D(transform.position.x, transform.position.y - _position.z);
                _position.x = point.x;
                _position.y = point.y;
                _point0 = _position + _point;
                SetPoint1();
                SnapToGrid();
                Position = _position;
                SetWorldPosition();
                needSort = true;
            }
            else if (Size != _size)
            {
                _size = Size;
                SetPoint1();
                needSort = true;
            }
            else if (Position != _position)
            {
                _position = Position;
                _point0 = _position + _point;
                SetPoint1();
                SetWorldPosition();
                needSort = true;
            } 
            else if (Point != _point)
            {
                _point = Point;
                _point0 = _position + _point;
                SetPoint1();
                needSort = true;
            }

            if (needSort)
                Topological(this);
        }

        void OnDestroy()
        {
            Sprites.Remove(this);
            foreach (var sprite in Sprites)
                if (sprite.SpritesBehind.Contains(this))
                    sprite.SpritesBehind.Remove(this);
        }

        void OnDrawGizmos()
        {
            bool selected = Selection.Contains(gameObject);
            var isometricGrid = FindObjectOfType<IsometricGrid>();
            if (isometricGrid != null && !isometricGrid.ShowSpritesLines && !selected)
                return;

            var oldColor = Gizmos.color;

            var p0 = IsometricGrid.TwoDToIso(_point0.x, _point0.y);
            var p1 = IsometricGrid.TwoDToIso(_point1.x, _point0.y);
            var p2 = IsometricGrid.TwoDToIso(_point1.x, _point1.y);
            var p3 = IsometricGrid.TwoDToIso(_point0.x, _point1.y);

            Gizmos.color = selected ? Color.cyan : Color.blue;

            Gizmos.DrawLine(new Vector3(p0.x, p0.y, 0.0f), new Vector3(p1.x, p1.y, 0.0f));
            Gizmos.DrawLine(new Vector3(p1.x, p1.y, 0.0f), new Vector3(p2.x, p2.y, 0.0f));
            Gizmos.DrawLine(new Vector3(p2.x, p2.y, 0.0f), new Vector3(p3.x, p3.y, 0.0f));
            Gizmos.DrawLine(new Vector3(p3.x, p3.y, 0.0f), new Vector3(p0.x, p0.y, 0.0f));
            Gizmos.DrawLine(new Vector3(p0.x, p0.y, 0.0f), new Vector3(p2.x, p2.y, 0.0f));
            Gizmos.DrawLine(new Vector3(p1.x, p1.y, 0.0f), new Vector3(p3.x, p3.y, 0.0f));

            Gizmos.color = selected ? Color.red : Color.magenta;

            Gizmos.DrawLine(new Vector3(p0.x, p0.y + _point0.z, 0.0f), new Vector3(p1.x, p1.y + _point0.z, 0.0f));
            Gizmos.DrawLine(new Vector3(p1.x, p1.y + _point0.z, 0.0f), new Vector3(p2.x, p2.y + _point0.z, 0.0f));
            Gizmos.DrawLine(new Vector3(p2.x, p2.y + _point0.z, 0.0f), new Vector3(p3.x, p3.y + _point0.z, 0.0f));
            Gizmos.DrawLine(new Vector3(p3.x, p3.y + _point0.z, 0.0f), new Vector3(p0.x, p0.y + _point0.z, 0.0f));

            Gizmos.DrawLine(new Vector3(p0.x, p0.y + _point1.z, 0.0f), new Vector3(p1.x, p1.y + _point1.z, 0.0f));
            Gizmos.DrawLine(new Vector3(p1.x, p1.y + _point1.z, 0.0f), new Vector3(p2.x, p2.y + _point1.z, 0.0f));
            Gizmos.DrawLine(new Vector3(p2.x, p2.y + _point1.z, 0.0f), new Vector3(p3.x, p3.y + _point1.z, 0.0f));
            Gizmos.DrawLine(new Vector3(p3.x, p3.y + _point1.z, 0.0f), new Vector3(p0.x, p0.y + _point1.z, 0.0f));

            Gizmos.DrawLine(new Vector3(p0.x, p0.y + _point0.z, 0.0f), new Vector3(p0.x, p0.y + _point1.z, 0.0f));
            Gizmos.DrawLine(new Vector3(p1.x, p1.y + _point0.z, 0.0f), new Vector3(p1.x, p1.y + _point1.z, 0.0f));
            Gizmos.DrawLine(new Vector3(p2.x, p2.y + _point0.z, 0.0f), new Vector3(p2.x, p2.y + _point1.z, 0.0f));
            Gizmos.DrawLine(new Vector3(p3.x, p3.y + _point0.z, 0.0f), new Vector3(p3.x, p3.y + _point1.z, 0.0f));

            Gizmos.color = oldColor;
        }
    }
}
