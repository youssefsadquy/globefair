using UnityEngine;
using UnityEngine.UI;

// Method attached to Q&A prefab to populate the 2 fields
public class FAQItem : MonoBehaviour
{
    [SerializeField] private Text questionText;
    [SerializeField] private Text answerText;

    public void SetQuestionAndAnswer(string question, string answer)
    {
        questionText.text = question;
        answerText.text = answer;
    }
}