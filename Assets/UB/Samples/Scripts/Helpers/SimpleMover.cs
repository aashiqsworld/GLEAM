using UnityEngine;
using System.Collections;

namespace UB
{
    public class SimpleMover : MonoBehaviour
    {

        public float MoveOnXMin = -5;
        public float MoveOnXMax = 5;

        private bool _isGoingLeft = true;

        void Update()
        {
            if (_isGoingLeft && gameObject.transform.position.x <= MoveOnXMin + 0.1)
            {
                _isGoingLeft = false;
            }
            else if (!_isGoingLeft && gameObject.transform.position.x >= MoveOnXMax - 0.1)
            {
                _isGoingLeft = true;
            }

            if (_isGoingLeft)
            {
                gameObject.transform.position = Vector3.Lerp(gameObject.transform.position,
                    new Vector3(MoveOnXMin,
                        gameObject.transform.position.y,
                        gameObject.transform.position.z), Time.deltaTime);

            }
            else
            {
                gameObject.transform.position = Vector3.Lerp(gameObject.transform.position,
                    new Vector3(MoveOnXMax,
                        gameObject.transform.position.y,
                        gameObject.transform.position.z), Time.deltaTime);
            }
        }
    }
}