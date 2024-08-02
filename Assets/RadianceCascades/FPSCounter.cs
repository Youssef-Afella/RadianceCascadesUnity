using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FPSCounter : MonoBehaviour
{
    private float count;

    private IEnumerator Start()
    {

        while (true)
        {
            count = 1f / Time.unscaledDeltaTime;

            GetComponent<Text>().text = "FPS : "+Mathf.RoundToInt(count);

            yield return new WaitForSeconds(0.1f);
        }
    }
}
