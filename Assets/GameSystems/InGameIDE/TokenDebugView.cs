using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Blackletter
{
    public sealed class TokenDebugView : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private RectTransform contentRoot;
        [SerializeField] private RectTransform rowPrefab;
        [SerializeField] private TokenBlock tokenPrefab;

        [Header("Layout")]
        [SerializeField] private float maxRowWidth = 800f;

        private RectTransform currentRow;
        private float currentRowWidth;

        public void Rebuild(List<Token> tokens)
        {
            Debug.Log("Rebuilding token view...");

            Clear();

            currentRow = CreateRow();
            currentRowWidth = 0f;

            foreach (var token in tokens)
            {
                var block = Instantiate(tokenPrefab, currentRow);
                block.Set(token);

                LayoutRebuilder.ForceRebuildLayoutImmediate(block.Rect);

                float blockWidth = block.Rect.rect.width;

                if (currentRowWidth + blockWidth > maxRowWidth)
                {
                    currentRow = CreateRow();
                    currentRowWidth = 0f;

                    block.transform.SetParent(currentRow, false);
                }

                currentRowWidth += blockWidth;
            }
        }

        private RectTransform CreateRow()
        {
            return Instantiate(rowPrefab, contentRoot);
        }

        private void Clear()
        {
            foreach (Transform child in contentRoot)
                Destroy(child.gameObject);
        }
    }
}
