using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace VertigoCase.UI
{
    // Top bar showing the player's current cash and gold totals.
    public class TopBarUI : MonoBehaviour
    {
        [SerializeField] private TMP_Text _ui_text_currency_cash_value;
        [SerializeField] private TMP_Text _ui_text_currency_gold_value;

        public void UpdateCurrency(int cash, int gold)
        {
            if (_ui_text_currency_cash_value != null)
                _ui_text_currency_cash_value.text = cash.ToString("N0");
            if (_ui_text_currency_gold_value != null)
                _ui_text_currency_gold_value.text = gold.ToString("N0");
        }

        // ── Editor auto-binding ────────────────────────────────────────────────
#if UNITY_EDITOR
        private void OnValidate()
        {
            if (_ui_text_currency_cash_value == null)
            {
                var t = transform.Find("ui_text_currency_cash_value");
                if (t != null) _ui_text_currency_cash_value = t.GetComponent<TMP_Text>();
                else Debug.LogWarning("[TopBarUI] 'ui_text_currency_cash_value' child not found.", this);
            }

            if (_ui_text_currency_gold_value == null)
            {
                var t = transform.Find("ui_text_currency_gold_value");
                if (t != null) _ui_text_currency_gold_value = t.GetComponent<TMP_Text>();
                else Debug.LogWarning("[TopBarUI] 'ui_text_currency_gold_value' child not found.", this);
            }
        }
#endif
    }
}
