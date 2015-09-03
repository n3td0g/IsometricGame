using System;
using UnityEditor;
using UnityEngine;

namespace Assets.Scripts.IsometricTools
{
    [ExecuteInEditMode]
    public class SimpleIsometricSprite : MonoBehaviour {
        public Vector2 Position = Vector2.zero;
        public Vector2 Point = Vector2.zero;
        public Vector2 Size = Vector2.one;

        private Vector2 _size = Vector2.one;
        private Vector2 _position = Vector2.zero;
        private Vector2 _point = Vector2.zero;
        private Vector2 _point0 = Vector2.zero;
        private Vector2 _point1 = Vector2.one;

        // Use this for initialization
        void Start () {
            _size = Size;
            _position = Position;
            _point = Point;
            _point0 = _position + _point;
            SetPoint1();
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
                var dx = IsometricSprite.FindMinValues(IsometricSprite.FindMinValues(p0Ceiling.x, p0Floor.x), IsometricSprite.FindMinValues(p1Ceiling.x, p1Floor.x));
                var dy = IsometricSprite.FindMinValues(IsometricSprite.FindMinValues(p0Ceiling.y, p0Floor.y), IsometricSprite.FindMinValues(p1Ceiling.y, p1Floor.y));
                _point0.x += dx;
                _point0.y += dy;
                _point1.x += dx;
                _point1.y += dy;
                _position.x += dx;
                _position.y += dy;
            }
        }

        void SetPoint1()
        {
            _point1.x = _point0.x + _size.x * transform.localScale.x;
            _point1.y = _point0.y + _size.y * transform.localScale.y;
        }

        void SetWorldPosition()
        {
            Vector3 position = IsometricGrid.TwoDToIso(_position.x, _position.y);
            position.z = transform.position.z;
            transform.position = position;
            transform.hasChanged = false;
        }
	
        // Update is called once per frame
        void Update () {
            if (transform.hasChanged)
            {
                _position = IsometricGrid.IsoTo2D(transform.position.x, transform.position.y);
                _point0 = _position + _point;
                SetPoint1();
                SnapToGrid();
                Position = _position;
                SetWorldPosition();
            }
            else if (Size != _size)
            {
                _size = Size;
                SetPoint1();
            }
            else if (Position != _position)
            {
                _position = Position;
                _point0 = _position + _point;
                SetPoint1();
                SetWorldPosition();
            }
            else if (Point != _point)
            {
                _point = Point;
                _point0 = _position + _point;
                SetPoint1();
            }
        }

        private void OnDrawGizmos()
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

            Gizmos.color = oldColor;
        }
    }
}
