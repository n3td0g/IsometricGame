using UnityEngine;

namespace Assets.Scripts
{
    public class CameraFollow : MonoBehaviour
    {
        public float XMargin = 0f;
        public float YMargin = 0f;
        public float XSmooth = 2f;
        public float YSmooth = 10f;
        public SpriteRenderer BorderSprite;

        private Vector2 _maxXAndY;
        private Vector2 _minXAndY;

        public Transform Player;

        void Start()
        {
            BorderSprite.color = new Color(0.0f, 0.0f, 0.0f, 0.0f);
            _minXAndY = new Vector2(BorderSprite.bounds.min.x, BorderSprite.bounds.min.y);
            _maxXAndY = new Vector2(BorderSprite.bounds.max.x, BorderSprite.bounds.max.y);
            transform.position = new Vector3(Player.position.x, Player.position.y, transform.position.z);
        }

        private bool CheckXMargin()
        {
            return Mathf.Abs(transform.position.x - Player.position.x) > XMargin;
        }

        private bool CheckYMargin()
        {
            return Mathf.Abs(transform.position.y - Player.position.y) > YMargin;
        }

        private void FixedUpdate()
        {
            TrackPlayer();
        }

        private void TrackPlayer()
        {
            float targetX = transform.position.x;
            float targetY = transform.position.y;

            if (CheckXMargin())
                targetX = Mathf.Lerp(transform.position.x, Player.position.x, XSmooth * Time.deltaTime);

            if (CheckYMargin())
                targetY = Mathf.Lerp(transform.position.y, Player.position.y, YSmooth * Time.deltaTime);

            var vertExtent = Camera.main.GetComponent<Camera>().orthographicSize;
            var horzExtent = vertExtent * Screen.width / Screen.height;

            targetX = Mathf.Clamp(targetX, _minXAndY.x + horzExtent, _maxXAndY.x - horzExtent);
            targetY = Mathf.Clamp(targetY, _minXAndY.y + vertExtent, _maxXAndY.y - vertExtent);

            transform.position = new Vector3(targetX, targetY, transform.position.z);
        }
    }
}

