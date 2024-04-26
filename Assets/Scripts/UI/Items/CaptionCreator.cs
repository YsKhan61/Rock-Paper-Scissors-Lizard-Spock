using System.Collections;
using TMPro;
using UnityEngine;

namespace RPSLS.UI.Items
{
    public class CaptionCreator : MonoBehaviour
    {
        [SerializeField] private Transform startPoint;
        [SerializeField] private Transform endPoint;

        [SerializeField] private TextMeshProUGUI captionTextTmp;
        [SerializeField] private AnimationCurve tweenCurve;

        private Vector3 _start, _end;

        /// <summary>
        /// Translate the caption text from start to end.
        /// </summary>
        /// <param name="caption"></param>
        internal void TranslateCaptionText(string caption)
        {
            captionTextTmp.text = caption;
            StartCoroutine(TranslationRoutine());
        }

        private void Awake()
        {
            _start = startPoint.position;
            _end = endPoint.position;
            captionTextTmp.transform.position = _start;
        }

        private IEnumerator TranslationRoutine()
        {
            var progress = 0F;
            var eof = new WaitForEndOfFrame();
            var textTransform = captionTextTmp.transform;
            var finalProgress = tweenCurve[tweenCurve.length - 1].time;
            while (progress < finalProgress)
            {
                textTransform.position = Vector3.Lerp(_start, _end, tweenCurve.Evaluate(progress));
                yield return eof;
                progress += Time.deltaTime;
            }

            textTransform.position = _end;
        }
    }
}