using UnityEngine;
using UnityEngine.UI;

public class CrewIcon : MonoBehaviour
{
    public Button Button { get; private set; }
    public Sprite IconImage { get; set; }

    private void Start()
    {
        if (Button == null)
        {
            Button = GetComponent<Button>();
        }
    }
}
